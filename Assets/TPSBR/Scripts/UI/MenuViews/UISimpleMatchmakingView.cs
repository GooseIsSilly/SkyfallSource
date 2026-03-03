using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Fusion;
using Fusion.Photon.Realtime;
using System.Linq;

namespace TPSBR.UI
{
	public class UISimpleMatchmakingView : UICloseView
	{
		[Header("UI References")]
		[SerializeField] private TMP_Dropdown _gamemodeDropdown;
		[SerializeField] private UIButton _playButton;
		[SerializeField] private TextMeshProUGUI _statusText;
		[SerializeField] private UIBehaviour _loadingIndicator;

		[Header("Matchmaking Settings")]
		[SerializeField] private int _maxPlayers = 20;
		[SerializeField] private bool _dedicatedServer = false;

		private List<MapSetup> _availableMaps = new List<MapSetup>();
		private EGameplayType _selectedGamemode = EGameplayType.BattleRoyale;
		private bool _isSearching = false;

		protected override void OnInitialize()
		{
			base.OnInitialize();

			_playButton.onClick.AddListener(OnPlayButtonClicked);
			_gamemodeDropdown.onValueChanged.AddListener(OnGamemodeChanged);

			PrepareGamemodeDropdown();
			PrepareMapData();
		}

		protected override void OnDeinitialize()
		{
			_playButton.onClick.RemoveListener(OnPlayButtonClicked);
			_gamemodeDropdown.onValueChanged.RemoveListener(OnGamemodeChanged);

			base.OnDeinitialize();
		}

		protected override void OnOpen()
		{
			base.OnOpen();

			Context.Matchmaking.SessionListUpdated += OnSessionListUpdated;
			Context.Matchmaking.LobbyJoined += OnLobbyJoined;
			Context.Matchmaking.LobbyJoinFailed += OnLobbyJoinFailed;

			SetStatus("Connecting to lobby...");
			SetLoading(true);

			if (PhotonAppSettings.Global.AppSettings.AppIdFusion.HasValue())
			{
				Context.Matchmaking.JoinLobby(true);
			}
			else
			{
				SetStatus("Error: No Photon App ID configured");
				SetLoading(false);
				_playButton.interactable = false;
			}
		}

		protected override void OnClose()
		{
			Context.Matchmaking.SessionListUpdated -= OnSessionListUpdated;
			Context.Matchmaking.LobbyJoined -= OnLobbyJoined;
			Context.Matchmaking.LobbyJoinFailed -= OnLobbyJoinFailed;

			Context.Matchmaking.LeaveLobby();

			base.OnClose();
		}

		protected override void OnTick()
		{
			base.OnTick();

			if (_isSearching == false && Context.Matchmaking.IsConnectedToLobby)
			{
				_playButton.interactable = true;
				SetLoading(false);
				SetStatus("Ready to play");
			}
		}

		private void PrepareGamemodeDropdown()
		{
			var options = new List<TMP_Dropdown.OptionData>();

			options.Add(new TMP_Dropdown.OptionData("Battle Royale"));

			_gamemodeDropdown.ClearOptions();
			_gamemodeDropdown.AddOptions(options);
			_gamemodeDropdown.SetValueWithoutNotify(0);

			_selectedGamemode = EGameplayType.BattleRoyale;
		}

		private void PrepareMapData()
		{
			_availableMaps.Clear();

			var mapSettings = Context.Settings.Map;
			if (mapSettings == null || mapSettings.Maps == null || mapSettings.Maps.Length == 0)
			{
				Debug.LogWarning("No maps configured in Settings");
				return;
			}

			foreach (var mapSetup in mapSettings.Maps)
			{
				if (mapSetup != null && mapSetup.ShowInMapSelection == true)
				{
					_availableMaps.Add(mapSetup);
				}
			}

			if (_availableMaps.Count == 0)
			{
				Debug.LogWarning("No valid maps found");
			}
		}

		private void OnGamemodeChanged(int index)
		{
			switch (index)
			{
				case 0:
					_selectedGamemode = EGameplayType.BattleRoyale;
					break;
				default:
					_selectedGamemode = EGameplayType.BattleRoyale;
					break;
			}
		}

		private void OnPlayButtonClicked()
		{
			if (_isSearching)
				return;

			if (Context == null || Context.Matchmaking == null)
			{
				SetStatus("Error: Matchmaking service not available");
				return;
			}

			if (Context.Matchmaking.IsConnectedToLobby == false)
			{
				SetStatus("Not connected to lobby. Reconnecting...");
				Context.Matchmaking.JoinLobby(true);
				return;
			}

			_isSearching = true;
			_playButton.interactable = false;
			SetLoading(true);
			SetStatus("Searching for available sessions...");

			StartMatchmaking();
		}

		private void StartMatchmaking()
		{
			Context.Matchmaking.SessionListUpdated += OnMatchmakingSessionListUpdated;
		}

		private void OnMatchmakingSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
		{
			Context.Matchmaking.SessionListUpdated -= OnMatchmakingSessionListUpdated;

			if (_isSearching == false)
				return;

			var availableSessions = sessionList
				.Where(s => s.IsValid && s.IsOpen && s.IsVisible)
				.Where(s => s.GetGameplayType() == _selectedGamemode)
				.Where(s => s.PlayerCount < s.MaxPlayers)
				.Where(s => s.HasMap())
				.OrderByDescending(s => s.PlayerCount)
				.ToList();

			if (availableSessions.Count > 0)
			{
				var bestSession = availableSessions[0];
				SetStatus($"Joining session with {bestSession.PlayerCount} players...");
				Context.Matchmaking.JoinSession(bestSession);
			}
			else
			{
				SetStatus("No sessions found. Creating new session...");
				CreateNewSession();
			}
		}

		private void CreateNewSession()
		{
			if (_availableMaps.Count == 0)
			{
				SetStatus("Error: No maps available");
				_isSearching = false;
				_playButton.interactable = true;
				SetLoading(false);
				return;
			}

			var randomMap = _availableMaps[Random.Range(0, _availableMaps.Count)];

			string sessionName = $"Game_{Random.Range(1000, 9999)}";

			var request = new SessionRequest
			{
				GameMode = _dedicatedServer ? GameMode.Server : GameMode.Host,
				DisplayName = Context.PlayerData.Nickname,
				SessionName = sessionName,
				ScenePath = randomMap.ScenePath,
				GameplayType = _selectedGamemode,
				MaxPlayers = _maxPlayers,
			};

			SetStatus($"Creating session on {randomMap.DisplayName}...");
			Global.Networking.StartGame(request);
		}

		private void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
		{
		}

		private void OnLobbyJoined()
		{
			SetStatus("Connected to lobby");
			SetLoading(false);
			_playButton.interactable = true;
		}

		private void OnLobbyJoinFailed(string region)
		{
			var regionInfo = Context.Settings.Network.GetRegionInfo(region);
			var regionText = regionInfo != null ? $"{regionInfo.DisplayName} ({regionInfo.Region})" : "Unknown";
			
			SetStatus($"Failed to join lobby in region {regionText}");
			SetLoading(false);
			_playButton.interactable = false;
		}

		private void SetStatus(string message)
		{
			if (_statusText != null)
			{
				_statusText.text = message;
			}
			Debug.Log($"[Simple Matchmaking] {message}");
		}

		private void SetLoading(bool isLoading)
		{
			if (_loadingIndicator != null)
			{
				_loadingIndicator.SetActive(isLoading);
			}
		}
	}
}
