using UnityEngine;

namespace TPSBR
{
    [RequireComponent(typeof(AudioSource))]
    public class ItemBoxProximitySound : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField]
        private float _radius = 6f;
        [SerializeField]
        private LayerMask _playerLayer;

        [Header("Audio")]
        [SerializeField]
        private AudioClip _loopClip;
        [SerializeField]
        private float _fadeInTime = 0.3f;
        [SerializeField]
        private float _fadeOutTime = 0.3f;
        [SerializeField]
        private float _maxVolume = 1f;

        private AudioSource _audioSource;
        private SphereCollider _triggerCollider;
        private bool _isOpened;
        private bool _isInRange;
        private float _targetVolume;

        // PUBLIC METHODS

        /// <summary>Call when the box opens � stops/cancels the loop immediately.</summary>
        public void OnOpened()
        {
            _isOpened = true;
            _targetVolume = 0f;
            // NOTE: intentionally not clearing _isInRange here � OnClosed() needs to know
            // whether the player is still physically in the trigger to correctly re-arm.
        }

        /// <summary>Call when the box becomes interactable again (re-locked/closed) so proximity sound can trigger again.</summary>
        public void OnClosed()
        {
            _isOpened = false;

            // Player may still be standing in the trigger from before the box opened �
            // re-check proximity manually since OnTriggerEnter won't fire again.
            if (_isInRange == true)
            {
                _targetVolume = _maxVolume;

                if (_audioSource.isPlaying == false && _loopClip != null)
                {
                    _audioSource.Play();
                }
            }
        }

        // MONOBEHAVIOUR

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.clip = _loopClip;
            _audioSource.loop = true;
            _audioSource.playOnAwake = false;
            _audioSource.volume = 0f;

            _triggerCollider = gameObject.AddComponent<SphereCollider>();
            _triggerCollider.isTrigger = true;
            _triggerCollider.radius = _radius;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsRelevant(other) == false)
                return;

            _isInRange = true;

            if (_isOpened == true)
                return;

            _targetVolume = _maxVolume;

            if (_audioSource.isPlaying == false && _loopClip != null)
            {
                _audioSource.Play();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsRelevant(other) == false)
                return;

            _isInRange = false;
            _targetVolume = 0f;
        }

        private void Update()
        {
            if (_audioSource.isPlaying == false)
                return;

            float fadeSpeed = _targetVolume > _audioSource.volume
                ? (_fadeInTime > 0f ? _maxVolume / _fadeInTime : 999f)
                : (_fadeOutTime > 0f ? _maxVolume / _fadeOutTime : 999f);

            _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, _targetVolume, fadeSpeed * Time.deltaTime);

            if (_audioSource.volume <= 0.0001f && _targetVolume <= 0.0001f)
            {
                _audioSource.Stop();
            }
        }

        private void OnValidate()
        {
            if (_triggerCollider != null)
            {
                _triggerCollider.radius = _radius;
            }
        }

        // PRIVATE METHODS

        private bool IsRelevant(Collider other)
        {
            if (_playerLayer.value != 0 && ((1 << other.gameObject.layer) & _playerLayer.value) == 0)
                return false;

            // Only react to the local player's Agent (recommended for multiplayer) so the proximity
            // sound reflects distance from the listener, not from remote players replicated in the scene.
            Agent agent = other.GetComponentInParent<Agent>();
            return agent != null && agent.HasInputAuthority == true;
        }
    }
}