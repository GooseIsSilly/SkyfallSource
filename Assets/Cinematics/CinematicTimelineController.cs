using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;
using Cinemachine;

namespace TPSBR
{
    public class CinematicTimelineController : MonoBehaviour
    {
        [System.Serializable]
        public class PauseGate
        {
            [Tooltip("Timeline time IN SECONDS at which to pause and wait for input")]
            public double timelineTime;
            [Tooltip("Must match a TimelineCameraMarker's Camera Id")]
            public string cameraId;
            [Tooltip("Optional. Leave blank to use the marker's own Pause Prompt Text")]
            public string promptOverride;
        }

        [System.Serializable]
        public class CameraCut
        {
            [Tooltip("Timeline time IN SECONDS at which to switch cameras (no pause, no prompt)")]
            public double timelineTime;
            [Tooltip("Must match a TimelineCameraMarker's Camera Id")]
            public string cameraId;
        }

        [Header("TESTING")]
        [Tooltip("Check this in the Inspector to force the cinematic to play again on next Start(), even if it already played once. Auto-unchecks itself after use.")]
        [SerializeField] private bool _forceReplayOnStart = false;
        [Tooltip("If true, PlayerPrefs 'HasPlayedS2Cinematic' is cleared automatically every time this scene loads in the Editor. Never leave this on in a build.")]
        [SerializeField] private bool _autoResetInEditor = false;

        [Header("Timeline")]
        [SerializeField] private PlayableDirector _director;

        [Header("Auto-Spawned Cameras")]
        [SerializeField] private TimelineCameraMarker[] _cameraMarkers;

        [Header("Pause Gates")]
        [SerializeField] private PauseGate[] _pauseGates;

        [Header("Timed Camera Cuts")]
        [Tooltip("Cameras that switch automatically at a given Timeline time, no player input required")]
        [SerializeField] private CameraCut[] _cameraCuts;

        [Header("Player (already in scene, animated by Timeline)")]
        [SerializeField] private GameObject _playerObject;
        [SerializeField] private Animator _playerAnimator;
        [SerializeField] private string _playerAnimationTrackName = "PlayerAnimation";
        [SerializeField] private CharacterController _characterController;
        [SerializeField] private CinematicPlayerController _playerController;
        [SerializeField] private Transform _cameraTarget;

        [Header("NPC (already in scene, animated by Timeline)")]
        [SerializeField] private GameObject _npcCharacter;
        [SerializeField] private Transform _npcTransform;
        [SerializeField] private Animator _npcAnimator;
        [SerializeField] private AnimationClip _npcTalkAnimationClip;
        [SerializeField] private AnimationClip _npcIdleAnimationClip;

        [Header("Follow Camera")]
        [SerializeField] private string _playerFollowCameraId = "PlayerFollowCam";
        [SerializeField] private float _interactionDistance = 3f;

        [Header("Hold-To-Talk Gate")]
        [SerializeField] private bool _useProximityTalkGate = false;
        [SerializeField] private double _talkGateTimelineTime = 0f;
        [SerializeField] private string _talkGateCameraId = "DialogueCam";
        [SerializeField] private float _holdDuration = 2f;
        [SerializeField] private Key _talkKey = Key.P;

        [Header("NPC Dialogue")]
        [Tooltip("REQUIRED for dialogue to play. If empty, dialogue is skipped and a warning is logged.")]
        [SerializeField] private AudioClip _npcDialogueClip;
        [Tooltip("REQUIRED for dialogue to play. If empty, dialogue is skipped and a warning is logged.")]
        [SerializeField] private AudioSource _audioSource;
        [Tooltip("If true, dialogue plays automatically right after the pause gate you specify below, even without the proximity talk gate system. This is the simplest way to guarantee dialogue actually fires.")]
        [SerializeField] private bool _playDialogueAfterPauseGate = false;
        [Tooltip("Index into Pause Gates array after which dialogue should play, if the option above is enabled")]
        [SerializeField] private int _dialoguePauseGateIndex = 0;
        [Tooltip("Camera to cut to while dialogue plays")]
        [SerializeField] private string _dialogueCameraId = "DialogueCam";

