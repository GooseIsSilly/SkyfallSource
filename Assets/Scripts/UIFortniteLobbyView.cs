using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Collections.Generic;
using System.Linq;
using TPSBR.Backend;

namespace TPSBR.UI
{
    public class UIFortniteLobbyView : UIView
    {
        [Header("Top Navigation Buttons")]
        [SerializeField] private UIButton _shopButton;
        [SerializeField] private UIButton _questButton;
        [SerializeField] private UIButton _lockerButton;
        [SerializeField] private UIButton _battlePassButton;
        [SerializeField] private UIButton _settingsButton;
        [SerializeField] private UIButton _regionButton;
        [SerializeField] private TextMeshProUGUI _regionButtonText;

        [Header("Main Action Buttons")]
        [SerializeField] private UIButton _playButton;
        [SerializeField] private TextMeshProUGUI _playButtonText;
        [SerializeField] private UIButton _gamemodeButton;
        [SerializeField] private TextMeshProUGUI _gamemodeButtonText;

        [Header("Player Info")]
        [SerializeField] private TextMeshProUGUI _playerNameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Image _levelProgressBar;
        [SerializeField] private TextMeshProUGUI _cloudCoinsText;

        [Header("Quick Play Settings")]
        [SerializeField] private float _searchTimeout = 10f;
        [SerializeField] private EGameplayType _gameplayType = EGameplayType.BattleRoyale;
        [SerializeField] private int _maxPlayers = 100;
        [SerializeField] private string _defaultMapScenePath = "TPSBR/Scenes/Game";

        [Header("Battle Pass")]
        [SerializeField] private GameObject _battlePassCanvas;

        private bool _isSearchingForGame;
        private bool _isConnectingForPlay;
        private float _searchStartTime;
        private List<SessionInfo> _availableSessions = new List<SessionInfo>();
        private UIMatchmakerView _matchmakerView;
        // Set to true to re-enable Manhunt in the quick-play rotation.
        private const bool MANHUNT_ENABLED = false;

        private int _currentGamemodeIndex = 0;
        private EGameplayType[] _availableGamemodes = MANHUNT_ENABLED
            ? new EGameplayType[] { EGameplayType.BattleRoyale, EGameplayType.Manhunt }
            : new EGameplayType[] { EGameplayType.BattleRoyale };

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (_playButton != null)
                _playButton.onClick.AddListener(OnPlayButtonClicked);

            if (_gamemodeButton != null)
                _gamemodeButton.onClick.AddListener(OnGamemodeButtonClicked);

            if (_shopButton != null)
                _shopButton.onClick.AddListener(OnShopButtonClicked);

            if (_questButton != null)
                _questButton.onClick.AddListener(OnQuestButtonClicked);

            if (_lockerButton != null)
                _lockerButton.onClick.AddListener(OnLockerButtonClicked);

            if (_battlePassButton != null)
                _battlePassButton.onClick.AddListener(OnBattlePassButtonClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsButtonClicked);

            if (_regionButton != null)
                _regionButton.onClick.AddListener(OnRegionButtonClicked);

            UpdateGamemodeDisplay();
        }

        protected override void OnDeinitialize()
        {
            if (_playButton != null)
                _playButton.onClick.RemoveListener(OnPlayButtonClicked);

            if (_gamemodeButton != null)
                _gamemodeButton.onClick.RemoveListener(OnGamemodeButtonClicked);

            if (_shopButton != null)
                _shopButton.onClick.RemoveListener(OnShopButtonClicked);

            if (_questButton != null)
                _questButton.onClick.RemoveListener(OnQuestButtonClicked);

            if (_lockerButton != null)
                _lockerButton.onClick.RemoveListener(OnLockerButtonClicked);

            if (_battlePassButton != null)
                _battlePassButton.onClick.RemoveListener(OnBattlePassButtonClicked);

            if (_settingsButton != null)
                _settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);

            if (_regionButton != null)
                _regionButton.onClick.RemoveListener(OnRegionButtonClicked);

            base.OnDeinitialize();
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            UpdatePlayerInfo();
            LoadPlayerDataFromBackend();

            if (Context.PlayerPreview != null && Context.PlayerData != null)
            {
                Context.PlayerPreview.ShowAgent(Context.PlayerData.AgentID);
                Context.PlayerPreview.ShowOutline(false);
            }

            Context.Matchmaking.SessionListUpdated += OnSessionListUpdated;
            Context.Matchmaking.LobbyJoined += OnLobbyJoined;
            Context.Matchmaking.LobbyJoinFailed += OnLobbyJoinFailed;

