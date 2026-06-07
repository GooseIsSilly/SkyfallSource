using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TPSBR;

namespace TPSBR.Backend
{
    public class BackendServiceManager : MonoBehaviour
    {
        public string BackendURL => backendURL;

        public static BackendServiceManager Instance { get; private set; }

        [Header("Backend Configuration")]
        [SerializeField] private string backendURL = "http://198.50.250.196:5000";
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

        public bool IsLoggedIn { get; private set; }

        // ============================================================
        // PUBLIC RESPONSE MODELS
        // ============================================================

        [Serializable]
        public class GameDataResponse
        {
            public string log;
            public int Level;
            public int XP;
            public int CloudCoins;
            public string[] OwnedSkins;
            public string EquippedSkin;
            public bool HasBattlePass;
            public int BattlePassTier;
            public int BattlePassXP;
        }

        [Serializable]
        public class BattlePassStatusData
        {
            public bool HasBattlePass;
            public int CurrentTier;
            public int BattlePassXP;
        }

        [Serializable]
        public class ShopCatalogResponse
        {
            public string log;
            public List<ShopItemData> daily;
            public List<ShopItemData> weekly;
            public string daily_reset;
            public string weekly_reset;
        }

        [Serializable]
        public class ShopItemData
        {
            public string id;
            public int price;
            public string rarity;
            public string type;
        }

        // ============================================================
        // PRIVATE RESPONSE MODELS
        // ============================================================

        [Serializable] private class BaseResponse { public string log; }
        [Serializable] private class LoginResponse { public string log; public string token; public string username; }
        [Serializable] private class BanStatusInner { public bool banned; public string reason; public string expiresAt; }
        [Serializable] private class BanStatusResponse { public string log; public BanStatusInner banStatus; }
        [Serializable] private class CoinsResponse { public string log; public int CloudCoins; }
        [Serializable] private class XPResponse { public string log; public int Level; public int XP; }
        [Serializable] private class BattlePassXPResponse { public string log; public int CurrentTier; public int BattlePassXP; }
        [Serializable] private class BattlePassStatusResponse { public string log; public bool HasBattlePass; public int CurrentTier; public int BattlePassXP; }

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

        // ============================================================
        // LIFECYCLE
        // ============================================================

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);

                if (ApplicationUtility.GetCommandLineArgument("-token", out string cliToken) && !string.IsNullOrEmpty(cliToken))
                {
                    PlayerPrefs.SetString(TOKEN_KEY, cliToken);
                    Debug.Log("[BackendServiceManager] Token received from launcher via command line.");
                }

                if (ApplicationUtility.GetCommandLineArgument("-username", out string cliUsername) && !string.IsNullOrEmpty(cliUsername))
                {
                    PlayerPrefs.SetString(USERNAME_KEY, cliUsername);
                    Debug.Log($"[BackendServiceManager] Username received from launcher: {cliUsername}");
                }

                PlayerPrefs.Save();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // ============================================================
        // TOKEN HELPERS
        // ============================================================

        public bool HasStoredToken() => PlayerPrefs.HasKey(TOKEN_KEY) && !string.IsNullOrEmpty(PlayerPrefs.GetString(TOKEN_KEY, ""));
        public string GetStoredToken() => PlayerPrefs.GetString(TOKEN_KEY, "");
        public string GetStoredUsername() => PlayerPrefs.GetString(USERNAME_KEY, "");

        private void ClearStoredCredentials()
        {
            PlayerPrefs.DeleteKey(TOKEN_KEY);
            PlayerPrefs.DeleteKey(USERNAME_KEY);
            PlayerPrefs.Save();
            IsLoggedIn = false;
        }

        // ============================================================
        // AUTH
        // ============================================================

