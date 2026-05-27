using UnityEngine;
using TPSBR.UI;
using System.Reflection;
using System.Collections.Generic;
using Fusion;

namespace TPSBR
{
	[RequireComponent(typeof(UIButton))]
	public class UIQuickMatchButton : MonoBehaviour
	{
		private UIButton _button;

		private void Awake()
		{
			_button = GetComponent<UIButton>();
			_button.onClick.AddListener(OnButtonClick);
		}

		private void OnDestroy()
		{
			if (_button != null)
			{
				_button.onClick.RemoveListener(OnButtonClick);
			}
		}

		private void OnButtonClick()
		{
			var multiplayerView = FindObjectOfType<UIMultiplayerView>();
			if (multiplayerView == null)
			{
				Debug.LogWarning("Quick Match: UIMultiplayerView not found in scene.");
				return;
			}

			var context = GetContext(multiplayerView);
			if (context == null || context.Matchmaking == null)
			{
				Debug.LogWarning("Quick Match: Context or Matchmaking service not available yet.");
				return;
			}

			if (context.Matchmaking.IsConnectedToLobby == false)
			{
				Debug.LogWarning("Quick Match: Not connected to lobby. Please wait for connection.");
				return;
			}

			List<SessionInfo> sessionList = GetSessionListFromView(multiplayerView);
			
			if (sessionList == null || sessionList.Count == 0)
			{
				Debug.Log("Quick Match: No sessions available, opening create session view");
				OpenCreateSessionView();
				return;
			}

			SessionInfo bestSession = FindBestAvailableSession(sessionList);

			if (bestSession != null)
			{
				Debug.Log($"Quick Match: Joining session {bestSession.GetDisplayName()} with {bestSession.PlayerCount} players");
				context.Matchmaking.JoinSession(bestSession);
			}
			else
			{
				Debug.Log("Quick Match: No joinable sessions found, opening create session view");
				OpenCreateSessionView();
			}
		}

		private SceneContext GetContext(UIMultiplayerView view)
		{
			var contextProperty = typeof(UIMultiplayerView).BaseType.GetProperty("Context", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			if (contextProperty != null)
			{
				return contextProperty.GetValue(view) as SceneContext;
			}
			return null;
		}

		private List<SessionInfo> GetSessionListFromView(UIMultiplayerView view)
		{
			var fieldInfo = typeof(UIMultiplayerView).GetField("_sessionInfo", BindingFlags.NonPublic | BindingFlags.Instance);
			if (fieldInfo != null)
			{
				return fieldInfo.GetValue(view) as List<SessionInfo>;
			}
			return null;
		}

		private void OpenCreateSessionView()
		{
			var multiplayerView = FindObjectOfType<UIMultiplayerView>();
			if (multiplayerView != null)
			{
				var createMethod = typeof(UIMultiplayerView).GetMethod("OnCreateGameButton", BindingFlags.NonPublic | BindingFlags.Instance);
				if (createMethod != null)
				{
					createMethod.Invoke(multiplayerView, null);
				}
				else
				{
					Debug.LogWarning("Could not find OnCreateGameButton method on UIMultiplayerView.");
				}
			}
			else
			{
				Debug.LogWarning("UIMultiplayerView not found in scene.");
			}
		}

		private SessionInfo FindBestAvailableSession(List<SessionInfo> sessionList)
		{
			SessionInfo bestSession = null;
			int highestPlayerCount = -1;

			foreach (var session in sessionList)
			{
				if (CanJoinSession(session) == false)
					continue;

				if (session.PlayerCount > highestPlayerCount)
				{
					highestPlayerCount = session.PlayerCount;
					bestSession = session;
				}
			}

			return bestSession;
		}

		private bool CanJoinSession(SessionInfo session)
		{
			if (session == null)
				return false;

			if (session.IsValid == false)
				return false;

			if (session.PlayerCount >= session.MaxPlayers)
				return false;

			if (session.IsOpen == false || session.IsVisible == false)
				return false;

			if (session.HasMap() == false)
				return false;

			return true;
		}
	}
}
