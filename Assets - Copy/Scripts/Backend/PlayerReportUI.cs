using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR.Backend
{
    public class PlayerReportUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject reportPanel;
        [SerializeField] private TMP_InputField reportedPlayerInput;
        [SerializeField] private TMP_Dropdown reasonDropdown;
        [SerializeField] private TMP_InputField descriptionInput;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI feedbackText;

        [Header("Settings")]
        [SerializeField] private float reportCooldown = 300f;

        private float _lastReportTime = -1000f;
        private string _targetPlayerName;

        private void Awake()
        {
            if (submitButton != null)
            {
                submitButton.onClick.AddListener(OnSubmitReport);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(HideReportPanel);
            }

            if (reportPanel != null)
            {
                reportPanel.SetActive(false);
            }

            SetupReasonDropdown();
        }

        private void SetupReasonDropdown()
        {
            if (reasonDropdown != null)
            {
                reasonDropdown.ClearOptions();
                reasonDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Hacking / Cheating",
                    "Teaming",
                    "Offensive Behavior",
                    "Exploiting",
                    "Other"
                });
            }
        }

        public void ShowReportPanel(string playerName)
        {
            if (Time.time - _lastReportTime < reportCooldown)
            {
                float remainingTime = reportCooldown - (Time.time - _lastReportTime);
                ShowFeedback($"Please wait {Mathf.CeilToInt(remainingTime)} seconds before reporting again.", true);
                return;
            }

            _targetPlayerName = playerName;

            if (reportedPlayerInput != null)
            {
                reportedPlayerInput.text = playerName;
                reportedPlayerInput.interactable = false;
            }

            if (descriptionInput != null)
            {
                descriptionInput.text = "";
            }

            if (reasonDropdown != null)
            {
                reasonDropdown.value = 0;
            }

            if (feedbackText != null)
            {
                feedbackText.text = "";
            }

            if (reportPanel != null)
            {
                reportPanel.SetActive(true);
            }
        }

        public void HideReportPanel()
        {
            if (reportPanel != null)
            {
                reportPanel.SetActive(false);
            }

            _targetPlayerName = "";
        }

        private void OnSubmitReport()
        {
            if (BackendServiceManager.Instance == null)
            {
                ShowFeedback("Backend service not available", true);
                return;
            }

            if (string.IsNullOrEmpty(_targetPlayerName))
            {
                ShowFeedback("No player selected to report", true);
                return;
            }

            string reason = reasonDropdown != null ? reasonDropdown.options[reasonDropdown.value].text : "Other";
            string description = descriptionInput != null ? descriptionInput.text : "";

            if (string.IsNullOrWhiteSpace(description))
            {
                ShowFeedback("Please provide a description", true);
                return;
            }

            if (submitButton != null)
            {
                submitButton.interactable = false;
            }

            ShowFeedback("Submitting report...", false);

            BackendServiceManager.Instance.SubmitReport(_targetPlayerName, reason, description, (success, message) =>
            {
                if (submitButton != null)
                {
                    submitButton.interactable = true;
                }

                if (success)
                {
                    _lastReportTime = Time.time;
                    ShowFeedback("Report submitted successfully", false);
                    StartCoroutine(CloseAfterDelay(2f));
                }
                else
                {
                    ShowFeedback(message, true);
                }
            });
        }

        private void ShowFeedback(string message, bool isError)
        {
            if (feedbackText != null)
            {
                feedbackText.text = message;
                feedbackText.color = isError ? Color.red : Color.green;
            }
        }

        private IEnumerator CloseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideReportPanel();
        }

        private void OnDestroy()
        {
            if (submitButton != null)
            {
                submitButton.onClick.RemoveListener(OnSubmitReport);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.RemoveListener(HideReportPanel);
            }
        }
    }
}
