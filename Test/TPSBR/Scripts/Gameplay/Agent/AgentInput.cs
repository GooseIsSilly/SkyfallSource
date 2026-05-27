using Fusion;
using Fusion.Addons.KCC;
using Fusion.Plugin;

namespace TPSBR
{
	using System;
	using UnityEngine;
	using UnityEngine.InputSystem;
	using TPSBR.UI;

	[DefaultExecutionOrder(-10)]
	public sealed partial class AgentInput : ContextBehaviour, IBeforeTick, IAfterAllTicks
	{
		// PUBLIC MEMBERS

		/// <summary>
		/// Holds input for fixed update.
		/// </summary>
		public GameplayInput FixedInput { get { CheckFixedAccess(false); return _fixedInput; } }

		/// <summary>
		/// Holds input for current frame render update.
		/// </summary>
		public GameplayInput RenderInput { get { CheckRenderAccess(false); return _renderInput; } }

		/// <summary>
		/// Holds accumulated inputs from all render frames since last fixed update. Used when Fusion input poll is triggered.
		/// </summary>
		public GameplayInput AccumulatedInput { get { CheckRenderAccess(false); return _accumulatedInput; } }

		public bool          IsCyclingGrenades => Time.time < _grenadesCyclingStartTime + _grenadesCycleDuration;

		/// <summary>
		/// These actions won't be accumulated and polled by Fusion if they are triggered in the same frame as the simulation.
		/// They are accumulated after Fusion simulation and before Render(), effectively defering actions to first fixed simulation in following frames.
		/// This makes fixed and render-predicted movement much more consistent (less prediction correction) at the cost of slight delay.
		/// </summary>
		[NonSerialized]
		public EGameplayInputAction[] DeferredInputActions = new EGameplayInputAction[] { EGameplayInputAction.Attack, EGameplayInputAction.Jump, EGameplayInputAction.ToggleJetpack };

		/// <summary>
		/// These actions trigger sending interpolation data required for render-accurate lag compensation queries.
		/// Like DeferredInputActions, these actions won't be accumulated and polled by Fusion if they are triggered in the same frame as the simulation.
		/// They are accumulated after Fusion simulation and before Render(), effectively defering actions to first fixed simulation in following frames.
		/// </summary>
		[NonSerialized]
		public EGameplayInputAction[] InterpolationDataActions = new EGameplayInputAction[] { EGameplayInputAction.Attack };

		// PRIVATE MEMBERS

		[SerializeField]
		private float _grenadesCycleDuration = 2f;
		[SerializeField][Range(0.0f, 0.1f)][Tooltip("Look rotation delta for a render frame is calculated as average from all frames within responsivity time.")]
		private float _lookResponsivity = 0.0f;
		[SerializeField][Range(0.0f, 1.0f)][Tooltip("How long the last known input is repeated before using default.")]
		private float _maxRepeatTime = 0.25f;
		[SerializeField][Tooltip("Outputs missing inputs to console.")]
		private bool  _logMissingInputs;

		// We need to store current input to compare against previous input (to track actions activation/deactivation). It is also reused if the input for current tick is not available.
		// This is not needed on proxies and will be replicated to input authority only.
		[Networked]
		private GameplayInput _fixedInput { get; set; }

		private Agent             _agent;
		private GameplayInput     _renderInput;
		private GameplayInput     _accumulatedInput;
		private GameplayInput     _previousFixedInput;
		private GameplayInput     _previousRenderInput;
		private GameplayInput     _deferActionsInput;
		private bool              _useDeferActionsInput;
		private bool              _updateInterpolationData;
		private Vector2           _partialMoveDirection;
		private float             _partialMoveDirectionSize;
		private Vector2           _accumulatedMoveDirection;
		private float             _accumulatedMoveDirectionSize;
		private SmoothVector2     _smoothLookRotationDelta = new SmoothVector2(256);
		private float             _repeatTime;
		private float             _lastRenderAlpha;
		private float             _inputPollDeltaTime;
		private int               _lastInputPollFrame;
		private int               _processInputFrame;
		private int               _missingInputsInRow;
		private int               _missingInputsTotal;
		private int               _logMissingInputFromTick;
		private float             _grenadesCyclingStartTime;
		private UIMobileInputView _mobileInputView;

		// PUBLIC METHODS