            // Subscribe to coin changes
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnCoinsChanged += OnCoinsChanged;
                PlayerDataManager.Instance.OnLevelChanged += OnLevelChanged;
                PlayerDataManager.Instance.OnDataLoaded += OnPlayerDataLoaded;
            }

            Context.Matchmaking.JoinLobby(false);
            UpdateRegionDisplay();
        }

        protected override void OnClose()
        {
            Context.Matchmaking.SessionListUpdated -= OnSessionListUpdated;
            Context.Matchmaking.LobbyJoined -= OnLobbyJoined;
            Context.Matchmaking.LobbyJoinFailed -= OnLobbyJoinFailed;

            // Unsubscribe from coin changes
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.OnCoinsChanged -= OnCoinsChanged;
                PlayerDataManager.Instance.OnLevelChanged -= OnLevelChanged;
                PlayerDataManager.Instance.OnDataLoaded -= OnPlayerDataLoaded;
            }

            base.OnClose();
        }

        protected override void OnTick()
        {
            base.OnTick();

            if (_isSearchingForGame)
            {
                float elapsedTime = Time.realtimeSinceStartup - _searchStartTime;

                if (elapsedTime >= _searchTimeout)
                {
                    Debug.LogWarning($"[UIFortniteLobbyView] Search timeout after {_searchTimeout} seconds - no games found");
                    _isSearchingForGame = false;
                    ShowCreateGameUI();
                }
            }
        }

        private void UpdatePlayerInfo()
        {
            if (_playerNameText != null)
            {
                _playerNameText.text = Context.PlayerData.Nickname;
            }
        }

        private void LoadPlayerDataFromBackend()
        {
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogWarning("[UIFortniteLobbyView] PlayerDataManager not found - skipping backend data load");
                return;
            }

            PlayerDataManager.Instance.LoadPlayerData((success, data) =>
            {
                if (success)
                {
                    UpdateCloudCoinsDisplay(data.CloudCoins);
                    UpdateLevelDisplay(data.Level, data.XP);
                    Debug.Log($"[UIFortniteLobbyView] Loaded player data: Level {data.Level}, {data.CloudCoins} coins");
                }
                else
                {
                    Debug.LogWarning("[UIFortniteLobbyView] Failed to load player data from backend");
                }
            });
        }

        private void UpdateCloudCoinsDisplay(int coins)
        {
            if (_cloudCoinsText != null)
            {
                _cloudCoinsText.text = coins.ToString();
            }
        }

        private void UpdateLevelDisplay(int level, int xp)
        {
            if (_levelText != null)
            {
                _levelText.text = $"Level {level}";
            }

            if (_levelProgressBar != null)
            {
                // XP needed per level is 1000 (configured in backend)
                float xpPerLevel = 1000f;
                float progress = xp / xpPerLevel;
                _levelProgressBar.fillAmount = progress;
            }
        }

        private void OnCoinsChanged(int newCoinAmount)
        {
            UpdateCloudCoinsDisplay(newCoinAmount);
            Debug.Log($"[UIFortniteLobbyView] Coins updated: {newCoinAmount}");
        }

        private void OnLevelChanged(int newLevel)
        {
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.CurrentData != null)
            {
                UpdateLevelDisplay(newLevel, PlayerDataManager.Instance.CurrentData.XP);
                Debug.Log($"[UIFortniteLobbyView] Level updated: {newLevel}");
            }
        }

        private void OnPlayerDataLoaded(PlayerGameData data)
        {
            UpdateCloudCoinsDisplay(data.CloudCoins);
            UpdateLevelDisplay(data.Level, data.XP);
        }

        private void OnPlayButtonClicked()
        {
            Debug.Log("[UIFortniteLobbyView] Play button clicked - Starting Quick Play");

            if (SeasonEndController.Instance != null && SeasonEndController.Instance.IsInDowntime)
            {
                Debug.LogWarning("[UIFortniteLobbyView] Cannot start game - Season downtime is active!");

                if (_playButtonText != null)
                {
                    _playButtonText.text = "SEASON DOWNTIME";
                }

                return;
            }

            _availableSessions.Clear();
            _isSearchingForGame = false;
            _isConnectingForPlay = true;

            if (_playButtonText != null)
            {
                _playButtonText.text = "CONNECTING...";
            }

            Debug.Log("[UIFortniteLobbyView] Rejoining lobby to refresh session list...");
            Context.Matchmaking.JoinLobby(true);
        }

        private void StartQuickPlay()
        {
            _availableSessions.Clear();
            _isSearchingForGame = true;
            _searchStartTime = Time.realtimeSinceStartup;

            if (_playButtonText != null)
            {
                _playButtonText.text = "SEARCHING...";
            }

            Debug.Log("[UIFortniteLobbyView] Starting quick play search");
            Debug.Log($"[UIFortniteLobbyView] Looking for {_gameplayType} games with max {_maxPlayers} players");
        }

        private void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            Debug.Log($"[UIFortniteLobbyView] ========== SESSION LIST UPDATE ==========");
            Debug.Log($"[UIFortniteLobbyView] Total sessions received: {sessionList.Count}");
            Debug.Log($"[UIFortniteLobbyView] Is searching: {_isSearchingForGame}");

            foreach (var session in sessionList)
            {
                Debug.Log($"[UIFortniteLobbyView] RAW SESSION:");
                Debug.Log($"  Name: {session.Name}");
                Debug.Log($"  IsValid: {session.IsValid}");
                Debug.Log($"  IsOpen: {session.IsOpen}");
                Debug.Log($"  IsVisible: {session.IsVisible}");
                Debug.Log($"  PlayerCount: {session.PlayerCount}/{session.MaxPlayers}");
                Debug.Log($"  Region: {session.Region}");
                Debug.Log($"  HasMap: {session.HasMap()}");
                if (session.HasMap())
                {
                    var mapSetup = session.GetMapSetup();
                    Debug.Log($"  Map: {(mapSetup != null ? mapSetup.DisplayName : "NULL")}");
                }
                Debug.Log($"  GameplayType: {session.GetGameplayType()}");
            }

            if (!_isSearchingForGame)
            {
                Debug.Log("[UIFortniteLobbyView] Not searching, ignoring session update");
                Debug.Log($"[UIFortniteLobbyView] ========================================");
                return;
            }

            _availableSessions.Clear();

            int filteredOutCount = 0;
            string filterReasons = "";

            foreach (var session in sessionList)
            {
                bool filtered = false;
                string reason = "";

                if (session.IsValid == false)
                {
                    filtered = true;
                    reason = "not valid";
                }
                else if (session.IsOpen == false)
                {
                    filtered = true;
                    reason = "not open";
                }
                else if (session.IsVisible == false)
                {
                    filtered = true;
                    reason = "not visible";
                }
                else if (session.PlayerCount >= session.MaxPlayers)
                {
                    filtered = true;
                    reason = "full";
                }
                else if (session.HasMap() == false)
                {
                    filtered = true;
                    reason = "no map";
                }
                else
                {
                    var sessionGameplayType = session.GetGameplayType();
                    if (sessionGameplayType != _gameplayType)
                    {
                        filtered = true;
                        reason = $"wrong type ({sessionGameplayType} vs {_gameplayType})";
                    }
                }

                if (filtered)
                {
                    filteredOutCount++;
                    filterReasons += $"\n  - {session.GetDisplayName()}: {reason}";
                }
                else
                {
                    _availableSessions.Add(session);
                    Debug.Log($"[UIFortniteLobbyView] ✓ Found valid session: {session.GetDisplayName()} ({session.PlayerCount}/{session.MaxPlayers})");
                }
            }

            if (filteredOutCount > 0)
            {
                Debug.Log($"[UIFortniteLobbyView] Filtered out {filteredOutCount} sessions:{filterReasons}");
            }

            Debug.Log($"[UIFortniteLobbyView] {_availableSessions.Count} sessions match criteria");
            Debug.Log($"[UIFortniteLobbyView] ========================================");

            if (_availableSessions.Count > 0)
            {
                JoinBestSession();
            }
        }

        private void JoinBestSession()
        {
            var bestSession = _availableSessions
                .OrderByDescending(s => s.PlayerCount)
                .ThenBy(s => s.MaxPlayers - s.PlayerCount)
                .First();

            Debug.Log($"[UIFortniteLobbyView] Found session: {bestSession.GetDisplayName()} - Joining...");

            if (_playButtonText != null)
            {
                _playButtonText.text = "JOINING...";
            }

            _isSearchingForGame = false;
            Context.Matchmaking.JoinSession(bestSession);
        }

        private void ShowCreateGameUI()
        {
            Debug.Log("[UIFortniteLobbyView] No sessions found - Opening Create Game UI");

            if (_playButtonText != null)
            {
                _playButtonText.text = "PLAY";
            }

            Open<UICreateSessionView>();
        }

        private void OnLobbyJoined()
        {
            Debug.Log("[UIFortniteLobbyView] Joined lobby successfully");

            if (_isConnectingForPlay)
            {
                _isConnectingForPlay = false;
                Debug.Log("[UIFortniteLobbyView] Lobby connected! Starting search...");
                StartQuickPlay();
            }
        }

        private void OnLobbyJoinFailed(string region)
        {
            Debug.LogWarning($"[UIFortniteLobbyView] Failed to join lobby in region: {region}");

            _isConnectingForPlay = false;

            if (_playButtonText != null)
            {
                _playButtonText.text = "PLAY";
            }
        }

        private void OnShopButtonClicked()
        {
            Debug.Log("[UIFortniteLobbyView] Shop button clicked - Opening Modern Shop");
            Open<ModernShopManager>();
        }

        private void OnQuestButtonClicked()
        {
            Debug.Log("[UIFortniteLobbyView] Quest button clicked - Opening Quests");
            var questView = Open<UIQuestView>();
            if (questView != null)
            {
                questView.BackView = this;
            }
        }

        private void OnLockerButtonClicked()
        {
            Debug.Log("[UIFortniteLobbyView] Locker button clicked");
            var lockerView = Open<UIAgentSelectionView>();
            if (lockerView != null)
            {
                lockerView.BackView = this;
            }
        }

        private void OnBattlePassButtonClicked()
        {
            Debug.Log("[UIFortniteLobbyView] Battle Pass button clicked");
            if (_battlePassCanvas != null)
                _battlePassCanvas.SetActive(true);
        }

        private void OnSettingsButtonClicked()
        {
            Debug.Log("[UIFortniteLobbyView] Settings button clicked - Opening Settings");
            Open<UISettingsView>();
        }

        private void OnRegionButtonClicked()
        {
            var regions = Context.Settings.Network.Regions;
            if (regions == null || regions.Length == 0) return;

            string currentRegion = Context.RuntimeSettings.Region;
            int currentIndex = System.Array.FindIndex(regions, r => r.Region == currentRegion);
            int nextIndex = (currentIndex + 1) % regions.Length;

            Context.RuntimeSettings.Region = regions[nextIndex].Region;
            UpdateRegionDisplay();

            // Cancel any in-progress play or search flow so OnLobbyJoined below
            // does not accidentally start a quick-play search in the new region.
            _isConnectingForPlay = false;
            _isSearchingForGame = false;
            _availableSessions.Clear();

            if (_playButtonText != null)
                _playButtonText.text = "PLAY";

            Debug.Log($"[UIFortniteLobbyView] Region changed to: {regions[nextIndex].DisplayName}");

            // Reconnect to the lobby for the new region.
            Context.Matchmaking.JoinLobby(true);
        }

        private void UpdateRegionDisplay()
        {
            if (_regionButtonText != null)
            {
                var regionInfo = Context.Settings.Network.GetRegionInfo(Context.RuntimeSettings.Region);
                _regionButtonText.text = regionInfo != null ? regionInfo.DisplayName.ToUpper() : "UNKNOWN REGION";
            }
        }

        private void OnGamemodeButtonClicked()
        {
            // Cycle through available gamemodes
            _currentGamemodeIndex = (_currentGamemodeIndex + 1) % _availableGamemodes.Length;
            _gameplayType = _availableGamemodes[_currentGamemodeIndex];

            // Manhunt caps at 10 players; restore default for other modes
            _maxPlayers = _gameplayType == EGameplayType.Manhunt ? 10 : 100;

            UpdateGamemodeDisplay();

            Debug.Log($"[UIFortniteLobbyView] Gamemode changed to: {GetGamemodeName(_gameplayType)}");
        }

        private void UpdateGamemodeDisplay()
        {
            if (_gamemodeButtonText != null)
            {
                string gamemodeName = GetGamemodeName(_gameplayType);
                _gamemodeButtonText.text = gamemodeName;
            }
        }

        private string GetGamemodeName(EGameplayType gameplayType)
        {
            switch (gameplayType)
            {
                case EGameplayType.BattleRoyale:
                    return "BATTLE ROYALE - SOLO";
                case EGameplayType.Manhunt:
                    return "LTM - MANHUNT";
                case EGameplayType.None:
                default:
                    return "SELECT GAMEMODE";
            }
        }
    }
}