using Fusion;
using UnityEngine;

namespace TPSBR
{
	public abstract class Weapon : ContextBehaviour, IDynamicPickupProvider
	{
		// PUBLIC MEMBERS

		public string        WeaponID           => _weaponID;
		public int           WeaponSlot         => _weaponSlot;
		public Transform     LeftHandTarget     => _leftHandTarget;
		public DynamicPickup PickupPrefab       => _pickupPrefab;
		public EHitType      HitType            => _hitType;
		public float         AimFOV             => _aimFOV;
		public string        DisplayName        => _displayName;
		public string        NameShortcut       => _nameShortcut;
		public Sprite        Icon               => _icon;
		public bool          ValidOnlyWithAmmo  => _validOnlyWithAmmo;
		public bool          IsInitialized      => _isInitialized;
		public bool          IsArmed            => _isArmed;
		public NetworkObject Owner              => _owner;
		public Character     Character          => _character;

		// PRIVATE MEMBERS

		[SerializeField]
		private string _weaponID;
		[SerializeField]
		private int _weaponSlot;
		[SerializeField]
		private bool _validOnlyWithAmmo;
		[SerializeField]
		private Transform _leftHandTarget;
		[SerializeField]
		private EHitType _hitType;
		[SerializeField]
		private float _aimFOV;

		[Header("Pickup")]
		[SerializeField]
		private string _displayName;
		[SerializeField, Tooltip("Up to 4 letter name shown in thumbnail")]
		private string _nameShortcut;
		[SerializeField]
		private Sprite _icon;
		[SerializeField]
		private Collider _pickupCollider;
		[SerializeField]
		private Transform _pickupInterpolationTarget;
		[SerializeField]
		private DynamicPickup _pickupPrefab;

		private bool          _isInitialized;
		private bool          _isArmed;
		private NetworkObject _owner;
		private Character     _character;
		private Transform     _armedParent;
		private Transform     _disarmedParent;
		private AudioEffect[] _audioEffects;

		// PUBLIC METHODS

		public void ArmWeapon()
		{
			if (_isArmed == true)
				return;

			_isArmed = true;
			OnIsArmedChanged();
		}

		public void DisarmWeapon()
		{
			if (_isArmed == false)
				return;

			_isArmed = false;
			OnIsArmedChanged();
		}

		public void Initialize(NetworkObject owner, Transform armedParent, Transform disarmedParent)
		{
			if (_isInitialized == true)
				return;

			_isInitialized  = true;
			_owner          = owner;
			_character      = owner.GetComponent<Character>();
			_armedParent    = armedParent;
			_disarmedParent = disarmedParent;

			RefreshParent();
		}

		public void Deinitialize(NetworkObject owner)
		{
			if (_owner != null && _owner != owner)
				return;

			_isInitialized  = default;
			_owner          = default;
			_character      = default;
			_armedParent    = default;
			_disarmedParent = default;

			AssignFireAudioEffects(null, null);
		}

		public virtual bool IsBusy() { return false; }

		public abstract bool CanFire(bool keyDown);
		public abstract void Fire(Vector3 firePosition, Vector3 targetPosition, LayerMask hitMask);

		public virtual bool CanReload(bool autoReload) { return false; }
		public virtual void Reload() {}

		public virtual bool CanAim() { return false; }

		public virtual void AssignFireAudioEffects(Transform root, AudioEffect[] audioEffects)
		{
			_audioEffects = audioEffects;
		}

		public virtual bool HasAmmo() { return true; }

		public virtual bool AddAmmo(int ammo) { return false; }

		public virtual bool CanFireToPosition(Vector3 firePosition, ref Vector3 targetPosition, LayerMask hitMask) { return true; }

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			if (ApplicationSettings.IsStrippedBatch == true)
			{
				gameObject.SetActive(false);
			}
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			if (hasState == true)
			{
				DisarmWeapon();
			}
			else
			{
				_isArmed = false;
			}

			Deinitialize(_owner);
		}

		// PROTECTED METHODS

		protected virtual void OnWeaponArmed()
		{
		}

		protected virtual void OnWeaponDisarmed()
		{
		}

		protected bool PlaySound(AudioSetup setup)
		{
			if (_audioEffects.PlaySound(setup, EForceBehaviour.ForceAny) == false)
			{
				Debug.LogWarning($"No free audio effects on weapon {gameObject.name}. Add more audio effects in Player prefab.");
				return false;
			}

			return true;
		}

		// IPickupProvider INTERFACE

		string    IDynamicPickupProvider.Name                => _displayName;
		string    IDynamicPickupProvider.Description         => null;
		Collider  IDynamicPickupProvider.Collider            => _pickupCollider;
		Transform IDynamicPickupProvider.InterpolationTarget => _pickupInterpolationTarget;
		float     IDynamicPickupProvider.DespawnTime         => 60f;

		void IDynamicPickupProvider.Assigned(DynamicPickup pickup)
		{
			Deinitialize(_owner);
		}

		void IDynamicPickupProvider.Unassigned(DynamicPickup pickup)
		{
		}

		// NETWORK CALLBACKS

		private void OnIsArmedChanged()
		{
			RefreshParent();

			if (IsArmed == true)
			{
				OnWeaponArmed();
			}
			else
			{
				OnWeaponDisarmed();
			}
		}

		private void RefreshParent()
		{
			if (_isInitialized == false)
				return;

			transform.SetParent(_isArmed == true ? _armedParent : _disarmedParent, false);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
		}
	}
}
