using UnityEngine;
using System.IO;

namespace Skyfall.Cinematics
{
    public class CinematicRecorder : MonoBehaviour
    {
        [Header("Recording Settings")]
        [Tooltip("Target frame rate for recording")]
        public int targetFrameRate = 60;
        
        [Tooltip("Super sampling multiplier")]
        [Range(1, 4)]
        public int superSize = 1;
        
        [Tooltip("Output folder path")]
        public string outputFolder = "Recordings";
        
        [Tooltip("Filename prefix")]
        public string filenamePrefix = "shot";
        
        [Header("Capture Settings")]
        [Tooltip("Capture transparency")]
        public bool captureAlpha = false;
        
        [Tooltip("Image format")]
        public ImageFormat imageFormat = ImageFormat.PNG;
        
        [Header("Status")]
        [SerializeField] private bool isRecording = false;
        [SerializeField] private int frameCount = 0;
        
        private string currentRecordingPath;
        private float originalTimeScale;
        private int originalFrameRate;
        
        public enum ImageFormat
        {
            PNG,
            JPG
        }
        
        void Start()
        {
            CreateOutputDirectory();
        }
        
        void CreateOutputDirectory()
        {
            currentRecordingPath = Path.Combine(Application.dataPath, "..", outputFolder);
            
            if (!Directory.Exists(currentRecordingPath))
            {
                Directory.CreateDirectory(currentRecordingPath);
                Debug.Log($"Created recording directory: {currentRecordingPath}");
            }
        }
        
        public void StartRecording()
        {
            if (isRecording)
            {
                Debug.LogWarning("Already recording!");
                return;
            }
            
            CreateOutputDirectory();
            
            originalTimeScale = Time.timeScale;
            originalFrameRate = Application.targetFrameRate;
            
            Time.captureFramerate = targetFrameRate;
            Time.timeScale = 1f;
            
            frameCount = 0;
            isRecording = true;
            
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            currentRecordingPath = Path.Combine(Application.dataPath, "..", outputFolder, $"{filenamePrefix}_{timestamp}");
            Directory.CreateDirectory(currentRecordingPath);
            
            Debug.Log($"Started recording to: {currentRecordingPath}");
        }
        
        public void StopRecording()
        {
            if (!isRecording)
            {
                Debug.LogWarning("Not currently recording!");
                return;
            }
            
            isRecording = false;
            
            Time.captureFramerate = 0;
            Time.timeScale = originalTimeScale;
            Application.targetFrameRate = originalFrameRate;
            
            Debug.Log($"Stopped recording. Captured {frameCount} frames to: {currentRecordingPath}");
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(currentRecordingPath);
            #endif
        }
        
        public void ToggleRecording()
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
        
        void LateUpdate()
        {
            if (isRecording)
            {
                CaptureFrame();
            }
        }
        
        void CaptureFrame()
        {
            string filename = Path.Combine(currentRecordingPath, $"frame_{frameCount:D6}.{GetFileExtension()}");
            
            int width = Screen.width * superSize;
            int height = Screen.height * superSize;
            
            RenderTexture renderTexture = new RenderTexture(width, height, 24);
            Camera.main.targetTexture = renderTexture;
            
            Texture2D screenshot = new Texture2D(width, height, captureAlpha ? TextureFormat.RGBA32 : TextureFormat.RGB24, false);
            
            Camera.main.Render();
            RenderTexture.active = renderTexture;
            screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenshot.Apply();
            
            Camera.main.targetTexture = null;
            RenderTexture.active = null;
            Destroy(renderTexture);
            
            byte[] bytes;
            if (imageFormat == ImageFormat.PNG)
            {
                bytes = screenshot.EncodeToPNG();
            }
            else
            {
                bytes = screenshot.EncodeToJPG(90);
            }
            
            File.WriteAllBytes(filename, bytes);
            Destroy(screenshot);
            
            frameCount++;
        }
        
        string GetFileExtension()
        {
            return imageFormat == ImageFormat.PNG ? "png" : "jpg";
        }
        
        public bool IsRecording()
        {
            return isRecording;
        }
        
        public int GetFrameCount()
        {
            return frameCount;
        }
    }
}
