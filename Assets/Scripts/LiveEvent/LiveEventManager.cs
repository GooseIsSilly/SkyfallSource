using System;
using System.Collections;
using Fusion;
using UnityEngine;

namespace TPSBR
{
    public class LiveEventManager : NetworkBehaviour
    {
        public static LiveEventManager Instance { get; private set; }
        
        [Header("Event Configuration")]
        [SerializeField] private LiveEventData[] _liveEvents;
        
        [Header("Audio Settings")]
        [Tooltip("AudioSource for playing event sounds (will be created if not assigned)")]
        [SerializeField] private AudioSource _eventAudioSource;
        
        [Tooltip("Global volume multiplier for event audio")]
        [SerializeField][Range(0f, 1f)] private float _masterVolume = 1f;
        
        [Networked] private int CurrentEventIndex { get; set; }
        [Networked] private TickTimer EventTriggerTimer { get; set; }
        [Networked] private NetworkBool EventHasTriggered { get; set; }
        [Networked] private NetworkBool IsEventInitialized { get; set; }
        
        private LiveEventData _activeEvent;
        private double _lastSyncTime;
        private const float SYNC_INTERVAL = 10f;
        private bool _initialized = false;
        
        public event Action<LiveEventData, float> OnCountdownUpdate;
        public event Action<LiveEventData> OnEventTriggered;
        public event Action<LiveEventData, float> OnEventStarted;
        
        public LiveEventData ActiveEvent => _activeEvent;
        public bool IsEventActive => _activeEvent != null;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            if (_eventAudioSource == null)
            {
                _eventAudioSource = gameObject.AddComponent<AudioSource>();
                _eventAudioSource.playOnAwake = false;
                _eventAudioSource.spatialBlend = 0f;
                _eventAudioSource.volume = _masterVolume;
                Debug.Log("[LiveEventManager] Created AudioSource for event audio");
            }
            else
            {
                _eventAudioSource.volume = _masterVolume;
            }
            
            Debug.Log("[LiveEventManager] Awake - Instance set");
        }
        
        public override void Spawned()
        {
            Debug.Log($"[LiveEventManager] Spawned! HasStateAuthority: {HasStateAuthority}, Runner: {Runner != null}");
            
            if (!_initialized && HasStateAuthority)
            {
                _initialized = true;
                InitializeServerEvents();
            }
            
            Runner.SetIsSimulated(Object, true);
            
            if (!HasStateAuthority && IsEventInitialized)
            {
                Debug.Log("[LiveEventManager] Client joined - syncing active event");
                SyncClientWithActiveEvent();
            }
        }
        
        private void Start()
        {
            StartCoroutine(CheckForSpawnedCallback());
        }
        
        private System.Collections.IEnumerator CheckForSpawnedCallback()
        {
            yield return new WaitForSeconds(2f);
            
            if (!_initialized)
            {
                Debug.LogWarning($"[LiveEventManager] Spawned() was not called! Runner: {Runner != null}, Object: {Object != null}, Object.IsValid: {Object != null && Object.IsValid}");
                
                if (Runner != null && Object != null && Object.IsValid)
                {
                    Debug.LogWarning("[LiveEventManager] Network is active but Spawned() wasn't called yet. Waiting for Spawned() callback...");
                }
                else
                {
                    Debug.LogError("[LiveEventManager] Cannot initialize - NetworkObject is not valid! This script requires Fusion networking to be active.");
                    Debug.LogError("[LiveEventManager] Make sure you're running as Host/Server and the NetworkObject is properly configured.");
                }
            }
        }
        
        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        private void InitializeServerEvents()
        {
            if (_liveEvents == null || _liveEvents.Length == 0)
            {
                Debug.LogWarning("No live events configured!");
                return;
            }
            
            CurrentEventIndex = -1;
            FindNextUpcomingEvent();
        }
        
