using UnityEngine;
using UnityEngine.InputSystem;

namespace TPSBR
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class CinematicPlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _walkSpeed = 2.5f;
        [SerializeField] private float _rotationSpeed = 10f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _groundCheckDistance = 2f;
        [SerializeField] private float _groundSnapSpeed = 10f;

        [Header("Animation")]
        [SerializeField] private string _speedParameter = "Speed";
        [SerializeField] private float _animationBlendSpeed = 5f;

        private CharacterController _characterController;
        private Animator _animator;
        private LocomotionBlender _locomotionBlender;
        private Vector3 _velocity;
        private float _currentSpeed;
        private bool _isEnabled = true;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _animator = GetComponent<Animator>();
            _locomotionBlender = GetComponent<LocomotionBlender>();
        }

        private void Update()
        {
            if (!_isEnabled || _characterController == null || !_characterController.enabled)
                return;

            HandleMovement();
            HandleRotation();
            ApplyGravityAndGroundSnap();
            UpdateAnimations();
        }

        private void HandleMovement()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return;

            Vector3 moveDirection = Vector3.zero;

            if (keyboard.wKey.isPressed)
                moveDirection += transform.forward;
            if (keyboard.sKey.isPressed)
                moveDirection -= transform.forward;
            if (keyboard.aKey.isPressed)
                moveDirection -= transform.right;
            if (keyboard.dKey.isPressed)
                moveDirection += transform.right;

            moveDirection.y = 0f;
            moveDirection.Normalize();

            if (moveDirection.magnitude > 0.1f)
            {
                Vector3 move = moveDirection * _walkSpeed;
                _characterController.Move(move * Time.deltaTime);
                _currentSpeed = Mathf.Lerp(_currentSpeed, 1f, Time.deltaTime * _animationBlendSpeed);
            }
            else
            {
                _currentSpeed = Mathf.Lerp(_currentSpeed, 0f, Time.deltaTime * _animationBlendSpeed);
            }
        }

        private void HandleRotation()
        {
            var mouse = Mouse.current;
            if (mouse == null)
                return;

            Vector2 mouseDelta = mouse.delta.ReadValue();
            float rotationAmount = mouseDelta.x * _rotationSpeed * 0.1f;

            transform.Rotate(Vector3.up, rotationAmount);
        }

        private void ApplyGravityAndGroundSnap()
        {
            bool isGrounded = _characterController.isGrounded;
            
            if (isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            else
            {
                _velocity.y += _gravity * Time.deltaTime;
            }

            _characterController.Move(_velocity * Time.deltaTime);

            if (!isGrounded)
            {
                RaycastHit hit;
                Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
                
                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, _groundCheckDistance))
                {
                    float distanceToGround = hit.distance - 0.5f;
                    
                    if (distanceToGround > 0.1f)
                    {
                        float snapAmount = Mathf.Min(distanceToGround, _groundSnapSpeed * Time.deltaTime);
                        _characterController.Move(Vector3.down * snapAmount);
                    }
                    else if (distanceToGround < -0.1f)
                    {
                        float raiseAmount = Mathf.Min(Mathf.Abs(distanceToGround), _groundSnapSpeed * Time.deltaTime);
                        _characterController.Move(Vector3.up * raiseAmount);
                    }
                }
            }
        }

        private void UpdateAnimations()
        {
            if (_locomotionBlender != null)
            {
                _locomotionBlender.UpdateBlend(_currentSpeed);
            }
        }

        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }
    }
}
