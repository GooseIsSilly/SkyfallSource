using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.Playables;
using UnityEngine.Animations;

namespace TPSBR
{
    public class SeasonCinematicController : MonoBehaviour
    {
        public static SeasonCinematicController Instance { get; private set; }
        public bool IsIntroActive => _isIntroActive;

        [Header("Player Setup")]
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private GameObject _playerPrefab;

        [Header("Player Animations")]
        [SerializeField] private AnimationClip _layingDownIdleClip;
        [SerializeField] private AnimationClip _wakeUpClip;
        [SerializeField] private AnimationClip _idleClip;
        [SerializeField] private AnimationClip _walkClip;

        [Header("NPC Setup")]
        [SerializeField] private GameObject _npcCharacter;
        [SerializeField] private Transform _npcTransform;
        [SerializeField] private Animator _npcAnimator;
        [SerializeField] private string _npcTalkStateName = "Talking";
        [SerializeField] private float _interactionDistance = 3f;

        [Header("UI")]
        [SerializeField] private Canvas _uiCanvas;
        [SerializeField] private Text _promptText;
        [SerializeField] private Image _fadeImage;
        [SerializeField] private float _fadeDuration = 2f;

        [Header("Cameras")]
        [SerializeField] private CinemachineVirtualCamera _introCam;
        [SerializeField] private CinemachineVirtualCamera _playerFollowCam;
        [SerializeField] private CinemachineVirtualCamera _dialogueCam;
        [SerializeField] private CinemachineVirtualCamera _flybyCamera;
        [SerializeField] private Transform[] _flybyWaypoints;
        [SerializeField] private float _flybyDuration = 10f;
        [SerializeField] private float _finalWaypointHoldTime = 3f;

        [Header("Audio - Music")]
        [SerializeField] private AudioClip _wakeUpMusic;
        [SerializeField] private AudioClip _planeMusic;
        [SerializeField] private float _musicFadeDuration = 2f;

        [Header("Audio - Dialog")]
        [SerializeField] private AudioClip _firstDialogClip;
        [SerializeField] private AudioClip _secondDialogClip;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _dialogAudioSource;
        [SerializeField] private AudioSource _musicAudioSource;

        [Header("Input")]
        [SerializeField] private float _holdDuration = 2f;

        private GameObject _spawnedPlayer;
        private Transform _cameraTarget;
        private Animator _playerAnimator;
        private CharacterController _characterController;
        private SimpleCinematicPlayerController _playerController;
        private CinemachineBrain _cinemachineBrain;
        private PlayableGraph _playableGraph;
        private AnimationClipPlayable _currentClipPlayable;

        private bool _isIntroActive = false;
        private bool _isNearNPC = false;
        private float _holdTimer = 0f;
        private bool _hasSpokenToNPC = false;
        private bool _hasWokenUp = false;

        private const string PREF_HAS_SEEN_INTRO = "Season_HasSeenIntro";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupAudioSources();
            SetupUI();

            if (_npcCharacter != null)
            {
                _npcCharacter.SetActive(false);
            }

            Camera mainCamera = Camera.main;
            
            if (mainCamera == null)
            {
                mainCamera = Object.FindFirstObjectByType<Camera>();
                if (mainCamera != null)
                {
                    Debug.LogWarning("SeasonCinematicController: Main Camera not tagged as 'MainCamera'. Found camera but it should be tagged properly.");
                }
            }

            if (mainCamera != null)
            {
                _cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
                if (_cinemachineBrain == null)
                {
                    _cinemachineBrain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
                    Debug.Log("SeasonCinematicController: Added CinemachineBrain to Main Camera");
                }
            }
            else
            {
                Debug.LogError("SeasonCinematicController: No Camera found in scene! Cinematic cameras won't work.");
            }
        }

