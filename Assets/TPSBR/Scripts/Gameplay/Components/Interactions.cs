namespace TPSBR
{
	using System;
	using UnityEngine;
	using Fusion;

	[DefaultExecutionOrder(-8)]
	public sealed class Interactions : ContextBehaviour
	{
		// PUBLIC MEMBERS

		public IInteraction InteractionTarget      { get; private set; }
		public Vector3      TargetPoint            { get; private set; }
		public bool         IsUndesiredTargetPoint { get; private set; }

		public float        ItemDropTime => _itemDropTime;

		[Networked, HideInInspector]
		public TickTimer    DropItemTimer { get; private set; }

		public event Action<string> InteractionFailed;

		// PRIVATE MEMBERS

		[SerializeField]
		private LayerMask _interactionMask;
		[SerializeField]
		private float     _interactionDistance = 2f;
		[SerializeField]
		private float     _interactionPrecisionRadius = 0.3f;
		[SerializeField]
		private float     _itemDropTime;

		private Health       _health;
		private Weapons      _weapons;
		private Character    _character;
		private RaycastHit[] _interactionHits = new RaycastHit[10];

		// PUBLIC METHODS

		public void TryInteract(bool interact, bool hold)
		{
			if (hold == false)
			{
				DropItemTimer = default;
				return;
			}

			if (_weapons.IsSwitchingWeapon() == true)
			{
				DropItemTimer = default;
				return;
			}

			if (_weapons.CurrentWeapon != null && _weapons.CurrentWeapon.IsBusy() == true)
			{
				DropItemTimer = default;
				return;
			}

			if (HasStateAuthority == false)
				return;

			UpdateInteractionTarget();

			if (InteractionTarget == null)
			{
				if (DropItemTimer.IsRunning == false && _weapons.CurrentWeaponSlot > 0 && interact == true)
				{
					DropItemTimer = TickTimer.CreateFromSeconds(Runner, _itemDropTime);
				}

				if (DropItemTimer.Expired(Runner) == true)
				{
					DropItemTimer = default;
					_weapons.DropCurrentWeapon();
				}

				return;
			}

			if (interact == false)
				return;

			if (InteractionTarget is DynamicPickup dynamicPickup && dynamicPickup.Provider is Weapon pickupWeapon)
			{
				_weapons.Pickup(dynamicPickup, pickupWeapon);
			}
			else if (InteractionTarget is WeaponPickup weaponPickup)
			{
				_weapons.Pickup(weaponPickup);
			}
			else if (InteractionTarget is ItemBox itemBox)
			{
				itemBox.Open();
			}
			else if (InteractionTarget is StaticPickup staticPickup)
			{
				bool success = staticPickup.TryConsume(gameObject, out string result);
				if (success == false && result.HasValue() == true)
				{
					RPC_InteractionFailed(result);
				}
			}
		}

		public Vector3 GetTargetPoint(bool checkReachability, bool resolveRenderHistory)
		{
			var cameraTransform = _character.GetCameraTransform(resolveRenderHistory);
			var cameraDirection = cameraTransform.Rotation * Vector3.forward;

			var fireTransform = _character.GetFireTransform(resolveRenderHistory);
			var targetPoint = cameraTransform.Position + cameraDirection * 500f;

			if (Runner.LagCompensation.Raycast(cameraTransform.Position, cameraDirection, 500f, Object.InputAuthority,
				out LagCompensatedHit hit, _weapons.HitMask, HitOptions.IncludePhysX | HitOptions.SubtickAccuracy | HitOptions.IgnoreInputAuthority) == true)
			{
				var firingDirection = (hit.Point - fireTransform.Position).normalized;

				// Check angle
				if (Vector3.Dot(cameraDirection, firingDirection) > 0.95f)
				{
					targetPoint = hit.Point;
				}
			}

			if (checkReachability == true)
			{
				IsUndesiredTargetPoint = _weapons.CurrentWeapon != null && _weapons.CurrentWeapon.CanFireToPosition(fireTransform.Position, ref targetPoint, _weapons.HitMask) == false;
			}

			return targetPoint;
		}

		// NetworkBehaviour INTERFACE

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			InteractionFailed = null;
		}

		public override void Render()
		{
			if (_character.HasInputAuthority == false)
			{
				InteractionTarget = null;
				return;
			}

			if (_health.IsAlive == false)
			{
				InteractionTarget = null;
				return;
			}

			UpdateInteractionTarget();

			TargetPoint = GetTargetPoint(true, false);
		}

		// MonoBehaviour INTERFACE

		private void Awake()
		{
			_health    = GetComponent<Health>();
			_weapons   = GetComponent<Weapons>();
			_character = GetComponent<Character>();
		}

		// PRIVATE METHODS

		private void UpdateInteractionTarget()
		{
			InteractionTarget = null;

			var cameraTransform = _character.GetCameraTransform(false);
			var cameraDirection = cameraTransform.Rotation * Vector3.forward;

			var physicsScene = Runner.GetPhysicsScene();
			int hitCount = physicsScene.SphereCast(cameraTransform.Position, _interactionPrecisionRadius, cameraDirection, _interactionHits, _interactionDistance, _interactionMask, QueryTriggerInteraction.Ignore);

			if (hitCount == 0)
				return;

			RaycastHit validHit = default;

			// Try to pick object that is directly in the center of the crosshair
			if (physicsScene.Raycast(cameraTransform.Position, cameraDirection, out RaycastHit raycastHit, _interactionDistance, _interactionMask, QueryTriggerInteraction.Ignore) == true && raycastHit.collider.gameObject.layer == ObjectLayer.Interaction)
			{
				validHit = raycastHit;
			}
			else
			{
				RaycastUtility.Sort(_interactionHits, hitCount);

				for (int i = 0; i < hitCount; i++)
				{
					var hit = _interactionHits[i];

					if (hit.collider.gameObject.layer == ObjectLayer.Default)
						return; // Something is blocking interaction

					if (hit.collider.gameObject.layer == ObjectLayer.Interaction)
					{
						validHit = hit;
						break;
					}
				}
			}

			var collider = validHit.collider;

			if (collider == null)
				return;

			var interaction = collider.GetComponent<IInteraction>();
			if (interaction == null)
			{
				interaction = collider.GetComponentInParent<IInteraction>();
			}

			if (interaction != null && interaction.IsActive == true)
			{
				InteractionTarget = interaction;
			}
		}

		// RPCs

		[Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
		private void RPC_InteractionFailed(string reason)
		{
			InteractionFailed?.Invoke(reason);
		}
	}
}
