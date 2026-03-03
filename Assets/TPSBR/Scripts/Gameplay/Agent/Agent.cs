using UnityEngine;
using UnityEngine.Profiling;
using Fusion;
using Fusion.Addons.KCC;

namespace TPSBR
{
	[DefaultExecutionOrder(-5)]
	public sealed class Agent : ContextBehaviour, ISortedUpdate
	{
		// PUBLIC METHODS

		public bool IsObserved => Context != null && Context.ObservedAgent == this;

		public AgentInput        AgentInput   => _agentInput;
		public Interactions      Interactions => _interactions;
		public Character         Character    => _character;
		public Weapons           Weapons      => _weapons;
		public Health            Health       => _health;
		public AgentSenses       Senses       => _senses;
		public Jetpack           Jetpack      => _jetpack;
		public SprintSystem      Sprint       => _sprint;
		public AgentVFX          Effects      => _agentVFX;
		public AgentInterestView InterestView => _interestView;

		[Networked]
		public NetworkBool LeftSide { get; private set; }

		// PRIVATE MEMBERS

		[SerializeField]
		private float _jumpPower;
		[SerializeField]
		private float _topCameraAngleLimit;
		[SerializeField]
		private float _bottomCameraAngleLimit;
		[SerializeField]
		private GameObject _visualRoot;

		[Header("Fall Damage")]
		[SerializeField]
		private float _minFallDamage = 5f;
		[SerializeField]
		private float _maxFallDamage = 200f;
		[SerializeField]
		private float _maxFallDamageVelocity = 20f;
		[SerializeField]
		private float _minFallDamageVelocity = 5f;

		private AgentInput          _agentInput;
		private Interactions        _interactions;
		private AgentFootsteps      _footsteps;
		private Character           _character;
		private Weapons             _weapons;
		private Jetpack             _jetpack;
		private SprintSystem        _sprint;
		private AgentSenses         _senses;
		private Health              _health;
		private AgentVFX            _agentVFX;
		private AgentInterestView   _interestView;
		private SortedUpdateInvoker _sortedUpdateInvoker;
		private Quaternion          _cachedLookRotation;
		private Quaternion          _cachedPitchRotation;

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			name = Object.InputAuthority.ToString();

			_sortedUpdateInvoker = Runner.GetSingleton<SortedUpdateInvoker>();

			_visualRoot.SetActive(true);

			_character.OnSpawned(this);
			_jetpack.OnSpawned(this);
			_health.OnSpawned(this);
			_agentVFX.OnSpawned(this);

			if (ApplicationSettings.IsStrippedBatch == true)
			{
				gameObject.SetActive(false);

				if (ApplicationSettings.GenerateInput == true)
				{
					NetworkEvents networkEvents = Runner.GetComponent<NetworkEvents>();
					networkEvents.OnInput.RemoveListener(GenerateRandomInput);
					networkEvents.OnInput.AddListener(GenerateRandomInput);
				}
			}

			return;

			void GenerateRandomInput(NetworkRunner runner, NetworkInput networkInput)
			{
				// Used for batch testing

				GameplayInput gameplayInput = new GameplayInput();
				gameplayInput.MoveDirection     = new Vector2(UnityEngine.Random.value * 2.0f - 1.0f, UnityEngine.Random.value > 0.25f ? 1.0f : -1.0f).normalized;
				gameplayInput.LookRotationDelta = new Vector2(UnityEngine.Random.value * 2.0f - 1.0f, UnityEngine.Random.value * 2.0f - 1.0f);
				gameplayInput.Jump              = UnityEngine.Random.value > 0.99f;
				gameplayInput.Attack            = UnityEngine.Random.value > 0.99f;
				gameplayInput.Reload            = UnityEngine.Random.value > 0.99f;
				gameplayInput.Interact          = UnityEngine.Random.value > 0.99f;
				gameplayInput.Weapon            = (byte)(UnityEngine.Random.value > 0.99f ? (UnityEngine.Random.value > 0.25f ? 2 : 1) : 0);

				networkInput.Set(gameplayInput);
			}
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			if (_weapons  != null) { _weapons.OnDespawned();  }
			if (_jetpack  != null) { _jetpack.OnDespawned();  }
			if (_health   != null) { _health.OnDespawned();   }
			if (_agentVFX != null) { _agentVFX.OnDespawned(); }
		}

