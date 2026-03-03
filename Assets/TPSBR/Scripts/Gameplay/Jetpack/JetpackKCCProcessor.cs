using Fusion.Addons.KCC;
using UnityEngine;

namespace TPSBR
{
	public sealed class JetpackKCCProcessor : KCCProcessor, IKCCProcessor, IPrepareData
	{
		// PRIVATE MEMBERS

		[SerializeField]
		private float _moveForce = 8f;
		[SerializeField]
		private float _moveDownForce = 8f;
		[SerializeField]
		private float _thrustUpForce = 20f;
		[SerializeField]
		private float _fullThrustUpForce = 30f;
		[SerializeField]
		private float _lookThrustForce = 2f;
		[SerializeField]
		private float _groundThrustImpulse = 1f;
		[SerializeField]
		private float _upThrustOppositeVelocityMultiplier = 0.2f;
		[SerializeField]
		private float _moveThrustOppositeVelocityMultiplier = 0.05f;
		[SerializeField]
		private float _gravityMultiplier = 1.5f;
		[SerializeField]
		private float _airPhysicsFriction = 0.001f;
		[SerializeField]
		private float _groundPhysicsFriction = 0.25f;
		[SerializeField]
		private float _moveVelocityDecreaseSpeed = 5f;

		private bool    _hasJetpack;
		private Jetpack _jetpack;

		// KCCProcessor INTERFACE

		public override float GetPriority(KCC kcc) => float.MaxValue;

		public override void OnEnter(KCC kcc, KCCData data)
		{
			_jetpack = kcc.GetComponent<Jetpack>();
			_hasJetpack = _jetpack != null;
		}

		public void Execute(PrepareData stage, KCC kcc, KCCData data)
		{
			data.Gravity = Physics.gravity * _gravityMultiplier;

			// We explicitly need data from fixed update.
			KCCData fixedData      = kcc.FixedData;
			float   fixedDeltaTime = fixedData.DeltaTime;

			if (kcc.IsInFixedUpdate == false)
			{
				// Reset to fixed update values. Instead of partial render predicted velocity we calculate velocity for next fixed update.
				data.KinematicVelocity    = fixedData.KinematicVelocity;
				data.ExternalVelocity     = fixedData.ExternalVelocity;
				data.ExternalAcceleration = fixedData.ExternalAcceleration;
				data.ExternalImpulse      = fixedData.ExternalImpulse;
				data.ExternalForce        = fixedData.ExternalForce;
				data.DynamicVelocity      = fixedData.DynamicVelocity;
			}

			data.KinematicDirection = data.InputDirection;
			data.KinematicTangent   = default;

			if (data.KinematicDirection != default)
			{
				data.KinematicTangent = data.KinematicDirection.normalized;
			}

			if (data.KinematicTangent == default)
			{
				data.KinematicTangent = data.TransformDirection;
			}

			// Move velocity is only decreasing (e.g. after jetpack activation)
			data.KinematicVelocity = Vector3.Lerp(data.KinematicVelocity, default, fixedDeltaTime * _moveVelocityDecreaseSpeed);

			float thrustForce = 0f;

			if (_jetpack.IsRunning == true)
			{
				thrustForce = _jetpack.FullThrust == true ? _fullThrustUpForce : _thrustUpForce;
			}

			data.ExternalForce += Vector3.up * thrustForce;

			if (thrustForce > 0f && _jetpack.FullThrust == true && fixedData.RealVelocity.y < 0f)
			{
				data.ExternalVelocity += new Vector3(0f, -data.DynamicVelocity.y * _upThrustOppositeVelocityMultiplier, 0f);
			}

			if (fixedData.IsGrounded == true && _jetpack.FullThrust == true)
			{
				data.ExternalImpulse += Vector3.up * _groundThrustImpulse;
			}

			if (fixedData.IsGrounded == true && fixedData.IsSnappingToGround == false && data.DynamicVelocity.y < 0.0f && data.DynamicVelocity.OnlyXZ().IsAlmostZero())
			{
				data.DynamicVelocity.y = 0f;
			}

			if (_lookThrustForce > 0f)
			{
				var lookDirection = data.LookRotation * Vector3.forward;
				data.ExternalForce += lookDirection * _lookThrustForce;
			}

			if (_moveDownForce > 0f && data.InputDirection != default)
			{
				// Player is able to fly down faster
				var rawInputDirection = Quaternion.Inverse(data.TransformRotation) * data.InputDirection;
				var desiredDirection = data.LookRotation * rawInputDirection;

				if (desiredDirection.y < 0f)
				{
					data.ExternalForce += Vector3.down * -desiredDirection.normalized.y * _moveDownForce;
				}
			}

			if (_moveForce > 0f && data.KinematicDirection != default)
			{
				var velocityXZ = fixedData.RealVelocity.OnlyXZ();
				float oppositeDirectionDot = Vector3.Dot(-velocityXZ.normalized, data.KinematicTangent);

				if (oppositeDirectionDot > 0f)
				{
					data.ExternalVelocity += -velocityXZ * oppositeDirectionDot * _moveThrustOppositeVelocityMultiplier;
				}

				data.ExternalForce += data.KinematicTangent * _moveForce;
			}

			data.DynamicVelocity += data.Gravity * fixedDeltaTime;

			data.DynamicVelocity += data.ExternalVelocity;
			data.DynamicVelocity += data.ExternalAcceleration * fixedDeltaTime;
			data.DynamicVelocity += (data.ExternalImpulse / kcc.Rigidbody.mass);
			data.DynamicVelocity += (data.ExternalForce / kcc.Rigidbody.mass) * fixedDeltaTime;

			Vector3 velocityDirection = data.KinematicTangent;
			float speed = data.DynamicVelocity.magnitude;

			if (speed > 0f)
			{
				velocityDirection = data.DynamicVelocity / speed;
			}

			data.DynamicVelocity += -0.5f * (fixedData.IsGrounded == true ? _groundPhysicsFriction : _airPhysicsFriction) * velocityDirection * speed * speed;

			if (kcc.IsInFixedUpdate == true)
			{
				// Consume one-time effects only in fixed update.
				// For render prediction we need them to be applied on top of fixed data in all frames.
				data.JumpImpulse      = default;
				data.ExternalVelocity = default;
				data.ExternalImpulse  = default;
			}

			// Forces applied over-time are reset always. These are set every tick/frame.
			data.ExternalAcceleration = default;
			data.ExternalForce        = default;

			kcc.SuppressProcessorsExcept(this, true);
		}

		// IKCCProcessor INTERFACE

		bool IKCCProcessor.IsActive(KCC kcc) => _hasJetpack == true && _jetpack.IsActive == true;
	}
}
