using UnityEngine;
using Fusion;

namespace TPSBR
{
    [DefaultExecutionOrder(6000)]
    public sealed class Jetpack : ContextBehaviour
    {
        // CONSTANTS

        private const int BIT_IS_ACTIVE = 0;
        private const int BIT_FULL_THRUST = 1;
        private const int BIT_HAS_STARTED = 2;

        // PUBLIC MEMBERS

        public bool IsActive { get { return _state.IsBitSet(BIT_IS_ACTIVE); } set { byte state = _state; state.SetBit(BIT_IS_ACTIVE, value); _state = state; } }
        public bool FullThrust { get { return _state.IsBitSet(BIT_FULL_THRUST); } set { byte state = _state; state.SetBit(BIT_FULL_THRUST, value); _state = state; } }
        public bool HasStarted { get { return _state.IsBitSet(BIT_HAS_STARTED); } set { byte state = _state; state.SetBit(BIT_HAS_STARTED, value); _state = state; } }

        public bool IsRunning => IsActive == true && _fuel > 0f;
        public float Fuel => _fuel;
        public float MaxFuel => _maxFuel;

        // PRIVATE MEMBERS

        [SerializeField]
        private float _initialFuel = 100f;
        [SerializeField]
        private float _maxFuel = 100f;
        [SerializeField]
        private float _idleThrustConsumption = 7f;
        [SerializeField]
        private float _fullThrustConsumption = 15f;
        [SerializeField]
        private GameObject _jetpackObject;
        [SerializeField]
        private Animation _jetpackAnimation;
        [SerializeField]
        private Transform _visualsRoot;
        [SerializeField]
        private Vector3 _centerOfGravityOffset = new Vector3(0f, 1.5f, 0f);
        [SerializeField]
        private ShakeSetup _cameraPositionShake;
        [SerializeField]
        private ShakeSetup _cameraRotationShake;
        [SerializeField]
        private float _fullThrustShakeMultiplier = 2f;

        [Header("Propellers")]
        [SerializeField]
        private Transform _rightMount;
        [SerializeField]
        private Transform _leftMount;
        [SerializeField]
        private Transform _rightPropeller;
        [SerializeField]
        private Transform _leftPropeller;
        [SerializeField]
        private float _mountForwardAngle = 30f;
        [SerializeField]
        private float _mountBackwardAngle = -15;
        [SerializeField]
        private float _mountChangeSpeed = 8f;
        [SerializeField]
        private float _idlePropellerSpeed = 720f;
        [SerializeField]
        private float _fullPropellerSpeed = 3500f;
        [SerializeField]
        private float _propellerSpeedChange = 8f;

        [Header("Audio")]
        [SerializeField]
        private AudioSource _engineSound;
        [SerializeField]
        private float _engineSoundChangeSpeed = 8f;
        [SerializeField]
        private float _fullThrustPitch = 1.5f;
        [SerializeField]
        private float _idleThrustVolume = 0.6f;
        [SerializeField]
        private float _fullThrustVolume = 1f;

        [Networked, HideInInspector]
        private byte _state { get; set; }

        [Networked]
        private float _fuel { get; set; }

        private Agent _agent;
        private byte _localState;

        private float _mountAngle;
        private float _propellerSpeed;

        private float _defaultPitch;

        private float _positionShakeMagnitude;
        private float _rotationShakeMagnitude;

        private Vector3 _tiltAngles;

        // PUBLIC METHODS

        public void OnSpawned(Agent agent)
        {
            _agent = agent;
            _localState = default;

            _jetpackObject.SetActive(false);

            AddFuel(_initialFuel);

            UpdateLocalState();
        }

        public void OnFixedUpdate()
        {
            if (IsActive == false)
                return;

            if (_agent.Health.IsAlive == false)
            {
                Deactivate();
                return;
            }

            bool isGrounded = _agent.Character.CharacterController.Data.IsGrounded;
            if (HasStarted == false)
            {
                HasStarted = isGrounded == false;
            }
            else if (isGrounded == true)
            {
                GameplayQuestHooks.NotifyPlayerLanded(_agent, _agent.transform.position);
                Deactivate();
                return;
            }

            HasStarted |= _agent.Character.CharacterController.Data.IsGrounded == false;

            UpdateLocalState();

            // Ensure no weapon can be held
            _agent.Weapons.DisarmCurrentWeapon();

            float consumption = FullThrust == true ? _fullThrustConsumption : _idleThrustConsumption;
            _fuel -= consumption * Runner.DeltaTime;

            if (_fuel <= 0f)
            {
                _fuel = 0f;
                //Deactivate();
            }
        }

        public void OnDespawned()
        {
            Deactivate();
        }

        public bool AddFuel(float fuel)
        {
            if (_fuel >= _maxFuel)
                return false;

            _fuel = Mathf.Min(_fuel + fuel, _maxFuel);

            return true;
        }

