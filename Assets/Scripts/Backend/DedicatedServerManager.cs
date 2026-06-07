using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TPSBR.Backend;

namespace TPSBR
{
    /// <summary>
    /// Handles dedicated server specific logic: command line arguments and backend notification.
    /// </summary>
    public class DedicatedServerManager : ContextBehaviour
    {
        public static DedicatedServerManager Instance { get; private set; }

        [Header("Server Configuration")]
        [SerializeField] private int _port = 7777;
        [SerializeField] private string _map = "Game";
        [SerializeField] private string _serverId = "";
        [SerializeField] private string _serverKey = "";

        public int Port => _port;
        public string Map => _map;
        public string ServerId => _serverId;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ParseCommandLineArguments();
        }

        private void ParseCommandLineArguments()
        {
            // Read -port
            if (ApplicationUtility.GetCommandLineArgument("-port", out int cliPort))
            {
                _port = cliPort;
                DedicatedServerSettings.Port = (ushort)cliPort;
                Debug.Log($"[DedicatedServerManager] Port set from command line: {_port}");
            }

            // Read -map
            if (ApplicationUtility.GetCommandLineArgument("-map", out string cliMap))
            {
                _map = cliMap;
                Debug.Log($"[DedicatedServerManager] Map set from command line: {_map}");
            }

            // Read -serverId (needed for match complete notification)
            if (ApplicationUtility.GetCommandLineArgument("-serverId", out string cliServerId))
            {
                _serverId = cliServerId;
                Debug.Log($"[DedicatedServerManager] ServerID set from command line: {_serverId}");
            }

            // Read -serverKey (security key for backend)
            if (ApplicationUtility.GetCommandLineArgument("-serverKey", out string cliServerKey))
            {
                _serverKey = cliServerKey;
                Debug.Log("[DedicatedServerManager] ServerKey set from command line.");
            }
        }

        private void Update()
        {
            if (Runner != null && Runner.IsServer)
            {
                CheckAlivePlayers();
            }
        }

        private void CheckAlivePlayers()
        {
            if (Context.GameplayMode == null || Context.GameplayMode.State != GameplayMode.EState.Active)
                return;

            int aliveCount = 0;
            foreach (var player in Context.NetworkGame.ActivePlayers)
            {
                if (player.Statistics.IsAlive)
                {
                    aliveCount++;
                }
            }

            // When only 1 player is left (the winner), notify backend
            if (aliveCount <= 1 && Context.NetworkGame.ActivePlayers.Count > 1)
            {
                Debug.Log($"[DedicatedServerManager] Match over! Alive players: {aliveCount}. Notifying backend.");
                NotifyMatchComplete();
                
                // Set state to finished to avoid multiple notifications
                // The actual win logic might be in GameplayMode, we just hook here for backend
            }
        }

        /// <summary>
        /// Notifies the backend that the match has ended and the server is available.
        /// </summary>
        public void NotifyMatchComplete()
        {
            if (string.IsNullOrEmpty(_serverId) || string.IsNullOrEmpty(_serverKey))
            {
                Debug.LogWarning("[DedicatedServerManager] Cannot notify match complete: serverId or serverKey is missing.");
                return;
            }

            StartCoroutine(NotifyMatchCompleteCoroutine());
        }

        private IEnumerator NotifyMatchCompleteCoroutine()
        {
            string baseUrl = "http://198.50.250.196:5000";
            if (BackendServiceManager.Instance != null)
            {
                baseUrl = BackendServiceManager.Instance.BackendURL;
            }

            string url = $"{baseUrl}/server/match/complete";
            
            WWWForm form = new WWWForm();
            form.AddField("serverId", _serverId);
            form.AddField("serverKey", _serverKey);

            Debug.Log($"[DedicatedServerManager] Sending POST to {url} with serverId: {_serverId}");

            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                request.timeout = 10;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[DedicatedServerManager] Backend notified of match completion successfully.");
                }
                else
                {
                    Debug.LogError($"[DedicatedServerManager] Failed to notify backend: {request.error}");
                }
            }
        }
    }
}
