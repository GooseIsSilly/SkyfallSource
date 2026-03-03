using System.Collections.Generic;
using UnityEngine;
using Fusion;

namespace TPSBR
{
	public class NetworkObjectPool : INetworkObjectProvider
	{
		public SceneContext Context { get; set; }

		private Dictionary<NetworkPrefabId, Stack<NetworkObject>> _cached   = new Dictionary<NetworkPrefabId, Stack<NetworkObject>>(32);
		private Dictionary<NetworkObject, NetworkPrefabId>        _borrowed = new Dictionary<NetworkObject, NetworkPrefabId>();

		NetworkObjectAcquireResult INetworkObjectProvider.AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result)
		{
			if (_cached.TryGetValue(context.PrefabId, out var objects) == false)
			{
				objects = _cached[context.PrefabId] = new Stack<NetworkObject>();
			}

			if (objects.Count > 0)
			{
				var oldInstance = objects.Pop();
				_borrowed[oldInstance] = context.PrefabId;

				oldInstance.SetActive(true);

				result = oldInstance;
				runner.MoveToRunnerScene(result);
				return NetworkObjectAcquireResult.Success;
			}

			NetworkObject original = runner.Config.PrefabTable.Load(context.PrefabId, true);
			if (original == null)
			{
				result = default;
				return NetworkObjectAcquireResult.Failed;
			}

			var instance = Object.Instantiate(original);
			_borrowed[instance] = context.PrefabId;

			AssignContext(instance);

			for (int i = 0; i < instance.NestedObjects.Length; i++)
			{
				AssignContext(instance.NestedObjects[i]);
			}

			result = instance;
			runner.MoveToRunnerScene(result);
			return NetworkObjectAcquireResult.Success;
		}

		void INetworkObjectProvider.ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
		{
			if (context.IsNestedObject == true)
				return;

			NetworkObject instance = context.Object;
			if (instance == null)
				return;

			if (instance.NetworkTypeId.IsSceneObject == false && runner.IsShutdown == false)
			{
				if (_borrowed.TryGetValue(instance, out var prefabID) == true)
				{
					_borrowed.Remove(instance);
					_cached[prefabID].Push(instance);

					instance.SetActive(false);
					instance.transform.parent = null;
					instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
				}
				else
				{
					Object.Destroy(instance.gameObject);
				}
			}
			else
			{
				Object.Destroy(instance.gameObject);
			}
		}

		public NetworkPrefabId GetPrefabId(NetworkRunner runner, NetworkObjectGuid prefabGuid)
		{
			return runner.Prefabs.GetId(prefabGuid);
		}

		private void AssignContext(NetworkObject instance)
		{
			for (int i = 0, count = instance.NetworkedBehaviours.Length; i < count; i++)
			{
				if (instance.NetworkedBehaviours[i] is IContextBehaviour cachedBehaviour)
				{
					cachedBehaviour.Context = Context;
				}
			}
		}
	}
}
