using UnityEngine;

namespace TPSBR
{
	public class AmmoPickup : StaticPickup
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private int _weaponSlot = 1;
		[SerializeField]
		private int _amount = 50;

		// StaticPickup INTERFACE

		protected override bool Consume(GameObject instigator, out string result)
		{
			if (instigator.TryGetComponent(out Weapons weapons) == false)
			{
				result = "Not applicable";
				return false;
			}

			return weapons.AddAmmo(_weaponSlot, _amount, out result);
		}
	}
}
