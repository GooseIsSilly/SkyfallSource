using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace TPSBR
{
    public class LocomotionBlender : MonoBehaviour
    {
        private AnimationMixerPlayable _mixer;
        private bool _isInitialized;
        private float _lastLogTime;

        public void Initialize(AnimationMixerPlayable mixer)
        {
            _mixer = mixer;
            _isInitialized = true;
            Debug.Log("[LocomotionBlender] Initialized with mixer");
        }

        public void UpdateBlend(float speed)
        {
            if (!_isInitialized)
            {
                if (Time.time - _lastLogTime > 1f)
                {
                    Debug.LogWarning("[LocomotionBlender] UpdateBlend called but not initialized!");
                    _lastLogTime = Time.time;
                }
                return;
            }

            float normalizedSpeed = Mathf.Clamp01(speed);

            _mixer.SetInputWeight(0, 1f - normalizedSpeed);
            _mixer.SetInputWeight(1, normalizedSpeed);

            if (Time.time - _lastLogTime > 2f)
            {
                Debug.Log($"[LocomotionBlender] Speed: {speed:F2}, Idle weight: {(1f - normalizedSpeed):F2}, Walk weight: {normalizedSpeed:F2}");
                _lastLogTime = Time.time;
            }
        }
    }
}
