namespace TPSBR
{
	using UnityEngine;
	using Fusion.Addons.AnimationController;

	public sealed class DeadState : MultiClipState
	{
		// PRIVATE MEMBERS

		private Weapons _weapons;

		// MultiClipState INTERFACE

		protected override int GetClipID()
		{
			if (_weapons.CurrentWeaponSlot > 2)
				return 1; // For grenades we use pistol set

			return Mathf.Max(0, _weapons.CurrentWeaponSlot);
		}

		// AnimationState INTERFACE

		protected override void OnInitialize()
		{
			base.OnInitialize();

			_weapons = Controller.GetComponentNoAlloc<Weapons>();
		}
	}
}
