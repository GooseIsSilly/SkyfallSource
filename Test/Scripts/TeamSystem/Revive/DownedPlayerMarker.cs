using UnityEngine;

namespace TPSBR
{
    public class DownedPlayerMarker : MonoBehaviour
    {
        [SerializeField]
        private GameObject _markerVisual;
        [SerializeField]
        private Color _downedColor = new Color(1f, 0.5f, 0f);
        [SerializeField]
        private float _pulseSpeed = 2f;
        [SerializeField]
        private float _heightOffset = 2f;

        private ReviveSystem _reviveSystem;
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _propertyBlock;
        private float _pulseTimer;

        private void Awake()
        {
            _reviveSystem = GetComponentInParent<ReviveSystem>();

            if (_markerVisual != null)
            {
                _renderer = _markerVisual.GetComponent<MeshRenderer>();
                if (_renderer != null)
                {
                    _propertyBlock = new MaterialPropertyBlock();
                }
            }
        }

        private void Update()
        {
            if (_reviveSystem == null)
                return;

            if (_markerVisual != null)
            {
                _markerVisual.SetActive(_reviveSystem.IsDown);
            }

            if (_reviveSystem.IsDown)
            {
                UpdateMarkerPosition();
                UpdateMarkerPulse();
            }
        }

        private void UpdateMarkerPosition()
        {
            if (_markerVisual == null)
                return;

            Vector3 position = transform.position;
            position.y += _heightOffset;
            _markerVisual.transform.position = position;
        }

        private void UpdateMarkerPulse()
        {
            if (_renderer == null || _propertyBlock == null)
                return;

            _pulseTimer += Time.deltaTime * _pulseSpeed;
            float alpha = (Mathf.Sin(_pulseTimer) + 1f) * 0.5f;
            alpha = Mathf.Lerp(0.5f, 1f, alpha);

            Color color = _downedColor;
            color.a = alpha;

            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor("_Color", color);
            _renderer.SetPropertyBlock(_propertyBlock);
        }
    }
}
