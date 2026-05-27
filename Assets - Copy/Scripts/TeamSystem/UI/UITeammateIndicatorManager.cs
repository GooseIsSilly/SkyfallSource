using System.Collections.Generic;
using UnityEngine;

namespace TPSBR.UI
{
    public class UITeammateIndicatorManager : UIWidget
    {
        [SerializeField]
        private UITeammateIndicator _indicatorPrefab;
        [SerializeField]
        private Transform _indicatorsContainer;
        [SerializeField]
        private bool _showIndicators = true;

        private List<UITeammateIndicator> _activeIndicators = new List<UITeammateIndicator>();
        private Player _localPlayer;

        protected void OnEnable()
        {
            if (TeamManager.Instance != null)
            {
                TeamManager.Instance.OnTeamUpdated += OnTeamUpdated;
            }
        }

        protected void OnDisable()
        {
            if (TeamManager.Instance != null)
            {
                TeamManager.Instance.OnTeamUpdated -= OnTeamUpdated;
            }

            ClearIndicators();
        }

        private void Update()
        {
            if (!_showIndicators)
            {
                ClearIndicators();
                return;
            }

            UpdateLocalPlayer();
            UpdateTeammateIndicators();
        }

        private void OnTeamUpdated(TeamData team)
        {
            RefreshIndicators();
        }

        private void UpdateLocalPlayer()
        {
            if (_localPlayer != null)
                return;

            if (Context == null || Context.NetworkGame == null)
                return;

            _localPlayer = Context.NetworkGame.GetPlayer(Context.LocalPlayerRef);
        }

        private void UpdateTeammateIndicators()
        {
            if (_localPlayer == null || TeamManager.Instance == null)
                return;

            var team = _localPlayer.GetTeam();
            if (team == null)
            {
                ClearIndicators();
                return;
            }

            var teammates = GetTeammates(team);
            
            while (_activeIndicators.Count < teammates.Count)
            {
                var indicator = Instantiate(_indicatorPrefab, _indicatorsContainer);
                _activeIndicators.Add(indicator);
            }

            while (_activeIndicators.Count > teammates.Count)
            {
                var indicator = _activeIndicators[_activeIndicators.Count - 1];
                _activeIndicators.RemoveAt(_activeIndicators.Count - 1);
                Destroy(indicator.gameObject);
            }

            for (int i = 0; i < teammates.Count; i++)
            {
                _activeIndicators[i].SetTarget(teammates[i]);
            }
        }

        private List<Player> GetTeammates(TeamData team)
        {
            var teammates = new List<Player>();

            if (Context == null || Context.NetworkGame == null)
                return teammates;

            foreach (var userID in team.MemberUserIDs)
            {
                if (userID == _localPlayer.UserID)
                    continue;

                foreach (var player in Context.NetworkGame.ActivePlayers)
                {
                    if (player != null && player.UserID == userID)
                    {
                        teammates.Add(player);
                        break;
                    }
                }
            }

            return teammates;
        }

        private void RefreshIndicators()
        {
            ClearIndicators();
        }

        private void ClearIndicators()
        {
            foreach (var indicator in _activeIndicators)
            {
                if (indicator != null)
                {
                    Destroy(indicator.gameObject);
                }
            }
            _activeIndicators.Clear();
        }

        public void SetIndicatorsVisible(bool visible)
        {
            _showIndicators = visible;
            if (!visible)
            {
                ClearIndicators();
            }
        }
    }
}
