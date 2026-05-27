namespace TPSBR
{
	using UnityEngine;
	using Fusion;

	public sealed class ConsumableWeapon : Weapon
	{
		// PUBLIC MEMBERS

		public EHitAction ActionType => _actionType;
		public int        Amount     => _amount;
		public float      UseTime    => _useTime;

		// PRIVATE MEMBERS

		[SerializeField]
		private EHitAction _actionType = EHitAction.Heal;
		[SerializeField]
		private int        _amount     = 50;
		[SerializeField]
		private float      _useTime    = 3f;

		// Weapon INTERFACE

		public override bool IsBusy()
		{
			// Busy if used by Agent (logic in Agent.cs)
			return false;
		}

		public override bool CanFire(bool keyDown)
		{
			return false;
		}

		public override void Fire(Vector3 firePosition, Vector3 targetPosition, LayerMask hitMask)
		{
			// Consumables are not "fired" like weapons
		}

		public override bool CanAim()
		{
			// Aiming triggers the usage
			return true;
		}
	}
}