		public void EarlyFixedUpdateNetwork()
		{
			Profiler.BeginSample($"{nameof(Agent)}(Early)");

			ProcessFixedInput();

			_weapons.OnFixedUpdate();
			_jetpack.OnFixedUpdate();
			_character.OnFixedUpdate();

			Profiler.EndSample();
		}

		public override void FixedUpdateNetwork()
		{
			Profiler.BeginSample($"{nameof(Agent)}(Regular)");

			// Performance optimization, unnecessary euler call
			Quaternion currentLookRotation = _character.CharacterController.FixedData.LookRotation;
			if (_cachedLookRotation.ComponentEquals(currentLookRotation) == false)
			{
				_cachedLookRotation  = currentLookRotation;
				_cachedPitchRotation = Quaternion.Euler(_character.CharacterController.FixedData.LookPitch, 0.0f, 0.0f);
			}

			_character.GetCameraHandle().transform.localRotation = _cachedPitchRotation;

			CheckFallDamage();

			if (_health.IsAlive == true)
			{
				float sortOrder = _agentInput.FixedInput.LocalAlpha;
				if (sortOrder <= 0.0f)
				{
					// Default LocalAlpha value results in update callback being executed last.
					sortOrder = 1.0f;
				}

				// Schedule update to process render-accurate shooting.
				_sortedUpdateInvoker.ScheduleSortedUpdate(this, sortOrder);

				if (Runner.IsServer == true)
				{
					_interestView.SetPlayerInfo(_character.CharacterController.Transform, _character.GetCameraHandle());
				}
			}

			_health.OnFixedUpdate();

			Profiler.EndSample();
		}

		public void EarlyRender()
		{
			if (HasInputAuthority == true)
			{
				ProcessRenderInput();
			}

			_character.OnRender();
		}

		public override void Render()
		{
			if (HasInputAuthority == true || IsObserved == true)
			{
				// Performance optimization, unnecessary euler call
				Quaternion currentLookRotation = _character.CharacterController.RenderData.LookRotation;
				if (_cachedLookRotation.ComponentEquals(currentLookRotation) == false)
				{
					_cachedLookRotation  = currentLookRotation;
					_cachedPitchRotation = Quaternion.Euler(_character.CharacterController.RenderData.LookPitch, 0.0f, 0.0f);
				}

				_character.GetCameraHandle().transform.localRotation = _cachedPitchRotation;
			}

			_character.OnAgentRender();
			_footsteps.OnAgentRender();
		}

		// ISortedUpdate INTERFACE

		void ISortedUpdate.SortedUpdate()
		{
			// This method execution is sorted by LocalAlpha property passed in input and preserves realtime order of input actions.

			bool attackWasActivated   = _agentInput.WasActivated(EGameplayInputAction.Attack);
			bool reloadWasActivated   = _agentInput.WasActivated(EGameplayInputAction.Reload);
			bool interactWasActivated = _agentInput.WasActivated(EGameplayInputAction.Interact);

			TryFire(attackWasActivated, _agentInput.FixedInput.Attack);
			TryReload(reloadWasActivated == false);

			_interactions.TryInteract(interactWasActivated, _agentInput.FixedInput.Interact);
		}

		// MonoBehaviour INTERFACE

		private void Awake()
		{
			_agentInput   = GetComponent<AgentInput>();
			_interactions = GetComponent<Interactions>();
			_footsteps    = GetComponent<AgentFootsteps>();
			_character    = GetComponent<Character>();
			_weapons      = GetComponent<Weapons>();
			_health       = GetComponent<Health>();
			_agentVFX     = GetComponent<AgentVFX>();
			_senses       = GetComponent<AgentSenses>();
			_jetpack      = GetComponent<Jetpack>();
			_sprint       = GetComponent<SprintSystem>();
			_interestView = GetComponent<AgentInterestView>();
		}

		// PRIVATE METHODS

