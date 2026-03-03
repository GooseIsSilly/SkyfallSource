using UnityEngine;

namespace TPSBR
{
    public class TeamSystemInitializer : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField]
        private bool _createPartyLobbyManager = true;
        [SerializeField]
        private bool _createTeamMatchmaker = true;

        [Header("Settings")]
        [SerializeField]
        private TeamMode _defaultTeamMode = TeamMode.Solo;
        [SerializeField]
        private bool _autoInitialize = true;

        [Header("Revive System")]
        [SerializeField]
        private bool _enableReviveSystem = true;

        private void Awake()
        {
            if (_autoInitialize)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (_createPartyLobbyManager && PartyLobbyManager.Instance == null)
            {
                var partyManager = new GameObject("PartyLobbyManager");
                partyManager.AddComponent<PartyLobbyManager>();
                DontDestroyOnLoad(partyManager);
            }

            if (_createTeamMatchmaker && TeamMatchmaker.Instance == null)
            {
                var matchmaker = new GameObject("TeamMatchmaker");
                matchmaker.AddComponent<TeamMatchmaker>();
                DontDestroyOnLoad(matchmaker);
            }

            if (PartyLobbyManager.Instance != null)
            {
                if (Global.PlayerService != null && Global.PlayerService.PlayerData != null)
                {
                    PartyLobbyManager.Instance.Initialize(Global.PlayerService.PlayerData.UserID);
                }
            }

            Debug.Log($"Team System Initialized (Revive: {_enableReviveSystem})");
        }

        public void SetTeamMode(TeamMode mode)
        {
            _defaultTeamMode = mode;

            if (TeamManager.Instance != null)
            {
                TeamManager.Instance.SetTeamMode(mode);
            }
        }

        public TeamMode GetDefaultTeamMode()
        {
            return _defaultTeamMode;
        }
    }
}
