namespace TPSBR
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using Fusion;
	using Fusion.Addons.InterestManagement;
	using Fusion.Addons.KCC;

	public sealed class AgentInterestView : PlayerInterestView
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private PlayerInterestConfig _lookInterest = new PlayerInterestConfig();
		[SerializeField]
		private PlayerInterestConfig _shootInterest = new PlayerInterestConfig();

		private KCC                     _kcc;
		private KCCShapeCastInfo        _shapeCastInfo = new KCCShapeCastInfo();
		private List<AgentInterestView> _otherPlayerViews = new List<AgentInterestView>();

		private static Func<AgentInterestView, AgentInterestView, float, float, float, bool, bool> _filterPlayersByView = (otherPlayerView, localPlayerView, sqrMinDistance, sqrMaxDistance, maxAngleCos, raycast) => FilterPlayersByView(otherPlayerView, localPlayerView, sqrMinDistance, sqrMaxDistance, maxAngleCos, raycast);

		// PUBLIC METHODS

		public void UpdateShootInterestTargets()
		{
			float maxViewAngleCos = Mathf.Cos(_shootInterest.MaxViewAngle * Mathf.Deg2Rad);

			GlobalInterestManager globalInterestManager = Runner.GetGlobalInterestManager();
			globalInterestManager.GetPlayerViews(_otherPlayerViews, _filterPlayersByView, this, _shootInterest.MinViewDistance * _shootInterest.MinViewDistance, _shootInterest.MaxViewDistance * _shootInterest.MaxViewDistance, maxViewAngleCos, false);

			// The agent is shooting but other players not be interest yet (looking in a different direction).
			// We add an interest shape around agent transform for all players in front of him.
			for (int i = 0, count = _otherPlayerViews.Count; i < count; ++i)
			{
				PlayerInterestView otherPlayerView = _otherPlayerViews[i];
				otherPlayerView.RegisterProvider(_shootInterest.Provider, _shootInterest.Duration);
			}
		}

		// PlayerInterestView INTERFACE

		protected override void OnViewSpawned()
		{
			_kcc = GetComponent<KCC>();

			if (Runner.TryGetPlayerInterestManager(Object.InputAuthority, out PlayerInterestManager playerInterestManager) == true)
			{
				playerInterestManager.InterestView = this;
			}
		}

		protected override void OnViewDespawned(NetworkRunner runner, bool hasState)
		{
			_lookInterest.Provider.Release();
			_shootInterest.Provider.Release();

			if (runner.TryGetPlayerInterestManager(Object.InputAuthority, out PlayerInterestManager playerInterestManager) == true)
			{
				if (playerInterestManager.InterestView == this)
				{
					playerInterestManager.InterestView = null;
				}
			}
		}

		protected override void OnViewFixedUpdateNetwork()
		{
			if (Runner.IsServer == true && Runner.IsForward == true)
			{
				// Check player in front only once per second.
				int tickRate = TickRate.Resolve(Runner.Config.Simulation.TickRateSelection).Server;
				if ((Runner.Tick.Raw % tickRate) == (Object.InputAuthority.AsIndex % tickRate))
				{
					UpdateLookInterestTargets();
				}
			}
		}

		protected override void OnDrawGizmosForPlayer(PlayerInterestView playerView, bool isSelected)
		{
			if (isSelected == false)
				return;

			Vector3    cameraPosition = playerView.CameraPosition;
			Quaternion cameraRotation = playerView.CameraRotation;

			if (Application.isPlaying == false)
			{
				playerView.Transform.GetPositionAndRotation(out cameraPosition, out cameraRotation);
			}

			if (_lookInterest.Provider != null)
			{
				InterestUtility.DrawAngle(cameraPosition, cameraRotation, _lookInterest.MaxViewAngle, _lookInterest.MinViewDistance, _lookInterest.MaxViewDistance, _lookInterest.GizmoColor);
			}

			if (_shootInterest.Provider != null)
			{
				InterestUtility.DrawAngle(cameraPosition, cameraRotation, _shootInterest.MaxViewAngle, _shootInterest.MinViewDistance, _shootInterest.MaxViewDistance, _shootInterest.GizmoColor);
			}

			InterestUtility.DrawCameraFrustum(Camera.main, cameraPosition, cameraRotation, Color.red);
		}

		// PRIVATE METHODS

		private void UpdateLookInterestTargets()
		{
			float maxViewAngleCos = Mathf.Cos(_lookInterest.MaxViewAngle * Mathf.Deg2Rad);

			GlobalInterestManager globalInterestManager = Runner.GetGlobalInterestManager();
			globalInterestManager.GetPlayerViews(_otherPlayerViews, _filterPlayersByView, this, _lookInterest.MinViewDistance * _lookInterest.MinViewDistance, _lookInterest.MaxViewDistance * _lookInterest.MaxViewDistance, maxViewAngleCos, true);

			// Add interest shapes around other players who are visible on screen, but not covered by default interest shapes of the player.
			for (int i = 0, count = _otherPlayerViews.Count; i < count; ++i)
			{
				PlayerInterestConfig otherPlayerConfig = ((AgentInterestView)_otherPlayerViews[i])._lookInterest;
				if (otherPlayerConfig.Duration > 0.0f && otherPlayerConfig.Provider != null)
				{
					RegisterProvider(otherPlayerConfig.Provider, otherPlayerConfig.Duration);
				}
			}
		}

		private static bool FilterPlayersByView(AgentInterestView otherPlayerView, AgentInterestView localPlayerView, float sqrMinDistance, float sqrMaxDistance, float maxAngleCos, bool raycast)
		{
			if (ReferenceEquals(otherPlayerView, localPlayerView) == true)
				return false;

			Vector3 positionDifference = otherPlayerView.PlayerPosition - localPlayerView.CameraPosition;

			float sqrDistance = Vector3.SqrMagnitude(positionDifference);
			if (sqrDistance > sqrMaxDistance || sqrDistance < sqrMinDistance)
				return false;

			float dot = Vector3.Dot(localPlayerView.CameraDirection, Vector3.Normalize(positionDifference));
			if (dot < maxAngleCos)
				return false;

			if (raycast == true)
			{
				KCC              kcc           = localPlayerView._kcc;
				KCCShapeCastInfo shapeCastInfo = localPlayerView._shapeCastInfo;
				Vector3          fromToVector  = (otherPlayerView.PlayerPosition + Vector3.up) - localPlayerView.CameraPosition;
				float            distance      = fromToVector.magnitude;

				if (kcc.RayCast(shapeCastInfo, localPlayerView.CameraPosition, fromToVector / distance, distance, QueryTriggerInteraction.Ignore) == true)
				{
					if (ReferenceEquals(otherPlayerView, shapeCastInfo.ColliderHits[0].Transform.GetComponentInParent<AgentInterestView>()) == true)
						return true;
				}

				return false;
			}

			return true;
		}

		[Serializable]
		private sealed class PlayerInterestConfig
		{
			public AgentInterestProvider Provider;
			public float                 Duration = 1.0f;
			[Range(0.0f, 90.0f)]
			public float                 MaxViewAngle = 25.0f;
			public float                 MinViewDistance = 10.0f;
			public float                 MaxViewDistance = 100.0f;
			public Color                 GizmoColor = Color.white;
		}
	}
}