		private void ProcessFixedInput()
		{
			KCC     kcc          = _character.CharacterController;
			KCCData kccFixedData = kcc.FixedData;

			GameplayInput input = default;

			if (_health.IsAlive == true)
			{
				input = _agentInput.FixedInput;
			}

			if (input.Aim == true)
			{
				input.Aim &= CanAim(kccFixedData);
			}

			if (input.Aim == true)
			{
				if (_weapons.CurrentWeapon != null && _weapons.CurrentWeapon.HitType == EHitType.Sniper)
				{
					input.LookRotationDelta *= 0.3f;
				}
			}

			kcc.SetAim(input.Aim);

			if (_agentInput.WasActivated(EGameplayInputAction.Jump, input) == true && _character.AnimationController.CanJump() == true)
			{
				kcc.Jump(Vector3.up * _jumpPower);
			}

			SetLookRotation(kccFixedData, input.LookRotationDelta, _weapons.GetRecoil(), out Vector2 newRecoil);
			_weapons.SetRecoil(newRecoil);

			kcc.SetInputDirection(input.MoveDirection.IsZero() == true ? Vector3.zero : kcc.FixedData.TransformRotation * input.MoveDirection.X0Y());

			if (_agentInput.WasActivated(EGameplayInputAction.ToggleSide, input) == true)
			{
				LeftSide = !LeftSide;
			}

			if (input.Weapon > 0 && _character.AnimationController.CanSwitchWeapons(true) == true && _weapons.SwitchWeapon(input.Weapon - 1) == true)
			{
				_character.AnimationController.SwitchWeapons();
			}
			else if (input.Weapon <= 0 && _weapons.PendingWeaponSlot != _weapons.CurrentWeaponSlot && _character.AnimationController.CanSwitchWeapons(false) == true)
			{
				_character.AnimationController.SwitchWeapons();
			}

			if (_agentInput.WasActivated(EGameplayInputAction.ToggleJetpack, input) == true)
			{
				if (_jetpack.IsActive == true)
				{
					_jetpack.Deactivate();
				}
				else if (_character.AnimationController.CanSwitchWeapons(true) == true)
				{
					_jetpack.Activate();
				}
			}

			if (_jetpack.IsActive == true)
			{
				_jetpack.FullThrust = input.Thrust;
			}

			_sprint.ProcessSprintInput(input.Sprint);

			BREnvironmentProcessor environmentProcessor = _character.CharacterController.GetProcessor<BREnvironmentProcessor>();
			if (environmentProcessor != null)
			{
				environmentProcessor.SpeedMultiplier = 1.6f * _sprint.SprintSpeedMultiplier;
			}

			_agentInput.SetFixedInput(input, false);
		}

		private void ProcessRenderInput()
		{
			KCC     kcc           = _character.CharacterController;
			KCCData kccFixedData  = kcc.FixedData;

			GameplayInput input = default;

			if (_health.IsAlive == true)
			{
				input = _agentInput.RenderInput;

				var accumulatedInput = _agentInput.AccumulatedInput;

				input.LookRotationDelta = accumulatedInput.LookRotationDelta;
				input.Aim               = accumulatedInput.Aim;
				input.Thrust            = accumulatedInput.Thrust;
			}

			if (input.Aim == true)
			{
				input.Aim &= CanAim(kccFixedData);
			}

			if (input.Aim == true)
			{
				if (_weapons.CurrentWeapon != null && _weapons.CurrentWeapon.HitType == EHitType.Sniper)
				{
					input.LookRotationDelta *= 0.3f;
				}
			}

			SetLookRotation(kccFixedData, input.LookRotationDelta, _weapons.GetRecoil(), out Vector2 newRecoil);

			kcc.SetInputDirection(input.MoveDirection.IsZero() == true ? Vector3.zero : kcc.RenderData.TransformRotation * input.MoveDirection.X0Y());

			kcc.SetAim(input.Aim);

			if (_agentInput.WasActivated(EGameplayInputAction.Jump, input) == true && _character.AnimationController.CanJump() == true)
			{
				kcc.Jump(Vector3.up * _jumpPower);
			}
		}

		private void TryFire(bool attack, bool hold)
		{
			var currentWeapon = _weapons.CurrentWeapon;
			if (currentWeapon is ThrowableWeapon && currentWeapon.WeaponSlot == _weapons.PendingWeaponSlot)
			{
				// Fire is handled form the grenade animation state itself
				_character.AnimationController.ProcessThrow(attack, hold);
				return;
			}

			if (hold == false)
				return;
			if (_weapons.CanFireWeapon(attack) == false)
				return;

			if (_character.AnimationController.StartFire() == true)
			{
				if (_weapons.Fire() == true)
				{
					_health.ResetRegenDelay();

					if (Runner.IsServer == true)
					{
						PlayerRef inputAuthority = Object.InputAuthority;
						if (inputAuthority.IsRealPlayer == true)
						{
							_interestView.UpdateShootInterestTargets();
						}
					}
				}
			}
		}

