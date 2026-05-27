namespace TPSBR
{
	using UnityEngine;
	using Fusion;

	[DisallowMultipleComponent]
	public class StaticNetworkTransform : NetworkTRSP
	{
		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			if (HasStateAuthority == true)
			{
				State.Position = transform.position;
				State.Rotation = transform.rotation;
			}
			else
			{
				transform.position = State.Position;
				transform.rotation = State.Rotation;
			}
		}
	}
}
