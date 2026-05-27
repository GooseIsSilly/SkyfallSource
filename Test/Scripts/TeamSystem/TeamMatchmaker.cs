using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TPSBR
{
    public class TeamMatchmaker : MonoBehaviour
    {
        public static TeamMatchmaker Instance { get; private set; }

        private const int MIN_PLAYERS_PER_MATCH = 2;
        private const float MATCHMAKING_TIMEOUT = 60.0f;

        private Dictionary<byte, TeamData> _queuedTeams = new Dictionary<byte, TeamData>();
        private float _matchmakingTimer;
        private TeamMode _currentMatchmakingMode = TeamMode.Solo;
        private bool _isMatchmaking;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (_isMatchmaking)
            {
                _matchmakingTimer += Time.deltaTime;

                if (_matchmakingTimer >= MATCHMAKING_TIMEOUT)
                {
                    Debug.LogWarning("Matchmaking timeout reached");
                    StopMatchmaking();
                }
                else
                {
                    TryCreateMatch();
                }
            }
        }

        public void StartMatchmaking(TeamData team, TeamMode mode)
        {
            if (team == null)
            {
                Debug.LogError("Cannot start matchmaking with null team");
                return;
            }

            _currentMatchmakingMode = mode;
            _queuedTeams[team.TeamID] = team;
            _matchmakingTimer = 0f;
            _isMatchmaking = true;

            Debug.Log($"Started matchmaking for team {team.TeamID} in {mode} mode");
        }

        public void StopMatchmaking()
        {
            _queuedTeams.Clear();
            _isMatchmaking = false;
            _matchmakingTimer = 0f;

            Debug.Log("Stopped matchmaking");
        }

        public bool IsMatchmaking()
        {
            return _isMatchmaking;
        }

        public float GetMatchmakingProgress()
        {
            return Mathf.Clamp01(_matchmakingTimer / MATCHMAKING_TIMEOUT);
        }

        private void TryCreateMatch()
        {
            int requiredTeams = GetRequiredTeamsForMode(_currentMatchmakingMode);
            int totalPlayers = GetTotalQueuedPlayers();

            if (_queuedTeams.Count >= requiredTeams || totalPlayers >= MIN_PLAYERS_PER_MATCH)
            {
                CreateMatch();
            }
        }

        private void CreateMatch()
        {
            var teams = _queuedTeams.Values.ToList();
            Debug.Log($"Creating match with {teams.Count} teams and {GetTotalQueuedPlayers()} total players");

            StopMatchmaking();
        }

        private int GetTotalQueuedPlayers()
        {
            int total = 0;
            foreach (var team in _queuedTeams.Values)
            {
                total += team.MemberUserIDs.Count;
            }
            return total;
        }

        private int GetRequiredTeamsForMode(TeamMode mode)
        {
            switch (mode)
            {
                case TeamMode.Solo:
                    return 20;
                case TeamMode.Duo:
                    return 10;
                case TeamMode.Squad:
                    return 5;
                default:
                    return 10;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