        private void Start()
        {
            Debug.Log("SeasonCinematicController: Start() called");
            Debug.Log($"  - HasSeenIntro: {PlayerPrefs.GetInt(PREF_HAS_SEEN_INTRO, 0)}");
            Debug.Log($"  - _introCam assigned: {_introCam != null}");
            Debug.Log($"  - _playerFollowCam assigned: {_playerFollowCam != null}");
            Debug.Log($"  - _dialogueCam assigned: {_dialogueCam != null}");
            Debug.Log($"  - _flybyCamera assigned: {_flybyCamera != null}");
            Debug.Log($"  - _cinemachineBrain found: {_cinemachineBrain != null}");
            
            if (PlayerPrefs.GetInt(PREF_HAS_SEEN_INTRO, 0) == 0)
            {
                Debug.Log("SeasonCinematicController: Starting cinematic sequence...");
                StartCoroutine(PlayCinematicSequence());
            }
            else
            {
                Debug.Log("SeasonCinematicController: Intro already seen, skipping.");
            }
        }

        private void Update()
        {
            if (!_isIntroActive || _spawnedPlayer == null)
                return;

            if (!_hasWokenUp)
            {
                UpdateWakeUpInput();
                return;
            }

            CheckDistanceToNPC();

            if (_isNearNPC && !_hasSpokenToNPC)
            {
                UpdateHoldInput();
            }
        }

