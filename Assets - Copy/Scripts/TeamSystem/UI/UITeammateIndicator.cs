using UnityEngine;
using TMPro;

namespace TPSBR.UI
{
    public class UITeammateIndicator : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _nameText;
        [SerializeField]
        private RectTransform _indicator;
        [SerializeField]
        private CanvasGroup _canvasGroup;
        [SerializeField]
        private float _maxDistance = 100f;
        [SerializeField]
        private float _fadeDistance = 50f;

        private Player _targetPlayer;
        private Camera _camera;

        public void SetTarget(Player player)
        {
            _targetPlayer = player;

            if (_nameText != null && player != null)
            {
                _nameText.text = player.Nickname;
            }
        }

        private void Update()
        {
            if (_targetPlayer == null || _targetPlayer.ActiveAgent == null)
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0;
                }
                return;
            }

            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null)
                    return;
            }

            Vector3 targetPosition = _targetPlayer.ActiveAgent.transform.position;
            targetPosition.y += 2.0f;

            Vector3 screenPos = _camera.WorldToScreenPoint(targetPosition);

            if (screenPos.z > 0)
            {
                _indicator.position = screenPos;

                float distance = Vector3.Distance(_camera.transform.position, targetPosition);

                if (distance > _maxDistance)
                {
                    if (_canvasGroup != null)
                    {
                        _canvasGroup.alpha = 0;
                    }
                }
                else
                {
                    float alpha = 1f;
                    if (distance > _fadeDistance)
                    {
                        alpha = 1f - ((distance - _fadeDistance) / (_maxDistance - _fadeDistance));
                    }

                    if (_canvasGroup != null)
                    {
                        _canvasGroup.alpha = alpha;
                    }
                }
            }
            else
            {
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = 0;
                }
            }
        }

        public Player GetTarget()
        {
            return _targetPlayer;
        }
    }
}
