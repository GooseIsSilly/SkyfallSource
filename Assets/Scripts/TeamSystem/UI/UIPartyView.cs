using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR.UI
{
    public class UIPartyView : UICloseView
    {
        [Header("Auto-Generation Settings")]
        [SerializeField]
        private bool _autoGenerateUI = true;
        [SerializeField]
        private float _widgetHeight = 70f;
        [SerializeField]
        private float _widgetSpacing = 8f;
        [SerializeField]
        private float _panelWidth = 450f;
        
        [Header("Character Preview Settings")]
        [SerializeField]
        private bool _enableCharacterPreviews = true;
        [SerializeField]
        private float _previewHeight = 250f;
        [SerializeField]
        private GameObject _playerPreviewPrefab;
        [SerializeField]
        private float _characterSpacing = 2.5f;
        [SerializeField]
        private Vector3 _characterRotation = new Vector3(0f, 180f, 0f);
        
        [Header("Colors")]
        [SerializeField]
        private Color _backgroundColor = new Color(0.1f, 0.1f, 0.12f, 0.95f);
        [SerializeField]
        private Color _panelColor = new Color(0.15f, 0.15f, 0.18f, 1f);
        [SerializeField]
        private Color _headerColor = new Color(0.2f, 0.25f, 0.35f, 1f);
        [SerializeField]
        private Color _accentColor = new Color(0.3f, 0.6f, 1f, 1f);
        
        private Transform _friendsContainer;
        private Transform _partyContainer;
        private GameObject _noFriendsMessage;
        private GameObject _noPartyMessage;
        private GameObject _addFriendDialog;
        
        private UIButton _addFriendButton;
        private UIButton _createPartyButton;
        private UIButton _leavePartyButton;
        private UIButton _confirmAddFriendButton;
        private UIButton _cancelAddFriendButton;
        
        private TMP_InputField _friendIDInput;
        private TMP_InputField _friendNicknameInput;

        private List<MonoBehaviour> _friendWidgets = new List<MonoBehaviour>();
        private List<UIPartyMemberWidget> _partyWidgets = new List<UIPartyMemberWidget>();
        
        private GameObject _previewPanel;
        private RawImage _previewImage;
        private Camera _previewCamera;
        private RenderTexture _renderTexture;
        private Transform _previewContainer;
        private List<GameObject> _characterPreviews = new List<GameObject>();
        
        private PartyLobbyManager _partyManager;
        private RectTransform _mainPanel;

        private void Awake()
        {
            Debug.Log("UIPartyView.Awake() called");
            
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                
                Debug.Log("Set UIPartyView RectTransform to fill screen");
            }
        }
        
        private GameObject CreateUIGameObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent, false);
            return go;
        }
        
        protected override void OnInitialize()
        {
            base.OnInitialize();
            
            Debug.Log("UIPartyView.OnInitialize() called");
            
            if (_autoGenerateUI)
            {
                Debug.Log("Auto-generating UI...");
                BuildCompleteUI();
            }
            
            SetupButtonListeners();
        }

        protected override void OnDeinitialize()
        {
            RemoveButtonListeners();
            CleanupPreviewResources();
            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            
            Debug.Log($"UIPartyView.OnOpen() called - childCount: {transform.childCount}");
            
            if (CanvasGroup != null)
            {
                CanvasGroup.alpha = 1f;
                CanvasGroup.interactable = true;
                CanvasGroup.blocksRaycasts = true;
                Debug.Log($"Set CanvasGroup alpha to 1, interactable and blocksRaycasts to true");
            }
            
            gameObject.SetActive(true);
            Debug.Log($"Set UIPartyView GameObject active=true");
            
            if (_autoGenerateUI && transform.childCount == 0)
            {
                Debug.Log("UIPartyView has no children - rebuilding UI");
                BuildCompleteUI();
                SetupButtonListeners();
            }
            
            _partyManager = PartyLobbyManager.Instance;
            
            if (_partyManager == null)
            {
                var managerObj = new GameObject("PartyLobbyManager");
                _partyManager = managerObj.AddComponent<PartyLobbyManager>();
                Debug.Log("PartyLobbyManager created automatically.");
            }
            
            if (_partyManager != null)
            {
                _partyManager.OnPartyUpdated += OnPartyUpdated;
                _partyManager.OnFriendStatusChanged += OnFriendStatusChanged;
                _partyManager.Initialize(Context.PlayerData.UserID);
            }
            
            if (_addFriendDialog != null)
                _addFriendDialog.SetActive(false);
            
            RefreshFriendsList();
            RefreshPartyList();
            RefreshCharacterPreviews();
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

        protected override void OnTick()
        {
            base.OnTick();
            UpdateButtons();
        }

        private void BuildCompleteUI()
        {
            Debug.Log("BuildCompleteUI() started");
            
            var canvas = GetComponentInParent<Canvas>();
            var canvasRect = canvas.GetComponent<RectTransform>();
            
            CreateBlurBackground();
            Debug.Log("Created blur background");
            
            CreateMainPanel();
            Debug.Log($"Created main panel - _mainPanel null? {_mainPanel == null}");
            
            CreateHeader();
            Debug.Log("Created header");
            
            if (_enableCharacterPreviews)
            {
                CreateCharacterPreviewPanel();
                Debug.Log("Created character preview panel");
            }
            
            CreateContentPanels();
            Debug.Log("Created content panels");
            
            CreateAddFriendDialog();
            Debug.Log("Created add friend dialog");
            
            Debug.Log($"BuildCompleteUI() completed - total children: {transform.childCount}");
        }

        private void CreateBlurBackground()
        {
            Debug.Log("CreateBlurBackground() START");
            
            var bgObj = CreateUIGameObject("BlurBackground", transform);
            bgObj.transform.SetAsFirstSibling();
            
            Debug.Log($"Created BlurBackground GameObject, parent: {bgObj.transform.parent.name}, active: {bgObj.activeSelf}, layer: {bgObj.layer}");
            
            var rect = bgObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var img = bgObj.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0.7f);
            
            Debug.Log($"Added Image to BlurBackground with color {img.color}");
        }

        private void CreateMainPanel()
        {
            var panelObj = new GameObject("MainPanel");
            panelObj.transform.SetParent(transform, false);
            
            _mainPanel = panelObj.AddComponent<RectTransform>();
            _mainPanel.anchorMin = new Vector2(0.5f, 0.5f);
            _mainPanel.anchorMax = new Vector2(0.5f, 0.5f);
            
            float totalHeight = _enableCharacterPreviews ? 950f : 700f;
            _mainPanel.sizeDelta = new Vector2(1000, totalHeight);
            _mainPanel.anchoredPosition = Vector2.zero;
            
            var img = panelObj.AddComponent<Image>();
            img.color = _backgroundColor;
            
            var shadow = panelObj.AddComponent<Shadow>();
            shadow.effectDistance = new Vector2(5, -5);
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
        }

        private void CreateHeader()
        {
            var headerObj = new GameObject("Header");
            headerObj.transform.SetParent(_mainPanel, false);
            
            var rect = headerObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(0, 80);
            rect.anchoredPosition = new Vector2(0, 0);
            rect.pivot = new Vector2(0.5f, 1);
            
            var img = headerObj.AddComponent<Image>();
            img.color = _headerColor;
            
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform, false);
            
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(30, 0);
            titleRect.offsetMax = new Vector2(-100, 0);
            
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "PARTY & FRIENDS";
            titleText.fontSize = 32;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.MidlineLeft;
            titleText.color = _accentColor;
            
            var closeButton = CreateStyledButton(headerObj.transform, "CloseButton", "✕", 
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), 
                new Vector2(60, 60), new Vector2(-20, 0),
                new Color(0.8f, 0.2f, 0.2f), 28);
            closeButton.onClick.AddListener(() => Close());
        }

        private void CreateCharacterPreviewPanel()
        {
            _previewPanel = new GameObject("CharacterPreviewPanel");
            _previewPanel.transform.SetParent(_mainPanel, false);
            
            var rect = _previewPanel.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.sizeDelta = new Vector2(-40, _previewHeight);
            rect.anchoredPosition = new Vector2(0, -100);
            rect.pivot = new Vector2(0.5f, 1);
            
            var img = _previewPanel.AddComponent<Image>();
            img.color = new Color(0.08f, 0.08f, 0.1f, 1f);
            
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_previewPanel.transform, false);
            
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = new Vector2(0, 40);
            titleRect.anchoredPosition = new Vector2(0, 0);
            titleRect.pivot = new Vector2(0.5f, 1);
            
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "PARTY LINEUP";
            titleText.fontSize = 20;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = _accentColor;
            
            var renderObj = new GameObject("PreviewRender");
            renderObj.transform.SetParent(_previewPanel.transform, false);
            
            var renderRect = renderObj.AddComponent<RectTransform>();
            renderRect.anchorMin = new Vector2(0, 0);
            renderRect.anchorMax = new Vector2(1, 1);
            renderRect.offsetMin = new Vector2(10, 10);
            renderRect.offsetMax = new Vector2(-10, -50);
            
            _previewImage = renderObj.AddComponent<RawImage>();
            
            SetupPreviewCamera();
        }

        private void SetupPreviewCamera()
        {
            var cameraObj = new GameObject("PartyPreviewCamera");
            cameraObj.transform.SetParent(_previewPanel.transform, false);
            cameraObj.layer = LayerMask.NameToLayer("UI");
            
            _previewCamera = cameraObj.AddComponent<Camera>();
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = new Color(0.05f, 0.05f, 0.08f, 1f);
            _previewCamera.orthographic = false;
            _previewCamera.fieldOfView = 30f;
            _previewCamera.nearClipPlane = 0.1f;
            _previewCamera.farClipPlane = 20f;
            _previewCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            
            _previewCamera.transform.localPosition = new Vector3(0, 1.2f, -6f);
            _previewCamera.transform.localRotation = Quaternion.Euler(5f, 0f, 0f);
            
            _renderTexture = new RenderTexture(1024, 512, 24);
            _renderTexture.antiAliasing = 4;
            _previewCamera.targetTexture = _renderTexture;
            
            if (_previewImage != null)
            {
                _previewImage.texture = _renderTexture;
            }
            
            var containerObj = new GameObject("CharacterContainer");
            containerObj.transform.SetParent(cameraObj.transform, false);
            containerObj.transform.localPosition = new Vector3(0, -1f, 3f);
            _previewContainer = containerObj.transform;
            
            var lightObj = new GameObject("PreviewLight");
            lightObj.transform.SetParent(cameraObj.transform, false);
            lightObj.transform.localPosition = new Vector3(2f, 3f, -2f);
            lightObj.transform.localRotation = Quaternion.Euler(45f, -30f, 0f);
            
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1.2f;
            light.shadows = LightShadows.None;
            light.cullingMask = 1 << LayerMask.NameToLayer("UI");
            
            var fillLightObj = new GameObject("FillLight");
            fillLightObj.transform.SetParent(cameraObj.transform, false);
            fillLightObj.transform.localPosition = new Vector3(-2f, 1f, -2f);
            
            var fillLight = fillLightObj.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.color = new Color(0.8f, 0.9f, 1f);
            fillLight.intensity = 0.5f;
            fillLight.range = 10f;
            fillLight.shadows = LightShadows.None;
            fillLight.cullingMask = 1 << LayerMask.NameToLayer("UI");
        }

        private void CreateContentPanels()
        {
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(_mainPanel, false);
            
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(20, 20);
            
            if (_enableCharacterPreviews)
            {
                contentRect.offsetMax = new Vector2(-20, -370);
            }
            else
            {
                contentRect.offsetMax = new Vector2(-20, -100);
            }
            
            var layout = contentObj.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 20;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            
            CreateFriendsPanel(contentObj.transform);
            CreatePartyPanel(contentObj.transform);
        }

        private void CreateFriendsPanel(Transform parent)
        {
            var panelObj = new GameObject("FriendsPanel");
            panelObj.transform.SetParent(parent, false);
            
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = _panelColor;
            
            var panelLayout = panelObj.AddComponent<VerticalLayoutGroup>();
            panelLayout.spacing = 10;
            panelLayout.padding = new RectOffset(15, 15, 15, 15);
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = false;
            panelLayout.childForceExpandWidth = true;
            
            CreatePanelTitle(panelObj.transform, "FRIENDS");
            
            _addFriendButton = CreateStyledButton(panelObj.transform, "AddFriendButton", "+ Add Friend",
                Vector2.zero, Vector2.zero, new Vector2(0, 45), Vector2.zero,
                new Color(0.2f, 0.6f, 0.3f), 16);
            
            var scrollObj = new GameObject("FriendsScroll");
            scrollObj.transform.SetParent(panelObj.transform, false);
            
            var scrollRect = scrollObj.AddComponent<RectTransform>();
            float scrollHeight = _enableCharacterPreviews ? 200f : 400f;
            scrollRect.sizeDelta = new Vector2(0, scrollHeight);
            
            var scrollView = scrollObj.AddComponent<ScrollRect>();
            scrollView.horizontal = false;
            scrollView.vertical = true;
            
            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
            var viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportObj.AddComponent<RectMask2D>();
            scrollView.viewport = viewportRect;
            
            var containerObj = new GameObject("FriendsContainer");
            containerObj.transform.SetParent(viewportObj.transform, false);
            _friendsContainer = containerObj.transform;
            
            var containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(0.5f, 1);
            containerRect.sizeDelta = new Vector2(0, 0);
            
            var containerLayout = containerObj.AddComponent<VerticalLayoutGroup>();
            containerLayout.spacing = _widgetSpacing;
            containerLayout.childControlWidth = true;
            containerLayout.childControlHeight = false;
            containerLayout.childForceExpandWidth = true;
            
            var contentSizeFitter = containerObj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollView.content = containerRect;
            
            _noFriendsMessage = CreateNoItemsMessage(_friendsContainer, "No friends added yet");
        }

        private void CreatePartyPanel(Transform parent)
        {
            var panelObj = new GameObject("PartyPanel");
            panelObj.transform.SetParent(parent, false);
            
            var panelImg = panelObj.AddComponent<Image>();
            panelImg.color = _panelColor;
            
            var panelLayout = panelObj.AddComponent<VerticalLayoutGroup>();
            panelLayout.spacing = 10;
            panelLayout.padding = new RectOffset(15, 15, 15, 15);
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = false;
            panelLayout.childForceExpandWidth = true;
            
            CreatePanelTitle(panelObj.transform, "CURRENT PARTY");
            
            var buttonsObj = new GameObject("PartyButtons");
            buttonsObj.transform.SetParent(panelObj.transform, false);
            var buttonsRect = buttonsObj.AddComponent<RectTransform>();
            buttonsRect.sizeDelta = new Vector2(0, 45);
            
            var buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 10;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childForceExpandWidth = true;
            buttonsLayout.childForceExpandHeight = true;
            
            _createPartyButton = CreateStyledButton(buttonsObj.transform, "CreatePartyButton", "Create Party",
                Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,
                new Color(0.3f, 0.6f, 1f), 16);
            
            _leavePartyButton = CreateStyledButton(buttonsObj.transform, "LeavePartyButton", "Leave Party",
                Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,
                new Color(0.8f, 0.3f, 0.2f), 16);
            _leavePartyButton.gameObject.SetActive(false);
            
            var scrollObj = new GameObject("PartyScroll");
            scrollObj.transform.SetParent(panelObj.transform, false);
            
            var scrollRect = scrollObj.AddComponent<RectTransform>();
            float scrollHeight = _enableCharacterPreviews ? 200f : 400f;
            scrollRect.sizeDelta = new Vector2(0, scrollHeight);
            
            var scrollView = scrollObj.AddComponent<ScrollRect>();
            scrollView.horizontal = false;
            scrollView.vertical = true;
            
            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
            var viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportObj.AddComponent<RectMask2D>();
            scrollView.viewport = viewportRect;
            
            var containerObj = new GameObject("PartyContainer");
            containerObj.transform.SetParent(viewportObj.transform, false);
            _partyContainer = containerObj.transform;
            
            var containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(0.5f, 1);
            containerRect.sizeDelta = new Vector2(0, 0);
            
            var containerLayout = containerObj.AddComponent<VerticalLayoutGroup>();
            containerLayout.spacing = _widgetSpacing;
            containerLayout.childControlWidth = true;
            containerLayout.childControlHeight = false;
            containerLayout.childForceExpandWidth = true;
            
            var contentSizeFitter = containerObj.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollView.content = containerRect;
            
            _noPartyMessage = CreateNoItemsMessage(_partyContainer, "No party created");
        }

        private void CreatePanelTitle(Transform parent, string titleText)
        {
            var titleObj = new GameObject("PanelTitle");
            titleObj.transform.SetParent(parent, false);
            
            var rect = titleObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 40);
            
            var text = titleObj.AddComponent<TextMeshProUGUI>();
            text.text = titleText;
            text.fontSize = 22;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Left;
            text.color = Color.white;
        }

        private GameObject CreateNoItemsMessage(Transform parent, string message)
        {
            var msgObj = new GameObject("NoItemsMessage");
            msgObj.transform.SetParent(parent, false);
            
            var rect = msgObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 100);
            
            var text = msgObj.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.fontSize = 16;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(0.6f, 0.6f, 0.6f);
            text.fontStyle = FontStyles.Italic;
            
            msgObj.SetActive(true);
            return msgObj;
        }

        private void CreateAddFriendDialog()
        {
            _addFriendDialog = new GameObject("AddFriendDialog");
            _addFriendDialog.transform.SetParent(transform, false);
            _addFriendDialog.transform.SetAsLastSibling();
            
            var dialogRect = _addFriendDialog.AddComponent<RectTransform>();
            dialogRect.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.sizeDelta = new Vector2(450, 320);
            
            var dialogImg = _addFriendDialog.AddComponent<Image>();
            dialogImg.color = _backgroundColor;
            
            var shadow = _addFriendDialog.AddComponent<Shadow>();
            shadow.effectDistance = new Vector2(8, -8);
            shadow.effectColor = new Color(0, 0, 0, 0.7f);
            
            var dialogLayout = _addFriendDialog.AddComponent<VerticalLayoutGroup>();
            dialogLayout.spacing = 15;
            dialogLayout.padding = new RectOffset(25, 25, 25, 25);
            dialogLayout.childControlWidth = true;
            dialogLayout.childControlHeight = false;
            dialogLayout.childForceExpandWidth = true;
            
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_addFriendDialog.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0, 50);
            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Add Friend";
            titleText.fontSize = 26;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = _accentColor;
            
            _friendIDInput = CreateInputField(_addFriendDialog.transform, "User ID");
            _friendNicknameInput = CreateInputField(_addFriendDialog.transform, "Nickname");
            
            var buttonsObj = new GameObject("Buttons");
            buttonsObj.transform.SetParent(_addFriendDialog.transform, false);
            var buttonsRect = buttonsObj.AddComponent<RectTransform>();
            buttonsRect.sizeDelta = new Vector2(0, 50);
            
            var buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
            buttonsLayout.spacing = 15;
            buttonsLayout.childControlWidth = true;
            buttonsLayout.childControlHeight = true;
            buttonsLayout.childForceExpandWidth = true;
            buttonsLayout.childForceExpandHeight = true;
            
            _confirmAddFriendButton = CreateStyledButton(buttonsObj.transform, "ConfirmButton", "Add",
                Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,
                new Color(0.2f, 0.7f, 0.3f), 18);
            
            _cancelAddFriendButton = CreateStyledButton(buttonsObj.transform, "CancelButton", "Cancel",
                Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero,
                new Color(0.5f, 0.5f, 0.5f), 18);
            
            _addFriendDialog.SetActive(false);
        }

        private TMP_InputField CreateInputField(Transform parent, string placeholder)
        {
            var fieldObj = new GameObject($"InputField_{placeholder}");
            fieldObj.transform.SetParent(parent, false);
            
            var rect = fieldObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 55);
            
            var img = fieldObj.AddComponent<Image>();
            img.color = new Color(0.25f, 0.25f, 0.28f);
            
            var inputField = fieldObj.AddComponent<TMP_InputField>();
            
            var textAreaObj = new GameObject("TextArea");
            textAreaObj.transform.SetParent(fieldObj.transform, false);
            var textAreaRect = textAreaObj.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 0);
            textAreaRect.offsetMax = new Vector2(-10, 0);
            textAreaObj.AddComponent<RectMask2D>();
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(textAreaObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 18;
            text.color = Color.white;
            
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textAreaObj.transform, false);
            var placeholderRect = placeholderObj.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;
            var placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholderText.text = placeholder;
            placeholderText.fontSize = 18;
            placeholderText.fontStyle = FontStyles.Italic;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f);
            
            inputField.textViewport = textAreaRect;
            inputField.textComponent = text;
            inputField.placeholder = placeholderText;
            
            return inputField;
        }

        private UIButton CreateStyledButton(Transform parent, string name, string label, 
            Vector2 anchorMin, Vector2 anchorMax, Vector2 sizeDelta, Vector2 anchoredPos,
            Color color, float fontSize)
        {
            var btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPos;
            
            var img = btnObj.AddComponent<Image>();
            img.color = color;
            
            var button = btnObj.AddComponent<UIButton>();
            
            var textObj = new GameObject("Label");
            textObj.transform.SetParent(btnObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = fontSize;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            
            return button;
        }

        private void SetupButtonListeners()
        {
            if (_addFriendButton != null)
                _addFriendButton.onClick.AddListener(OnAddFriendButton);
            if (_createPartyButton != null)
                _createPartyButton.onClick.AddListener(OnCreatePartyButton);
            if (_leavePartyButton != null)
                _leavePartyButton.onClick.AddListener(OnLeavePartyButton);
            if (_confirmAddFriendButton != null)
                _confirmAddFriendButton.onClick.AddListener(OnConfirmAddFriend);
            if (_cancelAddFriendButton != null)
                _cancelAddFriendButton.onClick.AddListener(OnCancelAddFriend);
        }

        private void RemoveButtonListeners()
        {
            if (_addFriendButton != null)
                _addFriendButton.onClick.RemoveListener(OnAddFriendButton);
            if (_createPartyButton != null)
                _createPartyButton.onClick.RemoveListener(OnCreatePartyButton);
            if (_leavePartyButton != null)
                _leavePartyButton.onClick.RemoveListener(OnLeavePartyButton);
            if (_confirmAddFriendButton != null)
                _confirmAddFriendButton.onClick.RemoveListener(OnConfirmAddFriend);
            if (_cancelAddFriendButton != null)
                _cancelAddFriendButton.onClick.RemoveListener(OnCancelAddFriend);
        }

        private void OnPartyUpdated(TeamData party)
        {
            RefreshPartyList();
            RefreshCharacterPreviews();
        }

        private void OnFriendStatusChanged(FriendData friend)
        {
            RefreshFriendsList();
        }

        private void RefreshFriendsList()
        {
            foreach (var widget in _friendWidgets)
            {
                if (widget != null)
                    Destroy(widget.gameObject);
            }
            _friendWidgets.Clear();
            
            if (_partyManager == null)
                return;
                
            var friends = _partyManager.GetFriends();
            
            if (_noFriendsMessage != null)
            {
                _noFriendsMessage.SetActive(friends.Count == 0);
            }
            
            foreach (var friend in friends)
            {
                var widget = CreateFriendWidget(friend);
                _friendWidgets.Add(widget);
            }
        }

        private void RefreshPartyList()
        {
            foreach (var widget in _partyWidgets)
            {
                if (widget != null)
                    Destroy(widget.gameObject);
            }
            _partyWidgets.Clear();
            
            if (_partyManager == null)
                return;
                
            var party = _partyManager.GetCurrentParty();
            
            if (_noPartyMessage != null)
            {
                _noPartyMessage.SetActive(party == null);
            }
            
            if (party != null)
            {
                foreach (var userID in party.MemberUserIDs)
                {
                    var widget = CreatePartyMemberWidget(userID);
                    _partyWidgets.Add(widget);
                }
            }
        }

        private void RefreshCharacterPreviews()
        {
            if (!_enableCharacterPreviews || _previewContainer == null)
                return;
                
            ClearCharacterPreviews();
            
            if (_partyManager == null)
                return;
                
            var party = _partyManager.GetCurrentParty();
            
            if (party == null || party.MemberUserIDs.Count == 0)
            {
                CreateLocalPlayerPreview();
                return;
            }
            
            int count = party.MemberUserIDs.Count;
            float totalWidth = (count - 1) * _characterSpacing;
            float startX = -(totalWidth * 0.5f);
            
            for (int i = 0; i < party.MemberUserIDs.Count; i++)
            {
                string userID = party.MemberUserIDs[i];
                Vector3 position = new Vector3(startX + (i * _characterSpacing), 0, 0);
                
                CreateCharacterPreview(userID, position, i);
            }
        }

        private void CreateLocalPlayerPreview()
        {
            if (Context == null || Context.PlayerData == null)
                return;
                
            CreateCharacterPreview(Context.PlayerData.UserID, Vector3.zero, 0);
        }

        private void CreateCharacterPreview(string userID, Vector3 position, int index)
        {
            GameObject preview = null;
            
            if (_playerPreviewPrefab != null)
            {
                preview = Instantiate(_playerPreviewPrefab, _previewContainer);
            }
            else
            {
                preview = CreateDefaultCharacterPreview(userID, index);
            }
            
            if (preview != null)
            {
                preview.layer = LayerMask.NameToLayer("UI");
                SetLayerRecursively(preview.transform, LayerMask.NameToLayer("UI"));
                
                preview.transform.localPosition = position;
                preview.transform.localRotation = Quaternion.Euler(_characterRotation);
                preview.transform.localScale = Vector3.one;
                
                _characterPreviews.Add(preview);
                
                var previewComponent = preview.GetComponent<CharacterPreview>();
                if (previewComponent != null)
                {
                    previewComponent.SetCharacter(userID, GetCharacterForUser(userID));
                }
            }
        }

        private GameObject CreateDefaultCharacterPreview(string userID, int index)
        {
            var previewObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            previewObj.name = $"Preview_{userID}";
            previewObj.transform.SetParent(_previewContainer, false);
            
            var collider = previewObj.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
            
            var renderer = previewObj.GetComponent<Renderer>();
            if (renderer != null)
            {
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color = GetColorForUser(userID, index);
                renderer.material = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
            
            var nicknameObj = new GameObject("Nickname");
            nicknameObj.transform.SetParent(previewObj.transform, false);
            nicknameObj.transform.localPosition = new Vector3(0, 1.5f, 0);
            
            var canvas = nicknameObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            
            var canvasRect = nicknameObj.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(200, 50);
            canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(nicknameObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = GetNicknameForUser(userID);
            text.fontSize = 36;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            
            var outline = textObj.AddComponent<UnityEngine.UI.Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, -2);
            
            if (_partyManager != null)
            {
                var party = _partyManager.GetCurrentParty();
                if (party != null && party.IsPartyLeader(userID))
                {
                    var crownObj = new GameObject("Crown");
                    crownObj.transform.SetParent(previewObj.transform, false);
                    crownObj.transform.localPosition = new Vector3(0, 1.8f, 0);
                    
                    var crownCanvas = crownObj.AddComponent<Canvas>();
                    crownCanvas.renderMode = RenderMode.WorldSpace;
                    
                    var crownRect = crownObj.GetComponent<RectTransform>();
                    crownRect.sizeDelta = new Vector2(100, 50);
                    crownRect.localScale = new Vector3(0.015f, 0.015f, 0.015f);
                    
                    var crownTextObj = new GameObject("CrownText");
                    crownTextObj.transform.SetParent(crownObj.transform, false);
                    
                    var crownTextRect = crownTextObj.AddComponent<RectTransform>();
                    crownTextRect.anchorMin = Vector2.zero;
                    crownTextRect.anchorMax = Vector2.one;
                    crownTextRect.offsetMin = Vector2.zero;
                    crownTextRect.offsetMax = Vector2.zero;
                    
                    var crownText = crownTextObj.AddComponent<TextMeshProUGUI>();
                    crownText.text = "★";
                    crownText.fontSize = 48;
                    crownText.fontStyle = FontStyles.Bold;
                    crownText.alignment = TextAlignmentOptions.Center;
                    crownText.color = new Color(1f, 0.9f, 0.2f);
                }
            }
            
            return previewObj;
        }

        private void ClearCharacterPreviews()
        {
            foreach (var preview in _characterPreviews)
            {
                if (preview != null)
                {
                    Destroy(preview);
                }
            }
            
            _characterPreviews.Clear();
        }

        private void CleanupPreviewResources()
        {
            ClearCharacterPreviews();
            
            if (_renderTexture != null)
            {
                _renderTexture.Release();
                Destroy(_renderTexture);
                _renderTexture = null;
            }
            
            if (_previewCamera != null)
            {
                Destroy(_previewCamera.gameObject);
                _previewCamera = null;
            }
        }

        private void SetLayerRecursively(Transform obj, int layer)
        {
            obj.gameObject.layer = layer;
            foreach (Transform child in obj)
            {
                SetLayerRecursively(child, layer);
            }
        }

        private string GetCharacterForUser(string userID)
        {
            if (Context != null && Context.PlayerData != null && userID == Context.PlayerData.UserID)
            {
                return Context.PlayerData.AgentID;
            }
            
            var friend = _partyManager?.GetFriend(userID);
            if (friend != null)
            {
                return "Agent1";
            }
            
            return "Agent1";
        }

        private string GetNicknameForUser(string userID)
        {
            if (Context != null && Context.PlayerData != null && userID == Context.PlayerData.UserID)
            {
                return Context.PlayerData.Nickname;
            }
            
            var friend = _partyManager?.GetFriend(userID);
            if (friend != null)
            {
                return friend.Nickname;
            }
            
            return "Player";
        }

        private Color GetColorForUser(string userID, int index)
        {
            Color[] teamColors = new Color[]
            {
                new Color(0.3f, 0.6f, 1f),
                new Color(1f, 0.4f, 0.3f),
                new Color(0.3f, 1f, 0.5f),
                new Color(1f, 0.8f, 0.2f),
                new Color(0.8f, 0.3f, 1f),
                new Color(1f, 0.5f, 0.7f)
            };
            
            return teamColors[index % teamColors.Length];
        }

        private MonoBehaviour CreateFriendWidget(FriendData friend)
        {
            var widgetObj = new GameObject($"Friend_{friend.UserID}");
            widgetObj.transform.SetParent(_friendsContainer, false);
            
            var rect = widgetObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, _widgetHeight);
            
            var friendListWidget = widgetObj.AddComponent<UIFriendListWidget>();
            friendListWidget.Initialize(friend, _widgetHeight, OnInviteFriend, OnRemoveFriend);
            
            return friendListWidget;
        }

        private UIPartyMemberWidget CreatePartyMemberWidget(string userID)
        {
            var widgetObj = new GameObject($"PartyMember_{userID}");
            widgetObj.transform.SetParent(_partyContainer, false);
            
            var rect = widgetObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, _widgetHeight);
            
            var widget = widgetObj.AddComponent<UIPartyMemberWidget>();
            
            var friend = _partyManager.GetFriend(userID);
            string nickname = friend != null ? friend.Nickname : "Player";
            bool isLeader = _partyManager.GetCurrentParty()?.IsPartyLeader(userID) ?? false;
            bool isLocalPlayer = userID == Context.PlayerData.UserID;
            
            widget.Initialize(userID, nickname, isLeader, isLocalPlayer, _widgetHeight, OnKickMember);
            
            return widget;
        }

        private void UpdateButtons()
        {
            var hasParty = _partyManager != null && _partyManager.GetCurrentParty() != null;
            
            if (_createPartyButton != null)
                _createPartyButton.gameObject.SetActive(!hasParty);
                
            if (_leavePartyButton != null)
                _leavePartyButton.gameObject.SetActive(hasParty);
        }

        private void OnAddFriendButton()
        {
            if (_addFriendDialog != null)
            {
                _addFriendDialog.SetActive(true);
                
                if (_friendIDInput != null)
                    _friendIDInput.text = "";
                if (_friendNicknameInput != null)
                    _friendNicknameInput.text = "";
            }
        }

        private void OnConfirmAddFriend()
        {
            if (_partyManager == null)
                return;
                
            string userID = _friendIDInput != null ? _friendIDInput.text : "";
            string nickname = _friendNicknameInput != null ? _friendNicknameInput.text : "Friend";
            
            if (string.IsNullOrEmpty(userID))
                return;
                
            _partyManager.AddFriend(userID, nickname);
            
            if (_addFriendDialog != null)
                _addFriendDialog.SetActive(false);
                
            RefreshFriendsList();
        }

        private void OnCancelAddFriend()
        {
            if (_addFriendDialog != null)
                _addFriendDialog.SetActive(false);
        }

        private void OnCreatePartyButton()
        {
            if (_partyManager != null)
            {
                _partyManager.CreateParty();
            }
        }

        private void OnLeavePartyButton()
        {
            if (_partyManager != null)
            {
                _partyManager.LeaveParty();
            }
        }

        private void OnInviteFriend(string friendUserID)
        {
            if (_partyManager != null)
            {
                bool success = _partyManager.InviteFriend(friendUserID);
                if (success)
                {
                    Debug.Log($"Invited friend {friendUserID} to party");
                }
                else
                {
                    Debug.LogWarning($"Failed to invite friend {friendUserID}");
                }
            }
        }

        private void OnRemoveFriend(string friendUserID)
        {
            if (_partyManager != null)
            {
                _partyManager.RemoveFriend(friendUserID);
                RefreshFriendsList();
            }
        }

        private void OnKickMember(string userID)
        {
            if (_partyManager != null && _partyManager.IsPartyLeader())
            {
                Debug.Log($"Kicking party member {userID}");
            }
        }
    }
}
