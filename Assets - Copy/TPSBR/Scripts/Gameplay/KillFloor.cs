using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace TPSBR
{
	public class KillFloor : ContextBehaviour
	{
		// NetworkBehaviour INTERFACE

		public override void FixedUpdateNetwork()
		{
			if (HasStateAuthority == false)
				return;

			if (Context == null || Context.NetworkGame.Object == null)
				return;

			Profiler.BeginSample(nameof(KillFloor));

			float yPosition = transform.position.y;
			int   splitPlayers = 1;

			List<Player> activePlayers = Context.NetworkGame.ActivePlayers;
			if (activePlayers.Count > 100)
			{
				splitPlayers = 5;
			}

			for (int i = Runner.Tick % splitPlayers; i < activePlayers.Count; i += splitPlayers)
			{
				Player player = activePlayers[i];
				if (player == null)
					continue;

				var agent = player.ActiveAgent;
				if (agent == null)
					continue;

				if (agent.Health.IsAlive == false)
					continue;

				if (agent.transform.position.y > yPosition)
					continue;

				var hitData = new HitData()
				{
					Action    = EHitAction.Damage,
					HitType   = EHitType.Suicide,
					Amount    = 99999f,
					IsFatal   = true,
					Position  = agent.transform.position,
					Target    = agent.Health,
					Normal    = Vector3.up,
				};

				HitUtility.ProcessHit(ref hitData);
			}

			Profiler.EndSample();
		}
	}
}
