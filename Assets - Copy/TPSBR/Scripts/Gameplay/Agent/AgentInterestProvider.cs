namespace TPSBR
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using Fusion.Addons.InterestManagement;

	public class AgentInterestProvider : MonoBehaviour, IInterestProvider
	{
		public int                 SortOrder;
		public EInterestMode       InterestMode;
		public List<InterestShape> Shapes = new List<InterestShape>();

		public Transform Transform { get { if (ReferenceEquals(_transform, null) == true) { _transform = transform; } return transform; } }

		int           IInterestProvider.Version      => _version;
		int           IInterestProvider.SortOrder    => SortOrder;
		EInterestMode IInterestProvider.InterestMode => InterestMode;

		private Transform _transform;
		private int       _version;

		public void Release()
		{
			++_version;
		}

		public bool RegisterProvider(IInterestProvider interestProvider, float duration = 0)
		{
			throw new NotImplementedException();
		}

		public bool UnregisterProvider(IInterestProvider interestProvider, bool recursive)
		{
			throw new NotImplementedException();
		}

		public void GetProviders(InterestProviderSet interestProviders, bool recursive)
		{
		}

		public bool IsPlayerInterested(PlayerInterestView playerView)
		{
			 return true;
		}

		public void GetProvidersForPlayer(PlayerInterestView playerView, InterestProviderSet interestProviders, bool recursive)
		{
		}

		public void GetCellsForPlayer(PlayerInterestView playerView, HashSet<int> cells)
		{
			if (InterestMode == EInterestMode.Override)
			{
				cells.Clear();
			}

			for (int i = 0, count = Shapes.Count; i < count; ++i)
			{
				InterestShape shape = Shapes[i];
				if (shape != null)
				{
					shape.GetCells(playerView, cells, InterestMode);
				}
			}
		}

		public void DrawGizmosForPlayer(PlayerInterestView playerView)
		{
			for (int i = 0, count = Shapes.Count; i < count; ++i)
			{
				InterestShape shape = Shapes[i];
				if (shape != null)
				{
					shape.DrawGizmo(playerView);
				}
			}
		}

		private void OnDestroy()
		{
			Release();
		}
	}
}
