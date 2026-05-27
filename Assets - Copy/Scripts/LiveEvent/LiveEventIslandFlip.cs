using UnityEngine;

namespace TPSBR
{
    public class LiveEventIslandFlip : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The Live Event to listen to")]
        [SerializeField] private LiveEventData _targetEvent;
        
        [Tooltip("The GameObject to flip (usually your island/map root)")]
        [SerializeField] private Transform _islandTransform;
        
        [Tooltip("Terrain object to flip separately (if using Unity Terrain)")]
        [SerializeField] private Terrain _terrainToFlip;
        
        [Header("Settings")]
        [Tooltip("Use custom flip settings instead of event settings")]
        [SerializeField] private bool _useCustomSettings = false;
        
        [SerializeField] private float _customFlipDuration = 120f;
        [SerializeField] private Vector3 _customFlipRotation = new Vector3(180f, 0f, 0f);
        
        private bool _isFlipping = false;
        private float _flipProgress = 0f;
        private Quaternion _startRotation;
        private Quaternion _targetRotation;
        private float _flipDuration;
        
        private Vector3 _terrainStartPosition;
        private Quaternion _terrainStartRotation;
        private Vector3 _terrainTargetPosition;
        private Quaternion _terrainTargetRotation;
        
        private void OnEnable()
        {
            if (_targetEvent != null && LiveEventManager.Instance != null)
            {
                LiveEventManager.Instance.OnEventTriggered += OnEventTriggered;
                Debug.Log($"[LiveEventIslandFlip] Subscribed to event '{_targetEvent.EventName}'");
            }
        }
        
        private void OnDisable()
        {
            if (_targetEvent != null && LiveEventManager.Instance != null)
            {
                LiveEventManager.Instance.OnEventTriggered -= OnEventTriggered;
            }
        }
        
        private void Start()
        {
            if (_targetEvent != null && LiveEventManager.Instance != null)
            {
                LiveEventManager.Instance.OnEventTriggered += OnEventTriggered;
                Debug.Log($"[LiveEventIslandFlip] Subscribed to event '{_targetEvent.EventName}' in Start");
            }
        }
        
        private void OnEventTriggered(LiveEventData eventData)
        {
            if (eventData != _targetEvent)
            {
                return;
            }
            
            if (!_targetEvent.EnableIslandFlip && !_useCustomSettings)
            {
                Debug.Log($"[LiveEventIslandFlip] Island flip disabled for event '{eventData.EventName}'");
                return;
            }
            
            StartIslandFlip();
        }
        
        private void StartIslandFlip()
        {
            if (_islandTransform == null && _terrainToFlip == null)
            {
                Debug.LogError("[LiveEventIslandFlip] Neither Island Transform nor Terrain assigned!");
                return;
            }
            
            _isFlipping = true;
            _flipProgress = 0f;
            
            Vector3 flipRotation = _useCustomSettings ? _customFlipRotation : _targetEvent.FlipRotation;
            _flipDuration = _useCustomSettings ? _customFlipDuration : _targetEvent.FlipDuration;
            
            if (_islandTransform != null)
            {
                _startRotation = _islandTransform.rotation;
                _targetRotation = _startRotation * Quaternion.Euler(flipRotation);
            }
            
            if (_terrainToFlip != null)
            {
                _terrainStartPosition = _terrainToFlip.transform.position;
                _terrainStartRotation = _terrainToFlip.transform.rotation;
                
                Vector3 pivotPoint = _islandTransform != null ? _islandTransform.position : _terrainToFlip.transform.position;
                
                _terrainTargetRotation = _terrainStartRotation * Quaternion.Euler(flipRotation);
                
                Vector3 offset = _terrainStartPosition - pivotPoint;
                Vector3 rotatedOffset = Quaternion.Euler(flipRotation) * offset;
                _terrainTargetPosition = pivotPoint + rotatedOffset;
                
                Debug.Log($"[LiveEventIslandFlip] Terrain flip - Start Pos: {_terrainStartPosition}, Target Pos: {_terrainTargetPosition}");
            }
            
            Debug.Log($"[LiveEventIslandFlip] Starting island flip! Duration: {_flipDuration}s, Rotation: {flipRotation}");
        }
        
        private void Update()
        {
            if (!_isFlipping)
            {
                return;
            }
            
            _flipProgress += Time.deltaTime / _flipDuration;
            
            float smoothProgress = Mathf.SmoothStep(0f, 1f, _flipProgress);
            
            if (_islandTransform != null)
            {
                _islandTransform.rotation = Quaternion.Slerp(_startRotation, _targetRotation, smoothProgress);
            }
            
            if (_terrainToFlip != null)
            {
                _terrainToFlip.transform.rotation = Quaternion.Slerp(_terrainStartRotation, _terrainTargetRotation, smoothProgress);
                _terrainToFlip.transform.position = Vector3.Lerp(_terrainStartPosition, _terrainTargetPosition, smoothProgress);
            }
            
            if (Time.frameCount % 60 == 0)
            {
                string rotationInfo = _islandTransform != null ? _islandTransform.rotation.eulerAngles.ToString() : _terrainToFlip.transform.rotation.eulerAngles.ToString();
                Debug.Log($"[LiveEventIslandFlip] Flipping... Progress: {_flipProgress * 100f:F1}%, Rotation: {rotationInfo}");
            }
            
            if (_flipProgress >= 1f)
            {
                _isFlipping = false;
                
                if (_islandTransform != null)
                {
                    _islandTransform.rotation = _targetRotation;
                }
                
                if (_terrainToFlip != null)
                {
                    _terrainToFlip.transform.rotation = _terrainTargetRotation;
                    _terrainToFlip.transform.position = _terrainTargetPosition;
                }
                
                Debug.Log("[LiveEventIslandFlip] Island flip complete!");
            }
        }
        
        [ContextMenu("Test Island Flip")]
        private void TestFlip()
        {
            Debug.Log("[LiveEventIslandFlip] TEST FLIP TRIGGERED FROM CONTEXT MENU!");
            
            if (_islandTransform == null && _terrainToFlip == null)
            {
                Debug.LogError("[LiveEventIslandFlip] Neither Island Transform nor Terrain assigned!");
                if (_islandTransform == null)
                {
                    _islandTransform = transform;
                }
            }
            
            if (_islandTransform != null)
            {
                Debug.Log($"[LiveEventIslandFlip] Island Transform: {_islandTransform.name}, Current Rotation: {_islandTransform.rotation.eulerAngles}");
            }
            
            if (_terrainToFlip != null)
            {
                Debug.Log($"[LiveEventIslandFlip] Terrain: {_terrainToFlip.name}, Current Rotation: {_terrainToFlip.transform.rotation.eulerAngles}, Position: {_terrainToFlip.transform.position}");
            }
            
            StartIslandFlip();
        }
        
        [ContextMenu("Reset Island Rotation")]
        private void ResetRotation()
        {
            if (_islandTransform != null)
            {
                _islandTransform.rotation = Quaternion.identity;
                Debug.Log("[LiveEventIslandFlip] Island rotation reset!");
            }
            
            if (_terrainToFlip != null)
            {
                _terrainToFlip.transform.rotation = Quaternion.identity;
                _terrainToFlip.transform.position = _terrainStartPosition;
                Debug.Log("[LiveEventIslandFlip] Terrain rotation and position reset!");
            }
            
            _isFlipping = false;
            _flipProgress = 0f;
        }
    }
}