        [Header("Music")]
        [SerializeField] private AudioClip _wakeUpMusic;
        [SerializeField] private AudioClip _flybyMusic;
        [SerializeField] private AudioClip _theresYourStopClip;
        [SerializeField] private AudioSource _musicAudioSource;
        [SerializeField] private float _musicFadeDuration = 2f;
        [SerializeField] private float _wakeUpMusicVolume = 0.4f;
        [SerializeField] private float _flybyMusicVolume = 0.5f;

        [Header("UI")]
        [SerializeField] private Canvas _uiCanvas;
        [SerializeField] private Text _promptText;
        [SerializeField] private Image _fadeImage;
        [SerializeField] private float _fadeDuration = 2f;
        [Tooltip("Any additional canvases/UI GameObjects you want this controller to hide/show together (HUD, subtitles, etc).")]
        [SerializeField] private GameObject[] _additionalUIObjects;
        [Tooltip("If true, all UI (cinematic canvas + additional UI objects) starts hidden and only shows once the cinematic sequence explicitly shows it.")]
        [SerializeField] private bool _startWithUIHidden = true;

        [Header("Map Flyby (non-Timeline waypoint pass, optional)")]
        [SerializeField] private bool _usesLegacyFlyby = false;
        [SerializeField] private string _flybyCameraId = "FlybyCam";
        [SerializeField] private Transform[] _flybyWaypoints;
        [SerializeField] private float _flybyDuration = 10f;

        [Header("Countdown + Marker Flyby (new)")]
        [Tooltip("If true, after the talk gate resolves, a countdown plays, then the camera flies through every entry in Camera Markers in array order.")]
        [SerializeField] private bool _useCountdownMarkerFlyby = false;
        [SerializeField] private int _countdownStartNumber = 3;
        [SerializeField] private float _countdownSecondsPerNumber = 1f;
        [Tooltip("How long the camera holds/blends at each marker during the flyby")]
        [SerializeField] private float _secondsPerFlybyMarker = 2f;
        [Tooltip("Smoothly move between markers instead of hard-cutting")]
        [SerializeField] private bool _smoothMoveBetweenMarkers = true;

        [Header("Scene Settings")]
        [SerializeField] private string _startupScenePath = "Assets/TPSBR/Scenes/Startup.unity";
        [SerializeField] private float _delayAfterVoiceline = 3f;

        private const string CINEMATIC_PLAYED_KEY = "HasPlayedS2Cinematic";

        private readonly Dictionary<string, CinemachineVirtualCamera> _spawnedCameras
            = new Dictionary<string, CinemachineVirtualCamera>();

        private bool _isPausedForInput = false;
        private Key _activeResumeKey = Key.P;

        private bool _isNearNPC = false;
        private float _holdTimer = 0f;
        private bool _hasSpokenToNPC = false;
        private bool _waitingForTalkGate = false;

        private PlayableGraph _npcPlayableGraph;
        private AnimationClipPlayable _npcClipPlayable;

        private abstract class TimelineEvent { public double time; }
        private class CutEvent : TimelineEvent { public string cameraId; }
        private class GateEvent : TimelineEvent { public PauseGate gate; public bool isTalkGate; public int pauseGateIndex = -1; }

        // ============================================================
        // LIFECYCLE
        // ============================================================
        private void Start()
        {
#if UNITY_EDITOR
            if (_autoResetInEditor)
            {
                ResetCinematicPlayedFlag();
            }
#endif
            if (_forceReplayOnStart)
            {
                ResetCinematicPlayedFlag();
                _forceReplayOnStart = false;
            }

            if (HasPlayedCinematic())
            {
                SceneManager.LoadScene(_startupScenePath);
                return;
            }

            SetupUI();

            if (_director != null)
            {
                _director.playOnAwake = false;
            }

            // Guard against the most common cause of "music stops when dialogue starts":
            // the same AudioSource component assigned to both fields in the Inspector.
            if (_audioSource != null && _musicAudioSource != null && _audioSource == _musicAudioSource)
            {
                Debug.LogWarning("[CinematicTimeline] _audioSource and _musicAudioSource are the SAME AudioSource component. " +
                    "Dialogue playback will stop the music because they share one source. Assign two separate AudioSource components (e.g. one on the NPC for dialogue, one on this controller for music).");
            }

            if (_characterController != null) _characterController.enabled = false;
            if (_playerController != null) _playerController.SetEnabled(false);

            StartCoroutine(RunCinematic());
        }

