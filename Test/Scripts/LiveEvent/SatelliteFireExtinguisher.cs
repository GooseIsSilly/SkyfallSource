using System.Collections;
using UnityEngine;

namespace TPSBR
{
    /// <summary>
    /// Hides the satellite's fire VFX when a live event triggers.
    /// Attach to any persistent GameObject in the Game scene.
    /// </summary>
    public class SatelliteFireExtinguisher : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The root fire VFX GameObject inside the satellite (VFX_Stylized_Fire_01).")]
        [SerializeField] private GameObject _fireRoot;

        [Header("Settings")]
        [Tooltip("Seconds after the event triggers before the fire is hidden. 0 = instant.")]
        [SerializeField] private float _hideDelay = 0f;

        // ----------------------------------------------------------------

        private void OnEnable()
        {
            LiveEventManager.OnAnyEventTriggered += HandleEventTriggered;
        }

        private void OnDisable()
        {
            LiveEventManager.OnAnyEventTriggered -= HandleEventTriggered;
        }

        private void HandleEventTriggered(LiveEventData eventData)
        {
            if (_fireRoot == null)
            {
                Debug.LogWarning("[SatelliteFireExtinguisher] _fireRoot is not assigned.");
                return;
            }

            if (_hideDelay <= 0f)
            {
                HideFire();
            }
            else
            {
                StartCoroutine(HideFireAfterDelay(_hideDelay));
            }
        }

        private IEnumerator HideFireAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            HideFire();
        }

        /// <summary>Stops all particle emission and deactivates the fire root.</summary>
        private void HideFire()
        {
            // Stop every ParticleSystem in the hierarchy cleanly before hiding.
            foreach (ParticleSystem ps in _fireRoot.GetComponentsInChildren<ParticleSystem>(includeInactive: true))
            {
                ps.Stop(withChildren: true, stopBehavior: ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            _fireRoot.SetActive(false);

            Debug.Log("[SatelliteFireExtinguisher] Satellite fire hidden.");
        }
    }
}
