using UnityEngine;
using Fusion;
using System.Collections.Generic;
using Fusion.Addons.KCC;

namespace TPSBR
{
	public sealed class BattleRoyaleGameplayMode : GameplayMode
	{
		// PUBLIC MEMBERS

		public bool     HasStarted         { get { return _state.IsBitSet(0); } set { _state = _state.SetBitNoRef(0, value); } }
		public bool     AirplaneActive     { get { return _state.IsBitSet(1); } set { _state = _state.SetBitNoRef(1, value); } }

		public float    WaitingCooldown    => State == EState.Active && HasStarted == false ? _waitingForPlayersCooldown.RemainingTime(Runner).Value : 0f;
		public float    DropCooldown       => AirplaneActive == true ? _dropCooldown.RemainingTime(Runner).Value : 0f;
		public Airplane Airplane           => _airplane;

		// PRIVATE MEMBERS

		[SerializeField]
		private Airplane _airplanePrefab;
		[SerializeField]
		private float _airplaneHeight = 80f;
		[SerializeField]
		private float _playerDropTime = 40f;
		[SerializeField]
		private float _airplaneJumpImpulse = 20f;
		[SerializeField]
		private AirplaneAgent _airplaneAgentPrefab;
		[SerializeField]
		private float _waitingForPlayersTime = 120f;
		[SerializeField]
		private float _forcedJumpDelay = 1f;

		[Networked]
		private byte _state { get; set; }
		[Networked]
		private TickTimer _waitingForPlayersCooldown { get; set; }
		[Networked]
		private TickTimer _dropCooldown { get; set; }
		[Networked]
		private TickTimer _forcedJumpCooldown { get; set; }
		[Networked]
		private Airplane _airplane { get; set; }

		private Dictionary<PlayerRef, AirplaneAgent> _airplaneAgents = new Dictionary<PlayerRef, AirplaneAgent>(byte.MaxValue);

		private BattleRoyaleComparer _playerComparer = new BattleRoyaleComparer();

		// PUBLIC METHODS

		public void StartImmediately()
		{
			if (HasStateAuthority == true)
			{
				StartAirdrop();
			}
			else if (ApplicationSettings.IsModerator == true)
			{
				RPC_StartAirdrop();
			}
		}

		public bool RequestAirplaneJump(PlayerRef playerRef, Vector3 direction)
		{
			if (HasStateAuthority == false)
				return false;

			if (HasStarted == false)
				return false;

			if (_airplaneAgents.TryGetValue(playerRef, out var airplaneAgent) == false)
				return false;

			Runner.Despawn(airplaneAgent.Object);
			_airplaneAgents.Remove(playerRef);

			var agent = SpawnAgent(playerRef, _airplane.AgentPosition.position, Quaternion.LookRotation(direction.OnlyXZ()));
			agent.Jetpack.Activate();
			agent.Character.CharacterController.AddExternalImpulse(direction.normalized * _airplaneJumpImpulse);

			RPC_PlayerJumped(playerRef);

			return true;
		}

		public void TryAddWaitTime(float time)
		{
			if (HasStateAuthority == true)
			{
				AddWaitTime(time);
			}
			else if (ApplicationSettings.IsModerator == true)
			{
				RPC_AddWaitTime(time);
			}
		}

		// GameplayMode INTERFACE

		protected override void OnActivate()
		{
			PrepareAirplane();

			if (_shrinkingArea != null)
			{
				_shrinkingArea.Pause(true);
			}

			_waitingForPlayersCooldown = TickTimer.CreateFromSeconds(Runner, _waitingForPlayersTime);
			HasStarted = false;
		}

		public override void FixedUpdateNetwork()
		{
			base.FixedUpdateNetwork();

			if (State != EState.Active)
				return;

			if (HasStateAuthority == false)
				return;

			if (HasStarted == false && _waitingForPlayersCooldown.ExpiredOrNotRunning(Runner) == true)
			{
				StartAirdrop();
			}

			if (HasStarted == true && AirplaneActive == true && _dropCooldown.ExpiredOrNotRunning(Runner) == true)
			{
				StopAirdrop();
			}

			if (HasStarted == true && AirplaneActive == false && _airplaneAgents.Count > 0 && _forcedJumpCooldown.ExpiredOrNotRunning(Runner) == true)
			{
				foreach (var agentPair in _airplaneAgents)
				{
					RequestAirplaneJump(agentPair.Key, Vector3.down);
					break;
				}

				_forcedJumpCooldown = TickTimer.CreateFromSeconds(Runner, _forcedJumpDelay);

				if (_airplaneAgents.Count == 0 && _airplane != null)
				{
					_airplane.DeactivateDropWindow();
				}
			}

			if (_airplane != null && _airplane.IsFinished == true)
			{
				Runner.Despawn(_airplane.Object);
				_airplane = null;
			}
		}

		protected override void SortPlayers(List<PlayerStatistics> allStatistics)
		{
			allStatistics.Sort(_playerComparer);
		}

