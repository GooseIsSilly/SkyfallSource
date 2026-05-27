using UnityEngine;
using TMPro;

namespace TPSBR
{
    [RequireComponent(typeof(TextMeshPro))]
    public class LiveEventWorldText : MonoBehaviour
    {
        [Header("World Text Settings")]
        [SerializeField] private bool _billboardToCamera = true;
        [SerializeField] private Vector3 _rotationOffset = Vector3.zero;
        
        [Header("Visual Settings")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _urgentColor = Color.red;
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseIntensity = 0.3f;
        [SerializeField] private Vector3 _baseScale = Vector3.one * 5f;
        
        [Header("Display Options")]
        [SerializeField] private bool _showEventName = true;
        [SerializeField] private float _eventNameOffsetY = 2f;
        
        [Header("Debug")]
        [SerializeField] private bool _showGizmo = true;
        [SerializeField] private Color _gizmoColor = Color.yellow;
        
        private TextMeshPro _countdownText;
        private TextMeshPro _eventNameText;
        private GameObject _eventNameObject;
        private Camera _mainCamera;
        private bool _isUrgent;
        private float _pulseTimer;
        private bool _isSubscribed = false;
        
        private void Awake()
        {
            _countdownText = GetComponent<TextMeshPro>();
            
            if (_countdownText != null)
            {
                _countdownText.alignment = TextAlignmentOptions.Center;
                _countdownText.fontSize = 10;
            }
            
            if (_showEventName)
            {
                CreateEventNameText();
            }
        }
        
        private void CreateEventNameText()
        {
            _eventNameObject = new GameObject("EventNameText");
            _eventNameObject.transform.SetParent(transform);
            _eventNameObject.transform.localPosition = new Vector3(0, _eventNameOffsetY, 0);
            _eventNameObject.transform.localRotation = Quaternion.identity;
            _eventNameObject.transform.localScale = Vector3.one * 0.5f;
            
            _eventNameText = _eventNameObject.AddComponent<TextMeshPro>();
            _eventNameText.alignment = TextAlignmentOptions.Center;
            _eventNameText.fontSize = 6;
            _eventNameText.color = _normalColor;
        }
        
        private void Start()
        {
            _mainCamera = Camera.main;
            transform.localScale = _baseScale;
        }
        
        private void OnEnable()
        {
            TrySubscribe();
        }
        
        private void TrySubscribe()
        {
            if (!_isSubscribed && LiveEventManager.Instance != null)
            {
                LiveEventManager.Instance.OnCountdownUpdate += UpdateCountdown;
                LiveEventManager.Instance.OnEventTriggered += HandleEventTriggered;
                LiveEventManager.Instance.OnEventStarted += HandleEventStarted;
                _isSubscribed = true;
                Debug.Log("[LiveEventWorldText] Subscribed to LiveEventManager events");
            }
        }
        
        private void OnDisable()
        {
            if (_isSubscribed && LiveEventManager.Instance != null)
            {
                LiveEventManager.Instance.OnCountdownUpdate -= UpdateCountdown;
                LiveEventManager.Instance.OnEventTriggered -= HandleEventTriggered;
                LiveEventManager.Instance.OnEventStarted -= HandleEventStarted;
                _isSubscribed = false;
            }
        }
        
        private void Update()
        {
            if (!_isSubscribed)
            {
                TrySubscribe();
            }
            
            if (_billboardToCamera && _mainCamera != null)
            {
                Vector3 directionToCamera = _mainCamera.transform.position - transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);
                targetRotation *= Quaternion.Euler(_rotationOffset);
                transform.rotation = targetRotation;
            }
            
            if (_isUrgent)
            {
                _pulseTimer += Time.deltaTime * _pulseSpeed;
                float pulse = 1f + Mathf.Sin(_pulseTimer) * _pulseIntensity;
                transform.localScale = _baseScale * pulse;
            }
        }
        
        private void HandleEventStarted(LiveEventData eventData, float remainingTime)
        {
            if (eventData == null || !eventData.ShowCountdownUI) 
            {
                gameObject.SetActive(false);
                return;
            }
            
            if (eventData.CountdownVisibilityThreshold > 0 && remainingTime > eventData.CountdownVisibilityThreshold)
            {
                gameObject.SetActive(false);
                return;
            }
            
            gameObject.SetActive(true);
            
            if (_eventNameText != null && _showEventName)
            {
                _eventNameText.text = eventData.EventName.ToUpper();
            }
            
            _isUrgent = false;
            if (_countdownText != null)
            {
                _countdownText.color = _normalColor;
            }
            
            transform.localScale = _baseScale;
        }
        
        private void UpdateCountdown(LiveEventData eventData, float remainingTime)
        {
            if (eventData == null || !eventData.ShowCountdownUI)
            {
                gameObject.SetActive(false);
                return;
            }
            
            if (eventData.CountdownVisibilityThreshold > 0 && remainingTime > eventData.CountdownVisibilityThreshold)
            {
                gameObject.SetActive(false);
                return;
            }
            
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
                
                if (_eventNameText != null && _showEventName)
                {
                    _eventNameText.text = eventData.EventName.ToUpper();
                }
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
                    if (_eventNameText != null)
                    {
                        _eventNameText.color = _urgentColor;
                    }
                    _pulseTimer = 0f;
                }
                else if (remainingTime > eventData.RedTextThreshold && _isUrgent)
                {
                    _isUrgent = false;
                    _countdownText.color = _normalColor;
                    if (_eventNameText != null)
                    {
                        _eventNameText.color = _normalColor;
                    }
                    transform.localScale = _baseScale;
                }
            }
        }
        
        private void HandleEventTriggered(LiveEventData eventData)
        {
            HideCountdown();
        }
        
        private void HideCountdown()
        {
            gameObject.SetActive(false);
            _isUrgent = false;
            transform.localScale = _baseScale;
        }
        
        private string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.CeilToInt(seconds);
            
            int days = totalSeconds / 86400;
            int hours = (totalSeconds % 86400) / 3600;
            int minutes = (totalSeconds % 3600) / 60;
            int secs = totalSeconds % 60;
            
            if (days > 0)
            {
                return $"{days}D {hours:D2}:{minutes:D2}:{secs:D2}";
            }
            else if (hours > 0)
            {
                return $"{hours:D2}:{minutes:D2}:{secs:D2}";
            }
            else
            {
                return $"{minutes:D2}:{secs:D2}";
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!_showGizmo) return;
            
            Gizmos.color = _gizmoColor;
            Gizmos.DrawWireSphere(transform.position, 1f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 3f);
        }
    }
}
