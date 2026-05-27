using UnityEngine;
using Fusion;

namespace TPSBR
{
    public class QuestEventIntegration : MonoBehaviour
    {
        private static QuestEventIntegration _instance;
        public static QuestEventIntegration Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("QuestEventIntegration");
                    _instance = go.AddComponent<QuestEventIntegration>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        private QuestTracker _questTracker;
        private NetworkGame _networkGame;
        private GameplayMode _gameplayMode;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            SetupQuestTracker();
        }

        private void SetupQuestTracker()
        {
            if (_questTracker == null)
            {
                _questTracker = gameObject.AddComponent<QuestTracker>();
            }
        }

        public void SetNetworkGame(NetworkGame networkGame)
        {
            _networkGame = networkGame;
        }

        public void SetGameplayMode(GameplayMode gameplayMode)
        {
            _gameplayMode = gameplayMode;

            if (_gameplayMode != null)
            {
                _gameplayMode.OnAgentDeath += OnAgentDeath;
                _gameplayMode.OnPlayerEliminated += OnPlayerEliminated;
            }
        }

        public void OnGameplayModeActivated()
        {
            if (_questTracker != null)
            {
                _questTracker.OnMatchStart();
            }
        }

        public void OnGameplayModeFinished(int playerPosition, int totalPlayers, bool isWinner)
        {
            if (_questTracker != null)
            {
                _questTracker.OnMatchEnd(playerPosition, totalPlayers, isWinner);
            }
        }

        public void OnAgentKilled(KillData killData)
        {
            OnAgentDeath(killData);
        }

        private void OnAgentDeath(KillData killData)
        {
            if (_questTracker == null || _networkGame == null)
                return;

            var killer = _networkGame.GetPlayer(killData.KillerRef);
            var victim = _networkGame.GetPlayer(killData.VictimRef);

            if (killer == null || victim == null)
                return;

            if (IsLocalPlayer(killer))
            {
                float distance = CalculateKillDistance(killer, victim);
                string weaponType = GetWeaponTypeFromHitType(killData.HitType);
                
                _questTracker.OnKillObtained(killData, distance, weaponType);
            }
        }

        private float CalculateKillDistance(Player killer, Player victim)
        {
            if (killer.ActiveAgent != null && victim.ActiveAgent != null)
            {
                Vector3 killerPos = killer.ActiveAgent.transform.position;
                Vector3 victimPos = victim.ActiveAgent.transform.position;
                return Vector3.Distance(killerPos, victimPos);
            }
            
            return 15f;
        }

        private void OnPlayerEliminated(PlayerRef playerRef)
        {
        }

        public void OnDamageDealt(int damage, PlayerRef attacker)
        {
            if (_questTracker == null || _networkGame == null)
                return;

            var attackerPlayer = _networkGame.GetPlayer(attacker);
            if (attackerPlayer != null && IsLocalPlayer(attackerPlayer))
            {
                _questTracker.OnDamageDealt(damage);
            }
        }

        public void OnStormDamageTaken(PlayerRef playerRef)
        {
            if (_questTracker == null || _networkGame == null)
                return;

            var player = _networkGame.GetPlayer(playerRef);
            if (player != null && IsLocalPlayer(player))
            {
                _questTracker.OnStormDamageTaken();
            }
        }

        public void OnItemPickedUp(PlayerRef playerRef)
        {
            if (_questTracker == null || _networkGame == null)
                return;

            var player = _networkGame.GetPlayer(playerRef);
            if (player != null && IsLocalPlayer(player))
            {
                _questTracker.OnItemPickedUp();
            }
        }

        public void OnHealingItemUsed(PlayerRef playerRef)
        {
            if (_questTracker == null || _networkGame == null)
                return;

            var player = _networkGame.GetPlayer(playerRef);
            if (player != null && IsLocalPlayer(player))
            {
                _questTracker.OnHealingItemUsed();
            }
        }

        public void OnPlayerLanded(PlayerRef playerRef, Vector3 position)
        {
            if (_questTracker == null || _networkGame == null)
                return;

            var player = _networkGame.GetPlayer(playerRef);
            if (player != null && IsLocalPlayer(player))
            {
                _questTracker.OnPlayerLanded(position);
            }
        }

        public void OnStormCircleChanged(int circleNumber)
        {
            if (_questTracker != null)
            {
                _questTracker.OnStormCircleChanged(circleNumber);
            }
        }

        private bool IsLocalPlayer(Player player)
        {
            if (_networkGame == null || _networkGame.Runner == null)
                return false;

            return player.Object.InputAuthority == _networkGame.Runner.LocalPlayer;
        }

        private string GetWeaponTypeFromHitType(EHitType hitType)
        {
            switch (hitType)
            {
                case EHitType.Pistol:
                    return "Pistol";
                case EHitType.Shotgun:
                    return "Shotgun";
                case EHitType.SMG:
                    return "SMG";
                case EHitType.Rifle:
                    return "Rifle";
                case EHitType.Sniper:
                    return "Sniper";
                case EHitType.Grenade:
                    return "Grenade";
                case EHitType.RocketLauncher:
                    return "RocketLauncher";
                default:
                    return "Unknown";
            }
        }

        private void OnDestroy()
        {
            if (_gameplayMode != null)
            {
                _gameplayMode.OnAgentDeath -= OnAgentDeath;
                _gameplayMode.OnPlayerEliminated -= OnPlayerEliminated;
            }
        }
    }
}
