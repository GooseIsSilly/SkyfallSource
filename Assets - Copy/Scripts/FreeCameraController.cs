using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

namespace Skyfall
{
    public class FreeCameraController : MonoBehaviour
    {
        [Header("Activation")]
        [Tooltip("Key combination to toggle free camera (e.g., F9, F10, etc.)")]
        public Key[] activationKeys = new Key[] { Key.LeftCtrl, Key.LeftShift, Key.F };
        
        [Tooltip("Allow activation only in specific builds")]
        public bool developmentBuildOnly = true;
        
        [Header("Movement Settings")]
        [Tooltip("Base movement speed")]
        public float moveSpeed = 10f;
        
        [Tooltip("Speed multiplier when holding Shift")]
        public float sprintMultiplier = 3f;
        
        [Tooltip("Speed multiplier when holding Ctrl")]
        public float slowMultiplier = 0.3f;
        
        [Tooltip("Smooth movement damping (higher = smoother but more lag)")]
        [Range(0f, 0.95f)]
        public float movementSmoothing = 0.25f;
        
        [Header("Look Settings")]
        [Tooltip("Mouse sensitivity")]
        public float lookSensitivity = 2f;
        
        [Tooltip("Smooth rotation damping (higher = smoother but more lag)")]
        [Range(0f, 0.95f)]
        public float rotationSmoothing = 0.15f;
        
        [Tooltip("Invert Y axis")]
        public bool invertY = false;
        
        [Header("Roll Settings")]
        [Tooltip("Enable camera roll with Q/E keys")]
        public bool enableRoll = true;
        
        [Tooltip("Roll speed in degrees per second")]
        public float rollSpeed = 45f;
        
        [Header("FOV Settings")]
        [Tooltip("Enable FOV adjustment with mouse wheel")]
        public bool enableFOVControl = true;
        
        [Tooltip("Minimum field of view")]
        public float minFOV = 20f;
        
        [Tooltip("Maximum field of view")]
        public float maxFOV = 120f;
        
        [Tooltip("FOV change speed")]
        public float fovChangeSpeed = 10f;
        
        [Header("Recording Settings")]
        [Tooltip("Key to start/stop recording")]
        public Key recordingKey = Key.F9;
        
        [Tooltip("Target framerate for recording")]
        public int targetFrameRate = 60;
        
        [Tooltip("Output folder for recordings")]
        public string recordingFolder = "TrailerFootage";
        
        [Tooltip("Recording resolution width")]
        public int recordingWidth = 1920;
        
        [Tooltip("Recording resolution height")]
        public int recordingHeight = 1080;
        
        [Tooltip("Image format for frames")]
        public ImageFormat imageFormat = ImageFormat.PNG;
        
        [Tooltip("Hide UI during recording")]
        public bool hideUIDuringRecording = true;
        
        [Header("UI")]
        [Tooltip("Show free camera UI")]
        public bool showUI = true;
        
        public enum ImageFormat
        {
            PNG,
            JPG
        }
        
        private bool isActive = false;
        private Camera freeCameraComponent;
        private Camera originalCamera;
        private Vector3 targetPosition;
        private Vector3 currentVelocity;
        private Vector2 currentRotation;
        private Vector2 targetRotation;
        private Vector2 rotationVelocity;
        private float currentRoll = 0f;
        private float currentFOV = 60f;
        private bool cursorWasVisible;
        private CursorLockMode previousCursorLockMode;
        
        private bool isRecording = false;
        private int frameCount = 0;
        private string currentRecordingPath;
        private float recordingStartTime;
        private int originalTargetFrameRate;
        
        private const float MAX_VERTICAL_ANGLE = 89f;
        
        void Start()
        {
            freeCameraComponent = GetComponent<Camera>();
            if (freeCameraComponent == null)
            {
                freeCameraComponent = gameObject.AddComponent<Camera>();
            }
            
            freeCameraComponent.farClipPlane = 90000f;
            freeCameraComponent.enabled = false;
            currentFOV = freeCameraComponent.fieldOfView;
            
            targetPosition = transform.position;
            currentRotation = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
            targetRotation = currentRotation;
        }
        