        private void FindNextUpcomingEvent()
        {
            if (!HasStateAuthority) return;
            
            if (_liveEvents == null || _liveEvents.Length == 0)
            {
                Debug.LogWarning("[LiveEventManager] No live events to schedule");
                return;
            }
            
            LiveEventData nextEvent = null;
            int nextIndex = -1;
            double shortestTime = double.MaxValue;
            
            for (int i = 0; i < _liveEvents.Length; i++)
            {
                if (_liveEvents[i] == null)
                {
                    Debug.LogWarning($"[LiveEventManager] Live event at index {i} is null - skipping");
                    continue;
                }
                
                double timeUntil = _liveEvents[i].GetSecondsUntilEvent();
                
                if (timeUntil > 0 && timeUntil < shortestTime)
                {
                    shortestTime = timeUntil;
                    nextEvent = _liveEvents[i];
                    nextIndex = i;
                }
            }
            
            if (nextEvent != null)
            {
                CurrentEventIndex = nextIndex;
                _activeEvent = nextEvent;
                
                float timerDuration = (float)shortestTime;
                EventTriggerTimer = TickTimer.CreateFromSeconds(Runner, timerDuration);
                EventHasTriggered = false;
                IsEventInitialized = true;
                
                Debug.Log($"[LiveEventManager] Next event '{nextEvent.EventName}' scheduled in {timerDuration:F0} seconds");
                
                RPC_NotifyEventStarted(nextIndex, timerDuration);
            }
            else
            {
                Debug.LogWarning("[LiveEventManager] No upcoming events found!");
                _activeEvent = null;
                IsEventInitialized = false;
            }
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_NotifyEventStarted(int eventIndex, float remainingTime)
        {
            if (HasStateAuthority) return;
            
            if (eventIndex >= 0 && eventIndex < _liveEvents.Length)
            {
                _activeEvent = _liveEvents[eventIndex];
                Debug.Log($"[LiveEventManager] Client notified: Event '{_activeEvent.EventName}' starting in {remainingTime:F0}s");
                OnEventStarted?.Invoke(_activeEvent, remainingTime);
            }
        }
        
        private void SyncClientWithActiveEvent()
        {
            if (CurrentEventIndex >= 0 && CurrentEventIndex < _liveEvents.Length)
            {
                _activeEvent = _liveEvents[CurrentEventIndex];
                float remainingTime = EventTriggerTimer.RemainingTime(Runner) ?? 0f;
                
                Debug.Log($"[LiveEventManager] Client synced with active event: '{_activeEvent.EventName}', {remainingTime:F0}s remaining");
                
                OnEventStarted?.Invoke(_activeEvent, remainingTime);
            }
        }
        
        public override void FixedUpdateNetwork()
        {
            if (!IsEventActive) return;
            
            if (HasStateAuthority)
            {
                ServerUpdateEvent();
            }
            
            ClientUpdateEvent();
        }
        
        private void ServerUpdateEvent()
        {
            double currentTime = Runner.SimulationTime;
            
            if (currentTime - _lastSyncTime >= SYNC_INTERVAL)
            {
                _lastSyncTime = currentTime;
                SyncEventTimer();
            }
            
            if (!EventHasTriggered && EventTriggerTimer.Expired(Runner))
            {
                TriggerEvent();
            }
        }
        
        private void SyncEventTimer()
        {
            if (_activeEvent == null) return;
            
            double actualTimeUntil = _activeEvent.GetSecondsUntilEvent();
            
            if (actualTimeUntil <= 0)
            {
                if (!EventHasTriggered)
                {
                    TriggerEvent();
                }
                return;
            }
            
            float currentTimerRemaining = EventTriggerTimer.RemainingTime(Runner) ?? 0f;
            float drift = Mathf.Abs(currentTimerRemaining - (float)actualTimeUntil);
            
            if (drift > 2f)
            {
                Debug.Log($"[LiveEventManager] Syncing timer. Drift: {drift:F2}s. Resetting to {actualTimeUntil:F0}s");
                EventTriggerTimer = TickTimer.CreateFromSeconds(Runner, (float)actualTimeUntil);
            }
        }
        
        private void ClientUpdateEvent()
        {
            if (_activeEvent == null)
            {
                if (CurrentEventIndex >= 0 && CurrentEventIndex < _liveEvents.Length && IsEventInitialized)
                {
                    _activeEvent = _liveEvents[CurrentEventIndex];
                    
                    if (_activeEvent != null)
                    {
                        float remainingTime = EventTriggerTimer.RemainingTime(Runner) ?? 0f;
                        Debug.Log($"[LiveEventManager] Client initialized active event: '{_activeEvent.EventName}'");
                        OnEventStarted?.Invoke(_activeEvent, remainingTime);
                    }
                }
            }
            
            if (_activeEvent != null && IsEventInitialized)
            {
                float remainingTime = EventTriggerTimer.RemainingTime(Runner) ?? 0f;
                OnCountdownUpdate?.Invoke(_activeEvent, remainingTime);
            }
        }
        
        private void TriggerEvent()
        {
            if (!HasStateAuthority) return;
            
            EventHasTriggered = true;
            
            Debug.Log($"[LiveEventManager] Event '{_activeEvent.EventName}' triggered!");
            
            RPC_TriggerEventEffects();
            
            FindNextUpcomingEvent();
        }
        
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        private void RPC_TriggerEventEffects()
        {
            if (_activeEvent == null) return;
            
            Debug.Log($"[LiveEventManager] Playing event effects for '{_activeEvent.EventName}'");
            
            if (_activeEvent.EventAudio != null && _eventAudioSource != null)
            {
                _eventAudioSource.clip = _activeEvent.EventAudio;
                _eventAudioSource.volume = _masterVolume;
                _eventAudioSource.Play();
                Debug.Log($"[LiveEventManager] Playing audio: {_activeEvent.EventAudio.name} at volume {_masterVolume}");
            }
            else if (_activeEvent.EventAudio == null)
            {
                Debug.LogWarning($"[LiveEventManager] No audio clip assigned for event '{_activeEvent.EventName}'");
            }
            
            OnEventTriggered?.Invoke(_activeEvent);
            
            if (_activeEvent.EnableExplosion)
            {
                StartCoroutine(TriggerExplosionSequence(_activeEvent));
            }
            
            if (_activeEvent.IsSeasonEndEvent)
            {
                Debug.Log("[LiveEventManager] This is a season end event!");
                StartCoroutine(TriggerSeasonEndSequence(_activeEvent.SeasonEndDelay));
            }
        }
        
        private IEnumerator TriggerExplosionSequence(LiveEventData eventData)
        {
            if (eventData.ExplosionDelay > 0f)
            {
                Debug.Log($"[LiveEventManager] Waiting {eventData.ExplosionDelay}s before explosion...");
                yield return new WaitForSeconds(eventData.ExplosionDelay);
            }
            
            Debug.Log($"[LiveEventManager] Spawning explosion at {eventData.ExplosionPosition}");
            
            if (eventData.ExplosionPrefab != null)
            {
                GameObject explosion = Instantiate(eventData.ExplosionPrefab, eventData.ExplosionPosition, Quaternion.identity);
                
                if (eventData.ExplosionLifetime > 0f)
                {
                    Destroy(explosion, eventData.ExplosionLifetime);
                }
                
                Debug.Log($"[LiveEventManager] Explosion spawned at {eventData.ExplosionPosition}");
            }
            else
            {
                Debug.LogWarning("[LiveEventManager] Explosion enabled but no ExplosionPrefab assigned!");
            }
            
            if (eventData.ExplosionSound != null)
            {
                AudioSource.PlayClipAtPoint(eventData.ExplosionSound, eventData.ExplosionPosition, _masterVolume);
                Debug.Log($"[LiveEventManager] Explosion sound played at {eventData.ExplosionPosition}");
            }
            else if (eventData.EnableExplosion)
            {
                Debug.LogWarning("[LiveEventManager] Explosion enabled but no ExplosionSound assigned!");
            }
        }
        
        private IEnumerator TriggerSeasonEndSequence(float delay)
        {
            Debug.Log($"[LiveEventManager] Waiting {delay} seconds before season end...");
            yield return new WaitForSeconds(delay);
            
            if (SeasonEndController.Instance != null)
            {
                Debug.Log("[LiveEventManager] Triggering season end controller...");
                SeasonEndController.Instance.TriggerSeasonEnd();
            }
            else
            {
                Debug.LogWarning("[LiveEventManager] SeasonEndController not found in scene!");
            }
        }
        
        public float GetRemainingTime()
        {
            if (!IsEventActive) return 0f;
            return EventTriggerTimer.RemainingTime(Runner) ?? 0f;
        }
        
        public bool IsCountdownActive()
        {
            return IsEventActive && GetRemainingTime() > 0f;
        }
        
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            if (_eventAudioSource != null)
            {
                _eventAudioSource.volume = _masterVolume;
            }
            Debug.Log($"[LiveEventManager] Master volume set to {_masterVolume}");
        }
        
        public void StopEventAudio()
        {
            if (_eventAudioSource != null && _eventAudioSource.isPlaying)
            {
                _eventAudioSource.Stop();
                Debug.Log("[LiveEventManager] Event audio stopped");
            }
        }
    }
}
