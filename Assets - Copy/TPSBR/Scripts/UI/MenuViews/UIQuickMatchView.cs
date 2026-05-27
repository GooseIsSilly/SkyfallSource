using Fusion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

#pragma warning disable 4014

namespace TPSBR.UI
{
	public class UIQuickMatchView : UIView
	{
		[SerializeField] private TextMeshProUGUI _statusText;
		[SerializeField] private UIButton _cancelButton;
		[SerializeField] private float _searchTimeout = 5f;
		[SerializeField] private EGameplayType _gameplayType = EGameplayType.BattleRoyale;
		[SerializeField] private int _maxPlayers = 100;
		[SerializeField] private string _defaultMapScenePath = "TPSBR/Scenes/Game";

		private List<SessionInfo> _availableSessions = new List<SessionInfo>();
		private float _searchStartTime;
		private bool _isSearching;
		private bool _hasJoinedLobby;
		private bool _isCancelled;

		protected override void OnInitialize()
		{
			base.OnInitialize();
			
			_cancelButton.onClick.AddListener(OnCancelButton);
		}

		protected override void OnDeinitialize()
		{
			_cancelButton.onClick.RemoveListener(OnCancelButton);
			base.OnDeinitialize();
		}

		protected override void OnOpen()
		{
			base.OnOpen();

			Context.Matchmaking.SessionListUpdated += OnSessionListUpdated;
			Context.Matchmaking.LobbyJoined += OnLobbyJoined;
			Context.Matchmaking.LobbyJoinFailed += OnLobbyJoinFailed;

			_isCancelled = false;
			_hasJoinedLobby = false;
			_isSearching = false;
			_availableSessions.Clear();

			UpdateStatus("Connecting to lobby...");

			Context.Matchmaking.JoinLobby(true);
		}

		protected override void OnClose()
		{
			Context.Matchmaking.SessionListUpdated -= OnSessionListUpdated;
			Context.Matchmaking.LobbyJoined -= OnLobbyJoined;
			Context.Matchmaking.LobbyJoinFailed -= OnLobbyJoinFailed;

			base.OnClose();
		}

		protected override void OnTick()
		{
			base.OnTick();

			if (_isCancelled)
				return;

			if (_isSearching)
			{
				float elapsedTime = Time.realtimeSinceStartup - _searchStartTime;

				if (elapsedTime >= _searchTimeout)
				{
					_isSearching = false;
					CreateNewSession();
				}
				else
				{
					UpdateStatus($"Searching for session... ({(_searchTimeout - elapsedTime):F0}s)");
				}
			}
		}

		private void OnLobbyJoined()
		{
			_hasJoinedLobby = true;
			StartSearching();
		}

		private void OnLobbyJoinFailed(string region)
		{
			UpdateStatus($"Failed to connect to lobby in {region}");
			Close();
		}

		private void StartSearching()
		{
			UpdateStatus("Searching for available sessions...");
			_isSearching = true;
			_searchStartTime = Time.realtimeSinceStartup;
		}

		private void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
		{
			if (!_isSearching || _isCancelled)
				return;

			_availableSessions.Clear();

			foreach (var session in sessionList)
			{
				if (session.IsValid == false || session.IsOpen == false || session.IsVisible == false)
					continue;

				if (session.PlayerCount >= session.MaxPlayers)
					continue;

				if (session.HasMap() == false)
					continue;

				var sessionGameplayType = session.GetGameplayType();
				if (sessionGameplayType != _gameplayType)
					continue;

				_availableSessions.Add(session);
			}

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

			UpdateStatus($"Joining session: {bestSession.GetDisplayName()}...");
			
			_isSearching = false;
			Context.Matchmaking.JoinSession(bestSession);
			Close();
		}

		private void CreateNewSession()
		{
			UpdateStatus("No sessions found. Creating new session...");

			var mapSetup = GetDefaultMapSetup();
			if (mapSetup == null)
			{
				UpdateStatus("Error: No map configuration found");
				return;
			}

			var request = new SessionRequest
			{
				UserID = Context.PlayerData.UserID,
				GameMode = GameMode.Host,
				DisplayName = $"{Context.PlayerData.Nickname}'s Game",
				SessionName = null,
				ScenePath = mapSetup.ScenePath,
				GameplayType = _gameplayType,
				MaxPlayers = _maxPlayers,
				ExtraPeers = 0,
			};

			Context.Matchmaking.CreateSession(request);
			Close();
		}

		private MapSetup GetDefaultMapSetup()
		{
			var mapSettings = Global.Settings.Map;
			
			if (mapSettings.Maps != null && mapSettings.Maps.Length > 0)
			{
				var mapWithPath = mapSettings.Maps.FirstOrDefault(m => m.ScenePath == _defaultMapScenePath);
				if (mapWithPath != null)
					return mapWithPath;

				return mapSettings.Maps[0];
			}

			return null;
		}

		private void OnCancelButton()
		{
			_isCancelled = true;
			_isSearching = false;
			UpdateStatus("Cancelled");
			Close();
		}

		private void UpdateStatus(string status)
		{
			if (_statusText != null)
			{
				_statusText.text = status;
			}
		}
	}
}
