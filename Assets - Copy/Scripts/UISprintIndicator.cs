using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TPSBR.UI;

namespace TPSBR
{
    public class UISprintIndicator : MonoBehaviour
    {
        [SerializeField]
        private Image _sprintBar;
        
        [SerializeField]
        private TextMeshProUGUI _statusText;
        
        [SerializeField]
        private Color _activeColor = Color.green;
        
        [SerializeField]
        private Color _cooldownColor = Color.red;
        
        [SerializeField]
        private Color _readyColor = Color.white;

        private Agent _agent;
        private SceneUI _sceneUI;

        private void Start()
        {
            if (_sprintBar != null)
            {
                _sprintBar.fillAmount = 1f;
                _sprintBar.color = _readyColor;
            }
            if (_statusText != null)
            {
                _statusText.text = "";
            }
        }

        private void Update()
        {
            if (_sceneUI == null)
            {
                _sceneUI = FindAnyObjectByType<SceneUI>();
                if (_sceneUI == null)
                    return;
            }

            if (_sceneUI.Context == null)
                return;

            _agent = _sceneUI.Context.ObservedAgent;

            if (_agent == null)
            {
                if (_sprintBar != null)
                {
                    _sprintBar.fillAmount = 1f;
                    _sprintBar.color = _readyColor;
                }
                if (_statusText != null)
                    _statusText.text = "";
                return;
            }

            SprintSystem sprint = _agent.GetComponent<SprintSystem>();
            
            if (sprint == null)
            {
                if (_sprintBar != null)
                {
                    _sprintBar.fillAmount = 1f;
                    _sprintBar.color = _readyColor;
                }
                if (_statusText != null)
                    _statusText.text = "";
                return;
            }

            if (sprint.IsSprinting)
            {
                float fillAmount = sprint.SprintTimeRemaining / 7.0f;
                if (_sprintBar != null)
                {
                    _sprintBar.fillAmount = fillAmount;
                    _sprintBar.color = _activeColor;
                }
                if (_statusText != null)
                    _statusText.text = $"SPRINTING {sprint.SprintTimeRemaining:F1}s";
            }
            else if (sprint.IsOnCooldown)
            {
                float fillAmount = 1.0f - (sprint.CooldownTimeRemaining / 7.0f);
                if (_sprintBar != null)
                {
                    _sprintBar.fillAmount = fillAmount;
                    _sprintBar.color = _cooldownColor;
                }
                if (_statusText != null)
                    _statusText.text = $"COOLDOWN {sprint.CooldownTimeRemaining:F1}s";
            }
            else
            {
                if (_sprintBar != null)
                {
                    _sprintBar.fillAmount = 1f;
                    _sprintBar.color = _readyColor;
                }
                if (_statusText != null)
                    _statusText.text = "SPRINT READY";
            }
        }
    }
}
