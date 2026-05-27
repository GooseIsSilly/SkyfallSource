using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TPSBR.Backend;

namespace TPSBR.UI
{
    public class UILoginView : UIView
    {
        [Header("References")]
        [SerializeField] private TMP_InputField _usernameInput;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private UIButton _loginButton;
        [SerializeField] private UIButton _createAccountButton;
        [SerializeField] private TextMeshProUGUI _feedbackText;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (_loginButton != null)
                _loginButton.onClick.AddListener(OnLoginClicked);

            if (_createAccountButton != null)
                _createAccountButton.onClick.AddListener(OnCreateAccountClicked);
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            
            if (_feedbackText != null)
                _feedbackText.text = string.Empty;

            if (_usernameInput != null)
                _usernameInput.text = string.Empty;

            if (_passwordInput != null)
                _passwordInput.text = string.Empty;
        }

        private void OnLoginClicked()
        {
            string username = _usernameInput.text.Trim();
            string password = _passwordInput.text;

            if (ValidateInput(username, password) == false)
                return;

            SetInteractable(false);
            ShowFeedback("Logging in...", Color.white);

            BackendServiceManager.Instance.LoginPlayer(username, password, (success, message) =>
            {
                SetInteractable(true);

                if (success)
                {
                    ShowFeedback($"Welcome, {username}!", Color.green);
                    Close();
                }
                else
                {
                    ShowFeedback($"Login failed: {message}", Color.red);
                }
            });
        }

        private void OnCreateAccountClicked()
        {
            string username = _usernameInput.text.Trim();
            string password = _passwordInput.text;

            if (ValidateInput(username, password) == false)
                return;

            SetInteractable(false);
            ShowFeedback("Creating account...", Color.white);

            BackendServiceManager.Instance.CreateAccount(username, password, (success, message) =>
            {
                SetInteractable(true);

                if (success)
                {
                    ShowFeedback("Account created!", Color.green);
                    Close();
                }
                else
                {
                    ShowFeedback($"Failed: {message}", Color.red);
                }
            });
        }

        private bool ValidateInput(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowFeedback("Enter username", Color.red);
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ShowFeedback("Enter password", Color.red);
                return false;
            }

            return true;
        }

        private void ShowFeedback(string message, Color color)
        {
            if (_feedbackText != null)
            {
                _feedbackText.text = message;
                _feedbackText.color = color;
            }
        }

        private void SetInteractable(bool interactable)
        {
            IsInteractable = interactable;
        }
    }
}