        private void OnDestroy()
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }
        }

        private void SetupAudioSources()
        {
            if (_dialogAudioSource == null)
            {
                _dialogAudioSource = gameObject.AddComponent<AudioSource>();
            }
            _dialogAudioSource.playOnAwake = false;
            _dialogAudioSource.spatialBlend = 0f;

            if (_musicAudioSource == null)
            {
                _musicAudioSource = gameObject.AddComponent<AudioSource>();
            }
            _musicAudioSource.playOnAwake = false;
            _musicAudioSource.spatialBlend = 0f;
            _musicAudioSource.loop = true;
            _musicAudioSource.volume = 0f;
        }

        private void SetupUI()
        {
            if (_uiCanvas == null)
            {
                GameObject canvasObj = new GameObject("SeasonCinematicCanvas");
                canvasObj.transform.SetParent(transform);

                _uiCanvas = canvasObj.AddComponent<Canvas>();
                _uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _uiCanvas.sortingOrder = 9999;

                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<GraphicRaycaster>();
            }

            if (_fadeImage == null)
            {
                GameObject fadeObj = new GameObject("FadeImage");
                fadeObj.transform.SetParent(_uiCanvas.transform, false);

                _fadeImage = fadeObj.AddComponent<Image>();
                _fadeImage.color = Color.black;

                RectTransform rt = fadeObj.GetComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
            }

            if (_promptText == null)
            {
                GameObject textObj = new GameObject("PromptText");
                textObj.transform.SetParent(_uiCanvas.transform, false);

                _promptText = textObj.AddComponent<Text>();
                _promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                _promptText.fontSize = 48;
                _promptText.alignment = TextAnchor.MiddleCenter;
                _promptText.color = Color.white;

                var outline = textObj.AddComponent<Outline>();
                outline.effectColor = Color.black;
                outline.effectDistance = new Vector2(2, -2);

                RectTransform rt = textObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.3f);
                rt.anchorMax = new Vector2(0.5f, 0.3f);
                rt.sizeDelta = new Vector2(1000, 100);
            }

            _uiCanvas.gameObject.SetActive(false);
        }

        private IEnumerator PlayCinematicSequence()
        {
            Debug.Log("SeasonCinematicController: PlayCinematicSequence started");
            _isIntroActive = true;

            _uiCanvas.gameObject.SetActive(true);

            yield return StartCoroutine(FadeFromBlack());

            SpawnPlayer();

            if (_npcCharacter != null)
            {
                _npcCharacter.SetActive(true);
            }

            Debug.Log("SeasonCinematicController: Setting intro camera active");
            SetActiveCamera(_introCam);

            StartCoroutine(FadeMusicIn(_wakeUpMusic));

            PlayLayingDownAnimation();

            _promptText.text = "Press P To Start New Decade";

            while (!_hasWokenUp)
            {
                yield return null;
            }

            while (!_hasSpokenToNPC)
            {
                yield return null;
            }

            yield return StartCoroutine(PlayFirstDialog());

            StartCoroutine(CrossfadeMusic(_planeMusic));

            yield return StartCoroutine(PlayMapFlyby());

            yield return StartCoroutine(PlaySecondDialog());

            yield return StartCoroutine(FadeMusicOut());

            CompleteIntro();

            PlayerPrefs.SetInt(PREF_HAS_SEEN_INTRO, 1);
            PlayerPrefs.Save();
        }

        private void SpawnPlayer()
        {
            Vector3 spawnPosition = _playerSpawnPoint.position;

            if (Physics.Raycast(spawnPosition + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 50f))
            {
                spawnPosition = hit.point;
            }

            _spawnedPlayer = Instantiate(_playerPrefab, spawnPosition, _playerSpawnPoint.rotation);

            _characterController = _spawnedPlayer.GetComponent<CharacterController>();
            if (_characterController == null)
            {
                _characterController = _spawnedPlayer.AddComponent<CharacterController>();
            }
            _characterController.enabled = false;

            _playerController = _spawnedPlayer.GetComponent<SimpleCinematicPlayerController>();
            if (_playerController == null)
            {
                _playerController = _spawnedPlayer.AddComponent<SimpleCinematicPlayerController>();
            }
            _playerController.EnableControl(false);

            _playerAnimator = _spawnedPlayer.GetComponentInChildren<Animator>();
            
            if (_playerAnimator == null)
            {
                Debug.LogWarning("SeasonCinematicController: No Animator found on player prefab! Adding one...");
                _playerAnimator = _spawnedPlayer.AddComponent<Animator>();
                _playerAnimator.runtimeAnimatorController = CreateRuntimeAnimatorController();
            }
            
            if (_playerAnimator != null)
            {
                _playerAnimator.enabled = false;
            }

            GameObject cameraTargetObj = new GameObject("CameraTarget");
            cameraTargetObj.transform.SetParent(_spawnedPlayer.transform);
            cameraTargetObj.transform.localPosition = new Vector3(0, 1.6f, 0);
            cameraTargetObj.transform.localRotation = Quaternion.identity;
            _cameraTarget = cameraTargetObj.transform;

            _playableGraph = PlayableGraph.Create("CinematicGraph");
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            AnimationPlayableOutput.Create(_playableGraph, "Animation", _playerAnimator);

            if (_playerFollowCam != null)
            {
                _playerFollowCam.Follow = _cameraTarget;
                _playerFollowCam.LookAt = _cameraTarget;
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            HidePlayerHead();
        }

        private void HidePlayerHead()
        {
            if (_spawnedPlayer == null)
                return;

            SkinnedMeshRenderer[] renderers = _spawnedPlayer.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var renderer in renderers)
            {
                if (renderer.name.ToLower().Contains("head") || renderer.name.ToLower().Contains("hair"))
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
                }
            }
        }

        private UnityEngine.RuntimeAnimatorController CreateRuntimeAnimatorController()
        {
            Debug.LogError("SeasonCinematicController: Player prefab needs an Animator with a Controller!");
            Debug.LogError("Please follow the steps in ANIMATION_SETUP_GUIDE.txt to set up animations.");
            Debug.LogError("Quick fix: Add an Animator component to your player prefab in the Inspector.");
            return null;
        }

        private void PlayLayingDownAnimation()
        {
            if (_layingDownIdleClip == null || !_playableGraph.IsValid())
            {
                Debug.LogWarning("SeasonCinematicController: No laying down animation clip assigned or PlayableGraph invalid.");
                Debug.LogWarning("Player will appear in default pose. Assign animation clip in Inspector.");
                return;
            }

            _currentClipPlayable = AnimationClipPlayable.Create(_playableGraph, _layingDownIdleClip);
            _currentClipPlayable.SetTime(0);
            _currentClipPlayable.SetSpeed(0);

            var playableOutput = _playableGraph.GetOutput(0);
            ((AnimationPlayableOutput)playableOutput).SetSourcePlayable(_currentClipPlayable);

            _playableGraph.Play();
            
            Debug.Log($"SeasonCinematicController: Playing laying down animation '{_layingDownIdleClip.name}'");
        }

        private void UpdateWakeUpInput()
        {
            if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            {
                StartCoroutine(PlayWakeUpAnimation());
            }
        }

        private IEnumerator PlayWakeUpAnimation()
        {
            _promptText.text = "";

            if (_wakeUpClip != null && _playableGraph.IsValid())
            {
                _currentClipPlayable.Destroy();

                _currentClipPlayable = AnimationClipPlayable.Create(_playableGraph, _wakeUpClip);
                var playableOutput = _playableGraph.GetOutput(0);
                ((AnimationPlayableOutput)playableOutput).SetSourcePlayable(_currentClipPlayable);

                _currentClipPlayable.Play();

                yield return new WaitForSeconds(_wakeUpClip.length);

                _playableGraph.Stop();
                _playableGraph.Destroy();
                _currentClipPlayable.Destroy();

                if (_characterController != null)
                {
                    _characterController.enabled = true;
                }

                if (_playerController != null)
                {
                    _playerController.EnableControl(true);
                }

                if (_playerAnimator != null)
                {
                    _playerAnimator.enabled = true;
                    _playerAnimator.Rebind();
                    _playerAnimator.Play("Idle");
                }
            }

            _hasWokenUp = true;
            SetActiveCamera(_playerFollowCam);
        }

        private void CheckDistanceToNPC()
        {
            if (_spawnedPlayer == null || _npcTransform == null)
                return;

            float distance = Vector3.Distance(_spawnedPlayer.transform.position, _npcTransform.position);

            bool wasNear = _isNearNPC;
            _isNearNPC = distance <= _interactionDistance;

            if (_isNearNPC && !wasNear && !_hasSpokenToNPC)
            {
                _promptText.text = "Hold P To Talk";
            }
            else if (!_isNearNPC && wasNear)
            {
                _promptText.text = "";
                _holdTimer = 0f;
            }
        }

        private void UpdateHoldInput()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.pKey.isPressed)
            {
                _holdTimer += Time.deltaTime;

                float progress = Mathf.Clamp01(_holdTimer / _holdDuration);
                _promptText.text = $"Hold P To Talk [{(int)(progress * 100)}%]";

                if (_holdTimer >= _holdDuration)
                {
                    OnTalkToNPC();
                }
            }
            else
            {
                _holdTimer = 0f;
                if (_isNearNPC)
                {
                    _promptText.text = "Hold P To Talk";
                }
            }
        }

        private void OnTalkToNPC()
        {
            _hasSpokenToNPC = true;
            _holdTimer = 0f;
            _promptText.text = "";
        }

        private IEnumerator PlayFirstDialog()
        {
            SetActiveCamera(_dialogueCam);

            if (_npcAnimator != null)
            {
                _npcAnimator.Play(_npcTalkStateName, 0, 0f);
            }

            if (_firstDialogClip != null && _dialogAudioSource != null)
            {
                _dialogAudioSource.clip = _firstDialogClip;
                _dialogAudioSource.Play();
                yield return new WaitForSeconds(_firstDialogClip.length);
            }
            else
            {
                yield return new WaitForSeconds(5f);
            }
        }

        private IEnumerator PlayMapFlyby()
        {
            SetActiveCamera(_flybyCamera);

            if (_flybyWaypoints == null || _flybyWaypoints.Length == 0)
            {
                yield return new WaitForSeconds(5f);
                yield break;
            }

            Transform cameraTransform = _flybyCamera.transform;
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
                        _flybyWaypoints[nextIndex].position,
                        segmentT
                    );

                    cameraTransform.rotation = Quaternion.Slerp(
                        _flybyWaypoints[currentIndex].rotation,
                        _flybyWaypoints[nextIndex].rotation,
                        segmentT
                    );
                }

                yield return null;
            }

            if (_flybyWaypoints[_flybyWaypoints.Length - 1] != null)
            {
                cameraTransform.position = _flybyWaypoints[_flybyWaypoints.Length - 1].position;
                cameraTransform.rotation = _flybyWaypoints[_flybyWaypoints.Length - 1].rotation;
            }

            yield return new WaitForSeconds(_finalWaypointHoldTime);
        }

        private IEnumerator PlaySecondDialog()
        {
            if (_secondDialogClip != null && _dialogAudioSource != null)
            {
                _dialogAudioSource.clip = _secondDialogClip;
                _dialogAudioSource.Play();
                yield return new WaitForSeconds(_secondDialogClip.length);
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }
        }

        private void CompleteIntro()
        {
            if (_spawnedPlayer != null)
            {
                Destroy(_spawnedPlayer);
            }

            if (_npcCharacter != null)
            {
                _npcCharacter.SetActive(false);
            }

            if (_uiCanvas != null)
            {
                _uiCanvas.gameObject.SetActive(false);
            }

            SetActiveCamera(null);

            _isIntroActive = false;

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            var battleRoyaleMode = FindFirstObjectByType<BattleRoyaleGameplayMode>();
            if (battleRoyaleMode != null)
            {
                battleRoyaleMode.StartImmediately();
            }
        }

        private void SetActiveCamera(CinemachineVirtualCamera camera)
        {
            Debug.Log($"SeasonCinematicController: SetActiveCamera called with {(camera != null ? camera.name : "null")}");
            
            if (_introCam != null) _introCam.Priority = 0;
            if (_playerFollowCam != null) _playerFollowCam.Priority = 0;
            if (_dialogueCam != null) _dialogueCam.Priority = 0;
            if (_flybyCamera != null) _flybyCamera.Priority = 0;

            if (camera != null)
            {
                camera.Priority = 100;
                Debug.Log($"SeasonCinematicController: Set {camera.name} priority to 100");
            }
        }

        private IEnumerator FadeFromBlack()
        {
            if (_fadeImage == null)
                yield break;

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

        private IEnumerator FadeMusicIn(AudioClip clip)
        {
            if (clip == null || _musicAudioSource == null)
                yield break;

            _musicAudioSource.clip = clip;
            _musicAudioSource.volume = 0f;
            _musicAudioSource.Play();

            float elapsed = 0f;
            while (elapsed < _musicFadeDuration)
            {
                elapsed += Time.deltaTime;
                _musicAudioSource.volume = elapsed / _musicFadeDuration;
                yield return null;
            }

            _musicAudioSource.volume = 1f;
        }

        private IEnumerator FadeMusicOut()
        {
            if (_musicAudioSource == null || !_musicAudioSource.isPlaying)
                yield break;

            float startVolume = _musicAudioSource.volume;
            float elapsed = 0f;

            while (elapsed < _musicFadeDuration)
            {
                elapsed += Time.deltaTime;
                _musicAudioSource.volume = startVolume * (1f - (elapsed / _musicFadeDuration));
                yield return null;
            }

            _musicAudioSource.volume = 0f;
            _musicAudioSource.Stop();
        }

        private IEnumerator CrossfadeMusic(AudioClip newClip)
        {
            if (newClip == null || _musicAudioSource == null)
                yield break;

            float halfDuration = _musicFadeDuration * 0.5f;
            float elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                _musicAudioSource.volume = 1f - (elapsed / halfDuration);
                yield return null;
            }

            _musicAudioSource.clip = newClip;
            _musicAudioSource.Play();

            elapsed = 0f;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                _musicAudioSource.volume = elapsed / halfDuration;
                yield return null;
            }

            _musicAudioSource.volume = 1f;
        }

        [ContextMenu("Reset Intro (Testing)")]
        public void ResetIntro()
        {
            PlayerPrefs.DeleteKey(PREF_HAS_SEEN_INTRO);
            PlayerPrefs.Save();
        }
    }
}
