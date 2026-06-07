using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Fusion;
using TPSBR.Backend;

namespace TPSBR
{
    /// <summary>
    /// Handles requesting a dedicated server from the backend and connecting as a client.
    /// </summary>
    public class DedicatedMatchmaker : ContextBehaviour
    {
        [Serializable]
        private class ServerRequestResponse
        {
            public string log;
            public string allocationId;
        }

        [Serializable]
        private class ServerStatusResponse
        {
            public string log;
            public string status; // "pending", "ready", etc.
            public string serverIp;
            public int serverPort;
        }

        public void StartMatchmaking()
        {
            if (BackendServiceManager.Instance == null || !BackendServiceManager.Instance.IsLoggedIn)
            {
                Debug.LogError("[DedicatedMatchmaker] Must be logged in to request a server.");
                return;
            }

            StartCoroutine(MatchmakingFlowCoroutine());
        }

        private IEnumerator MatchmakingFlowCoroutine()
        {
            string token = BackendServiceManager.Instance.GetStoredToken();
            string baseUrl = BackendServiceManager.Instance.BackendURL;

            // 1. Request Server
            Debug.Log("[DedicatedMatchmaker] Requesting server...");
            string requestUrl = $"{baseUrl}/server/request?token={UnityWebRequest.EscapeURL(token)}";
            
            string allocationId = "";
            using (UnityWebRequest request = UnityWebRequest.PostWwwForm(requestUrl, ""))
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[DedicatedMatchmaker] Request failed: {request.error}");
                    yield break;
                }

                var response = JsonUtility.FromJson<ServerRequestResponse>(request.downloadHandler.text);
                if (response.log != "ok")
                {
                    Debug.LogError($"[DedicatedMatchmaker] Backend error: {response.log}");
                    yield break;
                }

                allocationId = response.allocationId;
                Debug.Log($"[DedicatedMatchmaker] Allocated! ID: {allocationId}");
            }

            // 2. Poll Status
            string status = "pending";
            string serverIp = "";
            int serverPort = 0;

            while (status != "ready")
            {
                yield return new WaitForSeconds(2f);

                string statusUrl = $"{baseUrl}/server/status?allocationId={UnityWebRequest.EscapeURL(allocationId)}";
                using (UnityWebRequest request = UnityWebRequest.Get(statusUrl))
                {
                    yield return request.SendWebRequest();

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning($"[DedicatedMatchmaker] Status check failed: {request.error}");
                        continue;
                    }

                    var response = JsonUtility.FromJson<ServerStatusResponse>(request.downloadHandler.text);
                    status = response.status;
                    
                    if (status == "ready")
                    {
                        serverIp = response.serverIp;
                        serverPort = response.serverPort;
                    }
                }
            }

            Debug.Log($"[DedicatedMatchmaker] Server ready at {serverIp}:{serverPort}. Connecting...");

            // 3. Connect to Server
            ConnectToServer(serverIp, (ushort)serverPort);
        }

        private void ConnectToServer(string ip, ushort port)
        {
            var request = new SessionRequest
            {
                UserID = Context.PlayerData.UserID,
                GameMode = GameMode.Client,
                DisplayName = Context.PlayerData.Nickname,
                ScenePath = "Assets/TPSBR/Scenes/Game.unity", // Default map
                IPAddress = ip,
                Port = port
            };

            if (Global.Networking != null)
            {
                Global.Networking.StartGame(request);
            }
            else
            {
                Debug.LogError("[DedicatedMatchmaker] Networking service not found in Global.");
            }
        }
    }
}
