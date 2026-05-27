using UnityEngine;
using UnityEngine.Rendering;

namespace TPSBR
{
    [ExecuteInEditMode]
    public class LobbyLightingFixer : MonoBehaviour
    {
        [Header("Settings")]
        public bool SyncLightingOnStart = true;
        public Color AmbientColor = new Color(0.7f, 0.7f, 0.8f);
        
        void Start()
        {
            if (SyncLightingOnStart)
            {
                ApplyLobbyLighting();
            }
        }

        public void ApplyLobbyLighting()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = AmbientColor;
            
            // Force a refresh of the skybox if one exists in the scene
            DynamicGI.UpdateEnvironment();
        }
    }
}
