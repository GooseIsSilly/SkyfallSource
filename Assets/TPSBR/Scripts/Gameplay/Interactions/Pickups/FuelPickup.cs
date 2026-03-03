using UnityEngine;

namespace TPSBR
{
	public class FuelPickup : StaticPickup
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private int _fuel = 200;

		// StaticPickup INTERFACE

		protected override bool Consume(GameObject instigator, out string result)
		{
			if (instigator.TryGetComponent(out Jetpack jetpack) == false)
			{
				result = "Not applicable";
				return false;
			}

			bool fuelAdded = jetpack.AddFuel(_fuel);
			result = fuelAdded == true ? string.Empty : "Cannot add more fuel";

			return fuelAdded;
		}
	}
}
