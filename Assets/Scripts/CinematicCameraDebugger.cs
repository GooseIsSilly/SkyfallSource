using UnityEngine;
using Cinemachine;

namespace TPSBR
{
    public class CinematicCameraDebugger : MonoBehaviour
    {
        private void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.green;
            style.fontSize = 16;
            style.normal.background = MakeTex(2, 2, new Color(0, 0, 0, 0.8f));
            style.padding = new RectOffset(10, 10, 10, 10);

            GUILayout.BeginArea(new Rect(10, 10, 500, 600), style);
            GUILayout.Label("=== CINEMACHINE DEBUG INFO ===");
            GUILayout.Space(10);

            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                mainCam = Object.FindFirstObjectByType<Camera>();
            }

            if (mainCam != null)
            {
                GUILayout.Label($"Main Camera: {mainCam.name}");
                GUILayout.Label($"Camera Tag: {mainCam.tag}");
                
                CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
                if (brain != null)
                {
                    GUILayout.Label($"CinemachineBrain: YES");
                    GUILayout.Label($"Active VCam: {(brain.ActiveVirtualCamera != null ? brain.ActiveVirtualCamera.Name : "None")}");
                    GUILayout.Label($"Brain Enabled: {brain.enabled}");
                }
                else
                {
                    GUILayout.Label("CinemachineBrain: NO - ADD IT!");
                }
            }
            else
            {
                GUILayout.Label("Main Camera: NOT FOUND!");
            }

            GUILayout.Space(10);
            GUILayout.Label("--- All Virtual Cameras ---");

            CinemachineVirtualCamera[] vcams = Object.FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None);
            if (vcams.Length > 0)
            {
                foreach (var vcam in vcams)
                {
                    string activeState = "";
                    if (mainCam != null)
                    {
                        CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();
                        if (brain != null && brain.ActiveVirtualCamera != null)
                        {
                            if (brain.ActiveVirtualCamera.VirtualCameraGameObject == vcam.gameObject)
                            {
                                activeState = " <-- ACTIVE";
                            }
                        }
                    }
                    
                    GUILayout.Label($"{vcam.name}: Priority {vcam.Priority}{activeState}");
                }
            }
            else
            {
                GUILayout.Label("NO VIRTUAL CAMERAS FOUND!");
            }

            GUILayout.Space(10);
            SeasonCinematicController controller = Object.FindFirstObjectByType<SeasonCinematicController>();
            if (controller != null)
            {
                GUILayout.Label($"Cinematic Controller: FOUND");
                GUILayout.Label($"Intro Active: {controller.IsIntroActive}");
            }
            else
            {
                GUILayout.Label("Cinematic Controller: NOT FOUND");
            }

            GUILayout.EndArea();
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
