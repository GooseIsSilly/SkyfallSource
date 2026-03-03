namespace TPSBR
{
	using System;
	using UnityEngine;
	using Fusion;

	[Serializable]
	public sealed class WeaponSlot
	{
		public Transform  Active;
		public Transform  Inactive;
		[NonSerialized]
		public Quaternion BaseRotation;
	}

	public sealed class Weapons : NetworkBehaviour, IBeforeTick
	{
		// PUBLIC MEMBERS

		public Weapon     PendingWeapon             { get; private set; }
		public Weapon     CurrentWeapon             { get; private set; }
		public Transform  CurrentWeaponHandle       { get; private set; }
		public Quaternion CurrentWeaponBaseRotation { get; private set; }

		public LayerMask  HitMask            => _hitMask;
		public int        CurrentWeaponSlot  => _currentWeaponSlot;
		public int        PendingWeaponSlot  => _pendingWeaponSlot;
		public int        PreviousWeaponSlot => _previousWeaponSlot;

		// PRIVATE MEMBERS

		[SerializeField]
		private WeaponSlot[] _slots;
		[SerializeField]
		private Weapon[]     _initialWeapons;
		[SerializeField]
		private Vector3      _dropWeaponImpulse = new Vector3(5, 5f, 10f);
		[SerializeField]
		private LayerMask    _hitMask;

		[Header("Audio")]
		[SerializeField]
		private Transform    _fireAudioEffectsRoot;

		[Networked, Capacity(8)]
		private NetworkArray<Weapon> _weapons { get; }
		[Networked]
		private byte _currentWeaponSlot { get; set; }
		[Networked]
		private byte _pendingWeaponSlot { get; set; }
		[Networked]
		private byte _previousWeaponSlot { get; set; }

		private Health        _health;
		private Character     _character;
		private Interactions  _interactions;
		private AudioEffect[] _fireAudioEffects;
		private Weapon[]      _localWeapons = new Weapon[8];

		// PUBLIC METHODS

		public void DisarmCurrentWeapon()
		{
			if (_currentWeaponSlot == 0)
				return;

			if (CurrentWeapon != null)
			{
				CurrentWeapon.DisarmWeapon();
			}

			if (_currentWeaponSlot > 0)
			{
				_previousWeaponSlot = _currentWeaponSlot;
			}

			_currentWeaponSlot = 0;

			CurrentWeapon             = _weapons[_currentWeaponSlot];
			CurrentWeaponHandle       = _slots[_currentWeaponSlot].Active;
			CurrentWeaponBaseRotation = _slots[_currentWeaponSlot].BaseRotation;

			if (CurrentWeapon != null)
			{
				CurrentWeapon.ArmWeapon();
			}
		}

		public void SetPendingWeapon(int slot)
		{
			if (_pendingWeaponSlot == slot)
				return;

			_pendingWeaponSlot = (byte)slot;
			PendingWeapon = _weapons[_pendingWeaponSlot];
		}

		public void ArmPendingWeapon()
		{
			if (_currentWeaponSlot == _pendingWeaponSlot)
				return;

			if (CurrentWeapon != null)
			{
				CurrentWeapon.DisarmWeapon();
			}

			if (_currentWeaponSlot > 0)
			{
				_previousWeaponSlot = _currentWeaponSlot;
			}

			_currentWeaponSlot = _pendingWeaponSlot;

			CurrentWeapon             = _weapons[_currentWeaponSlot];
			CurrentWeaponHandle       = _slots[_currentWeaponSlot].Active;
			CurrentWeaponBaseRotation = _slots[_currentWeaponSlot].BaseRotation;

			if (CurrentWeapon != null)
			{
				CurrentWeapon.ArmWeapon();
			}
		}

		public void DropCurrentWeapon()
		{
			DropWeapon(_currentWeaponSlot);
		}

		public void Pickup(DynamicPickup dynamicPickup, Weapon pickupWeapon)
		{
			if (HasStateAuthority == false)
				return;

			var ownedWeapon = _weapons[pickupWeapon.WeaponSlot];
			if (ownedWeapon != null && ownedWeapon.WeaponID == pickupWeapon.WeaponID)
			{
				// We already have this weapon, try add at least the ammo
				var firearmWeapon = pickupWeapon as FirearmWeapon;
				bool consumed = firearmWeapon != null && ownedWeapon.AddAmmo(firearmWeapon.TotalAmmo);

				if (consumed == true)
				{
					dynamicPickup.UnassignObject();
					Runner.Despawn(pickupWeapon.Object);
				}
			}
			else
			{
				dynamicPickup.UnassignObject();
				PickupWeapon(pickupWeapon);
			}
		}

		public void Pickup(WeaponPickup weaponPickup)
		{
			if (HasStateAuthority == false)
				return;

			if (weaponPickup.Consumed == true || weaponPickup.IsDisabled == true)
				return;

			if (weaponPickup.WeaponPrefab == null)
			{
				Debug.LogWarning($"[Weapons] WeaponPickup '{weaponPickup.name}' has no WeaponPrefab assigned. Cannot pickup weapon.");
				return;
			}

			var ownedWeapon = _weapons[weaponPickup.WeaponPrefab.WeaponSlot];
			if (ownedWeapon != null && ownedWeapon.WeaponID == weaponPickup.WeaponPrefab.WeaponID)
			{
				// We already have this weapon, try add at least the ammo
				var firearmWeapon = weaponPickup.WeaponPrefab as FirearmWeapon;
				bool consumed = firearmWeapon != null && ownedWeapon.AddAmmo(firearmWeapon.InitialAmmo);

				if (consumed == true)
				{
					weaponPickup.TryConsume(gameObject, out string weaponPickupResult);
				}
			}
			else
			{
				weaponPickup.TryConsume(gameObject, out string weaponPickupResult2);

				var weapon = Runner.Spawn(weaponPickup.WeaponPrefab, inputAuthority: Object.InputAuthority);
				PickupWeapon(weapon);
			}
		}

		public override void Spawned()
		{
			if (HasStateAuthority == false)
			{
				RefreshWeapons();
				return;
			}

			_currentWeaponSlot  = 0;
			_pendingWeaponSlot  = 0;
			_previousWeaponSlot = 0;

			byte bestWeaponSlot = 0;

			// Spawn initial weapons
			for (byte i = 0; i < _initialWeapons.Length; i++)
			{
				var weaponPrefab = _initialWeapons[i];
				if (weaponPrefab == null)
					continue;

				var weapon = Runner.Spawn(weaponPrefab, inputAuthority: Object.InputAuthority);
				AddWeapon(weapon);

				if (weapon.WeaponSlot > bestWeaponSlot && weapon.WeaponSlot < 3)
				{
					bestWeaponSlot = (byte)weapon.WeaponSlot;
				}
			}

			_previousWeaponSlot = bestWeaponSlot;

			SetPendingWeapon(bestWeaponSlot);
			ArmPendingWeapon();
			RefreshWeapons();
		}

		public void OnDespawned()
		{
			// Cleanup weapons
			for (int i = 0; i < _weapons.Length; i++)
			{
				Weapon weapon = _weapons[i];
				if (weapon != null)
				{
					weapon.Deinitialize(Object);
					Runner.Despawn(weapon.Object);
					_weapons.Set(i, null);
					_localWeapons[i] = null;
				}
			}

			for (int i = 0; i < _localWeapons.Length; i++)
			{
				Weapon weapon = _localWeapons[i];
				if (weapon != null)
				{
					weapon.Deinitialize(Object);
					_localWeapons[i] = null;
				}
			}

			_currentWeaponSlot  = 0;
			_pendingWeaponSlot  = 0;
			_previousWeaponSlot = 0;

			PendingWeapon             = default;
			CurrentWeapon             = default;
			CurrentWeaponHandle       = default;
			CurrentWeaponBaseRotation = default;
		}

		public void OnFixedUpdate()
		{
			if (HasStateAuthority == false)
				return;

			if (_health.IsAlive == false)
			{
				DropAllWeapons();
				return;
			}

			// Autoswitch to valid weapon if current is invalid
			if (CurrentWeapon != null && CurrentWeapon.ValidOnlyWithAmmo == true && CurrentWeapon.HasAmmo() == false)
			{
				byte bestWeaponSlot = _previousWeaponSlot;
				if (bestWeaponSlot == 0 || bestWeaponSlot == _currentWeaponSlot)
				{
					bestWeaponSlot = FindBestWeaponSlot(_currentWeaponSlot);
				}

				DisarmCurrentWeapon();
				SetPendingWeapon(bestWeaponSlot);

				_previousWeaponSlot = bestWeaponSlot;
			}
		}

		public override void Render()
		{
			RefreshWeapons();
		}

		public bool IsSwitchingWeapon()
		{
			return _pendingWeaponSlot != _currentWeaponSlot;
		}

		public bool CanFireWeapon(bool keyDown)
		{
			return IsSwitchingWeapon() == false && CurrentWeapon != null && CurrentWeapon.CanFire(keyDown) == true;
		}

		public bool CanReloadWeapon(bool autoReload)
		{
			return IsSwitchingWeapon() == false && CurrentWeapon != null && CurrentWeapon.CanReload(autoReload) == true;
		}

		public bool CanAim()
		{
			return IsSwitchingWeapon() == false && CurrentWeapon != null && CurrentWeapon.CanAim() == true;
		}

		public Vector2 GetRecoil()
		{
			var firearmWeapon = CurrentWeapon as FirearmWeapon;
			var recoil = firearmWeapon != null ? firearmWeapon.Recoil : Vector2.zero;
			return new Vector2(-recoil.y, recoil.x); // Convert to axis angles
		}

		public void SetRecoil(Vector2 axisRecoil)
		{
			var firearmWeapon = CurrentWeapon as FirearmWeapon;

			if (firearmWeapon == null)
				return;

			firearmWeapon.Recoil = new Vector2(axisRecoil.y, -axisRecoil.x);
		}

		public bool SwitchWeapon(int weaponSlot)
		{
			if (weaponSlot == _pendingWeaponSlot)
				return false;

			var weapon = _weapons[weaponSlot];
			if (weapon == null || (weapon.ValidOnlyWithAmmo == true && weapon.HasAmmo() == false))
				return false;

			SetPendingWeapon(weaponSlot);
			return true;
		}

		public bool HasWeapon(int slot, bool checkAmmo = false)
		{
			if (slot < 0 || slot >= _weapons.Length)
				return false;

			var weapon = _weapons[slot];
			return weapon != null && (checkAmmo == false || (weapon.Object != null && weapon.HasAmmo() == true));
		}

		public Weapon GetWeapon(int slot)
		{
			return _weapons[slot];
		}

		public int GetNextWeaponSlot(int fromSlot, int minSlot = 0, bool checkAmmo = true)
		{
			int weaponCount = _weapons.Length;

			for (int i = 0; i < weaponCount; i++)
			{
				int slot = (i + fromSlot + 1) % weaponCount;

				if (slot < minSlot)
					continue;

				var weapon = _weapons[slot];

				if (weapon == null)
					continue;

				if (checkAmmo == true && weapon.HasAmmo() == false)
					continue;

				return slot;
			}

			return 0;
		}

		public bool Fire()
		{
			if (CurrentWeapon == null)
				return false;

			Vector3       targetPoint   = _interactions.GetTargetPoint(false, true);
			TransformData fireTransform = _character.GetFireTransform(true);

			CurrentWeapon.Fire(fireTransform.Position, targetPoint, _hitMask);

			return true;
		}

		public bool Reload()
		{
			if (CurrentWeapon == null)
				return false;

			CurrentWeapon.Reload();
			return true;
		}

		public bool AddAmmo(int weaponSlot, int amount, out string result)
		{
			if (weaponSlot < 0 || weaponSlot >= _weapons.Length)
			{
				result = string.Empty;
				return false;
			}

			var weapon = _weapons[weaponSlot];
			if (weapon == null)
			{
				result = "No weapon with this type of ammo";
				return false;
			}

			bool ammoAdded = weapon.AddAmmo(amount);
			result = ammoAdded == true ? string.Empty : "Cannot add more ammo";

			return ammoAdded;
		}

		// IBeforeTick INTERFACE

		void IBeforeTick.BeforeTick()
		{
			RefreshWeapons();
		}

		// MONOBEHAVIOUR

		private void Awake()
		{
			_health = GetComponent<Health>();
			_character = GetComponent<Character>();
			_interactions = GetComponent<Interactions>();
			_fireAudioEffects = _fireAudioEffectsRoot.GetComponentsInChildren<AudioEffect>();

			foreach (WeaponSlot slot in _slots)
			{
				if (slot.Active != null)
				{
					slot.BaseRotation = slot.Active.localRotation;
				}
			}
		}

		// PRIVATE METHODS

		private void RefreshWeapons()
		{
			PendingWeapon = _weapons[_pendingWeaponSlot];

			Vector2 lastRecoil = Vector2.zero;

			for (int i = 0; i < _weapons.Length; i++)
			{
				var weapon = _weapons[i];
				if (weapon == null)
					continue;

				if (weapon.IsInitialized == false)
				{
					weapon.Initialize(Object, _slots[weapon.WeaponSlot].Active, _slots[weapon.WeaponSlot].Inactive);
					weapon.AssignFireAudioEffects(_fireAudioEffectsRoot, _fireAudioEffects);
					_localWeapons[weapon.WeaponSlot] = weapon;
				}

				if (weapon.IsArmed == true)
				{
					if (weapon.WeaponSlot != _currentWeaponSlot)
					{
						weapon.DisarmWeapon();
					}

					if (weapon is FirearmWeapon firearmWeapon)
					{
						lastRecoil = firearmWeapon.Recoil;
					}
				}
			}

			Weapon currentWeapon = _weapons[_currentWeaponSlot];
			if (CurrentWeapon != currentWeapon)
			{
				if (currentWeapon == null)
				{
					CurrentWeapon.Deinitialize(Object);
					_localWeapons[_currentWeaponSlot] = default;
				}

				CurrentWeapon             = currentWeapon;
				CurrentWeaponHandle       = _slots[_currentWeaponSlot].Active;
				CurrentWeaponBaseRotation = _slots[_currentWeaponSlot].BaseRotation;

				if (CurrentWeapon != null)
				{
					CurrentWeapon.ArmWeapon();

					if (CurrentWeapon is FirearmWeapon firearmWeapon)
					{
						// Recoil transfers to new weapon
						// (might be better to have recoil as an agent property instead of a weapon property)
						firearmWeapon.Recoil = lastRecoil;
					}
				}
			}
		}

		private void DropAllWeapons()
		{
			for (int i = 1; i < _weapons.Length; i++)
			{
				DropWeapon(i);
			}
		}

		private void DropWeapon(int weaponSlot)
		{
			var weapon = _weapons[weaponSlot];
			if (weapon == null)
				return;

			if (weapon.PickupPrefab == null)
			{
				Debug.LogWarning($"Cannot drop weapon {gameObject.name}, pickup prefab not assigned.");
				return;
			}

			weapon.Deinitialize(Object);

			if (weaponSlot == _currentWeaponSlot)
			{
				byte bestWeaponSlot = _previousWeaponSlot;
				if (bestWeaponSlot == 0 || bestWeaponSlot == _currentWeaponSlot)
				{
					bestWeaponSlot = FindBestWeaponSlot(_currentWeaponSlot);
				}

				SetPendingWeapon(bestWeaponSlot);
				ArmPendingWeapon();

				_previousWeaponSlot = bestWeaponSlot;
			}

			var weaponTransform = weapon.transform;

			var pickup = Runner.Spawn(weapon.PickupPrefab, weaponTransform.position, weaponTransform.rotation,
				PlayerRef.None, BeforePickupSpawned);

			RemoveWeapon(weaponSlot);

			var pickupRigidbody = pickup.GetComponent<Rigidbody>();
			if (pickupRigidbody != null)
			{
				var forcePosition = weaponTransform.TransformPoint(new Vector3(-0.005f, 0.005f, 0.015f) * weaponSlot);
				pickupRigidbody.AddForceAtPosition(weaponTransform.rotation * _dropWeaponImpulse, forcePosition, ForceMode.Impulse);
			}

			void BeforePickupSpawned(NetworkRunner runner, NetworkObject obj)
			{
				var dynamicPickup = obj.GetComponent<DynamicPickup>();
				dynamicPickup.AssignObject(_weapons[weaponSlot].Object.Id);
			}
		}

		private void PickupWeapon(Weapon weapon)
		{
			if (weapon == null)
				return;

			DropWeapon(weapon.WeaponSlot);
			AddWeapon(weapon);

			if (weapon.WeaponSlot >= _currentWeaponSlot && weapon.WeaponSlot < 5)
			{
				SetPendingWeapon(weapon.WeaponSlot);
				ArmPendingWeapon();
			}
		}

		private void AddWeapon(Weapon weapon)
		{
			if (weapon == null)
				return;

			RemoveWeapon(weapon.WeaponSlot);

			weapon.Object.AssignInputAuthority(Object.InputAuthority);
			weapon.Initialize(Object, _slots[weapon.WeaponSlot].Active, _slots[weapon.WeaponSlot].Inactive);
			weapon.AssignFireAudioEffects(_fireAudioEffectsRoot, _fireAudioEffects);

			var aoiProxy = weapon.GetComponent<NetworkAreaOfInterestProxy>();
			aoiProxy.SetPositionSource(transform);

			Runner.SetPlayerAlwaysInterested(Object.InputAuthority, weapon.Object, true);

			_weapons.Set(weapon.WeaponSlot, weapon);
			_localWeapons[weapon.WeaponSlot] = weapon;
		}

		private void RemoveWeapon(int slot)
		{
			var weapon = _weapons[slot];
			if (weapon == null)
				return;

			weapon.Deinitialize(Object);
			weapon.Object.RemoveInputAuthority();

			var aoiProxy = weapon.GetComponent<NetworkAreaOfInterestProxy>();
			aoiProxy.ResetPositionSource();

			Runner.SetPlayerAlwaysInterested(Object.InputAuthority, weapon.Object, false);

			_weapons.Set(slot, null);
			_localWeapons[slot] = null;
		}

		private byte FindBestWeaponSlot(int ignoreSlot)
		{
			byte bestWeaponSlot = 0;

			for (int i = 0; i < _weapons.Length; i++)
			{
				Weapon weapon = _weapons[i];
				if (weapon != null)
				{
					if (weapon.WeaponSlot == ignoreSlot)
						continue;

					if (weapon.WeaponSlot > bestWeaponSlot && weapon.WeaponSlot < 3)
					{
						bestWeaponSlot = (byte)weapon.WeaponSlot;
					}
				}
			}

			return bestWeaponSlot;
		}
	}
}
