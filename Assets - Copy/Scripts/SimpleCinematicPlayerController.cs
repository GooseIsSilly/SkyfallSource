using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSBR
{
    [RequireComponent(typeof(CharacterController))]
    public class SimpleCinematicPlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _gravity = -9.81f;

        [Header("Camera")]
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _minPitch = -80f;
        [SerializeField] private float _maxPitch = 80f;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _moveSpeedParam = "MoveSpeed";
        [SerializeField] private string _isMovingParam = "IsMoving";

        private CharacterController _characterController;
        private Camera _mainCamera;
        private Vector3 _velocity;
        private bool _isEnabled = false;
        private float _cameraPitch = 0f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();

            if (_animator == null)
            {
                _animator = GetComponentInChildren<Animator>();
            }
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                _mainCamera = Object.FindFirstObjectByType<Camera>();
            }
        }

        private void Update()
        {
            if (!_isEnabled || _characterController == null || !_characterController.enabled)
                return;

            HandleCameraRotation();
            HandleMovement();
        }

        public void EnableControl(bool enable)
        {
            _isEnabled = enable;

            if (enable)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }

            if (!enable && _animator != null)
            {
                _animator.SetFloat(_moveSpeedParam, 0f);
                _animator.SetBool(_isMovingParam, false);
            }
        }

        private void HandleCameraRotation()
        {
            if (Mouse.current == null || _mainCamera == null)
                return;

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();

            float yaw = mouseDelta.x * _mouseSensitivity;
            transform.Rotate(Vector3.up, yaw, Space.World);

            _cameraPitch -= mouseDelta.y * _mouseSensitivity;
            _cameraPitch = Mathf.Clamp(_cameraPitch, _minPitch, _maxPitch);

            Transform cameraTarget = _mainCamera.transform.parent;
            if (cameraTarget != null && cameraTarget.parent == transform)
            {
                cameraTarget.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
            }
        }

        private void HandleMovement()
        {
            Vector2 moveInput = Vector2.zero;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed) moveInput.y += 1f;
                if (Keyboard.current.sKey.isPressed) moveInput.y -= 1f;
                if (Keyboard.current.aKey.isPressed) moveInput.x -= 1f;
                if (Keyboard.current.dKey.isPressed) moveInput.x += 1f;
            }

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 move = (forward * moveInput.y + right * moveInput.x).normalized;

            if (move.magnitude > 0.1f)
            {
                _characterController.Move(move * _moveSpeed * Time.deltaTime);

                if (_animator != null)
                {
                    _animator.SetFloat(_moveSpeedParam, move.magnitude);
                    _animator.SetBool(_isMovingParam, true);
                }
            }
            else
            {
                if (_animator != null)
                {
                    _animator.SetFloat(_moveSpeedParam, 0f);
                    _animator.SetBool(_isMovingParam, false);
                }
            }

            if (!_characterController.isGrounded)
            {
                _velocity.y += _gravity * Time.deltaTime;
            }
            else if (_velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            _characterController.Move(_velocity * Time.deltaTime);
        }
    }
}
