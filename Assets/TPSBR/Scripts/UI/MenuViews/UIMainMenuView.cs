using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Fusion;
using Fusion.Photon.Realtime;
using System.Collections.Generic;
using System.Linq;

namespace TPSBR.UI
{
    public class UIMainMenuView : UIView
    {
        // PRIVATE MEMBERS

        [Header("Quick Play Settings")]
        [SerializeField]
        [Tooltip("Maximum players for created sessions")]
        private int _maxPlayers = 20;
        [SerializeField]
        [Tooltip("Create as dedicated server (otherwise Host)")]
        private bool _dedicatedServer = false;

        [Header("UI Elements")]
        [SerializeField]
        private UIButton _playButton;
        [SerializeField]
        private UIButton _settingsButton;
        [SerializeField]
        private UIButton _shopButton;
        [SerializeField]
        private UIButton _questsButton;
        [SerializeField]
        private UIButton _creditsButton;
        [SerializeField]
        private UIButton _changeNicknameButton;
        [SerializeField]
        private UIButton _quitButton;
        [SerializeField]
        private UIButton _playerButton;
        [SerializeField]
        private UIButton _partyButton;
        [SerializeField]
        private UIButton _replaysButton;
        [SerializeField]
        private UIPlayer _player;
        [SerializeField]
        private TextMeshProUGUI _agentName;
        [SerializeField]
        private TextMeshProUGUI _applicationVersion;
        [SerializeField]
        private TMP_Dropdown _regionDropdown;

        private bool _quickPlayInProgress = false;
        private List<SessionInfo> _latestSessionList = new List<SessionInfo>();

        // PUBLIC METHODS

        public void OnPlayerButtonPointerEnter()
        {
            Context.PlayerPreview.ShowOutline(true);
        }

        public void OnPlayerButtonPointerExit()
        {
            Context.PlayerPreview.ShowOutline(false);
        }

        // UIView INTEFACE

        protected override void OnInitialize()
        {
            base.OnInitialize();

            Debug.Log("[UIMainMenuView] OnInitialize - Setting up button listeners");

            _settingsButton.onClick.AddListener(OnSettingsButton);
            _playButton.onClick.AddListener(OnPlayButton);
            _creditsButton.onClick.AddListener(OnCreditsButton);
            _changeNicknameButton.onClick.AddListener(OnChangeNicknameButton);
            _quitButton.onClick.AddListener(OnQuitButton);
            _playerButton.onClick.AddListener(OnPlayerButton);

            if (_shopButton != null)
                _shopButton.onClick.AddListener(OnShopButton);

            if (_questsButton != null)
                _questsButton.onClick.AddListener(OnQuestsButton);

            if (_partyButton != null)
                _partyButton.onClick.AddListener(OnPartyButton);

            if (_replaysButton != null)
                _replaysButton.onClick.AddListener(OnReplaysButton);

            if (_regionDropdown != null)
            {
                _regionDropdown.onValueChanged.AddListener(OnRegionChanged);
                PrepareRegionDropdown();
            }

            _applicationVersion.text = $"Version {Application.version}";
        }

        protected override void OnDeinitialize()
        {
            _settingsButton.onClick.RemoveListener(OnSettingsButton);
            _playButton.onClick.RemoveListener(OnPlayButton);
            _creditsButton.onClick.RemoveListener(OnCreditsButton);
            _changeNicknameButton.onClick.RemoveListener(OnChangeNicknameButton);
            _quitButton.onClick.RemoveListener(OnQuitButton);
            _playerButton.onClick.RemoveListener(OnPlayerButton);

            if (_shopButton != null)
                _shopButton.onClick.RemoveListener(OnShopButton);

            if (_questsButton != null)
                _questsButton.onClick.RemoveListener(OnQuestsButton);

            if (_partyButton != null)
                _partyButton.onClick.RemoveListener(OnPartyButton);

            if (_replaysButton != null)
                _replaysButton.onClick.RemoveListener(OnReplaysButton);

            if (_regionDropdown != null)
                _regionDropdown.onValueChanged.RemoveListener(OnRegionChanged);

            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            Debug.Log("[UIMainMenuView] OnOpen called");

            UpdatePlayer();

            Global.PlayerService.PlayerDataChanged += OnPlayerDataChanged;
            Context.PlayerPreview.ShowAgent(Context.PlayerData.AgentID);
            Context.PlayerPreview.ShowOutline(false);

            _quickPlayInProgress = false;
            _latestSessionList.Clear();
            
            Context.Matchmaking.SessionListUpdated += OnSessionListUpdate;

            if (_regionDropdown != null)
            {
                var currentRegion = Context.RuntimeSettings.Region;
                int regionIndex = System.Array.FindIndex(Context.Settings.Network.Regions, t => t.Region == currentRegion);
                if (regionIndex >= 0)
                {
                    _regionDropdown.SetValueWithoutNotify(regionIndex);
                }
            }

            if (PhotonAppSettings.Global.AppSettings.AppIdFusion.HasValue())
            {
                string appId = PhotonAppSettings.Global.AppSettings.AppIdFusion;
                string region = Context.RuntimeSettings.Region;
                string lobby = $"FusionBR.{Application.version}";
                
                Debug.Log($"[UIMainMenuView] Joining Photon lobby...");
                Debug.Log($"  App ID: {appId.Substring(0, Mathf.Min(8, appId.Length))}... (length: {appId.Length})");
                Debug.Log($"  Region: {region}");
                Debug.Log($"  Lobby Name: {lobby}");
                Debug.Log($"  App Version: {Application.version}");
                
                Context.Matchmaking.JoinLobby(true);
            }
            else
            {
                Debug.LogWarning("[UIMainMenuView] No Fusion App ID configured - matchmaking disabled");
            }
        }

