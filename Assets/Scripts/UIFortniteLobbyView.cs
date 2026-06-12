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

            if (PartyLobbyManager.Instance != null)
            {
                PartyLobbyManager.Instance.OnMatchFound += OnMatchFound;
                PartyLobbyManager.Instance.OnPartyUpdated += OnPartyUpdated;
            }

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

            if (PartyLobbyManager.Instance != null)
            {
                PartyLobbyManager.Instance.OnMatchFound -= OnMatchFound;
                PartyLobbyManager.Instance.OnPartyUpdated -= OnPartyUpdated;
            }

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

        private void OnMatchFound(string owner)
        {
            Debug.Log($"[UIFortniteLobbyView] Match found! Traveling with owner: {owner}");
            
            if (_playButtonText != null)
                _playButtonText.text = "JOINING...";

            if (Context.Matchmaking != null)
            {
                Context.Matchmaking.JoinSession(owner);
            }
        }

        private void OnPartyUpdated(TeamData party)
        {
            if (party != null && !PartyLobbyManager.Instance.IsPartyLeader())
            {
                if (_playButtonText != null && !_isSearchingForGame)
                {
                    _playButtonText.text = "NOT READY";
                }
            }
        }

        private void OnPlayButtonClicked()
        {
            Debug.Log("[UIFortniteLobbyView] Play button clicked - Starting Quick Play Matchmaking");

            if (SeasonEndController.Instance != null && SeasonEndController.Instance.IsInDowntime)
            {
                Debug.LogWarning("[UIFortniteLobbyView] Cannot start game - Season downtime is active!");
                if (_playButtonText != null) _playButtonText.text = "SEASON DOWNTIME";
                return;
            }

            _availableSessions.Clear();
            _isSearchingForGame = false;
            _isConnectingForPlay = true;

            if (_playButtonText != null)
                _playButtonText.text = "CONNECTING...";

            // Use the built-in Fusion matchmaking flow
            if (Context.Matchmaking != null)
            {
                Context.Matchmaking.JoinLobby(true);
            }
        }

        private void UpdatePlayerInfo()
        {
            if (_playerNameText != null)
                _playerNameText.text = Context.PlayerData.Nickname;
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
                _cloudCoinsText.text = coins.ToString();
        }

        private void UpdateLevelDisplay(int level, int xp)
        {
            if (_levelText != null)
                _levelText.text = $"Level {level}";

            if (_levelProgressBar != null)
            {
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

        private void StartQuickPlay()
        {
            _availableSessions.Clear();
            _isSearchingForGame = true;
            _searchStartTime = Time.realtimeSinceStartup;

            if (_playButtonText != null)
                _playButtonText.text = "SEARCHING...";

            Debug.Log("[UIFortniteLobbyView] Starting quick play search");
        }

        private void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
            if (!_isSearchingForGame) return;

            _availableSessions.Clear();

            foreach (var session in sessionList)
            {
                if (session.IsValid && session.IsOpen && session.IsVisible && session.PlayerCount < session.MaxPlayers && session.HasMap())
                {
                    var sessionGameplayType = session.GetGameplayType();
                    if (sessionGameplayType == _gameplayType)
                    {
                        _availableSessions.Add(session);
                    }
                }
            }

            if (_availableSessions.Count > 0)
                JoinBestSession();
        }

        private void JoinBestSession()
        {
            var bestSession = _availableSessions
                .OrderByDescending(s => s.PlayerCount)
                .ThenBy(s => s.MaxPlayers - s.PlayerCount)
                .First();

            Debug.Log($"[UIFortniteLobbyView] Found session: {bestSession.Name} - Joining...");

            if (_playButtonText != null)
                _playButtonText.text = "JOINING...";

            _isSearchingForGame = false;
            Context.Matchmaking.JoinSession(bestSession);
        }

        private void ShowCreateGameUI()
        {
            Debug.Log("[UIFortniteLobbyView] No sessions found - Opening Create Game UI");
            if (_playButtonText != null) _playButtonText.text = "PLAY";
            Open<UICreateSessionView>();
        }

        private void OnLobbyJoined()
        {
            Debug.Log("[UIFortniteLobbyView] Joined lobby successfully");
            if (_isConnectingForPlay)
            {
                _isConnectingForPlay = false;
                StartQuickPlay();
            }
        }

        private void OnLobbyJoinFailed(string region)
        {
            Debug.LogWarning($"[UIFortniteLobbyView] Failed to join lobby in region: {region}");
            _isConnectingForPlay = false;
            if (_playButtonText != null) _playButtonText.text = "PLAY";
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
            if (questView != null) questView.BackView = this;
        }

        private void OnLockerButtonClicked()
        {
            Debug.Log("[UIFortniteLobbyView] Locker button clicked");
            var lockerView = Open<UIAgentSelectionView>();
            if (lockerView != null) lockerView.BackView = this;
        }

        private void OnBattlePassButtonClicked()
        {
            Debug.Log("[UIFortniteLobbyView] Battle Pass button clicked");
            if (_battlePassCanvas != null) _battlePassCanvas.SetActive(true);
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

            _isConnectingForPlay = false;
            _isSearchingForGame = false;
            _availableSessions.Clear();

            if (_playButtonText != null) _playButtonText.text = "PLAY";
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
            _currentGamemodeIndex = (_currentGamemodeIndex + 1) % _availableGamemodes.Length;
            _gameplayType = _availableGamemodes[_currentGamemodeIndex];
            _maxPlayers = _gameplayType == EGameplayType.Manhunt ? 10 : 100;
            UpdateGamemodeDisplay();
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
                case EGameplayType.BattleRoyale: return "BATTLE ROYALE - SOLO";
                case EGameplayType.Manhunt: return "LTM - MANHUNT";
                default: return "SELECT GAMEMODE";
            }
        }
    }
}
