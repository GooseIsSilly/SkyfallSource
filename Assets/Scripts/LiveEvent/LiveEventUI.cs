using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR
{
    public class LiveEventUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _countdownPanel;
        [SerializeField] private TextMeshProUGUI _countdownText;
        [SerializeField] private TextMeshProUGUI _eventNameText;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _urgentColor = Color.red;
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseIntensity = 0.3f;
        
        private bool _isUrgent;
        private float _pulseTimer;
        
        private void OnEnable()
        {
            if (LiveEventManager.Instance != null)
            {
                LiveEventManager.Instance.OnCountdownUpdate += UpdateCountdown;
                LiveEventManager.Instance.OnEventTriggered += HandleEventTriggered;
                LiveEventManager.Instance.OnEventStarted += HandleEventStarted;
            }
        }
        
        private void OnDisable()
        {
            if (LiveEventManager.Instance != null)
            {
                LiveEventManager.Instance.OnCountdownUpdate -= UpdateCountdown;
                LiveEventManager.Instance.OnEventTriggered -= HandleEventTriggered;
                LiveEventManager.Instance.OnEventStarted -= HandleEventStarted;
            }
        }
        
        private void Update()
        {
            if (_isUrgent && _countdownText != null)
            {
                _pulseTimer += Time.deltaTime * _pulseSpeed;
                float pulse = 1f + Mathf.Sin(_pulseTimer) * _pulseIntensity;
                _countdownText.transform.localScale = Vector3.one * pulse;
            }
        }
        
        private void HandleEventStarted(LiveEventData eventData, float remainingTime)
        {
            if (eventData == null || !eventData.ShowCountdownUI) return;
            
            if (_eventNameText != null)
            {
                _eventNameText.text = eventData.EventName;
            }
            
            if (_countdownPanel != null)
            {
                _countdownPanel.SetActive(true);
            }
            
            _isUrgent = false;
            if (_countdownText != null)
            {
                _countdownText.color = _normalColor;
                _countdownText.transform.localScale = Vector3.one;
            }
        }
        
        private void UpdateCountdown(LiveEventData eventData, float remainingTime)
        {
            if (eventData == null || !eventData.ShowCountdownUI) return;
            
            if (_countdownPanel != null && !_countdownPanel.activeSelf)
            {
                _countdownPanel.SetActive(true);
            }
            
            if (remainingTime <= 0f)
            {
                HideCountdown();
                return;
            }
            
            if (_countdownText != null)
            {
                _countdownText.text = FormatTime(remainingTime);
                
                if (remainingTime <= eventData.RedTextThreshold && !_isUrgent)
                {
                    _isUrgent = true;
                    _countdownText.color = _urgentColor;
                    _pulseTimer = 0f;
                }
                else if (remainingTime > eventData.RedTextThreshold && _isUrgent)
                {
                    _isUrgent = false;
                    _countdownText.color = _normalColor;
                    _countdownText.transform.localScale = Vector3.one;
                }
            }
        }
        
        private void HandleEventTriggered(LiveEventData eventData)
        {
            HideCountdown();
        }
        
        private void HideCountdown()
        {
            if (_countdownPanel != null)
            {
                _countdownPanel.SetActive(false);
            }
            
            _isUrgent = false;
            if (_countdownText != null)
            {
                _countdownText.transform.localScale = Vector3.one;
            }
        }
        
        private string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.CeilToInt(seconds);
            int hours = totalSeconds / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int secs = totalSeconds % 60;
            
            if (hours > 0)
            {
                return $"{hours:D2}:{minutes:D2}:{secs:D2}";
            }
            else
            {
                return $"{minutes:D2}:{secs:D2}";
            }
        }
    }
}