        protected override void OnClose()
        {
            Global.PlayerService.PlayerDataChanged -= OnPlayerDataChanged;
            Context.PlayerPreview.ShowOutline(false);
            
            Context.Matchmaking.SessionListUpdated -= OnSessionListUpdate;

            if (_quickPlayInProgress)
            {
                StopAllCoroutines();
                _quickPlayInProgress = false;
            }

            Context.Matchmaking.LeaveLobby();

            base.OnClose();
        }

        protected override bool OnBackAction()
        {
            if (IsInteractable == false)
                return false;

            OnQuitButton();
            return true;
        }

        // PRIVATE METHODS

        private void OnSettingsButton()
        {
            Open<UISettingsView>();
        }

        private void OnPlayButton()
        {
            Open<UIMultiplayerView>();
        }

        private void OnPlayButtonOLD_DISABLED()
        {
            Debug.Log("[Quick Play] Play button clicked!");

            if (_quickPlayInProgress)
            {
                Debug.LogWarning("[Quick Play] Quick play already in progress, ignoring click");
                return;
            }

            if (PhotonAppSettings.Global.AppSettings.AppIdFusion.HasValue() == false)
            {
                var errorDialog = Open<UIErrorDialogView>();
                errorDialog.Title.text = "Missing App Id";
                errorDialog.Description.text = "Fusion App Id is not assigned in the Photon App Settings asset.\n\nPlease follow instructions in the Fusion BR200 documentation on how to create and assign App Id.";
                
                #if UNITY_EDITOR
                errorDialog.HasClosed += () =>
                {
                    UnityEditor.Selection.activeObject = PhotonAppSettings.Global;
                    UnityEditor.EditorGUIUtility.PingObject(PhotonAppSettings.Global);
                };
                #endif
                return;
            }

            _quickPlayInProgress = true;
            _playButton.interactable = false;

            Debug.Log("[Quick Play] Leaving lobby before starting game...");
            Context.Matchmaking.LeaveLobby();

            string region = Context.RuntimeSettings.Region;
            string appVersion = Application.version;
            
            string quickPlaySessionName = $"QuickPlay_{region}_{appVersion}";

            Debug.Log($"[Quick Play] Starting Quick Play...");
            Debug.Log($"[Quick Play] Session Name: {quickPlaySessionName}");
            Debug.Log($"[Quick Play] Region: {region}");
            Debug.Log($"[Quick Play] App Version: {appVersion}");
            Debug.Log($"[Quick Play] This session name is shared - all players in this region/version will join the same game!");

            var mapSettings = Context.Settings.Map;
            if (mapSettings == null || mapSettings.Maps == null || mapSettings.Maps.Length == 0)
            {
                Debug.LogError("[Quick Play] No maps configured!");
                ResetQuickPlayState();

                var errorDialog = Open<UIErrorDialogView>();
                errorDialog.Title.text = "No Maps Available";
                errorDialog.Description.text = "No maps are configured in the game settings. Please configure maps in GlobalSettings.";
                return;
            }

            var availableMaps = mapSettings.Maps.Where(m => m != null && m.ShowInMapSelection).ToList();
            if (availableMaps.Count == 0)
            {
                Debug.LogError("[Quick Play] No valid maps found for selection!");
                ResetQuickPlayState();

                var errorDialog = Open<UIErrorDialogView>();
                errorDialog.Title.text = "No Valid Maps";
                errorDialog.Description.text = "No valid maps available. Please check map configuration in GlobalSettings.";
                return;
            }

            var randomMap = availableMaps[Random.Range(0, availableMaps.Count)];

            var request = new SessionRequest
            {
                UserID = Context.PlayerData.UserID,
                GameMode = _dedicatedServer ? GameMode.Server : GameMode.Host,
                DisplayName = Context.PlayerData.Nickname,
                SessionName = quickPlaySessionName,
                ScenePath = randomMap.ScenePath,
                GameplayType = EGameplayType.BattleRoyale,
                MaxPlayers = _maxPlayers,
            };

            Debug.Log($"[Quick Play] Creating/Joining session on map '{randomMap.DisplayName}' (MaxPlayers: {_maxPlayers})");
            Global.Networking.StartGame(request);
        }

