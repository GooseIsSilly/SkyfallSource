namespace Fusion.Plugin
{
	using System.Reflection;

	public static class FusionExtensions
	{
		private static readonly FieldInfo _simulationFieldInfo = typeof(NetworkRunner).GetField("_simulation", BindingFlags.Instance | BindingFlags.NonPublic);

		public static void SetLocalPlayer(this NetworkRunner runner, PlayerRef playerRef)
		{
			Simulation simulation = (Simulation)_simulationFieldInfo.GetValue(runner);
			if (simulation is Simulation.Client client)
			{
				// Hack - Local player is reset back after disconnect, otherwise exceptions are thrown all over the code because Object.HasStateAuthority == true on proxies
				// This shouldn't be harmful as NetworkRunner gets destroyed anyway.
				typeof(Simulation.Client).GetField("_player", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(client, playerRef);
			}
		}

		public static void GetInterpolationData(this NetworkRunner runner, out int fromTick, out int toTick, out float alpha)
		{
			Simulation simulation = (Simulation)_simulationFieldInfo.GetValue(runner);

			if (runner.IsServer == true)
			{
				fromTick = simulation.TickPrevious;
				toTick   = simulation.Tick;
				alpha    = simulation.LocalAlpha;
			}
			else
			{
				fromTick = simulation.RemoteTickPrevious;
				toTick   = simulation.RemoteTick;
				alpha    = simulation.RemoteAlpha;
			}
		}
	}
}