		protected override void CheckWinCondition()
		{
			var alivePlayers    = 0;
			var lastAlivePlayer = PlayerRef.None;

			foreach (var player in Context.NetworkGame.ActivePlayers)
			{
				if (player == null)
					continue;

				var statistics = player.Statistics;
				if (statistics.ExtraLives > 0 || statistics.IsAlive == true || statistics.RespawnTimer.IsRunning == true)
				{
					if (alivePlayers > 0)
						return;

					alivePlayers    += 1;
					lastAlivePlayer  = player.Object.InputAuthority;
				}
			}

			if (alivePlayers == 1)
			{
				FinishGameplay();
				Log.Info($"Player {lastAlivePlayer} won the match!");
			}
			else if (alivePlayers == 0)
			{
				Log.Error("No player alive, this should not happen");
				FinishGameplay();
			}
		}

		protected override void TrySpawnAgent(Player player)
		{
			var statistics = player.Statistics;

			if (AirplaneActive == true)
			{
				var playerRef = player.Object.InputAuthority;

				if (_airplaneAgents.ContainsKey(playerRef) == true)
					return;

				var agent = Runner.Spawn(_airplaneAgentPrefab, inputAuthority: playerRef);
				Runner.SetPlayerAlwaysInterested(playerRef, agent.Object, true);

				agent.Airplane = _airplane;

				_airplaneAgents.Add(player.Object.InputAuthority, agent);

				statistics.IsAlive = true;

				player.UpdateStatistics(statistics);
			}
			else
			{
				// Too late, player is automatically eliminated
				statistics.IsEliminated = true;
				statistics.IsAlive = false;

				player.UpdateStatistics(statistics);

				SetSpectatorTargetToBestPlayer(player);
			}
		}

		// PRIVATE METHODS

		private void PrepareAirplane()
		{
			if (_shrinkingArea == null)
			{
				Debug.LogError("PrepareAirplane: _shrinkingArea is null! Cannot prepare airplane.");
				return;
			}

			if (_airplanePrefab == null)
			{
				Debug.LogError("PrepareAirplane: _airplanePrefab is null! Make sure it's assigned in the prefab.");
				return;
			}

			var randomOnCircle = MathUtility.RandomOnUnitCircle() * (_shrinkingArea.Radius + _airplanePrefab.OutZoneDistance + 30f);
			var position = _shrinkingArea.Center + new Vector3(randomOnCircle.x, _airplaneHeight, randomOnCircle.y);

			var lookDirection = Vector3.Cross((_shrinkingArea.Center - position).OnlyXZ(), Vector3.up);
			lookDirection = Random.value > 0.5f ? -lookDirection : lookDirection;

			//var rotation = Quaternion.LookRotation((_shrinkingArea.Center - position).OnlyXZ(), Vector3.up);

			_airplane = Runner.Spawn(_airplanePrefab, position, Quaternion.LookRotation(lookDirection));
			AirplaneActive = true;
		}

		private void StartAirdrop()
		{
			if (_airplane == null)
			{
				Debug.LogError("StartAirdrop: _airplane is null! Cannot start airdrop.");
				return;
			}

			HasStarted = true;

			_airplane.ActivateDropWindow();
			_dropCooldown = TickTimer.CreateFromSeconds(Runner, _playerDropTime);
		}

		private void AddWaitTime(float time)
		{
			if (_waitingForPlayersCooldown.ExpiredOrNotRunning(Runner) == true)
				return;

			float remaining = _waitingForPlayersCooldown.RemainingTime(Runner).Value;
			_waitingForPlayersCooldown = TickTimer.CreateFromSeconds(Runner, remaining + time);
		}

		private void StopAirdrop()
		{
			AirplaneActive = false;

			if (_airplaneAgents.Count > 0)
			{
				// Force remainining agents out in certain intervals
				_forcedJumpCooldown = TickTimer.CreateFromSeconds(Runner, _forcedJumpDelay);
			}
			else if (_airplane != null)
			{
				_airplane.DeactivateDropWindow();
			}

			if (_shrinkingArea != null)
			{
				_shrinkingArea.Pause(false);
			}
		}

		// RPCs

		[Rpc(RpcSources.Proxies, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
		private void RPC_StartAirdrop()
		{
			StartAirdrop();
		}

		[Rpc(RpcSources.StateAuthority, RpcTargets.All, Channel = RpcChannel.Reliable)]
		private void RPC_PlayerJumped(PlayerRef playerRef)
		{
			if (_airplane != null)
			{
				_airplane.OnPlayerJumped(playerRef);
			}
		}

		[Rpc(RpcSources.Proxies, RpcTargets.StateAuthority, Channel = RpcChannel.Reliable)]
		private void RPC_AddWaitTime(float time)
		{
			AddWaitTime(time);
		}

		// HELPERS

		private class BattleRoyaleComparer : IComparer<PlayerStatistics>
		{
			public int Compare(PlayerStatistics x, PlayerStatistics y)
			{
				var result = x.IsEliminated.CompareTo(y.IsEliminated);
				if (result != 0)
					return result;

				return x.Score.CompareTo(y.Score);
			}
		}
	}
}
