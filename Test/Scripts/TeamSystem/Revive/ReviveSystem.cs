using System;
using Fusion;
using UnityEngine;

namespace TPSBR
{
    public class ReviveSystem : NetworkBehaviour, IInteraction
    {
        [Header("Downed Animation")]
        [SerializeField]
        [Tooltip("Animation clip to play when player is downed (optional)")]
        private AnimationClip _downedAnimationClip;

        public Action<Player> OnPlayerDowned;
        public Action<Player, Player> OnReviveStarted;
        public Action<Player, Player> OnReviveCompleted;
        public Action<Player> OnReviveCancelled;
        public Action<Player> OnPlayerBledOut;

        [Networked]
        public ReviveData ReviveData { get; set; }

        private Player _player;
        private Agent _agent;
        private NetworkGame _networkGame;
        private Animator _animator;
        private int _downedAnimationHash;
        private GUIStyle _guiStyle;
        private CapsuleCollider _downedCollider;
        private float _lastBleedOutLog;

        public bool IsDown => ReviveData.IsDown;
        public bool IsBeingRevived => ReviveData.HasRevivingPlayer;
        public float BleedOutProgress => ReviveData.BleedOutTimer.RemainingTime(Runner) ?? 0f;
        public float ReviveProgress => 1f - (ReviveData.ReviveTimer.RemainingTime(Runner) ?? ReviveSettings.REVIVE_DURATION) / ReviveSettings.REVIVE_DURATION;

        public string Name => "";
        public string Description => "";
        public Vector3 HUDPosition => Vector3.zero;
        public bool IsActive => false;

        public override void Spawned()
        {
            _player = GetComponent<Player>();
            _networkGame = Runner.SimulationUnityScene.GetComponent<NetworkGame>(true);

            if (_downedAnimationClip != null)
            {
                _downedAnimationHash = Animator.StringToHash(_downedAnimationClip.name);
            }

            _guiStyle = new GUIStyle();
            _guiStyle.fontSize = 40;
            _guiStyle.fontStyle = FontStyle.Bold;
            _guiStyle.alignment = TextAnchor.MiddleCenter;
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
                return;

            if (!ReviveData.IsDown)
                return;

            float remainingTime = ReviveData.BleedOutTimer.RemainingTime(Runner) ?? 0f;
            
            if (Time.time - _lastBleedOutLog >= 1f)
            {
                _lastBleedOutLog = Time.time;
                Debug.Log($"[ReviveSystem] {_player?.Nickname} bleed-out timer: {remainingTime:F1}s remaining (HasAuthority: {HasStateAuthority})");
            }
            
            if (remainingTime <= 0f)
            {
                Debug.Log($"[ReviveSystem] Bleed-out timer expired for {_player?.Nickname}");
                OnBleedOut();
                return;
            }

            if (ReviveData.HasRevivingPlayer)
            {
                float reviveRemaining = ReviveData.ReviveTimer.RemainingTime(Runner) ?? 0f;
                if (reviveRemaining <= 0f)
                {
                    Debug.Log($"[ReviveSystem] Revive completed for {_player?.Nickname}");
                    CompleteRevive();
                }
            }
        }

        public void EnterDownedState()
        {
            if (!HasStateAuthority)
            {
                RPC_EnterDownedState();
                return;
            }

            if (ReviveData.IsDown)
            {
                Debug.LogWarning("[ReviveSystem] Player already downed, ignoring EnterDownedState call");
                return;
            }

            var reviveData = ReviveData;
            reviveData.IsDown = true;
            reviveData.BleedOutTimer = TickTimer.CreateFromSeconds(Runner, ReviveSettings.BLEED_OUT_DURATION);
            reviveData.HasRevivingPlayer = false;
            ReviveData = reviveData;

            Debug.Log($"[ReviveSystem] Entered downed state. Bleed-out timer set to {ReviveSettings.BLEED_OUT_DURATION} seconds. Timer IsRunning: {ReviveData.BleedOutTimer.IsRunning}, ExpiredOrNotRunning: {ReviveData.BleedOutTimer.ExpiredOrNotRunning(Runner)}");

            _agent = _player.ActiveAgent;
            if (_agent != null)
            {
                _agent.Character.CharacterController.SetActive(false);
                DisableAgentActions();
                
                if (Physics.Raycast(_agent.transform.position, Vector3.down, out RaycastHit hit, 10f, LayerMask.GetMask("Default", "Dirt", "Wood", "Metal")))
                {
                    _agent.transform.position = hit.point + Vector3.up * 0.1f;
                }

                CreateDownedCollider();
                PlayDownedAnimation();
            }

            OnPlayerDowned?.Invoke(_player);
        }