        public bool Activate()
        {
            if (IsActive == true)
                return false;

            if (_fuel <= 0)
                return false;

            IsActive = true;

            UpdateLocalState();

            return true;
        }

        public void Deactivate()
        {
            if (IsActive == false)
                return;

            IsActive = false;
            FullThrust = false;
            HasStarted = false;

            UpdateLocalState();
        }

        public override void Render()
        {
            UpdateLocalState();

            float moveDirectionY = _agent.HasInputAuthority == true ? _agent.AgentInput.AccumulatedInput.MoveDirection.y : default;

            float targetMountAngle = MathUtility.Map(-1, 1, _mountBackwardAngle, _mountForwardAngle, moveDirectionY);
            _mountAngle = Mathf.Lerp(_mountAngle, targetMountAngle, Time.deltaTime * _mountChangeSpeed);

            _rightMount.localRotation = Quaternion.Euler(new Vector3(_mountAngle, 0f, 0f));
            _leftMount.localRotation = _rightMount.localRotation;

            float targetPropellerSpeed = _fuel > 0f ? (FullThrust == true ? _fullPropellerSpeed : _idlePropellerSpeed) : 0f;
            _propellerSpeed = Mathf.Lerp(_propellerSpeed, targetPropellerSpeed, Time.deltaTime * _propellerSpeedChange);

            _rightPropeller.Rotate(Vector3.up, _propellerSpeed * Time.deltaTime, Space.Self);
            _leftPropeller.Rotate(Vector3.up, _propellerSpeed * Time.deltaTime, Space.Self);

            float targetVolume = IsRunning == true ? (FullThrust == true ? _fullThrustVolume : _idleThrustVolume) : 0f;
            float targetPitch = IsRunning == true && FullThrust == true ? _fullThrustPitch : _defaultPitch;

            _engineSound.volume = Mathf.Lerp(_engineSound.volume, targetVolume, Time.deltaTime * _engineSoundChangeSpeed);
            _engineSound.pitch = Mathf.Lerp(_engineSound.pitch, targetPitch, Time.deltaTime * _engineSoundChangeSpeed);

            _visualsRoot.localPosition = Vector3.zero;
            _visualsRoot.localRotation = Quaternion.identity;

            var localVelocity = IsRunning == true ? Quaternion.Inverse(transform.rotation) * _agent.Character.CharacterController.Data.RealVelocity : Vector3.zero;

            _tiltAngles.x = Mathf.Lerp(_tiltAngles.x, -localVelocity.x, Time.deltaTime * 8f);
            _tiltAngles.z = Mathf.Lerp(_tiltAngles.z, localVelocity.z * 2f, Time.deltaTime * 8f);

            _visualsRoot.RotateAround(transform.position + _centerOfGravityOffset, transform.forward, _tiltAngles.x);
            _visualsRoot.RotateAround(transform.position + _centerOfGravityOffset, transform.right, _tiltAngles.z);

            _cameraPositionShake.Magnitude = FullThrust == true ? _positionShakeMagnitude * _fullThrustShakeMultiplier : _positionShakeMagnitude;
            _cameraRotationShake.Magnitude = FullThrust == true ? _rotationShakeMagnitude * _fullThrustShakeMultiplier : _rotationShakeMagnitude;
        }

        // MONOBEHAVIOUR

        private void Awake()
        {
            _positionShakeMagnitude = _cameraPositionShake.Magnitude;
            _rotationShakeMagnitude = _cameraRotationShake.Magnitude;

            _defaultPitch = _engineSound.pitch;
        }

        // PRIVATE METHODS

        private void Activate_Internal()
        {
            _jetpackAnimation.PlayForward();
            _engineSound.Play();

            if (HasInputAuthority == true)
            {
                var shake = Context.Camera.ShakeEffect;

                shake.Play(_cameraPositionShake);
                shake.Play(_cameraRotationShake);
            }
        }

        private void Deactivate_Internal()
        {
            _jetpackAnimation.Play(_jetpackAnimation.clip.name, -3f);
            _engineSound.Stop();

            if (HasInputAuthority == true)
            {
                var shake = Context.Camera.ShakeEffect;

                shake.Stop(_cameraPositionShake);
                shake.Stop(_cameraRotationShake);
            }
        }

        // NETWORK CALLBACKS

        private void UpdateLocalState()
        {
            bool stateIsActive = _state.IsBitSet(BIT_IS_ACTIVE);
            bool localStateIsActive = _localState.IsBitSet(BIT_IS_ACTIVE);

            _localState = _state;

            if (stateIsActive != localStateIsActive)
            {
                if (stateIsActive == true)
                {
                    Activate_Internal();
                }
                else
                {
                    Deactivate_Internal();
                }
            }
        }
    }
}
