using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR.UI
{
    public class UIPartyViewSimple : UICloseView
    {
        private PartyLobbyManager _partyManager;
        
        private GameObject _mainPanel;
        private Transform _friendsContainer;
        private Transform _partyContainer;
        
        private UIButton _addFriendButton;
        private UIButton _createPartyButton;
        private UIButton _leavePartyButton;
        private UIButton _closeButtonRef;
        
        private GameObject _addFriendDialog;
        private TMP_InputField _friendUserIDInput;
        private TMP_InputField _friendNicknameInput;
        
        private List<GameObject> _friendWidgets = new List<GameObject>();
        private List<UIPartyMemberWidget> _partyWidgets = new List<UIPartyMemberWidget>();

        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            BuildUI();
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            
            _partyManager = PartyLobbyManager.Instance;
            if (_partyManager == null)
            {
                var managerObj = new GameObject("PartyLobbyManager");
                _partyManager = managerObj.AddComponent<PartyLobbyManager>();
            }
            
            if (_partyManager != null)
            {
                _partyManager.OnPartyUpdated += OnPartyUpdated;
                _partyManager.OnFriendStatusChanged += OnFriendStatusChanged;
                _partyManager.Initialize(Context.PlayerData.UserID);
                RefreshUI();
            }
        }

        protected override void OnClose()
        {
            if (_partyManager != null)
            {
                _partyManager.OnPartyUpdated -= OnPartyUpdated;
                _partyManager.OnFriendStatusChanged -= OnFriendStatusChanged;
            }
            
            base.OnClose();
        }

        private void BuildUI()
        {
            CreateBackground();
            CreateMainPanel();
        }

        private void CreateBackground()
        {
            var bg = new GameObject("Background");
            bg.layer = LayerMask.NameToLayer("UI");
            bg.transform.SetParent(transform, false);
            
            var rect = bg.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var img = bg.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.8f);
        }

        private void CreateMainPanel()
        {
            _mainPanel = new GameObject("MainPanel");
            _mainPanel.layer = LayerMask.NameToLayer("UI");
            _mainPanel.transform.SetParent(transform, false);
            
            var rect = _mainPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(800, 600);
            rect.anchoredPosition = Vector2.zero;
            
            var img = _mainPanel.AddComponent<Image>();
            img.color = new Color(0.15f, 0.15f, 0.18f, 1f);
            
            CreateHeader();
            CreateContent();
        }

        private void CreateHeader()
        {
            var header = new GameObject("Header");
            header.layer = LayerMask.NameToLayer("UI");
            header.transform.SetParent(_mainPanel.transform, false);
            
            var rect = header.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(0, 60);
            rect.anchoredPosition = new Vector2(0, 0);
            rect.pivot = new Vector2(0.5f, 1f);
            
            var img = header.AddComponent<Image>();
            img.color = new Color(0.2f, 0.25f, 0.35f, 1f);
            
            var title = new GameObject("Title");
            title.layer = LayerMask.NameToLayer("UI");
            title.transform.SetParent(header.transform, false);
            
            var titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(20, 0);
            titleRect.offsetMax = new Vector2(-80, 0);
            
            var titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "PARTY & FRIENDS";
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.color = Color.white;
            
            _closeButtonRef = CreateButton(header.transform, "CloseButton", "✕", 
                new Vector2(1, 0.5f), new Vector2(60, 50), new Vector2(-30, 0));
            _closeButtonRef.onClick.AddListener(() => Close());
            
            var closeBg = _closeButtonRef.GetComponent<Image>();
            if (closeBg != null)
                closeBg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        }

        private void CreateContent()
        {
            var content = new GameObject("Content");
            content.layer = LayerMask.NameToLayer("UI");
            content.transform.SetParent(_mainPanel.transform, false);
            
            var rect = content.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = new Vector2(20, 20);
            rect.offsetMax = new Vector2(-20, -80);
            
            CreateFriendsPanel(content.transform);
            CreatePartyPanel(content.transform);
        }

        private void CreateFriendsPanel(Transform parent)
        {
            var panel = new GameObject("FriendsPanel");
            panel.layer = LayerMask.NameToLayer("UI");
            panel.transform.SetParent(parent, false);
            
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0.48f, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.12f, 0.5f);
            
            var title = new GameObject("Title");
            title.layer = LayerMask.NameToLayer("UI");
            title.transform.SetParent(panel.transform, false);
            
            var titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = new Vector2(0, 40);
            titleRect.anchoredPosition = new Vector2(0, 0);
            titleRect.pivot = new Vector2(0.5f, 1f);
            
            var titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "FRIENDS";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            _addFriendButton = CreateButton(panel.transform, "AddFriendButton", "Add Friend", 
                new Vector2(0.5f, 0f), new Vector2(200, 40), new Vector2(0, 10));
            _addFriendButton.onClick.AddListener(OnAddFriendClicked);
            
            var scroll = CreateScrollView(panel.transform, "FriendsScroll", 
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(10, 60), new Vector2(-10, -60));
            _friendsContainer = scroll.transform.Find("Viewport/Content");
        }

        private void CreatePartyPanel(Transform parent)
        {
            var panel = new GameObject("PartyPanel");
            panel.layer = LayerMask.NameToLayer("UI");
            panel.transform.SetParent(parent, false);
            
            var rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.52f, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.12f, 0.5f);
            
            var title = new GameObject("Title");
            title.layer = LayerMask.NameToLayer("UI");
            title.transform.SetParent(panel.transform, false);
            
            var titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = new Vector2(0, 40);
            titleRect.anchoredPosition = new Vector2(0, 0);
            titleRect.pivot = new Vector2(0.5f, 1f);
            
            var titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "PARTY";
            titleText.fontSize = 18;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            
            var buttonsPanel = new GameObject("Buttons");
            buttonsPanel.layer = LayerMask.NameToLayer("UI");
            buttonsPanel.transform.SetParent(panel.transform, false);
            
            var buttonsPanelRect = buttonsPanel.AddComponent<RectTransform>();
            buttonsPanelRect.anchorMin = new Vector2(0, 0);
            buttonsPanelRect.anchorMax = new Vector2(1, 0);
            buttonsPanelRect.sizeDelta = new Vector2(0, 40);
            buttonsPanelRect.anchoredPosition = new Vector2(0, 10);
            buttonsPanelRect.pivot = new Vector2(0.5f, 0f);
            
            buttonsPanel.AddComponent<HorizontalLayoutGroup>().spacing = 10;
            
            _createPartyButton = CreateButton(buttonsPanel.transform, "CreateParty", "Create Party", Vector2.zero, new Vector2(150, 40), Vector2.zero);
            _createPartyButton.onClick.AddListener(OnCreatePartyClicked);
            
            _leavePartyButton = CreateButton(buttonsPanel.transform, "LeaveParty", "Leave Party", Vector2.zero, new Vector2(150, 40), Vector2.zero);
            _leavePartyButton.onClick.AddListener(OnLeavePartyClicked);
            
            var scroll = CreateScrollView(panel.transform, "PartyScroll", 
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(10, 60), new Vector2(-10, -60));
            _partyContainer = scroll.transform.Find("Viewport/Content");
        }

        private UIButton CreateButton(Transform parent, string name, string label, Vector2 anchor, Vector2 size, Vector2 position)
        {
            var btnObj = new GameObject(name);
            btnObj.layer = LayerMask.NameToLayer("UI");
            btnObj.transform.SetParent(parent, false);
            
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            rect.pivot = new Vector2(0.5f, 0.5f);
            
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.3f, 0.6f, 1f, 1f);
            
            var btn = btnObj.AddComponent<UIButton>();
            
            var playClickSoundField = typeof(UIButton).GetField("_playClickSound", BindingFlags.NonPublic | BindingFlags.Instance);
            if (playClickSoundField != null)
            {
                playClickSoundField.SetValue(btn, false);
            }
            
            var labelObj = new GameObject("Label");
            labelObj.layer = LayerMask.NameToLayer("UI");
            labelObj.transform.SetParent(btnObj.transform, false);
            
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            var labelText = labelObj.AddComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 16;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
            
            return btn;
        }

        private GameObject CreateScrollView(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var scroll = new GameObject(name);
            scroll.layer = LayerMask.NameToLayer("UI");
            scroll.transform.SetParent(parent, false);
            
            var scrollRect = scroll.AddComponent<RectTransform>();
            scrollRect.anchorMin = anchorMin;
            scrollRect.anchorMax = anchorMax;
            scrollRect.offsetMin = offsetMin;
            scrollRect.offsetMax = offsetMax;
            
            var viewport = new GameObject("Viewport");
            viewport.layer = LayerMask.NameToLayer("UI");
            viewport.transform.SetParent(scroll.transform, false);
            
            var viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            
            viewport.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.08f, 0.5f);
            viewport.AddComponent<Mask>().showMaskGraphic = true;
            
            var content = new GameObject("Content");
            content.layer = LayerMask.NameToLayer("UI");
            content.transform.SetParent(viewport.transform, false);
            
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0, 0);
            
            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 5;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.padding = new RectOffset(5, 5, 5, 5);
            
            var scrollComponent = scroll.AddComponent<ScrollRect>();
            scrollComponent.content = contentRect;
            scrollComponent.viewport = viewportRect;
            scrollComponent.horizontal = false;
            scrollComponent.vertical = true;
            scrollComponent.movementType = ScrollRect.MovementType.Clamped;
            
            return scroll;
        }

        private void OnAddFriendClicked()
        {
            ShowAddFriendDialog();
        }

        private void OnCreatePartyClicked()
        {
            if (_partyManager != null)
            {
                _partyManager.CreateParty();
            }
        }

        private void OnLeavePartyClicked()
        {
            if (_partyManager != null)
            {
                _partyManager.LeaveParty();
            }
        }

        private void OnPartyUpdated(TeamData party)
        {
            RefreshUI();
        }

        private void OnFriendStatusChanged(FriendData friend)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            foreach (var widget in _friendWidgets)
            {
                if (widget != null)
                    Destroy(widget.gameObject);
            }
            _friendWidgets.Clear();

            foreach (var widget in _partyWidgets)
            {
                if (widget != null)
                    Destroy(widget.gameObject);
            }
            _partyWidgets.Clear();

            if (_partyManager == null) return;

            var friends = _partyManager.GetFriends();
            if (friends != null && friends.Count > 0)
            {
                foreach (var friend in friends)
                {
                    CreateFriendWidget(friend);
                }
            }
            else
            {
                CreateNoFriendsMessage();
            }

            var party = _partyManager.GetCurrentParty();
            if (party != null && party.MemberUserIDs.Count > 0)
            {
                foreach (var memberID in party.MemberUserIDs)
                {
                    CreatePartyMemberWidget(memberID);
                }
                
                _createPartyButton.gameObject.SetActive(false);
                _leavePartyButton.gameObject.SetActive(true);
            }
            else
            {
                CreateNoPartyMessage();
                _createPartyButton.gameObject.SetActive(true);
                _leavePartyButton.gameObject.SetActive(false);
            }
        }

        private void CreateFriendWidget(FriendData friend)
        {
            var widget = new GameObject($"Friend_{friend.UserID}");
            widget.layer = LayerMask.NameToLayer("UI");
            widget.transform.SetParent(_friendsContainer, false);
            
            var rect = widget.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 50);
            
            var bg = widget.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.22f, 1f);
            
            var text = new GameObject("Text");
            text.layer = LayerMask.NameToLayer("UI");
            text.transform.SetParent(widget.transform, false);
            
            var textRect = text.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-150, 0);
            
            var tmp = text.AddComponent<TextMeshProUGUI>();
            tmp.text = $"{friend.Nickname} {(friend.IsOnline ? "(Online)" : "(Offline)")}";
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = friend.IsOnline ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f);
            
            var party = _partyManager?.GetCurrentParty();
            bool hasParty = party != null && party.MemberUserIDs.Count > 0;
            
            if (hasParty && friend.IsOnline)
            {
                var inviteBtn = CreateButton(widget.transform, "InviteBtn", "Invite", new Vector2(1, 0.5f), new Vector2(70, 35), new Vector2(-75, 0));
                var inviteBtnImg = inviteBtn.GetComponent<Image>();
                if (inviteBtnImg != null)
                {
                    inviteBtnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f);
                }
                inviteBtn.onClick.AddListener(() => OnInviteFriendClicked(friend.UserID));
            }
            
            var removeBtn = CreateButton(widget.transform, "RemoveBtn", "X", new Vector2(1, 0.5f), new Vector2(60, 35), new Vector2(-35, 0));
            var removeBtnImg = removeBtn.GetComponent<Image>();
            if (removeBtnImg != null)
            {
                removeBtnImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            }
            removeBtn.onClick.AddListener(() => OnRemoveFriendClicked(friend.UserID));
            
            _friendWidgets.Add(widget);
        }

        private void CreatePartyMemberWidget(string memberID)
        {
            var widget = new GameObject($"Member_{memberID}");
            widget.layer = LayerMask.NameToLayer("UI");
            widget.transform.SetParent(_partyContainer, false);
            
            var rect = widget.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 50);
            
            var bg = widget.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.25f, 0.3f, 1f);
            
            var text = new GameObject("Text");
            text.layer = LayerMask.NameToLayer("UI");
            text.transform.SetParent(widget.transform, false);
            
            var textRect = text.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);
            
            var tmp = text.AddComponent<TextMeshProUGUI>();
            tmp.text = $"Player {memberID}";
            tmp.fontSize = 14;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = Color.white;
            
            var widgetComponent = widget.AddComponent<UIPartyMemberWidget>();
            _partyWidgets.Add(widgetComponent);
        }

        private void CreateNoFriendsMessage()
        {
            var msg = new GameObject("NoFriends");
            msg.layer = LayerMask.NameToLayer("UI");
            msg.transform.SetParent(_friendsContainer, false);
            
            var rect = msg.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 100);
            
            var text = msg.AddComponent<TextMeshProUGUI>();
            text.text = "No friends added yet.\nClick 'Add Friend' to get started!";
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            
            _friendWidgets.Add(msg);
        }

        private void CreateNoPartyMessage()
        {
            var msg = new GameObject("NoParty");
            msg.layer = LayerMask.NameToLayer("UI");
            msg.transform.SetParent(_partyContainer, false);
            
            var rect = msg.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 100);
            
            var text = msg.AddComponent<TextMeshProUGUI>();
            text.text = "You are not in a party.\nCreate one to get started!";
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            
            var widgetComponent = msg.AddComponent<UIPartyMemberWidget>();
            _partyWidgets.Add(widgetComponent);
        }

        private void ShowAddFriendDialog()
        {
            if (_addFriendDialog != null)
            {
                _addFriendDialog.SetActive(true);
                return;
            }

            _addFriendDialog = new GameObject("AddFriendDialog");
            _addFriendDialog.layer = LayerMask.NameToLayer("UI");
            _addFriendDialog.transform.SetParent(transform, false);

            var dialogRect = _addFriendDialog.AddComponent<RectTransform>();
            dialogRect.anchorMin = Vector2.zero;
            dialogRect.anchorMax = Vector2.one;
            dialogRect.offsetMin = Vector2.zero;
            dialogRect.offsetMax = Vector2.zero;

            var dialogBg = _addFriendDialog.AddComponent<Image>();
            dialogBg.color = new Color(0, 0, 0, 0.8f);

            var panel = new GameObject("Panel");
            panel.layer = LayerMask.NameToLayer("UI");
            panel.transform.SetParent(_addFriendDialog.transform, false);

            var panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(400, 300);
            panelRect.anchoredPosition = Vector2.zero;

            var panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

            var title = new GameObject("Title");
            title.layer = LayerMask.NameToLayer("UI");
            title.transform.SetParent(panel.transform, false);

            var titleRect = title.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = new Vector2(0, 50);
            titleRect.anchoredPosition = new Vector2(0, -25);

            var titleText = title.AddComponent<TextMeshProUGUI>();
            titleText.text = "Add Friend";
            titleText.fontSize = 20;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;

            var userIDLabel = CreateLabel(panel.transform, "UserIDLabel", "Friend User ID:", new Vector2(0, 1), new Vector2(0, 30), new Vector2(0, -80));
            _friendUserIDInput = CreateInputField(panel.transform, "UserIDInput", "Enter User ID...", new Vector2(0, 1), new Vector2(-20, 40), new Vector2(0, -110));

            var nicknameLabel = CreateLabel(panel.transform, "NicknameLabel", "Friend Nickname:", new Vector2(0, 1), new Vector2(0, 30), new Vector2(0, -160));
            _friendNicknameInput = CreateInputField(panel.transform, "NicknameInput", "Enter Nickname...", new Vector2(0, 1), new Vector2(-20, 40), new Vector2(0, -190));

            var addBtn = CreateButton(panel.transform, "AddButton", "Add", new Vector2(0.5f, 0), new Vector2(150, 40), new Vector2(-80, 30));
            addBtn.onClick.AddListener(OnConfirmAddFriend);

            var cancelBtn = CreateButton(panel.transform, "CancelButton", "Cancel", new Vector2(0.5f, 0), new Vector2(150, 40), new Vector2(80, 30));
            cancelBtn.onClick.AddListener(OnCancelAddFriend);

            _addFriendDialog.SetActive(true);
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string name, string text, Vector2 anchor, Vector2 size, Vector2 position)
        {
            var labelObj = new GameObject(name);
            labelObj.layer = LayerMask.NameToLayer("UI");
            labelObj.transform.SetParent(parent, false);

            var rect = labelObj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = new Vector2(1, anchor.y);
            rect.sizeDelta = new Vector2(0, size.y);
            rect.anchoredPosition = position;

            var label = labelObj.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 16;
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.color = Color.white;
            label.margin = new Vector4(20, 0, 0, 0);

            return label;
        }

        private TMP_InputField CreateInputField(Transform parent, string name, string placeholder, Vector2 anchor, Vector2 size, Vector2 position)
        {
            var inputObj = new GameObject(name);
            inputObj.layer = LayerMask.NameToLayer("UI");
            inputObj.transform.SetParent(parent, false);

            var rect = inputObj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = new Vector2(1, anchor.y);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var inputBg = inputObj.AddComponent<Image>();
            inputBg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            var inputField = inputObj.AddComponent<TMP_InputField>();

            var textArea = new GameObject("TextArea");
            textArea.layer = LayerMask.NameToLayer("UI");
            textArea.transform.SetParent(inputObj.transform, false);

            var textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 2);
            textAreaRect.offsetMax = new Vector2(-10, -2);

            var text = new GameObject("Text");
            text.layer = LayerMask.NameToLayer("UI");
            text.transform.SetParent(textArea.transform, false);

            var textRect = text.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var textComponent = text.AddComponent<TextMeshProUGUI>();
            textComponent.fontSize = 14;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.MidlineLeft;

            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.layer = LayerMask.NameToLayer("UI");
            placeholderObj.transform.SetParent(textArea.transform, false);

            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;

            var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = placeholder;
            placeholderText.fontSize = 14;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.alignment = TextAlignmentOptions.MidlineLeft;

            inputField.textViewport = textAreaRect;
            inputField.textComponent = textComponent;
            inputField.placeholder = placeholderText;

            return inputField;
        }

        private void OnConfirmAddFriend()
        {
            if (_partyManager == null)
            {
                Debug.LogWarning("PartyLobbyManager is null!");
                return;
            }

            string userID = _friendUserIDInput.text.Trim();
            string nickname = _friendNicknameInput.text.Trim();

            if (string.IsNullOrEmpty(userID))
            {
                Debug.LogWarning("User ID cannot be empty!");
                return;
            }

            if (string.IsNullOrEmpty(nickname))
            {
                nickname = userID;
            }

            bool success = _partyManager.AddFriend(userID, nickname);
            
            if (success)
            {
                Debug.Log($"Friend added successfully: {nickname} ({userID})");
                _friendUserIDInput.text = "";
                _friendNicknameInput.text = "";
                _addFriendDialog.SetActive(false);
                RefreshUI();
            }
            else
            {
                Debug.LogWarning($"Failed to add friend: {userID} (may already exist)");
            }
        }

        private void OnCancelAddFriend()
        {
            _friendUserIDInput.text = "";
            _friendNicknameInput.text = "";
            _addFriendDialog.SetActive(false);
        }

        private void OnRemoveFriendClicked(string userID)
        {
            if (_partyManager == null)
            {
                Debug.LogWarning("PartyLobbyManager is null!");
                return;
            }

            bool success = _partyManager.RemoveFriend(userID);
            
            if (success)
            {
                Debug.Log($"Friend removed successfully: {userID}");
                RefreshUI();
            }
            else
            {
                Debug.LogWarning($"Failed to remove friend: {userID}");
            }
        }

        private void OnInviteFriendClicked(string userID)
        {
            if (_partyManager == null)
            {
                Debug.LogWarning("PartyLobbyManager is null!");
                return;
            }

            var friend = _partyManager.GetFriend(userID);
            if (friend != null)
            {
                Debug.Log($"Party invite sent to {friend.Nickname} ({userID})");
            }
            else
            {
                Debug.LogWarning($"Friend not found: {userID}");
            }
        }
    }
}