		private void TryReload(bool autoReload)
		{
			if (_weapons.CanReloadWeapon(autoReload) == false)
				return;

			if (_character.AnimationController.StartReload() == true)
			{
				_weapons.Reload();
			}
		}

		private bool CanAim(KCCData kccData)
		{
			if (kccData.IsGrounded == false)
				return false;

			return _weapons.CanAim();
		}

		private void SetLookRotation(KCCData kccData, Vector2 lookRotationDelta, Vector2 recoil, out Vector2 newRecoil)
		{
			if (lookRotationDelta.IsZero() == true && recoil.IsZero() == true && _character.CharacterController.Data.Recoil == Vector2.zero)
			{
				newRecoil = recoil;
				return;
			}

			Vector2 baseLookRotation = kccData.GetLookRotation(true, true) - kccData.Recoil;
			Vector2 recoilReduction  = Vector2.zero;

			if (recoil.x > 0f && lookRotationDelta.x < 0)
			{
				recoilReduction.x = Mathf.Clamp(lookRotationDelta.x, -recoil.x, 0f);
			}

			if (recoil.x < 0f && lookRotationDelta.x > 0f)
			{
				recoilReduction.x = Mathf.Clamp(lookRotationDelta.x, 0, -recoil.x);
			}

			if (recoil.y > 0f && lookRotationDelta.y < 0)
			{
				recoilReduction.y = Mathf.Clamp(lookRotationDelta.y, -recoil.y, 0f);
			}

			if (recoil.y < 0f && lookRotationDelta.y > 0f)
			{
				recoilReduction.y = Mathf.Clamp(lookRotationDelta.y, 0, -recoil.y);
			}

			lookRotationDelta -= recoilReduction;
			recoil            += recoilReduction;

			lookRotationDelta.x = Mathf.Clamp(baseLookRotation.x + lookRotationDelta.x, -_topCameraAngleLimit, _bottomCameraAngleLimit) - baseLookRotation.x;

			_character.CharacterController.SetLookRotation(baseLookRotation + recoil + lookRotationDelta);
			_character.CharacterController.SetRecoil(recoil);

			_character.AnimationController.Turn(lookRotationDelta.y);

			newRecoil = recoil;
		}

		private void CheckFallDamage()
		{
			if (IsProxy == true)
				return;

			if (_health.IsAlive == false)
				return;

			var kccData = _character.CharacterController.Data;

			if (kccData.IsGrounded == false || kccData.WasGrounded == true)
				return;

			float fallVelocity = -kccData.DesiredVelocity.y;
			for (int i = 1; i < 3; ++i)
			{
				var historyData = _character.CharacterController.GetHistoryData(kccData.Tick - i);
				if (historyData != null)
				{
					fallVelocity = Mathf.Max(fallVelocity, -historyData.DesiredVelocity.y);
				}
			}

			if (fallVelocity < 0f)
				return;

			float damage = MathUtility.Map(_minFallDamageVelocity, _maxFallDamageVelocity, 0f, _maxFallDamage, fallVelocity);

			if (damage <= _minFallDamage)
				return;

			var hitData = new HitData
			{
				Action           = EHitAction.Damage,
				Amount           = damage,
				Position         = transform.position,
				Normal           = Vector3.up,
				Direction        = -Vector3.up,
				InstigatorRef    = Object.InputAuthority,
				Instigator       = _health,
				Target           = _health,
				HitType          = EHitType.Suicide,
			};

			(_health as IHitTarget).ProcessHit(ref hitData);
		}

		private void OnCullingUpdated(bool isCulled)
		{
			bool isActive = isCulled == false;

			// Show/hide the game object based on AoI (Area of Interest)

			_visualRoot.SetActive(isActive);

			if (_character.CharacterController.Collider != null)
			{
				_character.CharacterController.Collider.enabled = isActive;
			}
		}
	}
}
