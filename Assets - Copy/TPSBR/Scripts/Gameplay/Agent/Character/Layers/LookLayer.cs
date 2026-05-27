namespace TPSBR
{
	using UnityEngine;
	using Fusion.Addons.AnimationController;

	public sealed class LookLayer : AnimationLayer
	{
		// PUBLIC MEMBERS

		public LookState Look => _look;

		// PRIVATE MEMBERS

		[SerializeField]
		private LookState _look;

		// AnimationLayer INTERFACE

		protected override void OnFixedUpdate()
		{
			if (_look.ShouldActivate() == true)
			{
				_look.Activate(0.2f);
			}
			else
			{
				_look.Deactivate(0.2f);
			}
		}
	}
}
