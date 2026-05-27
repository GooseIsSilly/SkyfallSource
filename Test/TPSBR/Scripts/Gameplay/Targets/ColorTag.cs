namespace TPSBR
{
	using UnityEngine;
	using Fusion;

	public class ColorTag : NetworkBehaviour, ISpawned
	{
		public Color ServerColor = Color.green;
		public Color ClientColor = Color.red;

		void ISpawned.Spawned()
		{
			var r = GetComponent<Renderer>();
			r.material.color = Runner.IsClient ? ClientColor : ServerColor;
		}
	}
}