        void Update()
        {
            CheckActivationInput();
            
            if (isActive)
            {
                HandleMovement();
                HandleRotation();
                HandleRoll();
                HandleFOV();
                HandleScreenshot();
                HandleRecording();
            }
        }
        
        void LateUpdate()
        {
            if (isRecording)
            {
                CaptureFrame();
            }
        }
        
        void CheckActivationInput()
        {
#if !UNITY_EDITOR
            if (developmentBuildOnly && !Debug.isDebugBuild)
            {
                return;
            }
#endif
            
            bool allKeysPressed = true;
            foreach (Key key in activationKeys)
            {
                if (!Keyboard.current[key].isPressed)
                {
                    allKeysPressed = false;
                    break;
                }
            }
            
            if (allKeysPressed && Keyboard.current[activationKeys[activationKeys.Length - 1]].wasPressedThisFrame)
            {
                ToggleFreeCamera();
            }
        }
        
        void ToggleFreeCamera()
        {
            isActive = !isActive;
            
            if (isActive)
            {
                ActivateFreeCamera();
            }
            else
            {
                DeactivateFreeCamera();
            }
        }
        
        void ActivateFreeCamera()
        {
            originalCamera = Camera.main;
            
            if (originalCamera != null)
            {
                transform.position = originalCamera.transform.position;
                transform.rotation = originalCamera.transform.rotation;
                
                freeCameraComponent.farClipPlane = 90000f;
                freeCameraComponent.nearClipPlane = originalCamera.nearClipPlane;
                freeCameraComponent.fieldOfView = originalCamera.fieldOfView;
                freeCameraComponent.clearFlags = originalCamera.clearFlags;
                freeCameraComponent.backgroundColor = originalCamera.backgroundColor;
                freeCameraComponent.cullingMask = -1;
                freeCameraComponent.allowHDR = originalCamera.allowHDR;
                freeCameraComponent.allowMSAA = originalCamera.allowMSAA;
                
                currentFOV = freeCameraComponent.fieldOfView;
                
                originalCamera.enabled = false;
            }
            
            freeCameraComponent.enabled = true;
            
            targetPosition = transform.position;
            currentRotation = new Vector2(transform.eulerAngles.y, transform.eulerAngles.x);
            targetRotation = currentRotation;
            currentRoll = 0f;
            
            cursorWasVisible = Cursor.visible;
            previousCursorLockMode = Cursor.lockState;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            Time.timeScale = 1f;
            
            Debug.Log("Free Camera ACTIVATED - Use WASD to move, Mouse to look, Shift to speed up, Ctrl to slow down, F12 for screenshot");
        }
        
        void DeactivateFreeCamera()
        {
            if (isRecording)
            {
                StopRecording();
            }
            
            freeCameraComponent.enabled = false;
            
            if (originalCamera != null)
            {
                originalCamera.enabled = true;
            }
            
            Cursor.visible = cursorWasVisible;
            Cursor.lockState = previousCursorLockMode;
            
            currentRoll = 0f;
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
            
            Debug.Log("Free Camera DEACTIVATED");
        }
        
        void HandleMovement()
        {
            Vector3 moveDirection = Vector3.zero;
            
            if (Keyboard.current[Key.W].isPressed) moveDirection += Vector3.forward;
            if (Keyboard.current[Key.S].isPressed) moveDirection += Vector3.back;
            if (Keyboard.current[Key.A].isPressed) moveDirection += Vector3.left;
            if (Keyboard.current[Key.D].isPressed) moveDirection += Vector3.right;
            if (Keyboard.current[Key.E].isPressed) moveDirection += Vector3.up;
            if (Keyboard.current[Key.Q].isPressed && !enableRoll) moveDirection += Vector3.down;
            if (Keyboard.current[Key.Space].isPressed) moveDirection += Vector3.up;
            if (Keyboard.current[Key.LeftCtrl].isPressed || Keyboard.current[Key.RightCtrl].isPressed)
            {
                if (moveDirection.y == 0)
                {
                    moveDirection += Vector3.down;
                }
            }
            
            float speedMultiplier = 1f;
            if (Keyboard.current[Key.LeftShift].isPressed || Keyboard.current[Key.RightShift].isPressed)
            {
                speedMultiplier = sprintMultiplier;
            }
            else if (Keyboard.current[Key.LeftAlt].isPressed || Keyboard.current[Key.RightAlt].isPressed)
            {
                speedMultiplier = slowMultiplier;
            }
            
            moveDirection = transform.TransformDirection(moveDirection.normalized);
            targetPosition += moveDirection * moveSpeed * speedMultiplier * Time.unscaledDeltaTime;
            
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, movementSmoothing);
        }
        
