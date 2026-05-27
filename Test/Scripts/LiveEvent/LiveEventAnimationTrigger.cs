using UnityEngine;

namespace TPSBR
{
    [RequireComponent(typeof(Animation))]
    public class LiveEventAnimationTrigger : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("The specific event that triggers this animation")]
        [SerializeField] private LiveEventData _targetEvent;
        
        [Tooltip("Override animation clip (uses event's clip if null)")]
        [SerializeField] private AnimationClip _customAnimationClip;
        
        [Tooltip("Play audio during animation")]
        [SerializeField] private bool _playAudio = true;
        
        private Animation _animation;
        private AudioSource _audioSource;
        private bool _hasTriggered;
        private bool _isSubscribed = false;
        
        private void Awake()
        {
            _animation = GetComponent<Animation>();
            _audioSource = GetComponent<AudioSource>();
            
            if (_audioSource == null && _playAudio)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 1f;
            }
        }
        
        private void OnEnable()
        {
            TrySubscribe();
        }
        
        private void Update()
        {
            if (!_isSubscribed)
            {
                TrySubscribe();
            }
        }
        
        private void TrySubscribe()
        {
            if (!_isSubscribed && LiveEventManager.Instance != null)
            {
                LiveEventManager.Instance.OnEventTriggered += HandleEventTriggered;
                LiveEventManager.Instance.OnEventStarted += HandleEventStarted;
                _isSubscribed = true;
                Debug.Log($"[LiveEventAnimationTrigger] '{gameObject.name}' subscribed to LiveEventManager events");
            }
        }
        
        private void OnDisable()
        {
            if (_isSubscribed && LiveEventManager.Instance != null)
            {
                LiveEventManager.Instance.OnEventTriggered -= HandleEventTriggered;
                LiveEventManager.Instance.OnEventStarted -= HandleEventStarted;
                _isSubscribed = false;
            }
        }
        
        private void HandleEventStarted(LiveEventData eventData, float remainingTime)
        {
            if (_targetEvent == null || eventData != _targetEvent) return;
            
            _hasTriggered = false;
        }
        
        private void HandleEventTriggered(LiveEventData eventData)
        {
            if (_hasTriggered) return;
            if (_targetEvent == null || eventData != _targetEvent) return;
            
            _hasTriggered = true;
            PlayAnimation(eventData);
        }
        
        private void PlayAnimation(LiveEventData eventData)
        {
            if (_animation == null)
            {
                _animation = GetComponent<Animation>();
            }
            
            if (_animation == null)
            {
                Debug.LogError($"[LiveEventAnimationTrigger] Animation component not found on '{gameObject.name}'!");
                return;
            }
            
            AnimationClip clipToPlay = _customAnimationClip != null ? _customAnimationClip : eventData.EventAnimation;
            
            if (clipToPlay == null)
            {
                Debug.LogWarning($"[LiveEventAnimationTrigger] No animation clip assigned for event '{eventData.EventName}'");
                return;
            }
            
            _animation.Stop();
            
            clipToPlay.wrapMode = WrapMode.Once;
            clipToPlay.legacy = true;
            
            if (_animation.GetClip(clipToPlay.name) == null)
            {
                _animation.AddClip(clipToPlay, clipToPlay.name);
                Debug.Log($"[LiveEventAnimationTrigger] Added clip '{clipToPlay.name}' to Animation component");
            }
            
            AnimationState animState = _animation[clipToPlay.name];
            if (animState != null)
            {
                animState.wrapMode = WrapMode.Once;
                animState.enabled = true;
                animState.weight = 1.0f;
                animState.speed = 1.0f;
            }
            
            _animation.clip = clipToPlay;
            bool didPlay = _animation.Play(clipToPlay.name, PlayMode.StopAll);
            
            Debug.Log($"[LiveEventAnimationTrigger] Play('{clipToPlay.name}') with PlayMode.StopAll returned: {didPlay}, isPlaying: {_animation.isPlaying}, enabled: {_animation.enabled}, clip length: {clipToPlay.length}s");
            
            if (_playAudio && _audioSource != null)
            {
                AudioClip audioClip = eventData.EventAudio;
                if (audioClip != null)
                {
                    _audioSource.clip = audioClip;
                    _audioSource.Play();
                    Debug.Log($"[LiveEventAnimationTrigger] Playing audio '{audioClip.name}'");
                }
            }
        }
        
        public void ManualTrigger()
        {
            if (_targetEvent != null)
            {
                PlayAnimation(_targetEvent);
            }
        }
    }
}
