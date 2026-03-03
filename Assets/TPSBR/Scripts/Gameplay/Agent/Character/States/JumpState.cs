namespace TPSBR
{
	using Fusion.Addons.AnimationController;

	public class JumpState : MirrorBlendTreeState
	{
		// PRIVATE MEMBERS

		private Weapons _weapons;

		// AnimationState INTERFACE

		protected override void OnInitialize()
		{
			base.OnInitialize();

			_weapons = Controller.GetComponentNoAlloc<Weapons>();
		}
	}
}
