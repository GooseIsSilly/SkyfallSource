using UnityEngine;

namespace TPSBR
{
    public class StartupCharacterDisplay : MonoBehaviour
    {
        [Header("Character Model")]
        [SerializeField] private GameObject _characterPrefab;
        [SerializeField] private Transform _characterSpawnPoint;
        
        [Header("Animation")]
        [SerializeField] private string _animationStateName = "Idle";
        [SerializeField] private bool _playOnStart = true;
        
        [Header("Camera Animation (Optional)")]
        [SerializeField] private Animator _cameraAnimator;
        [SerializeField] private string _cameraAnimationTrigger = "Show";
        
        private GameObject _spawnedCharacter;
        private Animator _characterAnimator;

        private void Start()
        {
            if (_playOnStart)
            {
                SpawnCharacter();
                PlayAnimation();
                
                if (_cameraAnimator != null && !string.IsNullOrEmpty(_cameraAnimationTrigger))
                {
                    _cameraAnimator.SetTrigger(_cameraAnimationTrigger);
                }
            }
        }

        public void SpawnCharacter()
        {
            if (_characterPrefab == null)
            {
                Debug.LogWarning("[StartupCharacterDisplay] No character prefab assigned!");
                return;
            }

            // Clean up existing character
            if (_spawnedCharacter != null)
            {
                Destroy(_spawnedCharacter);
            }

            // Determine spawn point
            Transform spawnPoint = _characterSpawnPoint != null ? _characterSpawnPoint : transform;

            // Spawn character
            _spawnedCharacter = Instantiate(_characterPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            
            // Get animator
            _characterAnimator = _spawnedCharacter.GetComponent<Animator>();
            if (_characterAnimator == null)
            {
                _characterAnimator = _spawnedCharacter.GetComponentInChildren<Animator>();
            }

            if (_characterAnimator == null)
            {
                Debug.LogWarning("[StartupCharacterDisplay] Spawned character has no Animator component!");
            }

            Debug.Log($"[StartupCharacterDisplay] Spawned character: {_characterPrefab.name}");
        }

        public void PlayAnimation()
        {
            if (_characterAnimator == null)
            {
                Debug.LogWarning("[StartupCharacterDisplay] No character animator available!");
                return;
            }

            if (string.IsNullOrEmpty(_animationStateName))
            {
                Debug.LogWarning("[StartupCharacterDisplay] No animation state name set!");
                return;
            }

            // Try to play the animation
            if (HasState(_animationStateName))
            {
                _characterAnimator.Play(_animationStateName, 0, 0f);
                Debug.Log($"[StartupCharacterDisplay] Playing animation: {_animationStateName}");
            }
            else
            {
                // Try as a trigger
                _characterAnimator.SetTrigger(_animationStateName);
                Debug.Log($"[StartupCharacterDisplay] Triggering animation: {_animationStateName}");
            }
        }

        public void SetAnimation(string stateName)
        {
            _animationStateName = stateName;
            PlayAnimation();
        }

        public void ChangeCharacter(GameObject newCharacterPrefab)
        {
            _characterPrefab = newCharacterPrefab;
            SpawnCharacter();
            PlayAnimation();
        }

        private bool HasState(string stateName)
        {
            if (_characterAnimator == null || _characterAnimator.runtimeAnimatorController == null)
                return false;

            foreach (var clip in _characterAnimator.runtimeAnimatorController.animationClips)
            {
                if (clip.name == stateName)
                    return true;
            }

            return false;
        }

        private void OnValidate()
        {
            // Auto-find spawn point if not set
            if (_characterSpawnPoint == null && transform.childCount > 0)
            {
                _characterSpawnPoint = transform.GetChild(0);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("List Available Animations")]
        private void ListAvailableAnimations()
        {
            if (_characterAnimator == null)
            {
                Debug.Log("No animator assigned. Spawn a character first.");
                return;
            }

            if (_characterAnimator.runtimeAnimatorController == null)
            {
                Debug.Log("No animator controller assigned.");
                return;
            }

            Debug.Log("=== Available Animations ===");
            foreach (var clip in _characterAnimator.runtimeAnimatorController.animationClips)
            {
                Debug.Log($"- {clip.name} ({clip.length}s)");
            }
        }
#endif
    }
}
