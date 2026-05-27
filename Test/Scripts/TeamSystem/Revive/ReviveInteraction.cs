using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSBR
{
    public class ReviveInteraction : ContextBehaviour
    {
        [SerializeField]
        private LayerMask _playerLayer = -1;
        [SerializeField]
        private float _checkInterval = 0.1f;

        private Player _localPlayer;
        private Agent _localAgent;
        private ReviveSystem _nearbyDownedPlayer;
        private float _checkTimer;
        private bool _isReviving;
        private GUIStyle _promptStyle;

        private void Awake()
        {
            _promptStyle = new GUIStyle();
            _promptStyle.fontSize = 36;
            _promptStyle.fontStyle = FontStyle.Bold;
            _promptStyle.alignment = TextAnchor.MiddleCenter;
        }

        private void Update()
        {
            if (Context == null || Context.Runner == null)
                return;

            if (_localPlayer == null || _localAgent == null)
            {
                TryGetLocalPlayer();
                return;
            }

            var localReviveSystem = _localPlayer.GetComponent<ReviveSystem>();
            if (localReviveSystem != null && localReviveSystem.IsDown)
            {
                _nearbyDownedPlayer = null;
                _isReviving = false;
                return;
            }

            _checkTimer += Time.deltaTime;
            if (_checkTimer >= _checkInterval)
            {
                _checkTimer = 0f;
                FindNearbyDownedPlayer();
            }

            HandleInput();
        }

        private void TryGetLocalPlayer()
        {
            if (Context?.NetworkGame == null)
                return;

            _localPlayer = Context.NetworkGame.GetPlayer(Context.LocalPlayerRef);
            if (_localPlayer != null)
            {
                _localAgent = _localPlayer.ActiveAgent;
                Debug.Log($"[ReviveInteraction] Found local player: {_localPlayer.Nickname}");
            }
        }

        private void FindNearbyDownedPlayer()
        {
            if (_localAgent == null)
            {
                _nearbyDownedPlayer = null;
                return;
            }

            if (_playerLayer.value == -1)
            {
                _playerLayer = LayerMask.GetMask("Agent");
            }

            Collider[] colliders = Physics.OverlapSphere(
                _localAgent.transform.position,
                ReviveSettings.REVIVE_INTERACTION_DISTANCE,
                _playerLayer
            );

            ReviveSystem closestDowned = null;
            float closestDistance = float.MaxValue;

            foreach (var col in colliders)
            {
                var reviveSystem = col.GetComponentInParent<ReviveSystem>();
                if (reviveSystem == null)
                    continue;

                if (!reviveSystem.IsDown)
                    continue;

                var downedPlayer = reviveSystem.GetComponent<Player>();
                if (downedPlayer == null || downedPlayer == _localPlayer)
                    continue;

                if (!_localPlayer.IsTeammateWith(downedPlayer))
                    continue;

                if (reviveSystem.IsBeingRevived && reviveSystem.GetRevivingPlayer() != _localPlayer)
                    continue;

                float dist = Vector3.Distance(_localAgent.transform.position, col.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestDowned = reviveSystem;
                }
            }

            if (closestDowned != _nearbyDownedPlayer)
            {
                _nearbyDownedPlayer = closestDowned;
                
                if (_nearbyDownedPlayer != null)
                {
                    var downedPlayer = _nearbyDownedPlayer.GetComponent<Player>();
                    Debug.Log($"[ReviveInteraction] Found downed player: {downedPlayer?.Nickname} at {closestDistance:F1}m");
                }
                else if (!_isReviving)
                {
                    Debug.Log("[ReviveInteraction] No downed players nearby");
                }
            }
        }

        private void HandleInput()
        {
            if (_nearbyDownedPlayer == null)
            {
                if (_isReviving)
                {
                    StopRevive();
                }
                return;
            }

            bool uKeyPressed = Keyboard.current != null && Keyboard.current.uKey.isPressed;

            if (uKeyPressed && !_isReviving)
            {
                StartRevive();
            }
            else if (!uKeyPressed && _isReviving)
            {
                StopRevive();
            }
        }

        private void StartRevive()
        {
            if (_nearbyDownedPlayer == null || _localPlayer == null)
                return;

            _isReviving = true;
            _nearbyDownedPlayer.StartRevive(_localPlayer.Object.InputAuthority);
            
            var downedPlayer = _nearbyDownedPlayer.GetComponent<Player>();
            Debug.Log($"[ReviveInteraction] Started reviving {downedPlayer?.Nickname}");
        }

        private void StopRevive()
        {
            if (_nearbyDownedPlayer == null)
                return;

            _isReviving = false;
            _nearbyDownedPlayer.CancelRevive();
            
            Debug.Log("[ReviveInteraction] Stopped reviving");
        }

        private void OnGUI()
        {
            if (_nearbyDownedPlayer == null || !_nearbyDownedPlayer.IsDown)
                return;

            if (_localPlayer == null)
                return;

            var downedPlayer = _nearbyDownedPlayer.GetComponent<Player>();
            if (downedPlayer == null)
                return;

            if (_promptStyle == null)
            {
                _promptStyle = new GUIStyle();
                _promptStyle.fontSize = 36;
                _promptStyle.fontStyle = FontStyle.Bold;
                _promptStyle.alignment = TextAnchor.MiddleCenter;
            }

            string message;
            Color color;

            if (_isReviving)
            {
                float progress = _nearbyDownedPlayer.ReviveProgress * 100f;
                message = $"Reviving {downedPlayer.Nickname}...\n{progress:F0}%";
                color = Color.green;
            }
            else
            {
                message = $"Hold [U] to Revive\n{downedPlayer.Nickname}";
                color = Color.yellow;
            }

            var rect = new Rect(Screen.width / 2 - 300, Screen.height * 0.35f, 600, 120);

            _promptStyle.normal.textColor = Color.black;
            GUI.Label(new Rect(rect.x + 2, rect.y + 2, rect.width, rect.height), message, _promptStyle);

            _promptStyle.normal.textColor = color;
            GUI.Label(rect, message, _promptStyle);
        }

        private void OnDisable()
        {
            if (_isReviving)
            {
                StopRevive();
            }
        }

        public ReviveSystem GetNearbyDownedPlayer()
        {
            return _nearbyDownedPlayer;
        }

        public bool IsReviving()
        {
            return _isReviving;
        }
    }
}
