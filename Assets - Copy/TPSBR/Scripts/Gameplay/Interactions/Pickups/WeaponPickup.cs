using UnityEngine;

namespace TPSBR
{
	public class WeaponPickup : StaticPickup
	{
		// PUBLIC MEMBERS

		public Weapon WeaponPrefab => _weaponPrefab;

		// PRIVATE MEMBERS

		[SerializeField]
		private Weapon _weaponPrefab;

		// StaticPickup INTERFACE

		protected override bool Consume(GameObject instigator, out string result)
		{
			if (instigator.TryGetComponent(out Weapons weapons) == false)
			{
				result = "Not applicable";
				return false;
			}

			result = string.Empty;
			return true;
		}

		protected override string InteractionName        => _weaponPrefab != null ? (_weaponPrefab as IDynamicPickupProvider).Name : "Weapon";
		protected override string InteractionDescription => _weaponPrefab != null ? (_weaponPrefab as IDynamicPickupProvider).Description : string.Empty;
	}
}
