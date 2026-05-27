namespace TPSBR
{
	using UnityEngine;

	public sealed class ConsumablePickup : StaticPickup
	{
		// PUBLIC MEMBERS

		public ConsumableWeapon ConsumablePrefab => _consumablePrefab;

		// PRIVATE MEMBERS

		[SerializeField]
		private ConsumableWeapon _consumablePrefab;

		// StaticPickup INTERFACE

		protected override bool Consume(GameObject instigator, out string result)
		{
			if (instigator.TryGetComponent(out Weapons weapons) == false)
			{
				result = "Not applicable";
				return false;
			}

			// Consumables use slot 4
			var existing = weapons.GetWeapon(4);
			if (existing != null)
			{
				result = "Already holding a consumable";
				return false;
			}

			if (HasStateAuthority == true)
			{
				var weapon = Runner.Spawn(_consumablePrefab, inputAuthority: Object.InputAuthority);
				weapons.Pickup(null, weapon);
			}

			result = string.Empty;
			return true;
		}

		protected override string InteractionName        => _consumablePrefab != null ? _consumablePrefab.DisplayName : "Consumable";
		protected override string InteractionDescription => _consumablePrefab != null ? (_consumablePrefab as IDynamicPickupProvider).Description : string.Empty;
	}
}
