using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Fusion;
using TPSBR.Backend;

namespace TPSBR
{
    public class DedicatedMatchmaker : SceneService
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
            public string status;
            public string sessionName; // Fusion session name — client joins via Photon normally
            public string map;
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

            // 1. Request a server
            Debug.Log("[DedicatedMatchmaker] Requesting server...");
            string requestUrl = $"{baseUrl}/server/request";
            string jsonBody = "{\"Token\":\"" + token + "\"}";

            string allocationId = "";
            using (UnityWebRequest request = new UnityWebRequest(requestUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

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

            // 2. Poll for session name
            string status = "pending";
            string sessionName = "";

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
                        sessionName = response.sessionName;
                    }
                }
            }

            Debug.Log($"[DedicatedMatchmaker] Server ready. Fusion session: '{sessionName}'. Connecting...");

            // 3. Connect via Photon using the session name — no direct IP needed
            ConnectToServer(sessionName);
        }

        private void ConnectToServer(string sessionName)
        {
            var request = new SessionRequest
            {
                UserID = Context.PlayerData.UserID,
                GameMode = GameMode.Client,
                DisplayName = Context.PlayerData.Nickname,
                ScenePath = "Assets/TPSBR/Scenes/Game.unity",
                SessionName = sessionName,
                CustomLobby = "FusionBR." + Application.version,
                // No IPAddress or Port — let Fusion find the session via Photon
            };

            if (Global.Networking != null)
            {
                Debug.Log($"[DedicatedMatchmaker] Joining Fusion session: '{sessionName}'");
                Global.Networking.StartGame(request);
            }
            else
            {
                Debug.LogError("[DedicatedMatchmaker] Networking service not found in Global.");
            }
        }
    }
}