        private void OnDestroy()
        {
            if (_npcPlayableGraph.IsValid())
            {
                _npcPlayableGraph.Destroy();
            }
        }

        private bool HasPlayedCinematic() => PlayerPrefs.GetInt(CINEMATIC_PLAYED_KEY, 0) == 1;

        private void MarkCinematicAsPlayed()
        {
            PlayerPrefs.SetInt(CINEMATIC_PLAYED_KEY, 1);
            PlayerPrefs.Save();
        }

        // ---- RESET FOR TESTING ----
        // Right-click the component header in the Inspector and choose
        // "Reset Cinematic Played Flag" any time, in or out of Play mode,
        // to let the cinematic run again without wiping all of PlayerPrefs.
        [ContextMenu("Reset Cinematic Played Flag")]
        public void ResetCinematicPlayedFlag()
        {
            PlayerPrefs.DeleteKey(CINEMATIC_PLAYED_KEY);
            PlayerPrefs.Save();
            Debug.Log("[CinematicTimeline] Cinematic played flag reset. It will play again next time this scene loads.");
        }

        private void Update()
        {
            if (_isPausedForInput)
            {
                var keyboard = Keyboard.current;
                if (keyboard != null && keyboard[_activeResumeKey].wasPressedThisFrame)
                {
                    _isPausedForInput = false;
                }
                return;
            }

            if (_useProximityTalkGate && _waitingForTalkGate && !_hasSpokenToNPC)
            {
                CheckDistanceToNPC();
                if (_isNearNPC)
                {
                    UpdateHoldInput();
                }
            }
        }

