using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using Fusion.Addons.KCC;

namespace TPSBR
{
	using TPSBR.UI;

	public sealed class AirplaneAgent : ContextBehaviour
	{
		// PUBLIC MEMBERS

		[Networked, HideInInspector]
		public Airplane Airplane { get; set; }

		// PRIVATE MEMBERS

		[SerializeField]
		private Transform _cameraTransform;

		[Networked]
		private Quaternion _lookRotation { get; set; }
		[Networked]
		private GameplayInput _lastKnownInput { get; set; }

		private bool              _jump;
		private bool              _isParented;
		private Vector2           _lookRotationDelta;
		private SmoothVector2     _smoothLookRotationDelta = new SmoothVector2(256);
		private UIMobileInputView _mobileInputView;

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			_smoothLookRotationDelta.ClearValues();

			NetworkEvents networkEvents = Runner.GetComponent<NetworkEvents>();
			networkEvents.OnInput.RemoveListener(OnInput);
			networkEvents.OnInput.AddListener(OnInput);

			_lookRotation = transform.localRotation;

			if (HasInputAuthority == true)
			{
				Context.WaitingAgentTransform = transform;
			}

			_isParented = false;
		}

		public override void FixedUpdateNetwork()
		{
			if (_isParented == false && Airplane != null)
			{
				transform.SetParent(Airplane.AgentPosition, false);
				_isParented = true;
			}

			if (IsProxy == true)
				return;

			if (Runner.TryGetInputForPlayer(Object.InputAuthority, out GameplayInput input) == true)
			{
				_lookRotation = UpdateRotation(_lookRotation, input.LookRotationDelta);

				if (EGameplayInputAction.Jump.WasActivated(input, _lastKnownInput) == true)
				{
					if ((Context.GameplayMode as BattleRoyaleGameplayMode).RequestAirplaneJump(Object.InputAuthority, transform.rotation * Vector3.forward) == true)
						return;
				}

				_lastKnownInput = input;
			}

			if (HasStateAuthority == true && Runner.IsPlayerValid(Object.InputAuthority) == true)
			{
				Vector3 lookDirection = _lookRotation * Vector3.forward;

				Runner.ClearPlayerAreaOfInterest(Object.InputAuthority);
				Runner.AddPlayerAreaOfInterest(Object.InputAuthority, transform.position + lookDirection *  25.0f,  50.0f);
				Runner.AddPlayerAreaOfInterest(Object.InputAuthority, transform.position + lookDirection * 100.0f,  75.0f);
				Runner.AddPlayerAreaOfInterest(Object.InputAuthority, transform.position + lookDirection * 175.0f, 100.0f);
			}
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			if (runner != null)
			{
				NetworkEvents networkEvents = runner.GetComponent<NetworkEvents>();
				networkEvents.OnInput.RemoveListener(OnInput);
			}

			if (HasInputAuthority == true && Context.WaitingAgentTransform == transform)
			{
				Context.WaitingAgentTransform = null;
			}
		}

		public override void Render()
		{
			if (Context.ObservedPlayerRef != Object.InputAuthority)
				return;

			if (_isParented == false && Airplane != null)
			{
				transform.SetParent(Airplane.AgentPosition, false);
				_isParented = true;
			}

			UpdateRotation(_lookRotation, _lookRotationDelta);

			Context.Camera.transform.position = _cameraTransform.position;
			Context.Camera.transform.rotation = _cameraTransform.rotation;
		}

		// MONOBEHAVIOUR

		private void Update()
		{
			if (HasInputAuthority == false || Context.HasInput == false)
				return;

			if ((Context.Input.IsCursorVisible == true && Context.Settings.SimulateMobileInput == false) || Context.GameplayMode.State != GameplayMode.EState.Active)
				return;

			Vector2 mouseDelta = default;

			if ((Application.isMobilePlatform == false || Application.isEditor == true) && Context.Settings.SimulateMobileInput == false)
			{
				mouseDelta = Mouse.current.delta.ReadValue() * 0.075f;
				_jump |= Keyboard.current.spaceKey.wasPressedThisFrame;
			}
			else
			{
				if (_mobileInputView == null)
				{
					if (Context.UI != null)
					{
						_mobileInputView = Context.UI.Get<UIMobileInputView>();
						_mobileInputView.Look = default;
					}
				}
				else
				{
					const float mobileSensitivityMultiplier = 32.0f;
					mouseDelta = _mobileInputView.Look * mobileSensitivityMultiplier;
					_mobileInputView.Look = default;
					_jump |= _mobileInputView.Jump;
				}
			}

			_lookRotationDelta += InputUtility.GetSmoothLookRotationDelta(_smoothLookRotationDelta, new Vector2(-mouseDelta.y, mouseDelta.x), Global.RuntimeSettings.Sensitivity, 0.025f);
		}

		// PRIVATE METHODS

		private void OnInput(NetworkRunner runner, Fusion.NetworkInput networkInput)
		{
			if (HasInputAuthority == false || Context.HasInput == false)
				return;

			GameplayInput gameplayInput = new GameplayInput();

			gameplayInput.Jump = _jump;
			gameplayInput.LookRotationDelta = _lookRotationDelta;

			networkInput.Set(gameplayInput);

			_jump = default;
			_lookRotationDelta = default;
		}

		private Quaternion UpdateRotation(Quaternion rotation, Vector2 rotationDelta)
		{
			if (rotationDelta == Vector2.zero)
			{
				transform.localRotation = rotation;
				return rotation;
			}

			Vector3 rotationEuler = KCCUtility.GetClampedEulerLookRotation(rotation);

			float lookRotationDeltaX = Mathf.Clamp(rotationEuler.x + rotationDelta.x, -85f, 85f) - rotationEuler.x;
			float lookRotationDeltaY = rotationDelta.y;

			var newRotation = Quaternion.Euler(rotationEuler + new Vector3(lookRotationDeltaX, lookRotationDeltaY, 0f));

			transform.localRotation = newRotation;
			return newRotation;
		}
	}
}