        private void OnCreditsButton()
        {
            Open<UICreditsView>();
        }

        private void OnChangeNicknameButton()
        {
            var changeNicknameView = Open<UIChangeNicknameView>();
            changeNicknameView.SetData("CHANGE NICKNAME", false);
        }

        private void OnQuitButton()
        {
            var dialog = Open<UIYesNoDialogView>();

            dialog.Title.text = "EXIT GAME";
            dialog.Description.text = "Are you sure you want to exit the game?";

            dialog.YesButtonText.text = "EXIT";
            dialog.NoButtonText.text = "CANCEL";

            dialog.HasClosed += (result) =>
            {
                if (result == true)
                {
                    SceneUI.Scene.Quit();
                }
            };
        }

        private void OnPlayerButton()
        {
            var agentSelection = Open<UIAgentSelectionView>();
            agentSelection.BackView = this;

            Close();
        }

        private void OnShopButton()
        {
            var modernShop = Open<ModernShopManager>();

            if (modernShop == null)
            {
                Debug.LogWarning("[Modern Shop] ModernShopManager not found. Trying old UIShopView...");
                var shopView = Open<UIShopView>();
                
                if (shopView == null)
                {
                    Debug.LogWarning("[Shop System] No shop view found. Please run: TPSBR → 🎨 Create Modern Shop UI");
                    return;
                }

                shopView.BackView = this;
                Close();
                return;
            }

            modernShop.BackView = this;

            var agentSelection = SceneUI.Get<UIAgentSelectionView>();
            if (agentSelection != null && agentSelection.IsOpen == true)
            {
                agentSelection.Close();
            }

            Close();
        }

        private void OnPartyButton()
        {
            Open<UIPartyViewSimple>();
        }

        private void OnReplaysButton()
        {
            SceneManager.LoadScene("ReplayViewer");
        }

        private void OnQuestsButton()
        {
            var questView = Open<UIQuestView>();
            
            if (questView == null)
            {
                Debug.LogWarning("[Quest System] UIQuestView not found. Please run: TPSBR → Generate Quest UI");
                return;
            }

            questView.BackView = this;

            var agentSelection = SceneUI.Get<UIAgentSelectionView>();
            if (agentSelection != null && agentSelection.IsOpen == true)
            {
                agentSelection.Close();
            }

            Close();
        }

        private void OnPlayerDataChanged(PlayerData playerData)
        {
            UpdatePlayer();
        }

        private void UpdatePlayer()
        {
            _player.SetData(Context, Context.PlayerData);
            Context.PlayerPreview.ShowAgent(Context.PlayerData.AgentID);

            var setup = Context.Settings.Agent.GetAgentSetup(Context.PlayerData.AgentID);
            _agentName.text = setup != null ? $"Playing as {setup.DisplayName}" : string.Empty;
        }

        private void ResetQuickPlayState()
        {
            _quickPlayInProgress = false;
            _playButton.interactable = true;
        }

        private void OnRegionChanged(int regionIndex)
        {
            if (regionIndex < 0 || regionIndex >= Context.Settings.Network.Regions.Length)
                return;

            var region = Context.Settings.Network.Regions[regionIndex].Region;
            var regionInfo = Context.Settings.Network.Regions[regionIndex];
            
            Debug.Log($"[UIMainMenuView] Region changed to: {regionInfo.DisplayName} ({region})");
            
            Context.RuntimeSettings.Region = region;

            if (PhotonAppSettings.Global.AppSettings.AppIdFusion.HasValue())
            {
                Debug.Log($"[UIMainMenuView] Rejoining lobby with new region: {region}");
                Context.Matchmaking.JoinLobby(true);
            }
        }