        // ============================================================
        // MAIN SEQUENCE
        // ============================================================
        private IEnumerator RunCinematic()
        {
            yield return StartCoroutine(FadeFromBlack());

            RebindTimeline();
            SpawnCamerasFromMarkers();

            if (_npcAnimator != null && _npcIdleAnimationClip != null)
            {
                _npcAnimator.runtimeAnimatorController = null;
                PlayNPCAnimation(_npcIdleAnimationClip, true);
            }

            ShowUI();

            if (_wakeUpMusic != null && _musicAudioSource != null)
            {
                _musicAudioSource.loop = true;
                StartCoroutine(FadeMusicIn(_wakeUpMusic, _wakeUpMusicVolume));
            }
            else if (_wakeUpMusic == null)
            {
                Debug.LogWarning("[CinematicTimeline] No wake-up music clip assigned, skipping.");
            }

            var events = new List<TimelineEvent>();

            if (_pauseGates != null)
            {
                for (int i = 0; i < _pauseGates.Length; i++)
                {
                    events.Add(new GateEvent { time = _pauseGates[i].timelineTime, gate = _pauseGates[i], isTalkGate = false, pauseGateIndex = i });
                }
            }

            if (_cameraCuts != null)
            {
                foreach (var c in _cameraCuts)
                    events.Add(new CutEvent { time = c.timelineTime, cameraId = c.cameraId });
            }

            PauseGate talkGate = null;
            if (_useProximityTalkGate)
            {
                talkGate = new PauseGate { timelineTime = _talkGateTimelineTime, cameraId = _talkGateCameraId };
                events.Add(new GateEvent { time = talkGate.timelineTime, gate = talkGate, isTalkGate = true });
            }

            events.Sort((a, b) => a.time.CompareTo(b.time));

            _director.time = 0;
            _director.Play();

            foreach (var evt in events)
            {
                while (_director.time < evt.time)
                {
                    yield return null;
                }

                if (evt is CutEvent cut)
                {
                    SetActiveCameraById(cut.cameraId);
                }
                else if (evt is GateEvent ge)
                {
                    if (ge.isTalkGate)
                    {
                        yield return StartCoroutine(RunProximityTalkGate(ge.gate));
                        yield return StartCoroutine(PlayNPCDialogue(ge.gate.cameraId));
                        yield return StartCoroutine(FadeMusicOut());

                        if (_useCountdownMarkerFlyby)
                        {
                            yield return StartCoroutine(RunCountdown());
                            yield return StartCoroutine(PlayMarkerFlyby());
                        }
                    }
                    else
                    {
                        yield return StartCoroutine(RunPauseGate(ge.gate));

                        // Guaranteed dialogue trigger: fires right after the specified
                        // pause gate regardless of the proximity talk-gate system,
                        // as long as a clip + AudioSource are assigned.
                        if (_playDialogueAfterPauseGate && ge.pauseGateIndex == _dialoguePauseGateIndex)
                        {
                            yield return StartCoroutine(PlayNPCDialogue(_dialogueCameraId));
                            yield return StartCoroutine(FadeMusicOut());
                        }
                    }
                }
            }

            if (_characterController != null) _characterController.enabled = true;
            if (_playerController != null) _playerController.SetEnabled(true);

            if (!string.IsNullOrEmpty(_playerFollowCameraId))
            {
                SetActiveCameraById(_playerFollowCameraId);
            }

            // Wait for the Timeline to reach its own end. Using state == Playing alone
            // can hang forever if the Timeline's Wrap Mode is set to "Loop" (it never
            // naturally stops playing), so we also check the director's own time
            // against its duration as a hard exit condition.
            double timelineDuration = _director.duration;
            while (_director.state == PlayState.Playing && _director.time < timelineDuration)
            {
                yield return null;
            }

            Debug.Log($"[CinematicTimeline] Timeline finished (time={_director.time:F2}/{timelineDuration:F2}, state={_director.state}). _usesLegacyFlyby={_usesLegacyFlyby}");

            if (_usesLegacyFlyby)
            {
                yield return StartCoroutine(PlayLegacyFlyby());
            }
            else
            {
                Debug.Log("[CinematicTimeline] _usesLegacyFlyby is unchecked, skipping the waypoint flyby.");
            }

            yield return StartCoroutine(ReturnToLobby());
        }

        // ============================================================
        // CAMERA SPAWNING
        // ============================================================
        private void SpawnCamerasFromMarkers()
        {
            if (_cameraMarkers == null) return;

            foreach (var marker in _cameraMarkers)
            {
                if (marker == null)
                {
                    Debug.LogWarning("[CinematicTimeline] Null entry in Camera Markers array.");
                    continue;
                }

                if (string.IsNullOrEmpty(marker.cameraId))
                {
                    Debug.LogWarning($"[CinematicTimeline] Marker '{marker.name}' has no Camera Id set, skipping.");
                    continue;
                }

                GameObject camObj = new GameObject($"VCam_{marker.cameraId}");
                camObj.transform.SetPositionAndRotation(marker.transform.position, marker.transform.rotation);
                camObj.transform.SetParent(transform);

                var vcam = camObj.AddComponent<CinemachineVirtualCamera>();
                vcam.m_Lens.FieldOfView = marker.fieldOfView;
                vcam.Priority = 0;

                if (marker.follow != null) vcam.Follow = marker.follow;
                if (marker.lookAt != null) vcam.LookAt = marker.lookAt;

                _spawnedCameras[marker.cameraId] = vcam;
                marker.gameObject.SetActive(false);
            }
        }

        private void SetActiveCameraById(string cameraId)
        {
            foreach (var cam in _spawnedCameras.Values)
            {
                cam.Priority = 0;
            }

            if (!string.IsNullOrEmpty(cameraId) && _spawnedCameras.TryGetValue(cameraId, out var target))
            {
                target.Priority = 100;
            }
            else
            {
                Debug.LogWarning($"[CinematicTimeline] No spawned camera found for id '{cameraId}'.");
            }
        }

