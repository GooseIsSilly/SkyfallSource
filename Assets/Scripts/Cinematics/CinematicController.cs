using UnityEngine;
using UnityEngine.InputSystem;

namespace Skyfall.Cinematics
{
    public class CinematicController : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The cinematic camera manager")]
        public CinematicCameraManager cameraManager;
        
        [Tooltip("The cinematic recorder")]
        public CinematicRecorder recorder;
        
        [Header("Input Settings")]
        [Tooltip("Enable keyboard controls")]
        public bool enableKeyboardControls = true;
        
        [Header("Keyboard Shortcuts")]
        public Key playStopKey = Key.Space;
        public Key nextShotKey = Key.RightArrow;
        public Key previousShotKey = Key.LeftArrow;
        public Key recordKey = Key.R;
        public Key screenshotKey = Key.F12;
        
        [Header("UI Display")]
        public bool showUI = true;
        public KeyCode toggleUIKey = KeyCode.H;
        
        private bool uiVisible = true;
        
        void Update()
        {
            if (!enableKeyboardControls) return;
            
            if (Keyboard.current[playStopKey].wasPressedThisFrame)
            {
                TogglePlayback();
            }
            
            if (Keyboard.current[nextShotKey].wasPressedThisFrame)
            {
                cameraManager?.NextShot();
            }
            
            if (Keyboard.current[previousShotKey].wasPressedThisFrame)
            {
                cameraManager?.PreviousShot();
            }
            
            if (Keyboard.current[recordKey].wasPressedThisFrame)
            {
                recorder?.ToggleRecording();
            }
            
            if (Keyboard.current[screenshotKey].wasPressedThisFrame)
            {
                CaptureScreenshot();
            }
            
            if (Input.GetKeyDown(toggleUIKey))
            {
                uiVisible = !uiVisible;
            }
        }
        
        void TogglePlayback()
        {
            if (cameraManager == null) return;
            
            if (cameraManager.IsPlaying())
            {
                cameraManager.StopSequence();
            }
            else
            {
                cameraManager.PlaySequence();
            }
        }
        
        void CaptureScreenshot()
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"screenshot_{timestamp}.png";
            ScreenCapture.CaptureScreenshot(filename);
            Debug.Log($"Screenshot saved: {filename}");
        }
        
        void OnGUI()
        {
            if (!showUI || !uiVisible) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("CINEMATIC CONTROLS", GUI.skin.box);
            
            if (cameraManager != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Shot: {cameraManager.GetCurrentShotIndex() + 1}/{cameraManager.shots.Count}");
                GUILayout.Label($"Status: {(cameraManager.IsPlaying() ? "Playing" : "Stopped")}");
                GUILayout.Label($"Total Duration: {cameraManager.GetTotalDuration():F1}s");
                
                GUILayout.Space(10);
                
                if (GUILayout.Button(cameraManager.IsPlaying() ? "Stop (Space)" : "Play (Space)"))
                {
                    TogglePlayback();
                }
                
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Previous (←)"))
                {
                    cameraManager.PreviousShot();
                }
                if (GUILayout.Button("Next (→)"))
                {
                    cameraManager.NextShot();
                }
                GUILayout.EndHorizontal();
            }
            
            if (recorder != null)
            {
                GUILayout.Space(10);
                GUILayout.Label($"Recording: {(recorder.IsRecording() ? "YES" : "NO")}");
                if (recorder.IsRecording())
                {
                    GUILayout.Label($"Frames: {recorder.GetFrameCount()}");
                }
                
                if (GUILayout.Button(recorder.IsRecording() ? "Stop Recording (R)" : "Start Recording (R)"))
                {
                    recorder.ToggleRecording();
                }
            }
            
            GUILayout.Space(10);
            if (GUILayout.Button("Screenshot (F12)"))
            {
                CaptureScreenshot();
            }
            
            GUILayout.Space(10);
            GUILayout.Label($"Press {toggleUIKey} to hide UI");
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
