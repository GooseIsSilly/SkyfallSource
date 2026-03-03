using System;
using UnityEngine;
using Fusion;
using Fusion.Addons.KCC;

namespace TPSBR
{
	[DefaultExecutionOrder(-10000)]
	[RequireComponent(typeof(Rigidbody))]
	public class Elevator : ContextTRSPBehaviour, IPlatform, IAfterClientPredictionReset, IBeforeAllTicks, IKCCProcessor, IKCCProcessorProvider
	{
		// PROTECTED MEMBERS

		[Networked]
		protected Vector3 _basePosition { get; private set; }
		[Networked]
		protected float _currentHeight { get; private set; }

		// PRIVATE MEMBERS

		[SerializeField]
		private float _height = -5f;
		[SerializeField]
		private float _speed = 1f;

		private Transform _transform;
		private Rigidbody _rigidbody;
		private int       _lastRenderFrame;
		private int       _syncFrames = 1;
		private int       _syncOffset;

		private static int _sharedOffset;

		// PUBLIC METHODS

		public void OverrideHeight(float height)
		{
			_currentHeight  = height;
		}

		// NetworkBehaviour INTERFACE

		public override void Spawned()
		{
			_lastRenderFrame = default;

			if (HasStateAuthority == true)
			{
				_transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);

				_basePosition  = position;
				_currentHeight = _height;
				_syncFrames    = TickRate.Resolve(Runner.Config.Simulation.TickRateSelection).Server;
				_syncOffset    = _sharedOffset;

				_sharedOffset++;

				// Set position of NetworkTRSP (compressed).
				State.Position = position;
				State.Rotation = rotation;
			}
			else
			{
				// Read initial values.
				RestoreTransform();
			}

			Runner.SetIsSimulated(Object, true);

			if (ApplicationSettings.IsStrippedBatch == true)
			{
				gameObject.SetActive(false);
			}
		}

		public override void FixedUpdateNetwork()
		{
			Vector3 position = CalculatePosition(Runner.Tick * Runner.DeltaTime);

			_transform.position = position;
			_rigidbody.position = position;

			if (((Runner.Tick.Raw + _syncOffset) % _syncFrames) == 0 && HasStateAuthority == true)
			{
				// Store transform once per second to lower network sync pressure.
				// Precise position is based on tick - calculated.
				State.Position = position;
			}
		}

		public override void Render()
		{
			_lastRenderFrame = Time.frameCount;

			Vector3 position = CalculatePosition((Runner.Tick + Runner.LocalAlpha) * Runner.DeltaTime);

			_transform.position = position;
			_rigidbody.position = position;
		}

		// IAfterClientPredictionReset INTERFACE

		void IAfterClientPredictionReset.AfterClientPredictionReset()
		{
			RestoreTransform();
		}

		// IBeforeAllTicks INTERFACE

		void IBeforeAllTicks.BeforeAllTicks(bool resimulation, int tickCount)
		{
			// Skip resimulation, the state is already restored from AfterClientPredictionReset().
			if (resimulation == true)
				return;

			// Restore state only if a render update was executed previous frame.
			// Otherwise we continue with state from previous fixed tick or the state is already restored from AfterClientPredictionReset().
			int previousFrame = Time.frameCount - 1;
			if (previousFrame != _lastRenderFrame)
				return;

			RestoreTransform();
		}

		// IKCCInteractionProvider INTERFACE

		bool IKCCInteractionProvider.CanStartInteraction(KCC kcc, KCCData data) => true;
		bool IKCCInteractionProvider.CanStopInteraction (KCC kcc, KCCData data) => true;

		// IKCCProcessorProvider INTERFACE

		IKCCProcessor IKCCProcessorProvider.GetProcessor()
		{
			return this;
		}

		// MonoBehaviour INTERFACE

		private void Awake()
		{
			_transform = transform;
			_rigidbody = GetComponent<Rigidbody>();

			if (_rigidbody == null)
				throw new NullReferenceException($"GameObject {name} has missing Rigidbody component!");

			_rigidbody.isKinematic   = true;
			_rigidbody.useGravity    = false;
			_rigidbody.interpolation = RigidbodyInterpolation.None;
			_rigidbody.constraints   = RigidbodyConstraints.FreezeAll;
		}

		// PRIVATE METHODS

		private void RestoreTransform()
		{
			Vector3 position = CalculatePosition(Runner.Tick * Runner.DeltaTime);

			_transform.SetPositionAndRotation(position, State.Rotation);
			_rigidbody.position = position;
		}

		private Vector3 CalculatePosition(float time)
		{
			Vector3 position = _basePosition;

			float absoluteHeight = Mathf.Abs(_currentHeight);
			if (absoluteHeight <= 0.0f)
				return _basePosition;

			float totalDistance = _speed * time;
			float distanceAlpha = (totalDistance % (absoluteHeight * 2.0f)) / absoluteHeight; // 0.0f - 2.0f

			if (distanceAlpha > 1.0f)
			{
				distanceAlpha = 2.0f - distanceAlpha; // 0.0f - 1.0f
			}

			return _basePosition + Vector3.up * distanceAlpha * _currentHeight;
		}
	}
}