		/// <summary>
		/// Check if an action is active in current input. FUN/Render input is resolved automatically.
		/// </summary>
		public bool HasActive(EGameplayInputAction action)
		{
			if (Runner.Stage != default)
			{
				CheckFixedAccess(false);
				return action.IsActive(_fixedInput);
			}
			else
			{
				CheckRenderAccess(false);
				return action.IsActive(_renderInput);
			}
		}

		/// <summary>
		/// Check if an action was activated in current input.
		/// In FUN this method compares current fixed input agains previous fixed input.
		/// In Render this method compares current render input against previous render input OR current fixed input (first Render call after FUN).
		/// </summary>
		public bool WasActivated(EGameplayInputAction action)
		{
			if (Runner.Stage != default)
			{
				CheckFixedAccess(false);
				return action.WasActivated(_fixedInput, _previousFixedInput);
			}
			else
			{
				CheckRenderAccess(false);
				return action.WasActivated(_renderInput, _previousRenderInput);
			}
		}

		/// <summary>
		/// Check if an action was activated in custom input.
		/// In FUN this method compares custom input agains previous fixed input.
		/// In Render this method compares custom input against previous render input OR current fixed input (first Render call after FUN).
		/// </summary>
		public bool WasActivated(EGameplayInputAction action, GameplayInput customInput)
		{
			if (Runner.Stage != default)
			{
				CheckFixedAccess(false);
				return action.WasActivated(customInput, _previousFixedInput);
			}
			else
			{
				CheckRenderAccess(false);
				return action.WasActivated(customInput, _previousRenderInput);
			}
		}

		/// <summary>
		/// Check if an action was deactivated in current input.
		/// In FUN this method compares current fixed input agains previous fixed input.
		/// In Render this method compares current render input against previous render input OR current fixed input (first Render call after FUN).
		/// </summary>
		public bool WasDeactivated(EGameplayInputAction action)
		{
			if (Runner.Stage != default)
			{
				CheckFixedAccess(false);
				return action.WasDeactivated(_fixedInput, _previousFixedInput);
			}
			else
			{
				CheckRenderAccess(false);
				return action.WasDeactivated(_renderInput, _previousRenderInput);
			}
		}

		/// <summary>
		/// Check if an action was deactivated in custom input.
		/// In FUN this method compares custom input agains previous fixed input.
		/// In Render this method compares custom input against previous render input OR current fixed input (first Render call after FUN).
		/// </summary>
		public bool WasDeactivated(EGameplayInputAction action, GameplayInput customInput)
		{
			if (Runner.Stage != default)
			{
				CheckFixedAccess(false);
				return action.WasDeactivated(customInput, _previousFixedInput);
			}
			else
			{
				CheckRenderAccess(false);
				return action.WasDeactivated(customInput, _previousRenderInput);
			}
		}

		/// <summary>
		/// Updates fixed input. Use after manipulating with fixed input outside.
		/// </summary>
		/// <param name="fixedInput">Input used in fixed update.</param>
		/// <param name="setPreviousInputs">Updates base fixed input and base render input.</param>
		public void SetFixedInput(GameplayInput fixedInput, bool setPreviousInputs)
		{
			CheckFixedAccess(true);

			_fixedInput = fixedInput;

			if (setPreviousInputs == true)
			{
				_previousFixedInput  = fixedInput;
				_previousRenderInput = fixedInput;
			}
		}

		/// <summary>
		/// Updates render input. Use after manipulating with render input outside.
		/// </summary>
		/// <param name="renderInput">Input used in render update.</param>
		/// <param name="setPreviousInput">Updates base render input.</param>
		public void SetRenderInput(GameplayInput renderInput, bool setPreviousInput)
		{
			CheckRenderAccess(false);

			_renderInput = renderInput;

			if (setPreviousInput == true)
			{
				_previousRenderInput = renderInput;
			}
		}

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			ReplicateToAll(false);
			ReplicateTo(Object.InputAuthority, true);

			SetDefaults();

			// Wait few seconds before the connection is stable to start tracking missing inputs.
			_logMissingInputFromTick = Runner.Tick + TickRate.Resolve(Runner.Config.Simulation.TickRateSelection).Client * 5;

			if (_agent.HasInputAuthority == false)
				return;

			// Register local player input polling.
			NetworkEvents networkEvents = Runner.GetComponent<NetworkEvents>();
			networkEvents.OnInput.RemoveListener(OnInput);
			networkEvents.OnInput.AddListener(OnInput);

