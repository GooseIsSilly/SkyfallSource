namespace TPSBR
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using Fusion;

	public interface ISortedUpdate
	{
		void SortedUpdate();
	}

	/// <summary>
	/// This component invokes ISortedUpdate.SortedUpdate() sorted by order on objects scheduled by ScheduleSortedUpdate().
	/// </summary>
	[DefaultExecutionOrder(100)]
	public sealed class SortedUpdateInvoker : SimulationBehaviour, IDespawned
	{
		// PRIVATE MEMBERS

		private List<CallbackObject>   _scheduledObjects = new List<CallbackObject>();
		private List<CallbackObject>   _invokeObjects    = new List<CallbackObject>();
		private Stack<CallbackObject>  _cache            = new Stack<CallbackObject>();
		private CallbackObjectComparer _comparer         = new CallbackObjectComparer();

		// PUBLIC METHODS

		public void ScheduleSortedUpdate(ISortedUpdate callback, float sortOrder)
		{
			CallbackObject scheduledObject;

			if (_cache.Count > 0)
			{
				scheduledObject = _cache.Pop();
			}
			else
			{
				scheduledObject = new CallbackObject();
			}

			scheduledObject.Callback  = callback;
			scheduledObject.SortOrder = sortOrder;

			_scheduledObjects.Add(scheduledObject);
		}

		// IDespawned INTERFACE

		void IDespawned.Despawned(NetworkRunner runner, bool hasState)
		{
			_scheduledObjects.Clear();
			_invokeObjects.Clear();
			_cache.Clear();
		}

		// SimulationBehaviour INTERFACE

		public override void FixedUpdateNetwork()
		{
			while (_scheduledObjects.Count > 0)
			{
				_invokeObjects.Clear();
				_invokeObjects.AddRange(_scheduledObjects);
				_invokeObjects.Sort(_comparer);

				_scheduledObjects.Clear();

				for (int i = 0, count = _invokeObjects.Count; i < count; ++i)
				{
					CallbackObject invokeObject = _invokeObjects[i];

					try
					{
						invokeObject.Callback.SortedUpdate();
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
					}

					invokeObject.Clear();
					_cache.Push(invokeObject);
				}
			}
		}

		// DATA STRUCTURES

		private sealed class CallbackObject
		{
			public ISortedUpdate Callback;
			public float         SortOrder;

			public void Clear()
			{
				Callback  = default;
				SortOrder = default;
			}
		}

		private sealed class CallbackObjectComparer : IComparer<CallbackObject>
		{
			public int Compare(CallbackObject o1, CallbackObject o2)
			{
				return o1.SortOrder <= o2.SortOrder ? -1 : 1;
			}
		}
	}
}
