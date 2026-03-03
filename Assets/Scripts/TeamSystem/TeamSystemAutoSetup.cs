using Fusion;
using UnityEngine;

namespace TPSBR
{
    public class TeamSystemAutoSetup : NetworkBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField]
        private bool _autoSetupPlayers = true;

        [Header("Team Mode")]
        [SerializeField]
        private TeamMode _teamMode = TeamMode.Duo;

        [Header("Revive System")]
        [SerializeField]
        private bool _enableReviveSystem = true;

        [Header("Teammate Visualization")]
        [SerializeField]
        private bool _enableTeammateVisuals = true;

        private bool _initialized = false;
        private NetworkGame _networkGame;

        public override void Spawned()
        {
            if (!HasStateAuthority)
                return;

            _networkGame = FindFirstObjectByType<NetworkGame>();

            if (_autoSetupPlayers)
            {
                SetupTeamMode();
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority || !_autoSetupPlayers)
                return;

            if (_networkGame == null)
            {
                _networkGame = FindFirstObjectByType<NetworkGame>();
            }

            if (!_initialized && _networkGame != null)
            {
                _initialized = true;
                SetupExistingPlayers();
            }

            SetupNewPlayers();
        }

        private void SetupTeamMode()
        {
            if (TeamManager.Instance != null)
            {
                TeamManager.Instance.SetTeamMode(_teamMode);
            }
        }

        private void SetupExistingPlayers()
        {
            if (_networkGame == null)
                return;

            foreach (var player in _networkGame.ActivePlayers)
            {
                SetupPlayer(player);
            }
        }

        private void SetupNewPlayers()
        {
            if (_networkGame == null)
                return;

            foreach (var player in _networkGame.ActivePlayers)
            {
                if (player == null)
                    continue;

                SetupPlayer(player);
            }
        }

        private void SetupPlayer(Player player)
        {
            if (player == null)
                return;

            if (_enableReviveSystem)
            {
                var reviveSystem = player.GetComponent<ReviveSystem>();
                if (reviveSystem == null)
                {
                    player.gameObject.AddComponent<ReviveSystem>();
                }
            }

            if (_enableTeammateVisuals)
            {
                var visualizer = player.GetComponent<TeammateVisualizer>();
                if (visualizer == null)
                {
                    player.gameObject.AddComponent<TeammateVisualizer>();
                }
            }
        }
    }
}