			// Hide cursor
			Context.Input.RequestCursorVisibility(false, ECursorStateSource.Agent);
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			if (runner != null)
			{
				// Unregister local player input polling.
				NetworkEvents networkEvents = runner.GetComponent<NetworkEvents>();
				networkEvents.OnInput.RemoveListener(OnInput);
			}

			SetDefaults();

			_mobileInputView = default;
		}

		public override void FixedUpdateNetwork()
		{
			_agent.EarlyFixedUpdateNetwork();
		}

		public override void Render()
		{
			// If the following flag is set true, it means that input was polled from OnInput callback, but Actions are deferred to match this Render() and processed next FixedUpdateNetwork().
			// Because alpha values are not set at that time, we need to explicitly update them from Render().
			if (_updateInterpolationData == true)
			{
				_updateInterpolationData = false;

				// Get alpha of the Render() call. Later in FUN we can identify when exactly the action was triggered (render-accurate processing).
				_renderInput.LocalAlpha = Runner.LocalAlpha;

				// Store interpolation data. This is used for render-accurate lag-compensated casts.
				Runner.GetInterpolationData(out _renderInput.InterpolationFromTick, out _renderInput.InterpolationToTick, out _renderInput.InterpolationAlpha);

				// This is first render after input polls, we can safely override the accumulated input.
				_accumulatedInput.LocalAlpha            = _renderInput.LocalAlpha;
				_accumulatedInput.InterpolationAlpha    = _renderInput.InterpolationAlpha;
				_accumulatedInput.InterpolationFromTick = _renderInput.InterpolationFromTick;
				_accumulatedInput.InterpolationToTick   = _renderInput.InterpolationToTick;
			}

			ProcessFrameInput(false);

			_lastRenderAlpha = Runner.LocalAlpha;

			_agent.EarlyRender();
		}

		// IBeforeTick INTERFACE

		void IBeforeTick.BeforeTick()
		{
			if (Object == null)
				return;

			if (Context == null || Context.GameplayMode == null || Context.GameplayMode.State != GameplayMode.EState.Active)
			{
				_fixedInput          = default;
				_renderInput         = default;
				_accumulatedInput    = default;
				_previousFixedInput  = default;
				_previousRenderInput = default;
				return;
			}

			// Store previous fixed input as a base. This will be compared agaisnt new fixed input.
			_previousFixedInput = _fixedInput;

			if (Object.InputAuthority == PlayerRef.None)
				return;

			// If this fails, fallback (last known) input will be used as current.
			if (Runner.TryGetInputForPlayer(Object.InputAuthority, out GameplayInput input) == true)
			{
				// New input received, we can store it.
				_fixedInput = input;

				if (Runner.Stage == SimulationStages.Forward)
				{
					// Reset statistics.
					_missingInputsInRow = 0;

					// Reset threshold for repeating inputs.
					_repeatTime = 0.0f;
				}
			}
			else
			{
				if (Runner.Stage == SimulationStages.Forward)
				{
					// Update statistics.
					++_missingInputsInRow;
					++_missingInputsTotal;

					// Update threshold for repeating inputs.
					_repeatTime += Runner.DeltaTime;

					if (_logMissingInputs == true && Runner.Tick >= _logMissingInputFromTick)
					{
						Debug.LogWarning($"Missing input for {Object.InputAuthority} {Runner.Tick}. In Row: {_missingInputsInRow} Total: {_missingInputsTotal} Repeating Last Known Input: {_repeatTime <= _maxRepeatTime}", gameObject);
					}
				}

				if (_repeatTime > _maxRepeatTime)
				{
					_fixedInput = default;
				}
			}
		}

		// IAfterAllTicks INTERFACE

