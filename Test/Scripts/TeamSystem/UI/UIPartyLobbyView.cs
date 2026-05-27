using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSBR.UI
{
    public class UIPartyLobbyView : UIView
    {
        [Header("Party Members")]
        [SerializeField]
        private UITeamMemberWidget _memberWidgetPrefab;
        [SerializeField]
        private Transform _membersContainer;

        [Header("Controls")]
        [SerializeField]
        private UIButton _readyButton;
        [SerializeField]
        private UIButton _startMatchmakingButton;
        [SerializeField]
        private UIButton _leavePartyButton;
        [SerializeField]
        private TextMeshProUGUI _readyButtonText;
        [SerializeField]
        private TextMeshProUGUI _statusText;

        [Header("Team Mode Selection")]
        [SerializeField]
        private UIButton _soloButton;
        [SerializeField]
        private UIButton _duoButton;
        [SerializeField]
        private UIButton _squadButton;
        [SerializeField]
        private Color _selectedModeColor = Color.green;
        [SerializeField]
        private Color _unselectedModeColor = Color.gray;

        [Header("Friends List")]
        [SerializeField]
        private UIFriendWidget _friendWidgetPrefab;
        [SerializeField]
        private Transform _friendsContainer;
        [SerializeField]
        private UIButton _addFriendButton;
        [SerializeField]
        private TMP_InputField _friendIDInput;

        private List<UITeamMemberWidget> _memberWidgets = new List<UITeamMemberWidget>();
        private List<UIFriendWidget> _friendWidgets = new List<UIFriendWidget>();
        private bool _isReady;
        private TeamMode _selectedMode = TeamMode.Solo;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (_readyButton != null)
            {
                _readyButton.onClick.AddListener(OnReadyButtonClicked);
            }

            if (_startMatchmakingButton != null)
            {
                _startMatchmakingButton.onClick.AddListener(OnStartMatchmakingClicked);
            }

            if (_leavePartyButton != null)
            {
                _leavePartyButton.onClick.AddListener(OnLeavePartyClicked);
            }

            if (_soloButton != null)
            {
                _soloButton.onClick.AddListener(() => OnTeamModeSelected(TeamMode.Solo));
            }

            if (_duoButton != null)
            {
                _duoButton.onClick.AddListener(() => OnTeamModeSelected(TeamMode.Duo));
            }

            if (_squadButton != null)
            {
                _squadButton.onClick.AddListener(() => OnTeamModeSelected(TeamMode.Squad));
            }

            if (_addFriendButton != null)
            {
                _addFriendButton.onClick.AddListener(OnAddFriendClicked);
            }

            if (PartyLobbyManager.Instance != null)
            {
                PartyLobbyManager.Instance.OnPartyUpdated += OnPartyUpdated;
                PartyLobbyManager.Instance.OnFriendStatusChanged += OnFriendStatusChanged;
                PartyLobbyManager.Instance.OnAllPlayersReady += OnAllPlayersReadyChanged;
            }

            UpdateTeamModeButtons();
        }

        protected override void OnDeinitialize()
        {
            if (PartyLobbyManager.Instance != null)
            {
                PartyLobbyManager.Instance.OnPartyUpdated -= OnPartyUpdated;
                PartyLobbyManager.Instance.OnFriendStatusChanged -= OnFriendStatusChanged;
                PartyLobbyManager.Instance.OnAllPlayersReady -= OnAllPlayersReadyChanged;
            }

            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            UpdateDisplay();
            UpdateFriendsList();
        }

        protected override void OnTick()
        {
            base.OnTick();
            UpdateStatusText();
        }

        private void OnReadyButtonClicked()
        {
            _isReady = !_isReady;

            if (PartyLobbyManager.Instance != null)
            {
                PartyLobbyManager.Instance.SetReady(_isReady);
            }

            UpdateReadyButton();
        }

        private void OnStartMatchmakingClicked()
        {
            if (PartyLobbyManager.Instance != null && PartyLobbyManager.Instance.IsPartyLeader())
            {
                var party = PartyLobbyManager.Instance.GetCurrentParty();
                if (party != null && TeamMatchmaker.Instance != null)
                {
                    TeamMatchmaker.Instance.StartMatchmaking(party, _selectedMode);
                }
            }
        }

        private void OnLeavePartyClicked()
        {
            if (PartyLobbyManager.Instance != null)
            {
                PartyLobbyManager.Instance.LeaveParty();
            }

            _isReady = false;
            UpdateDisplay();
        }

        private void OnTeamModeSelected(TeamMode mode)
        {
            _selectedMode = mode;

            if (TeamManager.Instance != null)
            {
                TeamManager.Instance.SetTeamMode(mode);
            }

            UpdateTeamModeButtons();
        }

        private void OnAddFriendClicked()
        {
            if (_friendIDInput != null && !string.IsNullOrEmpty(_friendIDInput.text))
            {
                string friendID = _friendIDInput.text.Trim();

                if (PartyLobbyManager.Instance != null)
                {
                    if (PartyLobbyManager.Instance.AddFriend(friendID, friendID))
                    {
                        _friendIDInput.text = "";
                        UpdateFriendsList();
                    }
                }
            }
        }

        private void OnPartyUpdated(TeamData party)
        {
            UpdateDisplay();
        }

        private void OnFriendStatusChanged(FriendData friend)
        {
            UpdateFriendsList();
        }

        private void OnAllPlayersReadyChanged(bool allReady)
        {
            if (_startMatchmakingButton != null)
            {
                _startMatchmakingButton.interactable = allReady && PartyLobbyManager.Instance != null && PartyLobbyManager.Instance.IsPartyLeader();
            }
        }

        private void UpdateDisplay()
        {
            ClearMemberWidgets();

            if (PartyLobbyManager.Instance != null)
            {
                var party = PartyLobbyManager.Instance.GetCurrentParty();
                if (party != null)
                {
                    for (int i = 0; i < party.MemberUserIDs.Count; i++)
                    {
                        string userID = party.MemberUserIDs[i];
                        bool isLeader = party.IsPartyLeader(userID);
                        bool isReady = PartyLobbyManager.Instance.IsReady(userID);

                        var widget = Instantiate(_memberWidgetPrefab, _membersContainer);
                        widget.SetData(userID, userID, true, isReady, isLeader);
                        _memberWidgets.Add(widget);
                    }
                }

                bool isPartyLeader = PartyLobbyManager.Instance.IsPartyLeader();
                if (_startMatchmakingButton != null)
                {
                    _startMatchmakingButton.SetActive(isPartyLeader);
                }

                if (_soloButton != null && _duoButton != null && _squadButton != null)
                {
                    _soloButton.interactable = isPartyLeader;
                    _duoButton.interactable = isPartyLeader;
                    _squadButton.interactable = isPartyLeader;
                }
            }

            UpdateReadyButton();
        }

        private void UpdateReadyButton()
        {
            if (_readyButtonText != null)
            {
                _readyButtonText.text = _isReady ? "NOT READY" : "READY";
            }

            if (_readyButton != null)
            {
                var colors = _readyButton.GetComponent<Image>().color;
                _readyButton.GetComponent<Image>().color = _isReady ? Color.green : Color.gray;
            }
        }

        private void UpdateTeamModeButtons()
        {
            if (_soloButton != null)
            {
                _soloButton.GetComponent<Image>().color = _selectedMode == TeamMode.Solo ? _selectedModeColor : _unselectedModeColor;
            }

            if (_duoButton != null)
            {
                _duoButton.GetComponent<Image>().color = _selectedMode == TeamMode.Duo ? _selectedModeColor : _unselectedModeColor;
            }

            if (_squadButton != null)
            {
                _squadButton.GetComponent<Image>().color = _selectedMode == TeamMode.Squad ? _selectedModeColor : _unselectedModeColor;
            }
        }

        private void UpdateFriendsList()
        {
            ClearFriendWidgets();

            if (PartyLobbyManager.Instance != null)
            {
                var friends = PartyLobbyManager.Instance.GetFriends();
                foreach (var friend in friends)
                {
                    var widget = Instantiate(_friendWidgetPrefab, _friendsContainer);
                    widget.SetData(friend, OnInviteFriendClicked);
                    _friendWidgets.Add(widget);
                }
            }
        }

        private void OnInviteFriendClicked(string friendUserID)
        {
            if (PartyLobbyManager.Instance != null)
            {
                PartyLobbyManager.Instance.InviteFriend(friendUserID);
            }
        }

        private void UpdateStatusText()
        {
            if (_statusText == null)
                return;

            if (TeamMatchmaker.Instance != null && TeamMatchmaker.Instance.IsMatchmaking())
            {
                float progress = TeamMatchmaker.Instance.GetMatchmakingProgress();
                _statusText.text = $"Searching for match... {(int)(progress * 100)}%";
            }
            else if (PartyLobbyManager.Instance != null)
            {
                var party = PartyLobbyManager.Instance.GetCurrentParty();
                if (party != null)
                {
                    _statusText.text = $"Party: {party.MemberUserIDs.Count}/{(int)_selectedMode} players";
                }
                else
                {
                    _statusText.text = "No active party";
                }
            }
        }

        private void ClearMemberWidgets()
        {
            foreach (var widget in _memberWidgets)
            {
                if (widget != null)
                {
                    Destroy(widget.gameObject);
                }
            }
            _memberWidgets.Clear();
        }

        private void ClearFriendWidgets()
        {
            foreach (var widget in _friendWidgets)
            {
                if (widget != null)
                {
                    Destroy(widget.gameObject);
                }
            }
            _friendWidgets.Clear();
        }
    }
}
