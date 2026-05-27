using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSBR
{
    public class ModernShopToggle : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup _shopCanvasGroup;
        
        [Header("Input")]
        [SerializeField] private Key _toggleKey = Key.B;
        
        private bool _isOpen = false;

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current[_toggleKey].wasPressedThisFrame)
            {
                ToggleShop();
            }
        }

        public void ToggleShop()
        {
            _isOpen = !_isOpen;
            SetShopState(_isOpen);
        }

        public void OpenShop()
        {
            _isOpen = true;
            SetShopState(true);
        }

        public void CloseShop()
        {
            _isOpen = false;
            SetShopState(false);
        }

        private void SetShopState(bool open)
        {
            if (_shopCanvasGroup != null)
            {
                _shopCanvasGroup.alpha = open ? 1f : 0f;
                _shopCanvasGroup.interactable = open;
                _shopCanvasGroup.blocksRaycasts = open;
            }
        }
    }
}
