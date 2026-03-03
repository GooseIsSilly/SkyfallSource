using UnityEngine;

namespace TPSBR.Backend
{
    /// <summary>
    /// Auto-login manager with persistent token support
    /// Automatically logs in with saved token, only shows login panel if needed
    /// Also sets the player's in-game nickname to match their backend username
    /// </summary>
    public class SimplerLoginManager : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The login panel to hide after login")]
        [SerializeField] private GameObject loginPanelToHide;
        
        [Tooltip("Objects to show after login (optional)")]
        [SerializeField] private GameObject[] objectsToShowAfterLogin;

        [Header("Auto-Login Settings")]
        [Tooltip("Show a loading message while auto-login is happening")]
        [SerializeField] private GameObject loadingPanel;
        
        [Tooltip("Auto-login with saved token on startup")]
        [SerializeField] private bool autoLoginOnStart = true;

        [Header("Player Name Settings")]
        [Tooltip("Automatically set player nickname to backend username")]
        [SerializeField] private bool setNicknameFromBackend = true;

        private bool hasAttemptedAutoLogin = false;

        private void Start()
        {
            // Subscribe to login events
            if (BackendServiceManager.Instance != null)
            {
                BackendServiceManager.Instance.OnLoginSuccess += OnLoginSuccess;
                BackendServiceManager.Instance.OnAccountCreated += OnLoginSuccess;
                BackendServiceManager.Instance.OnLoginFailed += OnLoginFailed;
            }

            // Initially hide everything
            if (loginPanelToHide != null)
            {
                loginPanelToHide.SetActive(false);
            }

            foreach (var obj in objectsToShowAfterLogin)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            // Show loading panel during auto-login
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
            }

            // Try auto-login with stored token
            if (autoLoginOnStart)
            {
                TryAutoLogin();
            }
            else
            {
                ShowLoginPanel();
            }
        }

        private void TryAutoLogin()
        {
            if (hasAttemptedAutoLogin)
                return;

            hasAttemptedAutoLogin = true;

            if (BackendServiceManager.Instance == null)
            {
                Debug.LogWarning("[SimplerLoginManager] BackendServiceManager not found!");
                ShowLoginPanel();
                return;
            }

            // Check if we have a stored token
            if (BackendServiceManager.Instance.HasStoredToken())
            {
                string username = BackendServiceManager.Instance.GetStoredUsername();
                Debug.Log($"[SimplerLoginManager] Found stored token for user: {username}. Attempting auto-login...");

                // Validate the stored token
                BackendServiceManager.Instance.ValidateToken((success, message) =>
                {
                    if (success)
                    {
                        Debug.Log($"[SimplerLoginManager] Auto-login successful! Welcome back, {username}!");
                        OnLoginSuccess(username);
                    }
                    else
                    {
                        Debug.Log($"[SimplerLoginManager] Auto-login failed: {message}. Showing login panel.");
                        ShowLoginPanel();
                    }
                });
            }
            else
            {
                Debug.Log("[SimplerLoginManager] No stored token found. Showing login panel.");
                ShowLoginPanel();
            }
        }

        private void ShowLoginPanel()
        {
            // Hide loading panel
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }

            // Show login panel
            if (loginPanelToHide != null)
            {
                loginPanelToHide.SetActive(true);
            }
        }

        private void OnLoginSuccess(string username)
        {
            Debug.Log($"[SimplerLoginManager] Login successful! Welcome, {username}!");

            // Set player nickname to backend username
            if (setNicknameFromBackend)
            {
                SetPlayerNickname(username);
            }

            // Hide loading panel
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(false);
            }

            // Hide login panel
            if (loginPanelToHide != null)
            {
                loginPanelToHide.SetActive(false);
                Debug.Log("[SimplerLoginManager] Login panel hidden!");
            }

            // Show game UI
            foreach (var obj in objectsToShowAfterLogin)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

            Debug.Log("[SimplerLoginManager] All done! You're logged in!");
        }

        private void SetPlayerNickname(string username)
        {
            // Try to get PlayerData from Global.PlayerService
            if (Global.PlayerService == null)
            {
                Debug.LogWarning("[SimplerLoginManager] Global.PlayerService not found. Cannot set player nickname yet.");
                // It might not be initialized yet, try again in a moment
                StartCoroutine(DelayedSetNickname(username));
                return;
            }

            var playerData = Global.PlayerService.PlayerData;
            if (playerData == null)
            {
                Debug.LogWarning("[SimplerLoginManager] PlayerData not found in PlayerService. Cannot set player nickname.");
                return;
            }

            // Set the nickname to the backend username
            string oldNickname = playerData.Nickname;
            playerData.Nickname = username;

            Debug.Log($"[SimplerLoginManager] Player nickname set from '{oldNickname}' to '{username}'");
        }

        private System.Collections.IEnumerator DelayedSetNickname(string username)
        {
            // Wait a frame for Global.PlayerService to initialize
            yield return null;

            // Try again
            if (Global.PlayerService != null && Global.PlayerService.PlayerData != null)
            {
                string oldNickname = Global.PlayerService.PlayerData.Nickname;
                Global.PlayerService.PlayerData.Nickname = username;
                Debug.Log($"[SimplerLoginManager] Player nickname set (delayed) from '{oldNickname}' to '{username}'");
            }
            else
            {
                Debug.LogWarning("[SimplerLoginManager] Still cannot access PlayerData. Nickname not set.");
            }
        }

        private void OnLoginFailed(string error)
        {
            Debug.LogWarning($"[SimplerLoginManager] Login failed: {error}");
            // Login failed, make sure login panel is visible
            ShowLoginPanel();
        }

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
