using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSBR
{
    public class SeasonEndTester : MonoBehaviour
    {
        [Header("Test Controls")]
        [Tooltip("Press F9 to manually trigger season end")]
        private bool _enableTesting = true;
        
        private void Update()
        {
            if (!_enableTesting) return;
            
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f9Key.wasPressedThisFrame)
            {
                TriggerSeasonEndTest();
            }
        }
        
        [ContextMenu("Test Season End")]
        public void TriggerSeasonEndTest()
        {
            Debug.Log("[SeasonEndTester] Manual season end test triggered!");
            
            if (SeasonEndController.Instance != null)
            {
                SeasonEndController.Instance.TriggerSeasonEnd();
            }
            else
            {
                Debug.LogError("[SeasonEndTester] SeasonEndController instance not found!");
            }
        }
    }
}