        void HandleRotation()
        {
            if (Mouse.current == null) return;
            
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            
            targetRotation.x += mouseDelta.x * lookSensitivity * 0.1f;
            targetRotation.y += mouseDelta.y * lookSensitivity * 0.1f * (invertY ? 1f : -1f);
            
            targetRotation.y = Mathf.Clamp(targetRotation.y, -MAX_VERTICAL_ANGLE, MAX_VERTICAL_ANGLE);
            
            currentRotation = Vector2.SmoothDamp(currentRotation, targetRotation, ref rotationVelocity, rotationSmoothing);
            
            transform.rotation = Quaternion.Euler(currentRotation.y, currentRotation.x, currentRoll);
        }
        
        void HandleRoll()
        {
            if (!enableRoll) return;
            
            float rollInput = 0f;
            
            if (Keyboard.current[Key.Q].isPressed) rollInput = -1f;
            if (Keyboard.current[Key.E].isPressed) rollInput = 1f;
            
            if (Keyboard.current[Key.R].wasPressedThisFrame)
            {
                currentRoll = 0f;
            }
            
            currentRoll += rollInput * rollSpeed * Time.unscaledDeltaTime;
            currentRoll = Mathf.Clamp(currentRoll, -180f, 180f);
        }
        
        void HandleFOV()
        {
            if (!enableFOVControl || Mouse.current == null) return;
            
            float scrollDelta = Mouse.current.scroll.ReadValue().y;
            
            if (scrollDelta != 0)
            {
                currentFOV -= scrollDelta * fovChangeSpeed * 0.1f;
                currentFOV = Mathf.Clamp(currentFOV, minFOV, maxFOV);
                freeCameraComponent.fieldOfView = currentFOV;
            }
        }
        
        void HandleScreenshot()
        {
            if (Keyboard.current[Key.F12].wasPressedThisFrame)
            {
                string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string filename = $"FreeCamera_{timestamp}.png";
                ScreenCapture.CaptureScreenshot(filename, 2);
                Debug.Log($"Screenshot saved: {filename}");
            }
        }
        
        void HandleRecording()
        {
            if (Keyboard.current[recordingKey].wasPressedThisFrame)
            {
                if (isRecording)
                {
                    StopRecording();
                }
                else
                {
                    StartRecording();
                }
            }
        }
        
        void StartRecording()
        {
            if (isRecording)
            {
                Debug.LogWarning("Already recording!");
                return;
            }
            
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string folderName = $"Recording_{timestamp}";
            currentRecordingPath = Path.Combine(Application.dataPath, "..", recordingFolder, folderName);
            
            if (!Directory.Exists(currentRecordingPath))
            {
                Directory.CreateDirectory(currentRecordingPath);
            }
            
            originalTargetFrameRate = Application.targetFrameRate;
            Time.captureFramerate = targetFrameRate;
            
            frameCount = 0;
            recordingStartTime = Time.unscaledTime;
            isRecording = true;
            
            Debug.Log($"<color=green>RECORDING STARTED</color>\nSaving to: {currentRecordingPath}\nTarget FPS: {targetFrameRate}");
        }
        
        void StopRecording()
        {
            if (!isRecording)
            {
                Debug.LogWarning("Not currently recording!");
                return;
            }
            
            isRecording = false;
            Time.captureFramerate = 0;
            Application.targetFrameRate = originalTargetFrameRate;
            
            float duration = Time.unscaledTime - recordingStartTime;
            
            Debug.Log($"<color=green>RECORDING STOPPED</color>\nCaptured: {frameCount} frames\nDuration: {duration:F2}s\nLocation: {currentRecordingPath}\n\n" +
                      $"<color=yellow>To create video, use FFmpeg:</color>\n" +
                      $"ffmpeg -framerate {targetFrameRate} -i frame_%06d.{GetFileExtension()} -c:v libx264 -preset slow -crf 18 -pix_fmt yuv420p output.mp4");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(currentRecordingPath);
            #endif
        }
        
