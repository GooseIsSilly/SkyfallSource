using UnityEngine;
using Fusion;

namespace TPSBR
{
	public sealed class ThrowableWeapon : FirearmWeapon, IDynamicPickupProvider
	{
		// PRIVATE MEMBERS

		[Header("Throwable")]
		[SerializeField]
		private KinematicProjectile _projectile;
		[SerializeField]
		private float _projectileSpeed = 100f;
		[SerializeField]
		private GameObject _dummyLoadedProjectile;
		[SerializeField]
		private float _minProjectileDespawnTime = 0.5f;
		[SerializeField]
		private AudioSetup _armSound;

		[Networked][OnChangedRender(nameof(OnArmStarted))]
		private int _armStartTick { get; set; }

		// PUBLIC MEMBERS

		public void ArmProjectile()
		{
			_armStartTick = Runner.Tick;
		}

		// FirearmWeapon INTERFACE

		protected override bool FireProjectile(Vector3 firePosition, Vector3 targetPosition, Vector3 direction, float distanceToTarget, LayerMask hitMask, bool isFirst)
		{
			if (HasStateAuthority == false)
				return false;

			var ownerVelocity = Character != null ? Character.CharacterController.FixedData.RealVelocity : Vector3.zero;
			if (ownerVelocity.y < 0f)
			{
				ownerVelocity.y = 0f;
			}

			var projectile = Runner.Spawn(_projectile, firePosition, Quaternion.LookRotation(direction), Object.InputAuthority);
			if (projectile == null)
				return false;

			projectile.Fire(Owner, firePosition, direction * _projectileSpeed + ownerVelocity, hitMask, HitType);

			float armedTime = (Runner.Tick - _armStartTick) * Runner.DeltaTime;
			projectile.SetDespawnCooldown(Mathf.Max(_minProjectileDespawnTime, projectile.FireDespawnTime - armedTime));

			return true;
		}

		public override void Render()
		{
			base.Render();

			_dummyLoadedProjectile.SetActiveSafe(IsReloading == false && MagazineAmmo > 0);
		}

		// IPickupProvider INTERFACE

		string IDynamicPickupProvider.Description => GetPickupDescription();
		float IDynamicPickupProvider.DespawnTime => WeaponAmmo == 0 && MagazineAmmo == 0 ? 1f : 60f;

		// PRIVATE METHODS

		private void OnArmStarted()
		{
			PlayLocalSound(_armSound);
		}

		private string GetPickupDescription()
		{
			// For prefab read initial data
			if (gameObject.scene.rootCount == 0)
				return $"Amount {_initialAmmo}";

			return $"Amount {MagazineAmmo + WeaponAmmo}";
		}
	}
}