        public void CreateAccount(string username, string password, Action<bool, string> callback)
        {
            StartCoroutine(CreateAccountCoroutine(username, password, callback));
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
                    var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                    if (response.log == "ok")
                    {
                        if (!string.IsNullOrEmpty(response.token))
                        {
                            PlayerPrefs.SetString(TOKEN_KEY, response.token);
                            PlayerPrefs.SetString(USERNAME_KEY, username);
                            PlayerPrefs.Save();
                            IsLoggedIn = true;
                        }

                        OnAccountCreated?.Invoke(username);
                        callback?.Invoke(true, username);
                    }
                    else
                    {
                        OnAccountCreationFailed?.Invoke(response.log);
                        callback?.Invoke(false, response.log);
                    }
                }
                else
                {
                    OnAccountCreationFailed?.Invoke(request.error);
                    callback?.Invoke(false, request.error);
                }
            }
        }

        public void LoginPlayer(string username, string password, Action<bool, string> callback)
        {
            StartCoroutine(LoginCoroutine(username, password, callback));
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
                    var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                    if (response.log == "ok")
                    {
                        PlayerPrefs.SetString(TOKEN_KEY, response.token);
                        PlayerPrefs.SetString(USERNAME_KEY, username);
                        PlayerPrefs.Save();
                        IsLoggedIn = true;

                        OnLoginSuccess?.Invoke(username);
                        callback?.Invoke(true, username);
                    }
                    else
                    {
                        OnLoginFailed?.Invoke(response.log);
                        callback?.Invoke(false, response.log);
                    }
                }
                else
                {
                    OnLoginFailed?.Invoke(request.error);
                    callback?.Invoke(false, request.error);
                }
            }
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

        private IEnumerator ValidateTokenCoroutine(string token, Action<bool, string> callback)
        {
            string url = $"{backendURL}/login/connectbytoken?Token={UnityWebRequest.EscapeURL(token)}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<LoginResponse>(request.downloadHandler.text);
                    if (response.log == "ok")
                    {
                        string resolvedName = string.IsNullOrEmpty(response.username) ? GetStoredUsername() : response.username;
                        if (!string.IsNullOrEmpty(response.username))
                        {
                            PlayerPrefs.SetString(USERNAME_KEY, response.username);
                            PlayerPrefs.Save();
                        }
                        IsLoggedIn = true;

                        OnLoginSuccess?.Invoke(resolvedName);
                        callback?.Invoke(true, resolvedName);
                    }
                    else
                    {
                        ClearStoredCredentials();
                        callback?.Invoke(false, response.log);
                    }
                }
                else
                {
                    callback?.Invoke(false, request.error);
                }
            }
        }

        public void Logout(Action callback = null)
        {
            string token = GetStoredToken();
            if (!string.IsNullOrEmpty(token))
                StartCoroutine(LogoutCoroutine(token, callback));
            else
            {
                ClearStoredCredentials();
                callback?.Invoke();
            }
        }

        private IEnumerator LogoutCoroutine(string token, Action callback)
        {
            string url = $"{backendURL}/login/logout?Token={UnityWebRequest.EscapeURL(token)}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
            }
            ClearStoredCredentials();
            OnLogoutSuccess?.Invoke();
            callback?.Invoke();
        }

        // ============================================================
        // BAN
        // ============================================================

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

        private IEnumerator CheckBanStatusCoroutine(string token, Action<bool, string, string> callback)
        {
            string url = $"{backendURL}/player/checkban?Token={UnityWebRequest.EscapeURL(token)}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BanStatusResponse>(request.downloadHandler.text);
                    bool isBanned = response.banStatus != null && response.banStatus.banned;
                    string reason = isBanned ? response.banStatus.reason : "";
                    string expiresAt = isBanned ? response.banStatus.expiresAt : "";
                    if (isBanned) OnBanDetected?.Invoke(reason);
                    callback?.Invoke(isBanned, reason, expiresAt);
                }
                else
                {
                    callback?.Invoke(false, "", "");
                }
            }
        }

        // ============================================================
        // REPORTS
        // ============================================================

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

        private IEnumerator SubmitReportCoroutine(string token, string reportedPlayer, string reason, string description, Action<bool, string> callback)
        {
            string url = $"{backendURL}/player/report?Token={UnityWebRequest.EscapeURL(token)}&ReportedPlayer={UnityWebRequest.EscapeURL(reportedPlayer)}&Reason={UnityWebRequest.EscapeURL(reason)}&Description={UnityWebRequest.EscapeURL(description)}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BaseResponse>(request.downloadHandler.text);
                    callback?.Invoke(response.log == "ok", response.log);
                }
                else
                {
                    callback?.Invoke(false, request.error);
                }
            }
        }

        // ============================================================
        // GAME DATA
        // ============================================================

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
                    var response = JsonUtility.FromJson<GameDataResponse>(request.downloadHandler.text);
                    if (response.log == "ok")
                    {
                        var data = new PlayerGameData
                        {
                            Level = response.Level,
                            XP = response.XP,
                            CloudCoins = response.CloudCoins,
                            OwnedSkins = response.OwnedSkins ?? new string[0],
                            EquippedSkin = response.EquippedSkin,
                            HasBattlePass = response.HasBattlePass,
                            BattlePassTier = response.BattlePassTier,
                            BattlePassXP = response.BattlePassXP
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

            string json = JsonUtility.ToJson(updateData);
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
                callback?.Invoke(request.result == UnityWebRequest.Result.Success);
            }
        }

        // ============================================================
        // COINS / XP
        // ============================================================

        public IEnumerator AddCoins(int amount, Action<bool, int> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                callback?.Invoke(false, 0);
                yield break;
            }

            string url = $"{backendURL}/account/addcoins?Token={UnityWebRequest.EscapeURL(token)}&Amount={amount}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<CoinsResponse>(request.downloadHandler.text);
                    callback?.Invoke(response.log == "ok", response.CloudCoins);
                }
                else callback?.Invoke(false, 0);
            }
        }

        public IEnumerator AddXP(int amount, Action<bool, int, int> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                callback?.Invoke(false, 0, 0);
                yield break;
            }

            string url = $"{backendURL}/account/addxp?Token={UnityWebRequest.EscapeURL(token)}&Amount={amount}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<XPResponse>(request.downloadHandler.text);
                    callback?.Invoke(response.log == "ok", response.Level, response.XP);
                }
                else callback?.Invoke(false, 0, 0);
            }
        }

        // ============================================================
        // SKINS
        // ============================================================

        public IEnumerator UnlockSkin(string skinID, Action<bool, string> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token))
            {
                callback?.Invoke(false, "Not logged in");
                yield break;
            }

            string url = $"{backendURL}/account/unlockskin?Token={UnityWebRequest.EscapeURL(token)}&SkinID={UnityWebRequest.EscapeURL(skinID)}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BaseResponse>(request.downloadHandler.text);
                    callback?.Invoke(response.log == "ok", response.log);
                }
                else callback?.Invoke(false, request.error);
            }
        }

        // ============================================================
        // BATTLE PASS
        // ============================================================

        public void PurchaseBattlePassFromServer(Action<bool, string> callback)
        {
            StartCoroutine(PurchaseBattlePassCoroutine(callback));
        }

        private IEnumerator PurchaseBattlePassCoroutine(Action<bool, string> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token)) { callback?.Invoke(false, "Not logged in"); yield break; }

            string url = $"{backendURL}/battlepass/purchase?Token={UnityWebRequest.EscapeURL(token)}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BaseResponse>(request.downloadHandler.text);
                    callback?.Invoke(response.log == "ok", response.log);
                }
                else callback?.Invoke(false, request.error);
            }
        }

        public void AddBattlePassXP(int amount, Action<bool, int, int> callback)
        {
            StartCoroutine(AddBattlePassXPCoroutine(amount, callback));
        }

        private IEnumerator AddBattlePassXPCoroutine(int amount, Action<bool, int, int> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token)) { callback?.Invoke(false, 0, 0); yield break; }

            string url = $"{backendURL}/battlepass/addxp?Token={UnityWebRequest.EscapeURL(token)}&Amount={amount}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BattlePassXPResponse>(request.downloadHandler.text);
                    callback?.Invoke(response.log == "ok", response.CurrentTier, response.BattlePassXP);
                }
                else callback?.Invoke(false, 0, 0);
            }
        }

        public void GetBattlePassStatus(Action<bool, BattlePassStatusData> callback)
        {
            StartCoroutine(GetBattlePassStatusCoroutine(callback));
        }

        private IEnumerator GetBattlePassStatusCoroutine(Action<bool, BattlePassStatusData> callback)
        {
            string token = GetStoredToken();
            if (string.IsNullOrEmpty(token)) { callback?.Invoke(false, null); yield break; }

            string url = $"{backendURL}/battlepass/status?Token={UnityWebRequest.EscapeURL(token)}";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    var response = JsonUtility.FromJson<BattlePassStatusResponse>(request.downloadHandler.text);
                    if (response.log == "ok")
                        callback?.Invoke(true, new BattlePassStatusData { HasBattlePass = response.HasBattlePass, CurrentTier = response.CurrentTier, BattlePassXP = response.BattlePassXP });
                    else
                        callback?.Invoke(false, null);
                }
                else callback?.Invoke(false, null);
            }
        }

        // ============================================================
        // SHOP CATALOG
        // ============================================================

        /// <summary>Fetches the current daily and weekly shop rotation from the backend.</summary>
        public void GetShopCatalog(Action<bool, ShopCatalogResponse> callback)
        {
            StartCoroutine(GetShopCatalogCoroutine(callback));
        }

        private IEnumerator GetShopCatalogCoroutine(Action<bool, ShopCatalogResponse> callback)
        {
            string url = $"{backendURL}/shop/catalog";
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = (int)apiTimeout;
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        var response = JsonUtility.FromJson<ShopCatalogResponse>(request.downloadHandler.text);
                        callback?.Invoke(response.log == "ok", response);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[Backend] Failed to parse shop catalog: {e.Message}");
                        callback?.Invoke(false, null);
                    }
                }
                else
                {
                    Debug.LogError($"[Backend] Shop catalog request failed: {request.error}");
                    callback?.Invoke(false, null);
                }
            }
        }
    }
}