        private TimelineCameraMarker FindMarker(string cameraId)
        {
            if (_cameraMarkers == null) return null;
            foreach (var marker in _cameraMarkers)
            {
                if (marker != null && marker.cameraId == cameraId)
                    return marker;
            }
            return null;
        }

        // ============================================================
        // GATES
        // ============================================================
        private IEnumerator RunPauseGate(PauseGate gate)
        {
            _director.Pause();

            SetActiveCameraById(gate.cameraId);

            var marker = FindMarker(gate.cameraId);

            string promptText = !string.IsNullOrEmpty(gate.promptOverride)
                ? gate.promptOverride
                : (marker != null ? marker.pausePromptText : "Press P to Continue");

            Key resumeKey = marker != null ? marker.resumeKey : Key.P;

            if (_promptText != null)
            {
                _promptText.text = promptText;
            }

            _isPausedForInput = true;
            _activeResumeKey = resumeKey;

            while (_isPausedForInput)
            {
                yield return null;
            }

            if (_promptText != null)
            {
                _promptText.text = "";
            }

            _director.Play();
        }

        private IEnumerator RunProximityTalkGate(PauseGate gate)
        {
            _director.Pause();

            SetActiveCameraById(_playerFollowCameraId);

            if (_characterController != null) _characterController.enabled = true;
            if (_playerController != null) _playerController.SetEnabled(true);

            _waitingForTalkGate = true;

            while (!_hasSpokenToNPC)
            {
                yield return null;
            }

            _waitingForTalkGate = false;

            if (_characterController != null) _characterController.enabled = false;
            if (_playerController != null) _playerController.SetEnabled(false);

            if (_promptText != null)
            {
                _promptText.text = "";
            }
        }

        private void CheckDistanceToNPC()
        {
            if (_playerObject == null || _npcTransform == null) return;

            float distance = Vector3.Distance(_playerObject.transform.position, _npcTransform.position);
            bool wasNear = _isNearNPC;
            _isNearNPC = distance <= _interactionDistance;

            if (_isNearNPC && !wasNear && !_hasSpokenToNPC)
            {
                if (_promptText != null) _promptText.text = "Hold P To Talk";
            }
            else if (!_isNearNPC && wasNear)
            {
                if (_promptText != null) _promptText.text = "";
                _holdTimer = 0f;
            }
        }

