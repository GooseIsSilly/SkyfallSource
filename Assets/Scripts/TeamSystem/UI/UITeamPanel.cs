using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR.UI
{
    public class UITeamPanel : UIWidget
    {
        [Header("Container")]
        [SerializeField]
        private Transform _membersContainer;
        
        [Header("Settings")]
        [SerializeField]
        private int _maxMembers = 4;
        [SerializeField]
        private bool _autoGenerateWidgets = true;
        
        [Header("Widget Styling")]
        [SerializeField]
        private float _widgetHeight = 60f;
        [SerializeField]
        private float _widgetSpacing = 10f;
        [SerializeField]
        private Color _aliveColor = Color.green;
        [SerializeField]
        private Color _downedColor = Color.red;
        [SerializeField]
        private Color _deadColor = Color.gray;

        private List<UITeamMemberWidget> _memberWidgets = new List<UITeamMemberWidget>();
        private TeamData _currentTeam;

        protected void Awake()
        {
            if (_autoGenerateWidgets)
            {
                SetupContainer();
                GenerateWidgets();
            }
        }
        
        private void SetupContainer()
        {
            if (_membersContainer == null)
            {
                var containerObj = new GameObject("MembersContainer");
                containerObj.transform.SetParent(transform, false);
                _membersContainer = containerObj.transform;
                
                var rect = containerObj.AddComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                var layout = containerObj.AddComponent<VerticalLayoutGroup>();
                layout.spacing = _widgetSpacing;
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childControlWidth = true;
                layout.childControlHeight = false;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                
                var fitter = containerObj.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }
        
        private void GenerateWidgets()
        {
            for (int i = 0; i < _maxMembers; i++)
            {
                var widget = CreateTeamMemberWidget(i);
                widget.gameObject.SetActive(false);
                _memberWidgets.Add(widget);
            }
        }
        
        private UITeamMemberWidget CreateTeamMemberWidget(int index)
        {
            var widgetObj = new GameObject($"TeamMember_{index}");
            widgetObj.transform.SetParent(_membersContainer, false);
            
            var rect = widgetObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, _widgetHeight);
            
            var widget = widgetObj.AddComponent<UITeamMemberWidget>();
            widget.AutoGenerate(_widgetHeight, _aliveColor, _downedColor, _deadColor);
            
            return widget;
        }

        protected void OnEnable()
        {
            if (TeamManager.Instance != null)
            {
                TeamManager.Instance.OnTeamUpdated += OnTeamUpdated;
                TeamManager.Instance.OnPlayerReadyChanged += OnPlayerReadyChanged;
            }
        }

        protected void OnDisable()
        {
            if (TeamManager.Instance != null)
            {
                TeamManager.Instance.OnTeamUpdated -= OnTeamUpdated;
                TeamManager.Instance.OnPlayerReadyChanged -= OnPlayerReadyChanged;
            }
        }

        private void Update()
        {
            if (_currentTeam == null)
                return;
            
            if (Context == null)
                return;
            
            if (Context.NetworkGame == null)
                return;
            
            UpdateTeamDisplay();
        }

        private void OnTeamUpdated(TeamData team)
        {
            _currentTeam = team;
            UpdateTeamDisplay();
        }

        private void OnPlayerReadyChanged(string userID)
        {
            UpdateTeamDisplay();
        }

        private void UpdateTeamDisplay()
        {
            if (_currentTeam == null || _memberWidgets == null || _memberWidgets.Count == 0)
            {
                HideAllWidgets();
                return;
            }

            if (_currentTeam.MemberUserIDs == null)
            {
                HideAllWidgets();
                return;
            }

            for (int i = 0; i < _memberWidgets.Count; i++)
            {
                if (i < _currentTeam.MemberUserIDs.Count)
                {
                    string userID = _currentTeam.MemberUserIDs[i];
                    bool isLeader = _currentTeam.IsPartyLeader(userID);
                    bool isReady = TeamManager.Instance != null && TeamManager.Instance.IsPlayerReady(userID);

                    var player = GetPlayerByUserID(userID);
                    string nickname = player != null ? player.Nickname : "Unknown";
                    bool isAlive = player != null && player.Statistics.IsAlive;
                    bool isDowned = player != null && player.IsDown();

                    _memberWidgets[i].gameObject.SetActive(true);
                    _memberWidgets[i].SetData(userID, nickname, isAlive, isReady, isLeader);
                    _memberWidgets[i].SetDownedStatus(isDowned);
                }
                else
                {
                    _memberWidgets[i].gameObject.SetActive(false);
                }
            }
        }

        private void HideAllWidgets()
        {
            if (_memberWidgets == null)
                return;

            foreach (var widget in _memberWidgets)
            {
                if (widget != null)
                {
                    widget.gameObject.SetActive(false);
                }
            }
        }

        private Player GetPlayerByUserID(string userID)
        {
            if (Context == null || Context.NetworkGame == null || Context.NetworkGame.ActivePlayers == null)
                return null;

            foreach (var player in Context.NetworkGame.ActivePlayers)
            {
                if (player != null && player.UserID == userID)
                {
                    return player;
                }
            }

            return null;
        }
    }
}