        private void CreateDownedCollider()
        {
            if (_downedCollider != null)
                return;

            var agentCollider = _agent.GetComponentInChildren<CapsuleCollider>();
            if (agentCollider == null)
            {
                Debug.LogWarning("[ReviveSystem] No CapsuleCollider found on agent, creating default collider");
                _downedCollider = _agent.gameObject.AddComponent<CapsuleCollider>();
                _downedCollider.height = 1f;
                _downedCollider.radius = 0.5f;
                _downedCollider.center = new Vector3(0, 0.5f, 0);
            }
            else
            {
                _downedCollider = _agent.gameObject.AddComponent<CapsuleCollider>();
                _downedCollider.height = agentCollider.height * 0.3f;
                _downedCollider.radius = agentCollider.radius;
                _downedCollider.center = new Vector3(0, _downedCollider.height * 0.5f, 0);
            }

            _downedCollider.isTrigger = false;
            
            int agentLayer = LayerMask.NameToLayer("Agent");
            if (agentLayer != -1)
            {
                _agent.gameObject.layer = agentLayer;
            }

            Debug.Log($"[ReviveSystem] Created downed collider - Height: {_downedCollider.height}, Radius: {_downedCollider.radius}");
        }

        private void RemoveDownedCollider()
        {
            if (_downedCollider != null)
            {
                Destroy(_downedCollider);
                _downedCollider = null;
                Debug.Log("[ReviveSystem] Removed downed collider");
            }
        }

        public void StartRevive(PlayerRef reviverRef)
        {
            if (!HasStateAuthority)
            {
                RPC_StartRevive(reviverRef);
                return;
            }

            if (!ReviveData.IsDown || ReviveData.HasRevivingPlayer)
                return;

            var reviver = _networkGame?.GetPlayer(reviverRef);
            if (reviver == null)
                return;

            if (!CanRevive(reviver))
                return;

            var reviveData = ReviveData;
            reviveData.RevivingPlayer = reviverRef;
            reviveData.ReviveTimer = TickTimer.CreateFromSeconds(Runner, ReviveSettings.REVIVE_DURATION);
            reviveData.HasRevivingPlayer = true;
            ReviveData = reviveData;

            OnReviveStarted?.Invoke(_player, reviver);
        }

        public void CancelRevive()
        {
            if (!HasStateAuthority)
            {
                RPC_CancelRevive();
                return;
            }

            if (!ReviveData.HasRevivingPlayer)
                return;

            var reviveData = ReviveData;
            reviveData.HasRevivingPlayer = false;
            reviveData.RevivingPlayer = PlayerRef.None;
            ReviveData = reviveData;

            OnReviveCancelled?.Invoke(_player);
        }

        private void CompleteRevive()
        {
            var reviver = _networkGame?.GetPlayer(ReviveData.RevivingPlayer);

            var reviveData = ReviveData;
            reviveData.IsDown = false;
            reviveData.HasRevivingPlayer = false;
            reviveData.RevivingPlayer = PlayerRef.None;
            reviveData.BleedOutTimer = TickTimer.None;
            ReviveData = reviveData;

            if (_agent != null)
            {
                _agent.Character.CharacterController.SetActive(true);
                EnableAgentActions();
                RemoveDownedCollider();

                var hitData = new HitData
                {
                    Action = EHitAction.Heal,
                    Amount = ReviveSettings.REVIVE_HEALTH_RESTORED,
                    Target = _agent.Health
                };
                ((IHitTarget)_agent.Health).ProcessHit(ref hitData);

                StopDownedAnimation();
            }

            var statistics = _player.Statistics;
            statistics.IsAlive = true;
            _player.UpdateStatistics(statistics);

            OnReviveCompleted?.Invoke(_player, reviver);
        }