        void CaptureFrame()
        {
            string filename = Path.Combine(currentRecordingPath, $"frame_{frameCount:D6}.{GetFileExtension()}");
            
            int width = recordingWidth;
            int height = recordingHeight;
            
            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            RenderTexture previousRT = freeCameraComponent.targetTexture;
            
            freeCameraComponent.targetTexture = renderTexture;
            
            Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
            
            freeCameraComponent.Render();
            RenderTexture.active = renderTexture;
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshot.Apply();
            
            freeCameraComponent.targetTexture = previousRT;
            RenderTexture.active = null;
            Destroy(renderTexture);
            
            byte[] bytes;
            if (imageFormat == ImageFormat.PNG)
            {
                bytes = screenshot.EncodeToPNG();
            }
            else
            {
                bytes = screenshot.EncodeToJPG(95);
            }
            
            File.WriteAllBytes(filename, bytes);
            Destroy(screenshot);
            
            frameCount++;
        }
        
        string GetFileExtension()
        {
            return imageFormat == ImageFormat.PNG ? "png" : "jpg";
        }
        
        void OnGUI()
        {
            if (!isActive) return;
            if (!showUI && !isRecording) return;
            if (hideUIDuringRecording && isRecording) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 350, 500));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("FREE CAMERA MODE", GUI.skin.box);
            GUILayout.Space(5);
            
            GUILayout.Label($"Position: {transform.position:F2}");
            GUILayout.Label($"Rotation: {transform.eulerAngles:F1}");
            GUILayout.Label($"FOV: {currentFOV:F0}°");
            
            GUILayout.Space(10);
            
            if (isRecording)
            {
                GUI.backgroundColor = Color.red;
                GUILayout.Box("● RECORDING IN PROGRESS", GUILayout.Height(30));
                GUI.backgroundColor = Color.white;
                
                GUILayout.Label($"Frames Captured: {frameCount}");
                GUILayout.Label($"Duration: {(Time.unscaledTime - recordingStartTime):F1}s");
                GUILayout.Label($"FPS Target: {targetFrameRate}");
                GUILayout.Label($"Resolution: {recordingWidth}x{recordingHeight}");
                
                GUILayout.Space(10);
                if (GUILayout.Button($"STOP RECORDING ({recordingKey})"))
                {
                    StopRecording();
                }
            }
            else
            {
                if (GUILayout.Button($"START RECORDING ({recordingKey})"))
                {
                    StartRecording();
                }
            }
            
            if (!isRecording && showUI)
            {
                GUILayout.Space(10);
                GUILayout.Label("CONTROLS:", GUI.skin.box);
                GUILayout.Label("WASD - Move horizontally");
                GUILayout.Label("Space - Move up");
                GUILayout.Label("Ctrl - Move down");
                GUILayout.Label("Mouse - Look around");
                GUILayout.Label("Shift - Speed boost");
                GUILayout.Label("Alt - Slow movement");
                
                if (enableRoll)
                {
                    GUILayout.Label("Q/E - Roll camera");
                    GUILayout.Label("R - Reset roll");
                }
                
                if (enableFOVControl)
                {
                    GUILayout.Label("Mouse Wheel - Adjust FOV");
                }
                
                GUILayout.Label("F12 - Screenshot (2x res)");
                GUILayout.Label($"{recordingKey} - Start/Stop Recording");
                
                GUILayout.Space(10);
                string keyCombo = string.Join("+", System.Array.ConvertAll(activationKeys, k => k.ToString()));
                GUILayout.Label($"Press {keyCombo} to exit");
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        public bool IsActive()
        {
            return isActive;
        }
        
        public void SetActive(bool active)
        {
            if (active != isActive)
            {
                ToggleFreeCamera();
            }
        }
    }
}
