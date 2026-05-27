using UnityEngine;

namespace TPSBR.Backend
{
    /// <summary>
    /// Auto-login manager with persistent token support.
    /// When launched from the Skyfall Launcher, the token and username are passed
    /// via command-line arguments (-token, -username) and picked up by
    /// BackendServiceManager.Awake(). This script then validates that token
    /// silently and proceeds into the game — no in-game login UI is shown.
    /// The login panel is only shown if there is no valid token at all.
    /// </summary>
    public class SimplerLoginManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The login panel to hide after login (shown only if token is missing/invalid)")]
        [SerializeField] private GameObject loginPanelToHide;

        [Tooltip("Objects to show after login (optional)")]
        [SerializeField] private GameObject[] objectsToShowAfterLogin;

        [Header("Auto-Login Settings")]
        [Tooltip("Panel shown while silent auto-login is in progress")]
        [SerializeField] private GameObject loadingPanel;

        [Tooltip("Try to auto-login with saved token on startup")]
        [SerializeField] private bool autoLoginOnStart = true;

        [Header("Player Name Settings")]
        [Tooltip("Set player nickname to backend username after login")]
        [SerializeField] private bool setNicknameFromBackend = true;

        private bool _hasAttemptedAutoLogin = false;

        private void Start()
        {
            // Subscribe to login events
            if (BackendServiceManager.Instance != null)
            {
                BackendServiceManager.Instance.OnLoginSuccess += OnLoginSuccess;
                BackendServiceManager.Instance.OnAccountCreated += OnLoginSuccess;
                BackendServiceManager.Instance.OnLoginFailed += OnLoginFailed;
            }

            // Start with everything hidden
            SetLoginPanelVisible(false);
            SetPostLoginObjectsVisible(false);

            // Show loading panel while we validate the token
            if (loadingPanel != null)
                loadingPanel.SetActive(true);

            if (autoLoginOnStart)
                TryAutoLogin();
            else
                ShowLoginPanel();
        }

        // ── Auto-login ────────────────────────────────────────────────────────

        private void TryAutoLogin()
        {
            if (_hasAttemptedAutoLogin) return;
            _hasAttemptedAutoLogin = true;

            if (BackendServiceManager.Instance == null)
            {
                Debug.LogWarning("[SimplerLoginManager] BackendServiceManager not found!");
                ShowLoginPanel();
                return;
            }

            if (BackendServiceManager.Instance.HasStoredToken())
            {
                string storedUser = BackendServiceManager.Instance.GetStoredUsername();
                Debug.Log($"[SimplerLoginManager] Found stored token for '{storedUser}'. Validating...");

                BackendServiceManager.Instance.ValidateToken((success, serverUsername) =>
                {
                    if (success)
                    {
                        // Use the username the SERVER returned — it's authoritative
                        string username = !string.IsNullOrEmpty(serverUsername) ? serverUsername : storedUser;
                        Debug.Log($"[SimplerLoginManager] Auto-login successful. Welcome, {username}!");
                        OnLoginSuccess(username);
                    }
                    else
                    {
                        Debug.Log($"[SimplerLoginManager] Token invalid ({serverUsername}). Showing login panel.");
                        ShowLoginPanel();
                    }
                });
            }
            else
            {
                Debug.Log("[SimplerLoginManager] No stored token. Showing login panel.");
                ShowLoginPanel();
            }
        }

        // ── UI helpers ────────────────────────────────────────────────────────

        private void ShowLoginPanel()
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            SetLoginPanelVisible(true);
        }

        private void SetLoginPanelVisible(bool visible)
        {
            if (loginPanelToHide != null)
                loginPanelToHide.SetActive(visible);
        }

        private void SetPostLoginObjectsVisible(bool visible)
        {
            foreach (var obj in objectsToShowAfterLogin)
            {
                if (obj != null)
                    obj.SetActive(visible);
            }
        }

        // ── Login success / failure ───────────────────────────────────────────

        private void OnLoginSuccess(string username)
        {
            Debug.Log($"[SimplerLoginManager] Logged in as '{username}'.");

            if (setNicknameFromBackend)
                SetPlayerNickname(username);

            if (loadingPanel != null)
                loadingPanel.SetActive(false);

            SetLoginPanelVisible(false);
            SetPostLoginObjectsVisible(true);
        }

        private void OnLoginFailed(string error)
        {
            Debug.LogWarning($"[SimplerLoginManager] Login failed: {error}");
            ShowLoginPanel();
        }

        // ── Nickname ──────────────────────────────────────────────────────────

        private void SetPlayerNickname(string username)
        {
            if (Global.PlayerService == null)
            {
                Debug.LogWarning("[SimplerLoginManager] Global.PlayerService not ready yet — retrying next frame.");
                StartCoroutine(DelayedSetNickname(username));
                return;
            }

            ApplyNickname(username);
        }

        private void ApplyNickname(string username)
        {
            var playerData = Global.PlayerService?.PlayerData;
            if (playerData == null)
            {
                Debug.LogWarning("[SimplerLoginManager] PlayerData not available. Nickname not set.");
                return;
            }

            string old = playerData.Nickname;
            playerData.Nickname = username;
            Debug.Log($"[SimplerLoginManager] Nickname changed: '{old}' → '{username}'");
        }

        private System.Collections.IEnumerator DelayedSetNickname(string username)
        {
            yield return null; // wait one frame

            if (Global.PlayerService != null && Global.PlayerService.PlayerData != null)
                ApplyNickname(username);
            else
                Debug.LogWarning("[SimplerLoginManager] PlayerData still unavailable after delay. Nickname not set.");
        }

        // ── Cleanup ───────────────────────────────────────────────────────────

        private void OnDestroy()
        {
            if (BackendServiceManager.Instance != null)
            {
                BackendServiceManager.Instance.OnLoginSuccess -= OnLoginSuccess;
                BackendServiceManager.Instance.OnAccountCreated -= OnLoginSuccess;
                BackendServiceManager.Instance.OnLoginFailed -= OnLoginFailed;
            }
        }
    }
}