		void IAfterAllTicks.AfterAllTicks(bool resimulation, int tickCount)
		{
			if (resimulation == true)
				return;

			// All OnInput callbacks were executed, we can reset the temporary flag for polling defer actions input.
			_useDeferActionsInput = default;

			// Input consumed in OnInput callback is always tick-aligned, but the input for this frame is aligned with engine/render time.
			// At this point the accumulated input was consumed up to latest tick time, remains only partial input from latest tick time to render time.
			// The remaining input is stored in render input and accumulated input should be equal.
			_accumulatedInput = _renderInput;

			// The current fixed input will be used as a base for first Render() after FixedUpdateNetwork().
			// This is used to detect changes like NetworkButtons press.
			_previousRenderInput = _fixedInput;

			if (_inputPollDeltaTime > 0.0f)
			{
				// The partial move direction contains input since last engine frame.
				// We need to scale it so it equals to FUN => Render delta time instead of Render => Render.
				float remainingRenderInputRatio = _inputPollDeltaTime / Time.unscaledDeltaTime;

				_partialMoveDirection     *= remainingRenderInputRatio;
				_partialMoveDirectionSize *= remainingRenderInputRatio;
			}

			// Resetting accumulated move direction to values from current frame.
			// Because input for current frame was already processed from OnInput callback, we need to reset accumulation to these values, not zero.
			_accumulatedMoveDirection     = _partialMoveDirection;
			_accumulatedMoveDirectionSize = _partialMoveDirectionSize;

			// Now we can reset last frame render input to defaults.
			_partialMoveDirection     = default;
			_partialMoveDirectionSize = default;
		}

		// MonoBehaviour INTERFACE

		private void Awake()
		{
			_agent = GetComponent<Agent>();
		}

		// PARTIAL METHODS

		partial void ProcessStandaloneInput(bool isInputPoll);
		partial void ProcessMobileInput(bool isInputPoll);
		partial void ProcessGamepadInput(bool isInputPoll);

		// PRIVATE METHODS

		private void OnInput(NetworkRunner runner, NetworkInput networkInput)
		{
			int currentFrame = Time.frameCount;

			bool isFirstPoll = _lastInputPollFrame != currentFrame;
			if (isFirstPoll == true)
			{
				_lastInputPollFrame = currentFrame;
				_inputPollDeltaTime = Time.unscaledDeltaTime;

				if (IsFrameInputProcessed() == false)
				{
					_deferActionsInput = _accumulatedInput;

					ProcessFrameInput(true);
				}
			}

			if (_agent.HasInputAuthority == false || Context.HasInput == false)
			{
				_accumulatedInput = default;
				_renderInput      = default;
				return;
			}

			GameplayInput pollInput = _accumulatedInput;

			if (_inputPollDeltaTime > 0.0001f)
			{
				// At this moment the poll input has render input already accumulated.
				// This "reverts" the poll input to a state before last render input accumulation.
				pollInput.LookRotationDelta -= _renderInput.LookRotationDelta;

				// In the first input poll (within single Unity frame) we want to accumulate only "missing" part to align timing with fixed tick (last Runner.LocalAlpha => 1.0).
				// All subsequent input polls return remaining input which is not yet consumed, but again within alignment limits of fixed ticks (0.0 => 1.0 = current => next).
				float baseRenderAlpha = isFirstPoll == true ? _lastRenderAlpha : 0.0f;

				// Here we calculate delta time between last render time (or last input poll simulation time) and time of the pending simulation tick.
				float pendingTickAlignedDeltaTime = (1.0f - baseRenderAlpha) * Runner.DeltaTime;

				// The full render input look rotation delta is not aligned with ticks, we need to remove delta which is ahead of fixed tick time.
				Vector2 pendingTickAlignedLookRotationDelta = _renderInput.LookRotationDelta * Mathf.Clamp01(pendingTickAlignedDeltaTime / _inputPollDeltaTime);

				// Accumulate look rotation delta up to aligned tick time.
				pollInput.LookRotationDelta += pendingTickAlignedLookRotationDelta;

				// Consume same look rotation delta from render input.
				_renderInput.LookRotationDelta -= pendingTickAlignedLookRotationDelta;

				// Decrease remaining input poll delta time by the partial delta time consumed by accumulation.
				_inputPollDeltaTime = Mathf.Max(0.0f, _inputPollDeltaTime - pendingTickAlignedDeltaTime);

				// Accumulated input is now consumed and should equal to remaining render input (after tick-alignment).
				// This will be fully/partially consumed by following OnInput call(s) or next frame.
				_accumulatedInput.LookRotationDelta = _renderInput.LookRotationDelta;
			}
			else
			{
				// Input poll delta time is too small, we consume whole input.
				_accumulatedInput.LookRotationDelta = default;
				_renderInput.LookRotationDelta      = default;
				_inputPollDeltaTime                 = default;
			}

			if (_useDeferActionsInput == true)
			{
				// An action was triggered but it should be processed by the first fixed simulation tick after Render().
				// Instead of polling the accumulated input, we replace actions by accumulated input before the action was triggered.
				pollInput.Actions               = _deferActionsInput.Actions;
				pollInput.LocalAlpha            = _deferActionsInput.LocalAlpha;
				pollInput.InterpolationAlpha    = _deferActionsInput.InterpolationAlpha;
				pollInput.InterpolationFromTick = _deferActionsInput.InterpolationFromTick;
				pollInput.InterpolationToTick   = _deferActionsInput.InterpolationToTick;
			}

			networkInput.Set(pollInput);
		}

