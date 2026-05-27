namespace TPSBR
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using Fusion;

	// !!! WARNING !!!
	// This updater is a 1:1 copy of default PlatformProcessorUpdater except it supports BRPlatformProcessor instead of PlatformProcessor.
	// !!! WARNING !!!

	/// <summary>
	/// Used to update BRPlatformProcessor instances independently of their Object.IsInSimulation state.
	/// </summary>
	[DefaultExecutionOrder(BRPlatformProcessor.EXECUTION_ORDER)]
    public unsafe class BRPlatformProcessorUpdater : SimulationBehaviour
    {
		private HashSet<BRPlatformProcessor> _processors = new HashSet<BRPlatformProcessor>();

		public void Register(BRPlatformProcessor processor)
		{
			_processors.Add(processor);
		}

		public void Unregister(BRPlatformProcessor processor)
		{
			_processors.Remove(processor);
		}

		public override void FixedUpdateNetwork()
		{
			foreach (BRPlatformProcessor processor in _processors)
			{
				try
				{
					processor.ProcessFixedUpdate();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		public override void Render()
		{
			foreach (BRPlatformProcessor processor in _processors)
			{
				try
				{
					processor.ProcessRender();
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}
	}
}
