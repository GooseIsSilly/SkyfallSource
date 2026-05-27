using UnityEngine;

namespace TPSBR
{
    public class LiveEventExample : MonoBehaviour
    {
        [Header("Example: Subscribe to Events")]
        [SerializeField] private GameObject _effectToSpawn;
        [SerializeField] private Transform _spawnPoint;
        
        private void OnEnable()
        {
            if (LiveEventManager.Instance != null)
            {
                SubscribeToEvents();
            }
        }
        
        private void Start()
        {
            if (LiveEventManager.Instance != null)
            {
                SubscribeToEvents();
            }
        }
        
        private void OnDisable()
        {
            if (LiveEventManager.Instance != null)
            {
                UnsubscribeFromEvents();
            }
        }
        
        private void SubscribeToEvents()
        {
            LiveEventManager.Instance.OnEventStarted += OnEventStarted;
            LiveEventManager.Instance.OnCountdownUpdate += OnCountdownUpdate;
            LiveEventManager.Instance.OnEventTriggered += OnEventTriggered;
        }
        
        private void UnsubscribeFromEvents()
        {
            LiveEventManager.Instance.OnEventStarted -= OnEventStarted;
            LiveEventManager.Instance.OnCountdownUpdate -= OnCountdownUpdate;
            LiveEventManager.Instance.OnEventTriggered -= OnEventTriggered;
        }
        
        private void OnEventStarted(LiveEventData eventData, float remainingTime)
        {
            Debug.Log($"Event '{eventData.EventName}' countdown started! Time remaining: {remainingTime:F0}s");
        }
        
        private void OnCountdownUpdate(LiveEventData eventData, float remainingTime)
        {
            if (remainingTime <= 10f && remainingTime > 9f)
            {
                Debug.Log($"Event '{eventData.EventName}' starting in 10 seconds!");
            }
        }
        
        private void OnEventTriggered(LiveEventData eventData)
        {
            Debug.Log($"EVENT TRIGGERED: {eventData.EventName}");
            
            if (_effectToSpawn != null && _spawnPoint != null)
            {
                Instantiate(_effectToSpawn, _spawnPoint.position, _spawnPoint.rotation);
            }
        }
    }
}