		private bool IsFrameInputProcessed() => _processInputFrame == Time.frameCount;

		private void ProcessFrameInput(bool isInputPoll)
		{
			// Collect input from devices.
			// Can be executed multiple times between FixedUpdateNetwork() calls because of faster rendering speed.
			// However the input is processed only once per frame.

			int currentFrame = Time.frameCount;
			if (currentFrame == _processInputFrame)
				return;

			_processInputFrame = currentFrame;

			// Store last render input as a base to current render input.
			_previousRenderInput = _renderInput;

			// Reset input for current frame to default.
			_renderInput = default;

			// Only input authority is tracking render input.
			if (HasInputAuthority == false)
				return;
			if (_agent.HasInputAuthority == false || Context.HasInput == false)
				return;
			if ((Context.Input.IsCursorVisible == true && Context.Settings.SimulateMobileInput == false) || Context.GameplayMode.State != GameplayMode.EState.Active)
				return;

			// Storing the accumulated input for reference.
			GameplayInput previousAccumulatedInput = _accumulatedInput;

			if ((Application.isMobilePlatform == false || Application.isEditor == true) && Context.Settings.SimulateMobileInput == false)
			{
				ProcessStandaloneInput(isInputPoll);
			}
			else
			{
				ProcessMobileInput(isInputPoll);
			}

			ProcessGamepadInput(isInputPoll);

			AccumulateRenderInput();

			if (isInputPoll == true)
			{
				// Check actions that were triggered in this frame and should be deferred - and processed by the first fixed simulation tick after Render().

				for (int i = 0; i < InterpolationDataActions.Length; ++i)
				{
					if (InterpolationDataActions[i].WasActivated(_renderInput, previousAccumulatedInput) == true)
					{
						// Actions that require interpolation data are always deferred.
						_useDeferActionsInput = true;

						// We cannot set alpha value because it is not calculated yet. Postponing to Render().
						_updateInterpolationData = true;

						break;
					}
				}

				if (_useDeferActionsInput == false)
				{
					for (int i = 0; i < DeferredInputActions.Length; ++i)
					{
						if (DeferredInputActions[i].WasActivated(_renderInput, previousAccumulatedInput) == true)
						{
							_useDeferActionsInput = true;
							break;
						}
					}
				}
			}
			else
			{
				// Actions were triggered from Render() in this frame.
				// Interpolation data is correctly calculated and can be directly written to input.

				for (int i = 0; i < InterpolationDataActions.Length; ++i)
				{
					if (InterpolationDataActions[i].WasActivated(_renderInput, previousAccumulatedInput) == true)
					{
						_renderInput.LocalAlpha = Runner.LocalAlpha;
						Runner.GetInterpolationData(out _renderInput.InterpolationFromTick, out _renderInput.InterpolationToTick, out _renderInput.InterpolationAlpha);

						_accumulatedInput.LocalAlpha            = _renderInput.LocalAlpha;
						_accumulatedInput.InterpolationAlpha    = _renderInput.InterpolationAlpha;
						_accumulatedInput.InterpolationFromTick = _renderInput.InterpolationFromTick;
						_accumulatedInput.InterpolationToTick   = _renderInput.InterpolationToTick;

						break;
					}
				}
			}
		}

