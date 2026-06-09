using Fusion;
using Fusion.Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Networking;
using TPSBR.Backend;

namespace TPSBR
{
	using System;

	[Serializable]
	public sealed class StandaloneConfiguration
	{
		public EGameplayType GameplayType;
		public GameMode      GameMode;
		public string        ServerName;
		public int           MaxPlayers;
		public int           ExtraPeers;
		public string        Region;
		public string        SessionName;
		public string        CustomLobby;
		public string        IPAddress;
		public ushort        Port;
		public bool          Multiplay;
		public bool			 QueryProtocol;
		public bool			 Matchmaking;
		public bool			 Backfill;
	}

	public class StandaloneManager : MonoBehaviour
	{
		// PUBLIC MEMBERS

		public static StandaloneConfiguration ExternalConfiguration;

		// PRIVATE MEMBERS

		[SerializeField]
		private StandaloneConfiguration _defaultConfiguration;

		// MONOBEHAVIOUR

		protected void Awake()
		{
			if (Global.Networking.HasSession == true)
			{
				Destroy(gameObject);
			}
		}

		protected void Start()
		{
			StandaloneConfiguration configuration = ExternalConfiguration ?? _defaultConfiguration;

			var playerData = Global.PlayerService.PlayerData;
			var scenePath = SceneManager.GetActiveScene().path;

			scenePath = scenePath.Substring("Assets/".Length, scenePath.Length - "Assets/".Length - ".unity".Length);

			PhotonAppSettings.Global.AppSettings.FixedRegion = configuration.Region;

			var request = new SessionRequest
			{
				UserID       = playerData.UserID.HasValue() ? playerData.UserID : new Guid().ToString(),
				GameMode     = configuration.GameMode,
				SessionName  = configuration.SessionName.HasValue() ? configuration.SessionName : Guid.NewGuid().ToString(),
				DisplayName  = configuration.ServerName,
				ScenePath    = scenePath,
				GameplayType = configuration.GameplayType,
				ExtraPeers   = configuration.ExtraPeers,
				MaxPlayers   = configuration.MaxPlayers,
				CustomLobby  = configuration.CustomLobby.HasValue() ? configuration.CustomLobby : "FusionBR." + Application.version,
				IPAddress    = configuration.IPAddress,
				Port         = configuration.Port,
			};

			if (configuration.Multiplay)
			{
				// A Multiplay allocation will trigger the game session creation
				Global.MultiplayManager.StartMultiplay(request, configuration);
			}
			else
			{
				Global.Networking.StartGame(request);
				// Link this Fusion session with the backend server ID
				StartCoroutine(RegisterWithBackend(request.SessionName));
			}
		}

		private IEnumerator RegisterWithBackend(string sessionName)
		{
			if (BackendServiceManager.Instance == null) yield break;

			string serverKey = ApplicationSettings.HasServerKey ? ApplicationSettings.ServerKey : "changeme_server_secret";
			string serverId  = ApplicationSettings.HasSessionName ? ApplicationSettings.SessionName : sessionName;
			string url       = $"{BackendServiceManager.Instance.BackendURL}/server/session";

			string json = $"{{\"serverId\":\"{serverId}\",\"serverKey\":\"{serverKey}\",\"sessionName\":\"{sessionName}\"}}";

			using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
			{
				byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
				request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
				request.downloadHandler = new DownloadHandlerBuffer();
				request.SetRequestHeader("Content-Type", "application/json");

				yield return request.SendWebRequest();

				if (request.result != UnityWebRequest.Result.Success)
					Debug.LogError($"[StandaloneManager] Session registration failed: {request.error}");
				else
					Debug.Log($"[StandaloneManager] Session registered: {sessionName}");
			}
		}
	}
}
