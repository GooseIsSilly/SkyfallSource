using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace TPSBR
{
    public class CinematicIntroController : MonoBehaviour
    {
        [Header("Player Spawn")]
        [SerializeField] private Transform _playerSpawnPoint;
        [SerializeField] private GameObject _playerPrefab;

        [Header("Player Animations")]
        [SerializeField] private AnimationClip _layingDownIdleClip;
        [SerializeField] private AnimationClip _wakeUpClip;

        [Header("Locomotion Animations")]
        [SerializeField] private AnimationClip _idleClip;
        [SerializeField] private AnimationClip _walkClip;

        [Header("NPC")]
        [SerializeField] private GameObject _npcCharacter;
        [SerializeField] private Transform _npcTransform;
        [SerializeField] private Animator _npcAnimator;
        [SerializeField] private AnimationClip _npcTalkAnimationClip;
        [SerializeField] private AnimationClip _npcIdleAnimationClip;
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

        [Header("Audio")]
        [SerializeField] private AudioClip _npcDialogueClip;
        [SerializeField] private AudioClip _theresYourStopClip;
        [SerializeField] private AudioClip _wakeUpMusic;
        [SerializeField] private AudioClip _flybyMusic;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioSource _musicAudioSource;
        [SerializeField] private float _musicFadeDuration = 2f;
        [SerializeField] private float _wakeUpMusicVolume = 0.4f;
        [SerializeField] private float _flybyMusicVolume = 0.5f;

        [Header("Input")]
        [SerializeField] private float _holdDuration = 2f;

        [Header("Scene Settings")]
        [SerializeField] private string _startupScenePath = "Assets/TPSBR/Scenes/Startup.unity";
        [SerializeField] private float _delayAfterVoiceline = 3f;

        private const string CINEMATIC_PLAYED_KEY = "HasPlayedS2Cinematic";

        private GameObject _spawnedPlayer;
        private Transform _cameraTarget;
        private Animator _playerAnimator;
        private CharacterController _characterController;
        private CinematicPlayerController _playerController;
        private PlayableGraph _playableGraph;
        private AnimationClipPlayable _currentClipPlayable;
        private PlayableGraph _npcPlayableGraph;
        private AnimationClipPlayable _npcClipPlayable;
        private bool _isNearNPC = false;
        private float _holdTimer = 0f;
        private bool _hasSpokenToNPC = false;
        private bool _hasWokenUp = false;

        private void Start()
        {
            if (HasPlayedCinematic())
            {
                SceneManager.LoadScene(_startupScenePath);
                return;
            }

            SetupUI();
            StartCoroutine(PlayCinematicSequence());
        }

        private bool HasPlayedCinematic()
        {
            return PlayerPrefs.GetInt(CINEMATIC_PLAYED_KEY, 0) == 1;
        }

        private void MarkCinematicAsPlayed()
        {
            PlayerPrefs.SetInt(CINEMATIC_PLAYED_KEY, 1);
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }
            if (_npcPlayableGraph.IsValid())
            {
                _npcPlayableGraph.Destroy();
            }
        }

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

            if (_uiCanvas != null)
            {
                _uiCanvas.gameObject.SetActive(false);
            }

            if (_fadeImage != null)
            {
                _fadeImage.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (_spawnedPlayer == null)
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

        private IEnumerator PlayCinematicSequence()
        {
            yield return StartCoroutine(FadeFromBlack());

            SpawnPlayer();

            if (_npcCharacter != null)
            {
                _npcCharacter.SetActive(true);
                if (_npcAnimator != null && _npcIdleAnimationClip != null)
                {
                    _npcAnimator.runtimeAnimatorController = null;
                    PlayNPCAnimation(_npcIdleAnimationClip, true);
                }
            }

            SetActiveCamera(_introCam);

            if (_uiCanvas != null)
            {
                _uiCanvas.gameObject.SetActive(true);
            }

            PlayLayingDownAnimation();

            if (_promptText != null)
            {
                _promptText.text = "Press P To Wake Up!";
            }

            if (_wakeUpMusic != null && _musicAudioSource != null)
            {
                _musicAudioSource.loop = true;
                StartCoroutine(FadeMusicIn(_wakeUpMusic, _wakeUpMusicVolume));
            }

            while (!_hasWokenUp)
            {
                yield return null;
            }

            while (!_hasSpokenToNPC)
            {
                yield return null;
            }

            yield return StartCoroutine(PlayNPCDialogue());

            yield return StartCoroutine(FadeMusicOut());

            if (_spawnedPlayer != null)
            {
                Destroy(_spawnedPlayer);
            }

            if (_npcCharacter != null)
            {
                _npcCharacter.SetActive(false);
            }

            yield return StartCoroutine(PlayMapFlyby());

            yield return StartCoroutine(ReturnToLobby());
        }

        private void SpawnPlayer()
        {
            if (_playerPrefab == null || _playerSpawnPoint == null)
            {
                Debug.LogError("[CinematicIntro] Player prefab or spawn point not assigned!");
                return;
            }

            Vector3 spawnPosition = _playerSpawnPoint.position;

            RaycastHit hit;
            if (Physics.Raycast(spawnPosition + Vector3.up * 2f, Vector3.down, out hit, 50f))
            {
                spawnPosition = hit.point;
            }

            _spawnedPlayer = Instantiate(_playerPrefab, spawnPosition, _playerSpawnPoint.rotation);

            _characterController = _spawnedPlayer.GetComponent<CharacterController>();
            if (_characterController == null)
            {
                _characterController = _spawnedPlayer.AddComponent<CharacterController>();
            }
            _characterController.radius = 0.5f;
            _characterController.height = 2f;
            _characterController.center = new Vector3(0, 1f, 0);
            _characterController.enabled = false;

            _playerController = _spawnedPlayer.GetComponent<CinematicPlayerController>();
            if (_playerController == null)
            {
                _playerController = _spawnedPlayer.AddComponent<CinematicPlayerController>();
            }
            _playerController.SetEnabled(false);

            Rigidbody rb = _spawnedPlayer.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            _playerAnimator = _spawnedPlayer.GetComponentInChildren<Animator>();

            GameObject cameraTargetObj = new GameObject("CameraTarget");
            cameraTargetObj.transform.SetParent(_spawnedPlayer.transform);
            cameraTargetObj.transform.localPosition = new Vector3(0, 1.6f, 0);
            cameraTargetObj.transform.localRotation = Quaternion.identity;
            _cameraTarget = cameraTargetObj.transform;

            _playableGraph = PlayableGraph.Create("CinematicGraph");
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            var playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", _playerAnimator);

            if (_playerFollowCam != null)
            {
                _playerFollowCam.Follow = _cameraTarget;
                _playerFollowCam.LookAt = _cameraTarget;

                var transposer = _playerFollowCam.GetCinemachineComponent<CinemachineTransposer>();
                if (transposer != null)
                {
                    transposer.m_FollowOffset = new Vector3(0, 0.3f, -3.5f);
                    transposer.m_BindingMode = CinemachineTransposer.BindingMode.LockToTargetWithWorldUp;
                }
            }

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void PlayLayingDownAnimation()
        {
            if (_layingDownIdleClip == null || !_playableGraph.IsValid())
                return;

            _currentClipPlayable = AnimationClipPlayable.Create(_playableGraph, _layingDownIdleClip);
            _currentClipPlayable.SetDuration(_layingDownIdleClip.length);
            _currentClipPlayable.Pause();

            var playableOutput = _playableGraph.GetOutput(0);
            ((AnimationPlayableOutput)playableOutput).SetSourcePlayable(_currentClipPlayable);

            _playableGraph.Play();
        }

        private void UpdateWakeUpInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.pKey.wasPressedThisFrame)
            {
                StartCoroutine(PlayWakeUpAnimation());
            }
        }

        private IEnumerator PlayWakeUpAnimation()
        {
            if (_promptText != null)
            {
                _promptText.text = "";
            }

            if (_wakeUpClip != null && _playableGraph.IsValid())
            {
                _currentClipPlayable.Destroy();

                _currentClipPlayable = AnimationClipPlayable.Create(_playableGraph, _wakeUpClip);
                _currentClipPlayable.SetDuration(_wakeUpClip.length);

                var playableOutput = _playableGraph.GetOutput(0);
                ((AnimationPlayableOutput)playableOutput).SetSourcePlayable(_currentClipPlayable);

                _currentClipPlayable.Play();

                yield return new WaitForSeconds(_wakeUpClip.length);

                _currentClipPlayable.Pause();
                _playableGraph.Stop();

                if (_characterController != null)
                {
                    _characterController.enabled = true;
                }

                Rigidbody rb = _spawnedPlayer.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }

                if (_playerAnimator != null)
                {
                    _playerAnimator.enabled = true;
                    _playerAnimator.runtimeAnimatorController = null;
                    _playerAnimator.Rebind();
                    _playerAnimator.Update(0f);
                    SetupLocomotionAnimator();
                }

                if (_playerController != null)
                {
                    _playerController.SetEnabled(true);
                }
            }
            else
            {
                yield return new WaitForSeconds(1f);
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
                if (_promptText != null)
                {
                    _promptText.text = "Hold P To Talk";
                }
            }
            else if (!_isNearNPC && wasNear)
            {
                if (_promptText != null)
                {
                    _promptText.text = "";
                }
                _holdTimer = 0f;
            }
        }

        private void UpdateHoldInput()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            if (keyboard.pKey.isPressed)
            {
                _holdTimer += Time.deltaTime;

                if (_promptText != null)
                {
                    float progress = Mathf.Clamp01(_holdTimer / _holdDuration);
                    _promptText.text = $"Hold P To Talk [{(int)(progress * 100)}%]";
                }

                if (_holdTimer >= _holdDuration)
                {
                    OnTalkToNPC();
                }
            }
            else
            {
                _holdTimer = 0f;
                if (_promptText != null && _isNearNPC)
                {
                    _promptText.text = "Hold P To Talk";
                }
            }
        }

        private void OnTalkToNPC()
        {
            _hasSpokenToNPC = true;
            _holdTimer = 0f;

            if (_promptText != null)
            {
                _promptText.text = "";
            }
        }

        private IEnumerator PlayNPCDialogue()
        {
            SetActiveCamera(_dialogueCam);

            if (_npcAnimator != null && _npcTalkAnimationClip != null)
            {
                PlayNPCAnimation(_npcTalkAnimationClip, false);
            }

            if (_npcDialogueClip != null && _audioSource != null)
            {
                _audioSource.clip = _npcDialogueClip;
                _audioSource.Play();
                yield return new WaitForSeconds(_npcDialogueClip.length);
            }
            else
            {
                yield return new WaitForSeconds(5f);
            }

            if (_npcAnimator != null && _npcIdleAnimationClip != null)
            {
                PlayNPCAnimation(_npcIdleAnimationClip, true);
            }
        }

        private IEnumerator PlayMapFlyby()
        {
            SetActiveCamera(_flybyCamera);

            if (_flybyMusic != null && _musicAudioSource != null)
            {
                _musicAudioSource.loop = false;
                StartCoroutine(FadeMusicIn(_flybyMusic, _flybyMusicVolume));
            }

            if (_flybyWaypoints != null && _flybyWaypoints.Length > 0)
            {
                Transform cameraTransform = _flybyCamera.transform;

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

        private void SetActiveCamera(CinemachineVirtualCamera camera)
        {
            if (_introCam != null) _introCam.Priority = 0;
            if (_playerFollowCam != null) _playerFollowCam.Priority = 0;
            if (_dialogueCam != null) _dialogueCam.Priority = 0;
            if (_flybyCamera != null) _flybyCamera.Priority = 0;

            if (camera != null)
            {
                camera.Priority = 100;
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

        private IEnumerator FadeToBlack()
        {
            if (_fadeImage == null)
                yield break;

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
            if (_musicAudioSource == null || clip == null)
                yield break;

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
            if (_musicAudioSource == null || !_musicAudioSource.isPlaying)
                yield break;

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

        private void SetupLocomotionAnimator()
        {
            if (_playerAnimator == null || _idleClip == null || _walkClip == null)
                return;

            if (_playableGraph.IsValid())
            {
                _playableGraph.Destroy();
            }

            _playableGraph = PlayableGraph.Create("LocomotionGraph");
            _playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            var mixer = AnimationMixerPlayable.Create(_playableGraph, 2);

            var idlePlayable = AnimationClipPlayable.Create(_playableGraph, _idleClip);
            idlePlayable.SetApplyFootIK(false);
            idlePlayable.SetDuration(_idleClip.length);
            idlePlayable.Play();

            var walkPlayable = AnimationClipPlayable.Create(_playableGraph, _walkClip);
            walkPlayable.SetApplyFootIK(false);
            walkPlayable.SetDuration(_walkClip.length);
            walkPlayable.Play();

            _playableGraph.Connect(idlePlayable, 0, mixer, 0);
            _playableGraph.Connect(walkPlayable, 0, mixer, 1);

            mixer.SetInputWeight(0, 1f);
            mixer.SetInputWeight(1, 0f);

            var output = AnimationPlayableOutput.Create(_playableGraph, "Animation", _playerAnimator);
            output.SetSourcePlayable(mixer);

            _playableGraph.Play();

            if (_playerController != null)
            {
                var locomotionBlender = _spawnedPlayer.GetComponent<LocomotionBlender>();
                if (locomotionBlender == null)
                {
                    locomotionBlender = _spawnedPlayer.AddComponent<LocomotionBlender>();
                }
                locomotionBlender.Initialize(mixer);
            }
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

            if (loop)
            {
                _npcClipPlayable.SetDuration(double.MaxValue);
            }
            else
            {
                _npcClipPlayable.SetDuration(clip.length);
            }

            var output = AnimationPlayableOutput.Create(_npcPlayableGraph, "NPCAnimation", _npcAnimator);
            output.SetSourcePlayable(_npcClipPlayable);

            _npcPlayableGraph.Play();
        }
    }
}
