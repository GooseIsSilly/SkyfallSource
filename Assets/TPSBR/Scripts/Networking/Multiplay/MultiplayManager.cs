using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Services.Matchmaker.Models;
using UnityEngine;

namespace TPSBR
{
	/// <summary>
	/// Unity 6 Implementation of MultiplayManager using Unity.Services.Multiplayer.
	/// This handles the server-side lifecycle for dedicated game server hosting.
	/// </summary>
	public sealed class MultiplayManager : MonoBehaviour
	{
		// PUBLIC MEMBERS

		public bool IsAllocated { get; private set; }
		public int  MaxPlayers;

		// The following members are kept for compatibility with Backfill.cs
		public bool Backfill; 
		public StoredMatchmakingResults MatchmakingResults { get; private set; }

		// PRIVATE MEMBERS

		private SessionRequest _sessionRequest;

		// PUBLIC METHODS

		public async void StartMultiplay(SessionRequest sessionRequest, StandaloneConfiguration configuration)
		{
			_sessionRequest = sessionRequest;
			MaxPlayers = configuration.MaxPlayers;
			Backfill = configuration.Backfill;

			Debug.Log("[MultiplayManager] Initializing Unity 6 Server Service...");

			try 
			{
				if (UnityServices.State == ServicesInitializationState.Uninitialized)
				{
					await UnityServices.InitializeAsync();
				}

				// In Unity 6, when a dedicated server is allocated, a Session is automatically added.
				MultiplayerService.Instance.SessionAdded += OnSessionAdded;
				
				Debug.Log("[MultiplayManager] Server is online and waiting for Matchmaker allocation...");
			}
			catch (Exception e)
			{
				Debug.LogError($"[MultiplayManager] Initialization failed: {e.Message}");
			}
		}

		// PRIVATE METHODS

		private async void OnSessionAdded(ISession session)
		{
			if (IsAllocated) return;
			IsAllocated = true;

			Debug.Log($"[MultiplayManager] Match Allocated! Session ID: {session.Id}");

			try 
			{
				// Retrieve the matchmaker data associated with this session
				MatchmakingResults = session.GetMatchmakingResults();
				
				if (MatchmakingResults != null)
				{
					Debug.Log($"[MultiplayManager] Match Found: {MatchmakingResults.MatchId}");
					_sessionRequest.SessionName = "mm-" + MatchmakingResults.MatchId;
				}
				else 
				{
					Debug.LogWarning("[MultiplayManager] No matchmaking payload found on session. Using session ID as name.");
					_sessionRequest.SessionName = "mm-" + session.Id;
				}

				// Start the Fusion game session as a Server
				_sessionRequest.GameMode = Fusion.GameMode.Server;
				Global.Networking.StartGame(_sessionRequest);

				// Wait for connection to be established
				while (!Global.Networking.IsConnected)
				{
					await Task.Delay(500);
				}

				Debug.Log("[MultiplayManager] Dedicated Server session started and ready for players.");
			}
			catch (Exception e)
			{
				Debug.LogError($"[MultiplayManager] Failed to start game from session: {e.Message}");
			}
		}

		private void OnDestroy()
		{
			if (MultiplayerService.Instance != null)
			{
				MultiplayerService.Instance.SessionAdded -= OnSessionAdded;
			}
		}
	}
}
