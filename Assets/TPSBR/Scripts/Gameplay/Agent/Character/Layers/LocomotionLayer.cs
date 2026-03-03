namespace TPSBR
{
	using UnityEngine;
	using Fusion.Addons.AnimationController;

	public sealed class LocomotionLayer : AnimationLayer
	{
		// PUBLIC MEMBERS

		public MoveState Move => _move;

		// PRIVATE MEMBERS

		[SerializeField]
		private MoveState _move;
	}
}
