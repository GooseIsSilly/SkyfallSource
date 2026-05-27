using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TPSBR.Backend;

namespace TPSBR.Backend
{
    public class SimpleLoginUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField usernameInput;
        [SerializeField] private TMP_InputField passwordInput;
        [SerializeField] private Button loginButton;
        [SerializeField] private Button createAccountButton;
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Optional - Hide After Login")]
        [SerializeField] private GameObject loginPanel;

        private void Start()
        {
            if (loginButton != null)
            {
                loginButton.onClick.AddListener(OnLoginButtonClick);
            }

            if (createAccountButton != null)
            {
                createAccountButton.onClick.AddListener(OnCreateAccountButtonClick);
            }

            CheckAutoLogin();
        }

        private void CheckAutoLogin()
        {
            if (BackendServiceManager.Instance == null)
            {
                ShowFeedback("Backend service not available", Color.red);
                return;
            }

            if (BackendServiceManager.Instance.HasStoredToken())
            {
                ShowFeedback("Validating session...", Color.yellow);

                BackendServiceManager.Instance.ValidateToken((success, username) =>
                {
                    if (success)
                    {
                        ShowFeedback($"Welcome back, {username}!", Color.green);
                        OnLoginSuccess(username);
                    }
                    else
                    {
                        ShowFeedback("Session expired. Please login again.", Color.yellow);
                    }
                });
            }
        }

        private void OnCreateAccountButtonClick()
        {
            string username = usernameInput.text.Trim();
            string password = passwordInput.text;

            if (!ValidateInput(username, password))
            {
                return;
            }

            if (BackendServiceManager.Instance == null)
            {
                ShowFeedback("Backend service not available", Color.red);
                return;
            }

            SetButtonsEnabled(false);
            ShowFeedback("Creating account...", Color.yellow);

            BackendServiceManager.Instance.CreateAccount(username, password, (success, message) =>
            {
                SetButtonsEnabled(true);

                if (success)
                {
                    ShowFeedback("Account created! Logging you in...", Color.green);
                    OnLoginSuccess(username);
                }
                else
                {
                    ShowFeedback($"Failed: {message}", Color.red);
                }
            });
        }

        private void OnLoginButtonClick()
        {
            string username = usernameInput.text.Trim();
            string password = passwordInput.text;

            if (!ValidateInput(username, password))
            {
                return;
            }

            if (BackendServiceManager.Instance == null)
            {
                ShowFeedback("Backend service not available", Color.red);
                return;
            }

            SetButtonsEnabled(false);
            ShowFeedback("Logging in...", Color.yellow);

            BackendServiceManager.Instance.LoginPlayer(username, password, (success, message) =>
            {
                SetButtonsEnabled(true);

                if (success)
                {
                    ShowFeedback($"Welcome, {username}!", Color.green);
                    OnLoginSuccess(username);
                }
                else
                {
                    ShowFeedback($"Login failed: {message}", Color.red);
                }
            });
        }

        private bool ValidateInput(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowFeedback("Please enter a username", Color.red);
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowFeedback("Please enter a password", Color.red);
                return false;
            }

            if (username.Length < 3)
            {
                ShowFeedback("Username must be at least 3 characters", Color.red);
                return false;
            }

            if (password.Length < 9)
            {
                ShowFeedback("Password must be at least 9 characters", Color.red);
                return false;
            }

            return true;
        }

        private void OnLoginSuccess(string username)
        {
            Debug.Log($"Login successful! Username: {username}");

            ClearPasswordField();
            
            // LoginSceneManager will handle hiding the panel via event
        }

        private void ShowFeedback(string message, Color color)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = color;
            }

            Debug.Log($"[Login] {message}");
        }

        private void SetButtonsEnabled(bool enabled)
        {
            if (loginButton != null)
            {
                loginButton.interactable = enabled;
            }

            if (createAccountButton != null)
            {
                createAccountButton.interactable = enabled;
            }
        }

        private void ClearPasswordField()
        {
            if (passwordInput != null)
            {
                passwordInput.text = "";
            }
        }

        private void OnDestroy()
        {
            if (loginButton != null)
            {
                loginButton.onClick.RemoveListener(OnLoginButtonClick);
            }

            if (createAccountButton != null)
            {
                createAccountButton.onClick.RemoveListener(OnCreateAccountButtonClick);
            }
        }
    }
}