		private void AccumulateRenderInput()
		{
			// We don't accumulate render move direction directly, instead we accumulate the value multiplied by delta time, the result is then divided by total time accumulated.
			// This approach correctly reflects full throttle in last frame with very fast rendering and is more consistent with fixed simulation.

			_partialMoveDirectionSize = Time.unscaledDeltaTime;
			_partialMoveDirection     = _renderInput.MoveDirection * _partialMoveDirectionSize;

			// In other words:
			// Move direction accumulation is a special case. Let's say simulation runs 30Hz (33.333ms delta time) and render runs 300Hz (3.333ms delta time).
			// If the player hits a key to run forward in last frame before fixed tick, the KCC will move in render by (velocity * 0.003333f).
			// Treating this input the same way for next fixed tick results in KCC moving by (velocity * 0.03333f) - 10x more.
			// Following accumulation proportionally scales move direction so it reflects frames in which input was active.
			// This way the next fixed tick will correspond more accurately to what happened in predicted render.

			_accumulatedMoveDirectionSize += _partialMoveDirectionSize;
			_accumulatedMoveDirection     += _partialMoveDirection;

			// Accumulate input for the OnInput() call, the result represents sum of inputs for all render frames since last fixed tick.
			_accumulatedInput.Actions            = new NetworkButtons(_accumulatedInput.Actions.Bits | _renderInput.Actions.Bits);
			_accumulatedInput.MoveDirection      = _accumulatedMoveDirection / _accumulatedMoveDirectionSize;
			_accumulatedInput.LookRotationDelta += _renderInput.LookRotationDelta;

			if (_renderInput.Weapon != default)
			{
				_accumulatedInput.Weapon = _renderInput.Weapon;
			}
		}

		private byte GetWeaponInput(Keyboard keyboard)
		{
			if (keyboard.qKey.wasPressedThisFrame == true)
				return (byte)(_agent.Weapons.PreviousWeaponSlot + 1); // Fast switch

			int weaponSlot = -1;

			if (keyboard.digit1Key.wasPressedThisFrame == true) { weaponSlot = 0; }
			if (keyboard.digit2Key.wasPressedThisFrame == true) { weaponSlot = 1; }
			if (keyboard.digit3Key.wasPressedThisFrame == true) { weaponSlot = 2; }
			if (keyboard.digit4Key.wasPressedThisFrame == true) { weaponSlot = 3; }
			if (keyboard.digit5Key.wasPressedThisFrame == true) { weaponSlot = 4; }

			if (weaponSlot < 0 && keyboard.gKey.wasPressedThisFrame == true)
			{
				weaponSlot = 3; // Cycle grenades
			}

			if (weaponSlot < 0)
				return 0;

			if (weaponSlot <= 2)
				return (byte)(weaponSlot + 1); // Standard weapon switch

			// Grenades (grenades are under slot 5, 6, 7 - but we cycle them with 4 numped key)
			if (weaponSlot == 3)
			{
				int pendingWeapon = _agent.Weapons.PendingWeaponSlot;
				int grenadesStart = IsCyclingGrenades == true && pendingWeapon < 7 ? Mathf.Max(pendingWeapon, 4) : 4;

				int grenadeToSwitch = _agent.Weapons.GetNextWeaponSlot(grenadesStart, 4);

				_grenadesCyclingStartTime = Time.time;

				if (grenadeToSwitch > 0 && grenadeToSwitch != pendingWeapon)
				{
					return (byte)(grenadeToSwitch + 1);
				}
			}

			return 0;
		}

		private void SetDefaults()
		{
			_fixedInput              = default;
			_renderInput             = default;
			_accumulatedInput        = default;
			_previousFixedInput      = default;
			_previousRenderInput     = default;
			_deferActionsInput       = default;
			_useDeferActionsInput    = default;
			_updateInterpolationData = default;
			_repeatTime              = default;
			_lastRenderAlpha         = default;
			_inputPollDeltaTime      = default;
			_lastInputPollFrame      = default;
			_processInputFrame       = default;
			_missingInputsTotal      = default;
			_missingInputsInRow      = default;

			_smoothLookRotationDelta.ClearValues();
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void CheckFixedAccess(bool checkStage)
		{
			if (checkStage == true && Runner.Stage == default)
			{
				throw new InvalidOperationException("This call should be executed from FixedUpdateNetwork!");
			}

			if (Runner.Stage != default && IsProxy == true)
			{
				throw new InvalidOperationException("Fixed input is available only on State & Input authority!");
			}
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void CheckRenderAccess(bool checkStage)
		{
			if (checkStage == true && Runner.Stage != default)
			{
				throw new InvalidOperationException("This call should be executed outside of FixedUpdateNetwork!");
			}

			if (Runner.Stage == default && HasInputAuthority == false)
			{
				throw new InvalidOperationException("Render and accumulated inputs are available only on Input authority!");
			}
		}
	}
}