        private void OnBleedOut()
        {
            Debug.Log($"[ReviveSystem] Player {_player?.Nickname} bled out, dealing fatal damage");

            var reviveData = ReviveData;
            reviveData.IsDown = false;
            reviveData.HasRevivingPlayer = false;
            ReviveData = reviveData;

            RemoveDownedCollider();

            if (_agent != null && _agent.Health != null)
            {
                var hitData = new HitData
                {
                    Action = EHitAction.Damage,
                    Amount = 9999f,
                    Target = _agent.Health,
                    InstigatorRef = PlayerRef.None,
                    IsFatal = true
                };
                ((IHitTarget)_agent.Health).ProcessHit(ref hitData);
            }

            OnPlayerBledOut?.Invoke(_player);
        }

        private bool CanRevive(Player reviver)
        {
            if (reviver == null || _player == null)
                return false;

            if (reviver.ActiveAgent == null || _player.ActiveAgent == null)
                return false;

            if (!reviver.IsTeammateWith(_player))
                return false;

            float distance = Vector3.Distance(
                reviver.ActiveAgent.transform.position,
                _player.ActiveAgent.transform.position
            );

            return distance <= ReviveSettings.REVIVE_INTERACTION_DISTANCE;
        }

        private void DisableAgentActions()
        {
            if (_agent == null)
                return;

            if (_agent.Weapons != null)
            {
                _agent.Weapons.DisarmCurrentWeapon();
            }
        }

        private void EnableAgentActions()
        {
            if (_agent == null)
                return;

            if (_agent.Weapons != null && _agent.Weapons.PreviousWeaponSlot > 0)
            {
                _agent.Weapons.SetPendingWeapon(_agent.Weapons.PreviousWeaponSlot);
                _agent.Weapons.ArmPendingWeapon();
            }
        }

        public Player GetRevivingPlayer()
        {
            if (!ReviveData.HasRevivingPlayer)
                return null;

            return _networkGame?.GetPlayer(ReviveData.RevivingPlayer);
        }

        private void OnGUI()
        {
            if (!IsDown || _player == null || !_player.HasInputAuthority)
                return;

            float bleedOutTime = BleedOutProgress;
            string message = IsBeingRevived 
                ? $"BEING REVIVED\n{(ReviveProgress * 100f):F0}%"
                : $"YOU ARE DOWNED\nBleed out in {Mathf.CeilToInt(bleedOutTime)}s";

            var rect = new Rect(Screen.width / 2 - 300, Screen.height * 0.7f, 600, 150);
            
            _guiStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height), message, _guiStyle);
            
            _guiStyle.normal.textColor = IsBeingRevived ? Color.green : Color.red;
            GUI.Label(rect, message, _guiStyle);
        }

        private void PlayDownedAnimation()
        {
            if (_downedAnimationClip == null || _agent == null)
                return;

            if (_animator == null)
            {
                _animator = _agent.GetComponentInChildren<Animator>();
            }

            if (_animator != null && _downedAnimationHash != 0)
            {
                _animator.CrossFade(_downedAnimationHash, 0.2f);
                Debug.Log($"[ReviveSystem] Playing downed animation: {_downedAnimationClip.name}");
            }
        }

        private void StopDownedAnimation()
        {
            if (_animator != null)
            {
                _animator.Rebind();
                Debug.Log("[ReviveSystem] Stopped downed animation");
            }
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_EnterDownedState()
        {
            EnterDownedState();
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_StartRevive(PlayerRef reviverRef)
        {
            StartRevive(reviverRef);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
        private void RPC_CancelRevive()
        {
            CancelRevive();
        }
    }
}
