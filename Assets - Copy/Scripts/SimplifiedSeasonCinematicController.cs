using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using TPSBR;

public class SimplifiedSeasonCinematicController : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private Transform _spawnPoint;

    [Header("Animation Clips (Optional - uses Animator Controller instead)")]
    [SerializeField] private AnimationClip _idleClip;
    [SerializeField] private AnimationClip _walkClip;

    [Header("Camera Settings")]
    [SerializeField] private CinemachineVirtualCamera _playerFollowCam;
    [SerializeField] private float _cameraDistance = 3f;
    [SerializeField] private float _cameraHeight = 1.6f;

    [Header("UI & Audio")]
    [SerializeField] private GameObject _wakeUpUI;
    [SerializeField] private AudioSource _cinematicMusicSource;
    [SerializeField] private AudioSource _gameplayMusicSource;

    [Header("Debug")]
    [SerializeField] private bool _skipLayingDown = true;

    private GameObject _spawnedPlayer;
    private Animator _playerAnimator;
    private Transform _cameraTarget;
    private SimpleCinematicPlayerController _playerController;
    private bool _cinematicComplete = false;

    private enum CinematicState
    {
        NotStarted,
        LayingDown,
        Awake,
        Complete
    }

    private CinematicState _currentState = CinematicState.NotStarted;

    private void Start()
    {
        if (_playerPrefab == null)
        {
            Debug.LogError("SimplifiedSeasonCinematicController: Player prefab not assigned!");
            return;
        }

        if (_spawnPoint == null)
        {
            _spawnPoint = transform;
        }

        StartCinematic();
    }

    private void Update()
    {
        if (_cinematicComplete)
            return;

        if (_currentState == CinematicState.LayingDown)
        {
            if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            {
                WakeUp();
            }
        }
    }

    private void StartCinematic()
    {
        SpawnPlayer();

        if (_skipLayingDown)
        {
            _currentState = CinematicState.Awake;
            EnablePlayerControl();
        }
        else
        {
            _currentState = CinematicState.LayingDown;
            
            if (_wakeUpUI != null)
                _wakeUpUI.SetActive(true);
        }

        Debug.Log($"SimplifiedSeasonCinematicController: Cinematic started in state: {_currentState}");
    }

    private void SpawnPlayer()
    {
        _spawnedPlayer = Instantiate(_playerPrefab, _spawnPoint.position, _spawnPoint.rotation);
        _spawnedPlayer.name = "CinematicPlayer";

        _playerAnimator = _spawnedPlayer.GetComponentInChildren<Animator>();
        
        if (_playerAnimator == null)
        {
            Debug.LogError("SimplifiedSeasonCinematicController: Player prefab must have an Animator component!");
            Debug.LogError("Please add an Animator to your player prefab with an Animator Controller.");
            Debug.LogError("See ANIMATION_QUICKFIX.txt for setup instructions.");
            _playerAnimator = _spawnedPlayer.AddComponent<Animator>();
        }
        else
        {
            Debug.Log($"SimplifiedSeasonCinematicController: Found Animator on player. Controller: {(_playerAnimator.runtimeAnimatorController != null ? _playerAnimator.runtimeAnimatorController.name : "NONE")}");
        }

        if (_playerAnimator.runtimeAnimatorController == null)
        {
            Debug.LogWarning("SimplifiedSeasonCinematicController: Animator has no controller! Animations won't work.");
            Debug.LogWarning("Create an Animator Controller and assign it to the Animator component.");
        }

        CharacterController charController = _spawnedPlayer.GetComponent<CharacterController>();
        if (charController == null)
        {
            charController = _spawnedPlayer.AddComponent<CharacterController>();
            charController.height = 1.8f;
            charController.radius = 0.3f;
            charController.center = new Vector3(0, 0.9f, 0);
        }

        _playerController = _spawnedPlayer.GetComponent<SimpleCinematicPlayerController>();
        if (_playerController == null)
        {
            _playerController = _spawnedPlayer.AddComponent<SimpleCinematicPlayerController>();
        }
        _playerController.enabled = false;

        GameObject cameraTargetObj = new GameObject("CameraTarget");
        cameraTargetObj.transform.SetParent(_spawnedPlayer.transform);
        cameraTargetObj.transform.localPosition = new Vector3(0, _cameraHeight, 0);
        cameraTargetObj.transform.localRotation = Quaternion.identity;
        _cameraTarget = cameraTargetObj.transform;

        if (_playerFollowCam != null)
        {
            _playerFollowCam.Follow = _cameraTarget;
            _playerFollowCam.LookAt = _cameraTarget;
            _playerFollowCam.Priority = 100;

            var transposer = _playerFollowCam.GetCinemachineComponent<CinemachineTransposer>();
            if (transposer != null)
            {
                transposer.m_FollowOffset = new Vector3(0, _cameraHeight, -_cameraDistance);
            }

            Debug.Log("SimplifiedSeasonCinematicController: Camera configured");
        }

        HidePlayerHead();

        if (_currentState == CinematicState.LayingDown && _playerAnimator != null && _playerAnimator.runtimeAnimatorController != null)
        {
            _playerAnimator.SetFloat("MoveSpeed", 0f);
            _playerAnimator.SetBool("IsMoving", false);
            _playerAnimator.SetBool("IsLayingDown", true);
        }
    }

    private void WakeUp()
    {
        Debug.Log("SimplifiedSeasonCinematicController: Player waking up!");

        _currentState = CinematicState.Awake;

        if (_wakeUpUI != null)
            _wakeUpUI.SetActive(false);

        if (_playerAnimator != null && _playerAnimator.runtimeAnimatorController != null)
        {
            _playerAnimator.SetBool("IsLayingDown", false);
            _playerAnimator.SetTrigger("WakeUp");
        }

        Invoke(nameof(EnablePlayerControl), 0.5f);
    }

    private void EnablePlayerControl()
    {
        Debug.Log("SimplifiedSeasonCinematicController: Enabling player control");

        if (_playerController != null)
        {
            _playerController.enabled = true;
        }

        if (_playerAnimator != null && _playerAnimator.runtimeAnimatorController != null)
        {
            _playerAnimator.SetFloat("MoveSpeed", 0f);
            _playerAnimator.SetBool("IsMoving", false);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _currentState = CinematicState.Complete;
        _cinematicComplete = true;

        Debug.Log("SimplifiedSeasonCinematicController: Cinematic complete!");
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
                Debug.Log($"SimplifiedSeasonCinematicController: Hiding renderer: {renderer.name}");
            }
        }
    }

    private void OnDestroy()
    {
        if (_spawnedPlayer != null)
        {
            Destroy(_spawnedPlayer);
        }
    }

    public void UpdateAnimatorParameters(float moveSpeed, bool isMoving)
    {
        if (_playerAnimator != null && _playerAnimator.runtimeAnimatorController != null)
        {
            _playerAnimator.SetFloat("MoveSpeed", moveSpeed);
            _playerAnimator.SetBool("IsMoving", isMoving);
        }
    }
}