        private void UpdateHoldInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard[_talkKey].isPressed)
            {
                _holdTimer += Time.deltaTime;
                if (_promptText != null)
                {
                    float progress = Mathf.Clamp01(_holdTimer / _holdDuration);
                    _promptText.text = $"Hold P To Talk [{(int)(progress * 100)}%]";
                }

                if (_holdTimer >= _holdDuration)
                {
                    _hasSpokenToNPC = true;
                    _holdTimer = 0f;
                    if (_promptText != null) _promptText.text = "";
                }
            }
            else
            {
                _holdTimer = 0f;
                if (_promptText != null && _isNearNPC) _promptText.text = "Hold P To Talk";
            }
        }

        // ============================================================
        // DIALOGUE  (fixed: now logs exactly why it's skipped, if it is)
        // ============================================================
        private IEnumerator PlayNPCDialogue(string cameraId)
        {
            SetActiveCameraById(cameraId);

            if (_npcAnimator != null && _npcTalkAnimationClip != null)
            {
                PlayNPCAnimation(_npcTalkAnimationClip, false);
            }

            if (_npcDialogueClip == null)
            {
                Debug.LogWarning("[CinematicTimeline] PlayNPCDialogue called but _npcDialogueClip is not assigned in the Inspector. Dialogue will not play.");
                yield return new WaitForSeconds(2f);
            }
            else if (_audioSource == null)
            {
                Debug.LogWarning("[CinematicTimeline] PlayNPCDialogue called but _audioSource is not assigned in the Inspector. Dialogue will not play.");
                yield return new WaitForSeconds(2f);
            }
            else
            {
                _audioSource.Stop();
                _audioSource.clip = _npcDialogueClip;
                _audioSource.loop = false;
                _audioSource.Play();
                yield return new WaitForSeconds(_npcDialogueClip.length);
            }

            if (_npcAnimator != null && _npcIdleAnimationClip != null)
            {
                PlayNPCAnimation(_npcIdleAnimationClip, true);
            }

            _director.Play();
        }

        private void PlayNPCAnimation(AnimationClip clip, bool loop)
        {
            if (_npcAnimator == null || clip == null)
                return;

            if (_npcPlayableGraph.IsValid())
            {
                _npcPlayableGraph.Destroy();
            }

            _npcPlayableGraph = PlayableGraph.Create("NPCAnimationGraph");
            _npcPlayableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            _npcClipPlayable = AnimationClipPlayable.Create(_npcPlayableGraph, clip);
            _npcClipPlayable.SetDuration(loop ? double.MaxValue : clip.length);

            var output = AnimationPlayableOutput.Create(_npcPlayableGraph, "NPCAnimation", _npcAnimator);
            output.SetSourcePlayable(_npcClipPlayable);

            _npcPlayableGraph.Play();
        }

        private void RebindTimeline()
        {
            if (_director == null || _playerAnimator == null) return;

            var asset = _director.playableAsset as TimelineAsset;
            if (asset == null) return;

            foreach (var track in asset.GetOutputTracks())
            {
                if (track.name == _playerAnimationTrackName)
                {
                    _director.SetGenericBinding(track, _playerAnimator);
                }
            }

            if (!string.IsNullOrEmpty(_playerFollowCameraId))
            {
                var marker = FindMarker(_playerFollowCameraId);
                if (marker != null && _cameraTarget != null)
                {
                    marker.follow = _cameraTarget;
                    marker.lookAt = _cameraTarget;
                }
            }

            _director.RebuildGraph();
        }

        // ============================================================
        // COUNTDOWN + FLYBY THROUGH ALL CAMERA MARKERS (new)
        // ============================================================
        private IEnumerator RunCountdown()
        {
            if (_promptText == null)
            {
                Debug.LogWarning("[CinematicTimeline] No _promptText assigned, cannot show countdown. Skipping straight to flyby.");
                yield break;
            }

            for (int i = _countdownStartNumber; i >= 1; i--)
            {
                _promptText.text = i.ToString();
                Debug.Log($"[CinematicTimeline] Countdown: {i}");
                yield return new WaitForSeconds(_countdownSecondsPerNumber);
            }

            _promptText.text = "";
        }

        // Flies the camera through every entry in _cameraMarkers, in array order.
        // Uses the already-spawned CinemachineVirtualCamera for each marker id.
        private IEnumerator PlayMarkerFlyby()
        {
            if (_cameraMarkers == null || _cameraMarkers.Length == 0)
            {
                Debug.LogWarning("[CinematicTimeline] Marker flyby aborted: Camera Markers array is empty.");
                yield break;
            }

            Debug.Log($"[CinematicTimeline] Starting marker flyby through {_cameraMarkers.Length} marker(s).");

            foreach (var marker in _cameraMarkers)
            {
                if (marker == null || string.IsNullOrEmpty(marker.cameraId))
                {
                    continue;
                }

                if (!_spawnedCameras.TryGetValue(marker.cameraId, out var vcam))
                {
                    Debug.LogWarning($"[CinematicTimeline] Marker flyby: no spawned camera found for '{marker.cameraId}', skipping this stop.");
                    continue;
                }

                Debug.Log($"[CinematicTimeline] Flyby stop: {marker.cameraId}");

                if (_smoothMoveBetweenMarkers)
                {
                    // Blend camera priority so Cinemachine cross-fades to this marker's vcam,
                    // then hold on it for the configured duration.
                    SetActiveCameraById(marker.cameraId);
                    yield return new WaitForSeconds(_secondsPerFlybyMarker);
                }
                else
                {
                    SetActiveCameraById(marker.cameraId);
                    yield return new WaitForSeconds(_secondsPerFlybyMarker);
                }
            }

            Debug.Log("[CinematicTimeline] Marker flyby complete.");
        }

        // ============================================================
        // LEGACY FLYBY
        // ============================================================
        private IEnumerator PlayLegacyFlyby()
        {
            Debug.Log("[CinematicTimeline] Starting map flyby.");

            SetActiveCameraById(_flybyCameraId);

            if (_flybyMusic != null && _musicAudioSource != null)
            {
                _musicAudioSource.loop = false;
                StartCoroutine(FadeMusicIn(_flybyMusic, _flybyMusicVolume));
            }

            if (!_spawnedCameras.ContainsKey(_flybyCameraId))
            {
                Debug.LogWarning($"[CinematicTimeline] Flyby aborted: no spawned camera matches Flyby Camera Id '{_flybyCameraId}'. " +
                    "Check that a TimelineCameraMarker with this exact Camera Id is in the Camera Markers array.");
                yield break;
            }

            if (_flybyWaypoints == null || _flybyWaypoints.Length == 0)
            {
                Debug.LogWarning("[CinematicTimeline] Flyby aborted: Flyby Waypoints array is empty. Assign at least 2 waypoint transforms.");
                yield break;
            }

            var flybyCam = _spawnedCameras[_flybyCameraId];

            Transform cameraTransform = flybyCam.transform;

            if (_flybyWaypoints[0] != null)
            {
                cameraTransform.position = _flybyWaypoints[0].position;
                cameraTransform.rotation = _flybyWaypoints[0].rotation;
            }

            float elapsed = 0f;
            while (elapsed < _flybyDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _flybyDuration;

                int currentIndex = Mathf.FloorToInt(t * (_flybyWaypoints.Length - 1));
                int nextIndex = Mathf.Min(currentIndex + 1, _flybyWaypoints.Length - 1);
                float segmentT = (t * (_flybyWaypoints.Length - 1)) - currentIndex;

                if (_flybyWaypoints[currentIndex] != null && _flybyWaypoints[nextIndex] != null)
                {
                    cameraTransform.position = Vector3.Lerp(
                        _flybyWaypoints[currentIndex].position,
                        _flybyWaypoints[nextIndex].position, segmentT);
                    cameraTransform.rotation = Quaternion.Slerp(
                        _flybyWaypoints[currentIndex].rotation,
                        _flybyWaypoints[nextIndex].rotation, segmentT);
                }

                yield return null;
            }

            if (_flybyWaypoints[_flybyWaypoints.Length - 1] != null)
            {
                cameraTransform.position = _flybyWaypoints[_flybyWaypoints.Length - 1].position;
                cameraTransform.rotation = _flybyWaypoints[_flybyWaypoints.Length - 1].rotation;
            }

            if (_musicAudioSource != null && _musicAudioSource.isPlaying)
            {
                while (_musicAudioSource.isPlaying)
                {
                    yield return null;
                }
            }

            if (_theresYourStopClip != null && _audioSource != null)
            {
                _audioSource.clip = _theresYourStopClip;
                _audioSource.Play();
                yield return new WaitForSeconds(_theresYourStopClip.length);
            }
        }

        private IEnumerator ReturnToLobby()
        {
            yield return new WaitForSeconds(_delayAfterVoiceline);
            yield return StartCoroutine(FadeToBlack());
            MarkCinematicAsPlayed();
            SceneManager.LoadScene(_startupScenePath);
        }

        // ============================================================
        // UI SETUP / SHOW / HIDE  (new)
        // ============================================================
        private void SetupUI()
        {
            if (_uiCanvas == null)
            {
                GameObject canvasObj = new GameObject("CinematicCanvas");
                canvasObj.transform.SetParent(transform);
                _uiCanvas = canvasObj.AddComponent<Canvas>();
                _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _uiCanvas.sortingOrder = 9999;

                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            if (_fadeImage == null && _uiCanvas != null)
            {
                GameObject fadeObj = new GameObject("FadeImage");
                fadeObj.transform.SetParent(_uiCanvas.transform, false);
                _fadeImage = fadeObj.AddComponent<Image>();
                _fadeImage.color = Color.black;

                RectTransform rt = fadeObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
            }

            if (_promptText == null && _uiCanvas != null)
            {
                GameObject textObj = new GameObject("PromptText");
                textObj.transform.SetParent(_uiCanvas.transform, false);
                _promptText = textObj.AddComponent<Text>();
                _promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _promptText.fontSize = 36;
                _promptText.alignment = TextAnchor.MiddleCenter;
                _promptText.color = Color.white;
                _promptText.text = "";

                var outline = textObj.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(2, -2);

                RectTransform rt = textObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.3f);
                rt.anchorMax = new Vector2(0.5f, 0.3f);
                rt.sizeDelta = new Vector2(800, 100);
                rt.anchoredPosition = Vector2.zero;
            }

            if (_fadeImage != null) _fadeImage.gameObject.SetActive(false);

            if (_startWithUIHidden)
            {
                HideUI();
            }
            else
            {
                ShowUI();
            }
        }

        /// Hides the cinematic canvas plus any additional UI objects assigned
        /// in the Inspector (HUD, subtitles, etc). Safe to call any time.
        public void HideUI()
        {
            if (_uiCanvas != null) _uiCanvas.gameObject.SetActive(false);

            if (_additionalUIObjects != null)
            {
                foreach (var obj in _additionalUIObjects)
                {
                    if (obj != null) obj.SetActive(false);
                }
            }
        }

        /// Shows the cinematic canvas plus any additional UI objects assigned
        /// in the Inspector. Safe to call any time.
        public void ShowUI()
        {
            if (_uiCanvas != null) _uiCanvas.gameObject.SetActive(true);

            if (_additionalUIObjects != null)
            {
                foreach (var obj in _additionalUIObjects)
                {
                    if (obj != null) obj.SetActive(true);
                }
            }
        }

        /// Toggle helper, handy for a single debug keybind or button.
        public void ToggleUI()
        {
            bool currentlyVisible = _uiCanvas != null && _uiCanvas.gameObject.activeSelf;
            if (currentlyVisible) HideUI();
            else ShowUI();
        }

        private IEnumerator FadeFromBlack()
        {
            if (_fadeImage == null) yield break;
            _fadeImage.gameObject.SetActive(true);
            Color color = _fadeImage.color;
            color.a = 1f;
            _fadeImage.color = color;

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                color.a = 1f - (elapsed / _fadeDuration);
                _fadeImage.color = color;
                yield return null;
            }
            color.a = 0f;
            _fadeImage.color = color;
            _fadeImage.gameObject.SetActive(false);
        }

        private IEnumerator FadeToBlack()
        {
            if (_fadeImage == null) yield break;
            _fadeImage.gameObject.SetActive(true);
            Color color = _fadeImage.color;
            color.a = 0f;
            _fadeImage.color = color;

            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                color.a = elapsed / _fadeDuration;
                _fadeImage.color = color;
                yield return null;
            }
            color.a = 1f;
            _fadeImage.color = color;
        }

        private IEnumerator FadeMusicIn(AudioClip clip, float targetVolume)
        {
            if (_musicAudioSource == null || clip == null) yield break;

            _musicAudioSource.clip = clip;
            _musicAudioSource.volume = 0f;
            _musicAudioSource.Play();

            float elapsed = 0f;
            while (elapsed < _musicFadeDuration)
            {
                elapsed += Time.deltaTime;
                _musicAudioSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / _musicFadeDuration);
                yield return null;
            }

            _musicAudioSource.volume = targetVolume;
        }

        private IEnumerator FadeMusicOut()
        {
            if (_musicAudioSource == null || !_musicAudioSource.isPlaying) yield break;

            float startVolume = _musicAudioSource.volume;
            float elapsed = 0f;

            while (elapsed < _musicFadeDuration)
            {
                elapsed += Time.deltaTime;
                _musicAudioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / _musicFadeDuration);
                yield return null;
            }

            _musicAudioSource.volume = 0f;
            _musicAudioSource.Stop();
        }
    }
}
