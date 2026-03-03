using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace TPSBR.Backend
{
    public class BackendServiceManager : MonoBehaviour
    {
        public static BackendServiceManager Instance { get; private set; }

        [Header("Backend Configuration")]
        [SerializeField] private string backendURL = "http://localhost:8000";
        [SerializeField] private float apiTimeout = 10f;
        [SerializeField] private int retryAttempts = 3;

        public event Action<string> OnLoginSuccess;
        public event Action<string> OnLoginFailed;
        public event Action<string> OnAccountCreated;
        public event Action<string> OnAccountCreationFailed;
        public event Action OnLogoutSuccess;
        public event Action<string> OnBanDetected;

        private const string TOKEN_KEY = "AuthToken";
        private const string USERNAME_KEY = "Username";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool HasStoredToken()
        {
            return PlayerPrefs.HasKey(TOKEN_KEY);
        }

        public string GetStoredToken()
        {
            return PlayerPrefs.GetString(TOKEN_KEY, "");
        }

        public string GetStoredUsername()
        {
            return PlayerPrefs.GetString(USERNAME_KEY, "");
        }

        public void CreateAccount(string username, string password, Action<bool, string> callback)
        {
            StartCoroutine(CreateAccountCoroutine(username, password, callback));
        }

        public void LoginPlayer(string username, string password, Action<bool, string> callback)
        {
            StartCoroutine(LoginCoroutine(username, password, callback));
        }

        public void ValidateToken(Action<bool, string> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                callback?.Invoke(false, "No token stored");
                return;
            }

            StartCoroutine(ValidateTokenCoroutine(token, callback));
        }

        public void Logout(Action callback = null)
        {
            string token = GetStoredToken();
            if (!string.IsNullOrEmpty(token))
            {
                StartCoroutine(LogoutCoroutine(token, callback));
            }
            else
            {
                ClearStoredCredentials();
                callback?.Invoke();
            }
        }

        public void CheckBanStatus(Action<bool, string, string> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                callback?.Invoke(false, "", "");
                return;
            }

            StartCoroutine(CheckBanStatusCoroutine(token, callback));
        }

        public void SubmitReport(string reportedPlayer, string reason, string description, Action<bool, string> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                callback?.Invoke(false, "Not logged in");
                return;
            }

            StartCoroutine(SubmitReportCoroutine(token, reportedPlayer, reason, description, callback));
        }

        public IEnumerator GetPlayerGameData(Action<bool, PlayerGameData> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                callback?.Invoke(false, null);
                yield break;
            }

            string url = $"{backendURL}/account/getgamedata?Token={UnityWebRequest.EscapeURL(token)}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    var response = JsonUtility.FromJson<GameDataResponse>(responseText);

                    if (response.log == "ok")
                    {
                        var data = new PlayerGameData
                        {
                            Level = response.Level,
                            XP = response.XP,
                            CloudCoins = response.CloudCoins,
                            OwnedSkins = response.OwnedSkins ?? new string[0],
                            EquippedSkin = response.EquippedSkin,
                            HasBattlePass = response.HasBattlePass
                        };
                        callback?.Invoke(true, data);
                    }
                    else
                    {
                        callback?.Invoke(false, null);
                    }
                }
                else
                {
                    callback?.Invoke(false, null);
                }
            }
        }

        public IEnumerator UpdatePlayerGameData(PlayerGameData data, Action<bool> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                callback?.Invoke(false);
                yield break;
            }

            string url = $"{backendURL}/account/updategamedata";

            var updateData = new GameDataUpdateRequest
            {
                Token = token,
                Level = data.Level,
                XP = data.XP,
                CloudCoins = data.CloudCoins,
                OwnedSkins = data.OwnedSkins,
                EquippedSkin = data.EquippedSkin,
                HasBattlePass = data.HasBattlePass
            };

            string jsonData = JsonUtility.ToJson(updateData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)apiTimeout;

                yield return request.SendWebRequest();

                callback?.Invoke(request.result == UnityWebRequest.Result.Success);
            }
        }

        public IEnumerator AddCoins(int amount, Action<bool, int> callback)
        {
            string token = GetStoredToken();
            string url = $"{backendURL}/account/addcoins?Token={UnityWebRequest.EscapeURL(token)}&Amount={amount}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<AddCoinsResponse>(request.downloadHandler.text);
                    if (response.log == "ok")
                    {
                        callback?.Invoke(true, response.NewTotal);
                    }
                    else
                    {
                        callback?.Invoke(false, 0);
                    }
                }
                else
                {
                    callback?.Invoke(false, 0);
                }
            }
        }

        public IEnumerator AddXP(int amount, Action<bool, int, int> callback)
        {
            string token = GetStoredToken();
            string url = $"{backendURL}/account/addxp?Token={UnityWebRequest.EscapeURL(token)}&Amount={amount}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<AddXPResponse>(request.downloadHandler.text);
                    if (response.log == "ok")
                    {
                        callback?.Invoke(true, response.Level, response.XP);
                    }
                    else
                    {
                        callback?.Invoke(false, 0, 0);
                    }
                }
                else
                {
                    callback?.Invoke(false, 0, 0);
                }
            }
        }

        public IEnumerator UnlockSkin(string skinID, Action<bool, string> callback)
        {
            string token = GetStoredToken();
            string url = $"{backendURL}/account/unlockskin?Token={UnityWebRequest.EscapeURL(token)}&SkinID={UnityWebRequest.EscapeURL(skinID)}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<UnlockSkinResponse>(request.downloadHandler.text);
                    if (response.log == "ok")
                    {
                        callback?.Invoke(true, response.message);
                    }
                    else
                    {
                        callback?.Invoke(false, response.log);
                    }
                }
                else
                {
                    callback?.Invoke(false, "Network error");
                }
            }
        }

        /// <summary>Fetches the player's current battle pass status from the server.</summary>
        public void GetBattlePassStatus(Action<bool, BattlePassStatusData> callback)
        {
            StartCoroutine(GetBattlePassStatusCoroutine(callback));
        }

        /// <summary>Awards battle pass XP and returns the new tier and XP values.</summary>
        public void AddBattlePassXP(int amount, Action<bool, int, int> callback)
        {
            StartCoroutine(AddBattlePassXPCoroutine(amount, callback));
        }

        /// <summary>Purchases the battle pass for the logged-in player.</summary>
        public void PurchaseBattlePassFromServer(Action<bool, string> callback)
        {
            StartCoroutine(PurchaseBattlePassCoroutine(callback));
        }

        /// <summary>Admin: sets a specific player's battle pass tier.</summary>
        public void AdminSetBattlePassTier(string adminKey, string playerID, int tier, Action<bool> callback)
        {
            StartCoroutine(AdminSetBattlePassTierCoroutine(adminKey, playerID, tier, callback));
        }

        /// <summary>Admin: grants or revokes the battle pass for a specific player.</summary>
        public void AdminSetBattlePassOwnership(string adminKey, string playerID, bool hasPass, Action<bool> callback)
        {
            StartCoroutine(AdminSetBattlePassOwnershipCoroutine(adminKey, playerID, hasPass, callback));
        }

        private IEnumerator GetBattlePassStatusCoroutine(Action<bool, BattlePassStatusData> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                callback?.Invoke(false, null);
                yield break;
            }

            string url = $"{backendURL}/battlepass/getstatus?Token={UnityWebRequest.EscapeURL(token)}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BattlePassStatusResponse>(request.downloadHandler.text);
                    if (response.log == "ok")
                    {
                        callback?.Invoke(true, new BattlePassStatusData
                        {
                            HasBattlePass = response.HasBattlePass,
                            CurrentTier = response.CurrentTier,
                            BattlePassXP = response.BattlePassXP,
                            Season = response.Season
                        });
                    }
                    else
                    {
                        callback?.Invoke(false, null);
                    }
                }
                else
                {
                    callback?.Invoke(false, null);
                }
            }
        }

        private IEnumerator AddBattlePassXPCoroutine(int amount, Action<bool, int, int> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                callback?.Invoke(false, 0, 0);
                yield break;
            }

            string url = $"{backendURL}/battlepass/addxp";
            var payload = new BattlePassXPRequest { Token = token, Amount = amount };
            byte[] body = Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload));

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BattlePassXPResponse>(request.downloadHandler.text);
                    if (response.log == "ok")
                    {
                        callback?.Invoke(true, response.CurrentTier, response.BattlePassXP);
                    }
                    else
                    {
                        callback?.Invoke(false, 0, 0);
                    }
                }
                else
                {
                    callback?.Invoke(false, 0, 0);
                }
            }
        }

        private IEnumerator PurchaseBattlePassCoroutine(Action<bool, string> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                callback?.Invoke(false, "Not logged in");
                yield break;
            }

            string url = $"{backendURL}/battlepass/purchase?Token={UnityWebRequest.EscapeURL(token)}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BattlePassPurchaseResponse>(request.downloadHandler.text);
                    switch (response.log)
                    {
                        case "ok":
                            callback?.Invoke(true, "Battle Pass purchased!");
                            break;
                        case "AlreadyOwned":
                            callback?.Invoke(false, "Battle Pass already owned");
                            break;
                        case "NotEnoughCoins":
                            callback?.Invoke(false, $"Not enough CloudCoins (need {response.Required})");
                            break;
                        default:
                            callback?.Invoke(false, "Purchase failed");
                            break;
                    }
                }
                else
                {
                    callback?.Invoke(false, $"Network error: {request.error}");
                }
            }
        }

        private IEnumerator AdminSetBattlePassTierCoroutine(string adminKey, string playerID, int tier, Action<bool> callback)
        {
            string url = $"{backendURL}/admin/battlepass/settier?AdminKey={UnityWebRequest.EscapeURL(adminKey)}&PlayerID={UnityWebRequest.EscapeURL(playerID)}&Tier={tier}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<SimpleLogResponse>(request.downloadHandler.text);
                    callback?.Invoke(response.log == "ok");
                }
                else
                {
                    callback?.Invoke(false);
                }
            }
        }

        private IEnumerator AdminSetBattlePassOwnershipCoroutine(string adminKey, string playerID, bool hasPass, Action<bool> callback)
        {
            string url = $"{backendURL}/admin/battlepass/setpass?AdminKey={UnityWebRequest.EscapeURL(adminKey)}&PlayerID={UnityWebRequest.EscapeURL(playerID)}&HasPass={hasPass}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<SimpleLogResponse>(request.downloadHandler.text);
                    callback?.Invoke(response.log == "ok");
                }
                else
                {
                    callback?.Invoke(false);
                }
            }
        }

        private void ClearStoredCredentials()
        {
            PlayerPrefs.DeleteKey(TOKEN_KEY);
            PlayerPrefs.DeleteKey(USERNAME_KEY);
            PlayerPrefs.Save();
        }

        private IEnumerator CreateAccountCoroutine(string username, string password, Action<bool, string> callback)
        {
            string url = $"{backendURL}/login/createaccount?ID={UnityWebRequest.EscapeURL(username)}&Pass={UnityWebRequest.EscapeURL(password)}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    var response = JsonUtility.FromJson<LoginResponse>(responseText);

                    if (response.log == "AccountCreated" && !string.IsNullOrEmpty(response.Token))
                    {
                        PlayerPrefs.SetString(TOKEN_KEY, response.Token);
                        PlayerPrefs.SetString(USERNAME_KEY, username);
                        PlayerPrefs.Save();

                        OnAccountCreated?.Invoke(username);
                        callback?.Invoke(true, "Account created successfully");
                    }
                    else
                    {
                        string errorMessage = GetErrorMessage(response.log, response.Log);
                        OnAccountCreationFailed?.Invoke(errorMessage);
                        callback?.Invoke(false, errorMessage);
                    }
                }
                else
                {
                    string error = $"Network error: {request.error}";
                    OnAccountCreationFailed?.Invoke(error);
                    callback?.Invoke(false, error);
                }
            }
        }

        private IEnumerator LoginCoroutine(string username, string password, Action<bool, string> callback)
        {
            string url = $"{backendURL}/login/connect?ID={UnityWebRequest.EscapeURL(username)}&Pass={UnityWebRequest.EscapeURL(password)}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    var response = JsonUtility.FromJson<LoginResponse>(responseText);

                    if (response.log == "ConnectionOK" && !string.IsNullOrEmpty(response.token))
                    {
                        PlayerPrefs.SetString(TOKEN_KEY, response.token);
                        PlayerPrefs.SetString(USERNAME_KEY, username);
                        PlayerPrefs.Save();

                        OnLoginSuccess?.Invoke(username);
                        callback?.Invoke(true, "Login successful");
                    }
                    else if (response.log == "AccountBanned")
                    {
                        string banMessage = FormatBanMessage(response.reason, response.expiresAt);
                        OnBanDetected?.Invoke(banMessage);
                        callback?.Invoke(false, banMessage);
                    }
                    else
                    {
                        string errorMessage = GetErrorMessage(response.log, null);
                        OnLoginFailed?.Invoke(errorMessage);
                        callback?.Invoke(false, errorMessage);
                    }
                }
                else
                {
                    string error = $"Network error: {request.error}";
                    OnLoginFailed?.Invoke(error);
                    callback?.Invoke(false, error);
                }
            }
        }

        private IEnumerator ValidateTokenCoroutine(string token, Action<bool, string> callback)
        {
            string url = $"{backendURL}/login/connectbytoken?Token={UnityWebRequest.EscapeURL(token)}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    var response = JsonUtility.FromJson<TokenValidationResponse>(responseText);

                    if (response.log == "ok")
                    {
                        callback?.Invoke(true, response.username);
                    }
                    else if (response.log == "AccountBanned")
                    {
                        ClearStoredCredentials();
                        string banMessage = FormatBanMessage(response.reason, response.expiresAt);
                        OnBanDetected?.Invoke(banMessage);
                        callback?.Invoke(false, banMessage);
                    }
                    else
                    {
                        ClearStoredCredentials();
                        callback?.Invoke(false, "Invalid token");
                    }
                }
                else
                {
                    callback?.Invoke(false, $"Network error: {request.error}");
                }
            }
        }

        private IEnumerator LogoutCoroutine(string token, Action callback)
        {
            string url = $"{backendURL}/login/logout?Token={UnityWebRequest.EscapeURL(token)}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                ClearStoredCredentials();
                OnLogoutSuccess?.Invoke();
                callback?.Invoke();
            }
        }

        private IEnumerator CheckBanStatusCoroutine(string token, Action<bool, string, string> callback)
        {
            string url = $"{backendURL}/player/checkban?Token={UnityWebRequest.EscapeURL(token)}";

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    var response = JsonUtility.FromJson<BanCheckResponse>(responseText);

                    if (response.log == "ok" && response.banStatus != null)
                    {
                        if (response.banStatus.banned)
                        {
                            callback?.Invoke(true, response.banStatus.reason, response.banStatus.expiresAt);
                        }
                        else
                        {
                            callback?.Invoke(false, "", "");
                        }
                    }
                    else
                    {
                        callback?.Invoke(false, "", "");
                    }
                }
                else
                {
                    callback?.Invoke(false, "", "");
                }
            }
        }

        private IEnumerator SubmitReportCoroutine(string token, string reportedPlayer, string reason, string description, Action<bool, string> callback)
        {
            string url = $"{backendURL}/reports/submit";

            var reportData = new ReportSubmission
            {
                Token = token,
                ReportedPlayer = reportedPlayer,
                Reason = reason,
                Description = description
            };

            string jsonData = JsonUtility.ToJson(reportData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)apiTimeout;

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseText = request.downloadHandler.text;
                    var response = JsonUtility.FromJson<ReportResponse>(responseText);

                    if (response.log == "ok")
                    {
                        callback?.Invoke(true, "Report submitted successfully");
                    }
                    else if (response.log == "ReportLimitReached")
                    {
                        callback?.Invoke(false, "Report limit reached. Please wait before submitting another report.");
                    }
                    else
                    {
                        callback?.Invoke(false, "Failed to submit report");
                    }
                }
                else
                {
                    callback?.Invoke(false, $"Network error: {request.error}");
                }
            }
        }

        private string GetErrorMessage(string logCode, string fallbackLog)
        {
            switch (logCode ?? fallbackLog)
            {
                case "IDTooShort":
                    return "Username is too short (minimum 3 characters)";
                case "PassTooShort":
                    return "Password is too short (minimum 9 characters)";
                case "ExistingID":
                    return "Username already exists";
                case "UserNameNotExist":
                    return "Username does not exist";
                case "PassWrong":
                    return "Incorrect password";
                case "TokenDoesNotExist":
                    return "Invalid or expired session";
                default:
                    return "Unknown error occurred";
            }
        }

        private string FormatBanMessage(string reason, string expiresAt)
        {
            string message = "YOU ARE BANNED";
            
            if (!string.IsNullOrEmpty(reason))
            {
                message += $"\n\nReason: {reason}";
            }
            
            if (!string.IsNullOrEmpty(expiresAt) && expiresAt != "None" && expiresAt != "null")
            {
                message += $"\n\nBan Duration: Until {expiresAt}";
            }
            else
            {
                message += $"\n\nBan Duration: Permanent";
            }
            
            return message;
        }

        [Serializable]
        private class LoginResponse
        {
            public string log;
            public string Log;
            public string token;
            public string Token;
            public string reason;
            public string expiresAt;
        }

        [Serializable]
        private class TokenValidationResponse
        {
            public string log;
            public string username;
            public string reason;
            public string expiresAt;
        }

        [Serializable]
        private class BanCheckResponse
        {
            public string log;
            public BanStatus banStatus;
        }

        [Serializable]
        private class BanStatus
        {
            public bool banned;
            public string reason;
            public string bannedAt;
            public string expiresAt;
        }

        [Serializable]
        private class ReportSubmission
        {
            public string Token;
            public string ReportedPlayer;
            public string Reason;
            public string Description;
        }

        [Serializable]
        private class ReportResponse
        {
            public string log;
            public string reportId;
        }

        [Serializable]
        private class GameDataResponse
        {
            public string log;
            public int Level;
            public int XP;
            public int CloudCoins;
            public string[] OwnedSkins;
            public string EquippedSkin;
            public bool HasBattlePass;
        }

        [Serializable]
        private class GameDataUpdateRequest
        {
            public string Token;
            public int Level;
            public int XP;
            public int CloudCoins;
            public string[] OwnedSkins;
            public string EquippedSkin;
            public bool HasBattlePass;
        }

        [Serializable]
        private class AddCoinsResponse
        {
            public string log;
            public int NewTotal;
        }

        [Serializable]
        private class AddXPResponse
        {
            public string log;
            public int Level;
            public int XP;
        }

        [Serializable]
        private class UnlockSkinResponse
        {
            public string log;
            public string message;
        }

        [Serializable]
        private class BattlePassStatusResponse
        {
            public string log;
            public bool HasBattlePass;
            public int CurrentTier;
            public int BattlePassXP;
            public int Season;
        }

        [Serializable]
        private class BattlePassXPRequest
        {
            public string Token;
            public int Amount;
        }

        [Serializable]
        private class BattlePassXPResponse
        {
            public string log;
            public int CurrentTier;
            public int BattlePassXP;
        }

        [Serializable]
        private class BattlePassPurchaseResponse
        {
            public string log;
            public int NewCoinTotal;
            public int Required;
            public int Current;
        }

        [Serializable]
        private class SimpleLogResponse
        {
            public string log;
        }
    }

    /// <summary>Runtime snapshot of a player's battle pass progress.</summary>
    [Serializable]
    public class BattlePassStatusData
    {
        public bool HasBattlePass;
        public int CurrentTier;
        public int BattlePassXP;
        public int Season;
    }
}
