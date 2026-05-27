namespace TPSBR
{
	using UnityEngine;
	using Fusion;

	/// <summary>
	/// This component serves as an AoI position proxy. It is useful for objects which don't need their own NetworkTransform component, but still need to be filtered by AoI.
	/// Example: Player + Weapon. Each Weapon is a separate dynamically spawned NetworkObject with NetworkAreaOfInterestProxy component. Upon spawn Weapon is parented under Player object (usually under a hand bone).
	/// There is no need to synchronize Weapon position because it is driven locally. But if a Player runs out of AoI, we want to stop updating both objects Player and Weapon.
	/// </summary>
	[DisallowMultipleComponent]
	[DefaultExecutionOrder(10000)]
	public sealed unsafe class NetworkAreaOfInterestProxy : NetworkTRSP
	{
		// PUBLIC MEMBERS

		public Transform PositionSource => _positionSource;

		// PRIVATE MEMBERS

		[SerializeField]
		private Transform _positionSource;

		private Transform _defaultPositionSource;
		private bool      _hasExplicitPositionSource;
		private bool      _isSpawned;

		// PUBLIC METHODS

		public void SetPosition(Vector3 position)
		{
			if (_isSpawned == true)
			{
				State.Position = position;
			}
		}

		public void SetPositionSource(Transform positionSource)
		{
			_positionSource = positionSource;
			_hasExplicitPositionSource = true;

			Synchronize();
		}

		public void ResetPositionSource()
		{
			_positionSource = _defaultPositionSource;
			_hasExplicitPositionSource = false;

			Synchronize();
		}

		public void FindPositionSourceInParent()
		{
			FindPositionSourceInParent(true);
			Synchronize();
		}

		public void Synchronize()
		{
			if (_isSpawned == true && _positionSource != null)
			{
				State.Position = _positionSource.position;
			}
		}

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			_isSpawned = true;

			ReplicateToAll(false);

			if (_hasExplicitPositionSource == false && _positionSource == null)
			{
				State.Position = default;

				FindPositionSourceInParent(false);
			}

			Synchronize();
		}

		public override void Despawned(NetworkRunner runner, bool hasState)
		{
			_isSpawned = false;

			ResetPositionSource();
		}

		public override void FixedUpdateNetwork()
		{
			Synchronize();
		}

		// MonoBehaviour INTERFACE

		private void Awake()
		{
			_defaultPositionSource = _positionSource;
		}

		// PRIVATE METHODS

		private void FindPositionSourceInParent(bool isExplicit)
		{
			Transform parentTransform = transform.parent;
			while (parentTransform != null)
			{
				NetworkObject networkObject = parentTransform.GetComponent<NetworkObject>();
				if (networkObject != null)
				{
					_positionSource = parentTransform;
					_hasExplicitPositionSource = isExplicit;
					break;
				}

				parentTransform = parentTransform.parent;
			}
		}
	}
}
