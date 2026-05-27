using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace TPSBR
{
    public class ReviveSystemManager : ContextBehaviour
    {
        private static ReviveSystemManager _instance;
        public static ReviveSystemManager Instance => _instance;

        [Header("Settings")]
        [SerializeField]
        private bool _enableInSoloMode = false;

        private Dictionary<string, ReviveSystem> _reviveSystems = new Dictionary<string, ReviveSystem>();
        private bool _initialized;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public override void FixedUpdateNetwork()
        {
            if (!_initialized && Context != null && Context.NetworkGame != null)
            {
                Initialize();
            }
        }

        private void Initialize()
        {
            _initialized = true;

            if (HasStateAuthority)
            {
                RegisterExistingPlayers();
            }
        }

        private void RegisterExistingPlayers()
        {
            if (Context.NetworkGame == null)
                return;

            foreach (var player in Context.NetworkGame.ActivePlayers)
            {
                RegisterPlayer(player);
            }
        }

        public void RegisterPlayer(Player player)
        {
            if (player == null)
                return;

            if (_reviveSystems.ContainsKey(player.UserID))
                return;

            var reviveSystem = player.GetComponent<ReviveSystem>();
            if (reviveSystem == null)
            {
                reviveSystem = player.gameObject.AddComponent<ReviveSystem>();
            }

            _reviveSystems[player.UserID] = reviveSystem;

            reviveSystem.OnPlayerDowned += OnPlayerDowned;
            reviveSystem.OnReviveCompleted += OnReviveCompleted;
            reviveSystem.OnPlayerBledOut += OnPlayerBledOut;
        }

        public void UnregisterPlayer(Player player)
        {
            if (player == null)
                return;

            if (!_reviveSystems.ContainsKey(player.UserID))
                return;

            var reviveSystem = _reviveSystems[player.UserID];
            if (reviveSystem != null)
            {
                reviveSystem.OnPlayerDowned -= OnPlayerDowned;
                reviveSystem.OnReviveCompleted -= OnReviveCompleted;
                reviveSystem.OnPlayerBledOut -= OnPlayerBledOut;
            }

            _reviveSystems.Remove(player.UserID);
        }

        public bool ShouldEnableRevive(Player player)
        {
            if (TeamManager.Instance == null)
                return false;

            var teamMode = TeamManager.Instance.GetTeamMode();

            if (teamMode == TeamMode.Solo)
                return _enableInSoloMode;

            return true;
        }

        public ReviveSystem GetReviveSystem(string userID)
        {
            _reviveSystems.TryGetValue(userID, out var reviveSystem);
            return reviveSystem;
        }

        public List<Player> GetAllDownedPlayers()
        {
            var downedPlayers = new List<Player>();

            foreach (var kvp in _reviveSystems)
            {
                if (kvp.Value != null && kvp.Value.IsDown)
                {
                    var player = kvp.Value.GetComponent<Player>();
                    if (player != null)
                    {
                        downedPlayers.Add(player);
                    }
                }
            }

            return downedPlayers;
        }

        public int GetDownedTeammateCount(Player player)
        {
            int count = 0;

            foreach (var kvp in _reviveSystems)
            {
                if (kvp.Value != null && kvp.Value.IsDown)
                {
                    var downedPlayer = kvp.Value.GetComponent<Player>();
                    if (downedPlayer != null && downedPlayer != player)
                    {
                        if (player.IsTeammateWith(downedPlayer))
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        private void OnPlayerDowned(Player player)
        {
            Debug.Log($"[ReviveSystem] {player.Nickname} is down!");
        }

        private void OnReviveCompleted(Player revivedPlayer, Player reviver)
        {
            Debug.Log($"[ReviveSystem] {reviver.Nickname} revived {revivedPlayer.Nickname}");
        }

        private void OnPlayerBledOut(Player player)
        {
            Debug.Log($"[ReviveSystem] {player.Nickname} bled out");
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _reviveSystems.Clear();
            base.Despawned(runner, hasState);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
