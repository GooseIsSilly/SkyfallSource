using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSBR
{
    // Drop this component on an empty GameObject positioned/rotated where you
    // want a camera shot to happen. At runtime, CinematicTimelineController
    // will spawn a CinemachineVirtualCamera at this transform automatically.
    public class TimelineCameraMarker : MonoBehaviour
    {
        [Header("Camera Identity")]
        [Tooltip("Unique name used to reference this camera from Pause Gates / Signals (e.g. \"DialogueCam\", \"NewEraCam\")")]
        public string cameraId;

        [Header("Cinemachine Settings")]
        public float fieldOfView = 40f;
        [Tooltip("Optional. Leave empty for a static camera.")]
        public Transform follow;
        [Tooltip("Optional. Leave empty for a static camera.")]
        public Transform lookAt;

        [Header("Pause Gate (optional)")]
        [Tooltip("If true, the Timeline will pause when this camera becomes active and wait for the resume key")]
        public bool pauseTimelineHere = false;

        [Tooltip("Text shown while paused, e.g. \"Press P to Start New Era\"")]
        public string pausePromptText = "Press P to Continue";

        [Tooltip("Key that resumes the Timeline when paused at this camera")]
        public Key resumeKey = Key.P;
    }
}