        private void PrepareRegionDropdown()
        {
            var options = ListPool.Get<TMP_Dropdown.OptionData>(16);
            var regions = Context.Settings.Network.Regions;

            for (int i = 0; i < regions.Length; i++)
            {
                var regionInfo = regions[i];
                var optionData = new TMP_Dropdown.OptionData();
                optionData.text = regionInfo.DisplayName;
                optionData.image = regionInfo.Icon;
                options.Add(optionData);
            }

            _regionDropdown.ClearOptions();
            _regionDropdown.AddOptions(options);

            ListPool.Return(options);
        }

        private void OnSessionListUpdate(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            _latestSessionList = new List<SessionInfo>(sessionList);
            
            string debugInfo = $"[Quick Play] Session list updated - now tracking {sessionList.Count} total sessions";
            
            if (sessionList.Count > 0)
            {
                debugInfo += "\n  Sessions in lobby:";
                foreach (var session in sessionList)
                {
                    debugInfo += $"\n    - '{session.Name}' | {session.PlayerCount}/{session.MaxPlayers} players | Type: {session.GetGameplayType()} | Open: {session.IsOpen} | Visible: {session.IsVisible}";
                }
            }
            
            Debug.Log(debugInfo);
        }

        private System.Collections.IEnumerator WaitForLobbyAndRetry()
        {
            float timeout = 10f;
            float elapsed = 0f;
            
            Debug.Log("[Quick Play] Waiting for lobby connection...");

            while (Context.Matchmaking.IsConnectedToLobby == false && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                
                if (Mathf.FloorToInt(elapsed) % 2 == 0 && elapsed - Time.deltaTime < Mathf.FloorToInt(elapsed))
                {
                    Debug.Log($"[Quick Play] Still connecting to lobby... ({Mathf.FloorToInt(elapsed)}s / {timeout}s)");
                }
                
                yield return null;
            }

            if (Context.Matchmaking.IsConnectedToLobby)
            {
                Debug.Log("[Quick Play] Connected to lobby! Retrying Play...");
                OnPlayButton();
            }
            else
            {
                Debug.LogError("[Quick Play] Failed to connect to lobby within timeout. Please check:");
                Debug.LogError("  1. Your internet connection");
                Debug.LogError("  2. Photon App ID is correctly configured");
                Debug.LogError("  3. Photon servers are accessible");
                
                ResetQuickPlayState();
                
                var errorDialog = Open<UIErrorDialogView>();
                errorDialog.Title.text = "Connection Failed";
                errorDialog.Description.text = "Failed to connect to Photon lobby. Please check:\n\n• Your internet connection\n• Photon App ID configuration\n• Try restarting the game";
            }
        }

        private System.Collections.IEnumerator WaitAndCreateSession(List<SessionInfo> lastSessionList)
        {
            float searchDuration = 10f;
            float checkInterval = 1.5f;
            float elapsed = 0f;
            
            Debug.Log($"[Quick Play] Searching for available sessions for {searchDuration} seconds before creating new game...");
            
            while (elapsed < searchDuration)
            {
                if (_quickPlayInProgress == false)
                {
                    Debug.Log("[Quick Play] Quick play was cancelled during search");
                    yield break;
                }

                var availableSessions = _latestSessionList
                    .Where(s => s.IsValid && s.IsOpen && s.IsVisible)
                    .Where(s => s.GetGameplayType() == EGameplayType.BattleRoyale)
                    .Where(s => s.PlayerCount < s.MaxPlayers)
                    .Where(s => s.HasMap())
                    .OrderByDescending(s => s.PlayerCount)
                    .ToList();

                if (availableSessions.Count > 0)
                {
                    var bestSession = availableSessions[0];
                    Debug.Log($"[Quick Play] Found session after {elapsed:F1}s! Joining '{bestSession.Name}' with {bestSession.PlayerCount}/{bestSession.MaxPlayers} players...");
                    Context.Matchmaking.JoinSession(bestSession);
                    yield break;
                }

                Debug.Log($"[Quick Play] Still searching... ({elapsed:F1}s / {searchDuration}s) - {_latestSessionList.Count} total sessions, 0 available BR sessions");
                yield return new WaitForSeconds(checkInterval);
                elapsed += checkInterval;
            }

            Debug.Log($"[Quick Play] No sessions found after {searchDuration} seconds. Creating new Battle Royale session...");
            // CreateQuickPlaySession(); // Method removed - now using deterministic session names
        }
    }
}
