using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR.UI
{
    [RequireComponent(typeof(Button))]
    public class UIPlayButtonAnimator : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float _pulseSpeed = 2f;
        [SerializeField] private float _pulseAmount = 0.1f;
        [SerializeField] private bool _enablePulse = true;
        
        [Header("Color Settings")]
        [SerializeField] private Color _normalColor = new Color(1f, 0.9f, 0f, 1f);
        [SerializeField] private Color _searchingColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [SerializeField] private Color _joiningColor = new Color(0f, 1f, 0.5f, 1f);
        
        private Button _button;
        private Image _image;
        private TextMeshProUGUI _text;
        private Vector3 _originalScale;
        private bool _isAnimating;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _image = GetComponent<Image>();
            _text = GetComponentInChildren<TextMeshProUGUI>();
            _originalScale = transform.localScale;
        }
        
        private void Update()
        {
            if (_enablePulse && _isAnimating)
            {
                float scale = 1f + Mathf.Sin(Time.time * _pulseSpeed) * _pulseAmount;
                transform.localScale = _originalScale * scale;
            }
            else if (!_isAnimating)
            {
                transform.localScale = _originalScale;
            }
        }
        
        public void SetState(PlayButtonState state)
        {
            switch (state)
            {
                case PlayButtonState.Ready:
                    SetReadyState();
                    break;
                case PlayButtonState.Searching:
                    SetSearchingState();
                    break;
                case PlayButtonState.Joining:
                    SetJoiningState();
                    break;
                case PlayButtonState.Disabled:
                    SetDisabledState();
                    break;
            }
        }
        
        private void SetReadyState()
        {
            _isAnimating = false;
            _button.interactable = true;
            
            if (_image != null)
                _image.color = _normalColor;
            
            if (_text != null)
                _text.text = "PLAY";
        }
        
        private void SetSearchingState()
        {
            _isAnimating = true;
            _button.interactable = false;
            
            if (_image != null)
                _image.color = _searchingColor;
            
            if (_text != null)
                _text.text = "SEARCHING...";
        }
        
        private void SetJoiningState()
        {
            _isAnimating = true;
            _button.interactable = false;
            
            if (_image != null)
                _image.color = _joiningColor;
            
            if (_text != null)
                _text.text = "JOINING...";
        }
        
        private void SetDisabledState()
        {
            _isAnimating = false;
            _button.interactable = false;
            
            if (_image != null)
                _image.color = Color.gray;
        }
    }
    
    public enum PlayButtonState
    {
        Ready,
        Searching,
        Joining,
        Disabled
    }
}
