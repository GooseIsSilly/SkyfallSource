using UnityEngine;
using UnityEditor;

namespace TPSBR
{
    public static class SeasonEndResetMenu
    {
        [MenuItem("Tools/Live Event/Reset Season Downtime")]
        public static void ResetSeasonDowntime()
        {
            PlayerPrefs.DeleteKey("SeasonEndController_SeasonEnded");
            PlayerPrefs.DeleteKey("SeasonEndController_SeasonVersion");
            PlayerPrefs.Save();
            
            Debug.Log("[SeasonEndResetMenu] ✅ Season downtime reset! PlayerPrefs cleared.");
            
            if (Application.isPlaying && SeasonEndController.Instance != null)
            {
                SeasonEndController.Instance.ResetDowntimeState();
                Debug.Log("[SeasonEndResetMenu] ✅ Runtime state reset in SeasonEndController.");
            }
            else
            {
                Debug.Log("[SeasonEndResetMenu] ℹ️ Not in Play mode - only PlayerPrefs cleared. Start Play mode to test event again.");
            }
        }
        
        [MenuItem("Tools/Live Event/Show Current Season State")]
        public static void ShowCurrentSeasonState()
        {
            bool seasonEnded = PlayerPrefs.GetInt("SeasonEndController_SeasonEnded", 0) == 1;
            string seasonVersion = PlayerPrefs.GetString("SeasonEndController_SeasonVersion", "None");
            
            string state = $"[Season State]\n" +
                          $"  Season Ended: {seasonEnded}\n" +
                          $"  Season Version: {seasonVersion}\n";
            
            if (Application.isPlaying && SeasonEndController.Instance != null)
            {
                state += $"  Is In Downtime: {SeasonEndController.Instance.IsInDowntime}\n";
                state += $"  Current Version: {SeasonEndController.Instance.CurrentVersion}";
            }
            
            Debug.Log(state);
            EditorUtility.DisplayDialog("Season State", state, "OK");
        }
    }
}
