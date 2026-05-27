namespace TPSBR
{
	using System;
	using UnityEngine;
	using Fusion.Addons.KCC;

	public sealed class TransformSampler
	{
		// CONSTANTS

		private const int HISTORY_SIZE = KCC.HISTORY_SIZE;

		// PUBLIC MEMBERS

		public bool EnableLogs;

		// PRIVATE MEMBERS

		private TransformSample[] _samples;
		private TransformSample   _lastFixedSample  = new TransformSample();
		private TransformSample   _lastRenderSample = new TransformSample();
		private int               _lastPredictedFixedTick;
		private int               _lastPredictedRenderFrame;

		// PUBLIC METHODS

		public void Sample(KCC kcc, Vector3 position, Quaternion rotation)
		{
			if (_samples == null)
			{
				_samples = new TransformSample[HISTORY_SIZE];
			}

			if (kcc.IsInFixedUpdate == true)
			{
				SampleFixedUpdate(kcc.Runner.Tick, position, rotation);
			}
			else
			{
				SampleRenderUpdate(kcc.Runner.Tick, kcc.Runner.LocalAlpha, position, rotation);
			}
		}

		public bool ResolveRenderPositionAndRotation(KCC kcc, float renderAlpha, out Vector3 renderPosition, out Quaternion renderRotation)
		{
			if (_samples == null)
			{
				renderPosition = default;
				renderRotation = Quaternion.identity;
				return false;
			}

			renderAlpha = Mathf.Clamp01(renderAlpha);

			KCCData         fromKCCData;
			KCCData         toKCCData;
			TransformSample fromSample;
			TransformSample toSample;

			bool useAntiJitter;
			bool shiftPosition = false;

			if (kcc.IsInFixedUpdate == true)
			{
				useAntiJitter = true;

				int currentTick = kcc.Runner.Tick;

				if (kcc.Settings.InputAuthorityBehavior == EKCCAuthorityBehavior.PredictFixed_PredictRender)
				{
					if (_lastPredictedFixedTick < currentTick)
					{
						if (EnableLogs == true)
						{
							kcc.LogError("Missing data for calculation of render position and rotation for current tick. The KCC is set to predict in render, therefore Sample() must be called before this method!");
						}

						renderPosition = default;
						renderRotation = Quaternion.identity;
						return false;
					}

					if (kcc.LastPredictedFixedTick < currentTick)
					{
						if (EnableLogs == true)
						{
							kcc.LogError("Missing data for calculation of render position and rotation for current tick. The KCC is set to predict in render and must run fixed update for current tick before calling this method!");
						}

						renderPosition = default;
						renderRotation = Quaternion.identity;
						return false;
					}

					fromKCCData = kcc.GetHistoryData(currentTick - 1);
					toKCCData   = kcc.GetHistoryData(currentTick);
					fromSample  = GetFixedSample(currentTick - 1);
					toSample    = GetFixedSample(currentTick);
				}
				else if (kcc.Settings.InputAuthorityBehavior == EKCCAuthorityBehavior.PredictFixed_InterpolateRender)
				{
					if (kcc.Settings.ForcePredictedLookRotation == true)
					{
						if (_lastPredictedFixedTick < currentTick)
						{
							if (EnableLogs == true)
							{
								kcc.LogError("Missing data for calculation of render position and rotation for current tick. The KCC has Force Predicted Look Rotation enabled, therefore Sample() must be called before this method!");
							}

							renderPosition = default;
							renderRotation = Quaternion.identity;
							return false;
						}

						if (kcc.LastPredictedFixedTick < currentTick)
						{
							if (EnableLogs == true)
							{
								kcc.LogError("Missing data for calculation of render position and rotation for current tick. The KCC has Force Predicted Look Rotation enabled and must run fixed update for current tick before calling this method!");
							}

							renderPosition = default;
							renderRotation = Quaternion.identity;
							return false;
						}

						fromKCCData = kcc.GetHistoryData(currentTick - 2);
						toKCCData   = kcc.GetHistoryData(currentTick - 1);
						fromSample  = GetFixedSample(currentTick - 1);
						toSample    = GetFixedSample(currentTick);

						shiftPosition = true;
					}
					else
					{
						fromKCCData = kcc.GetHistoryData(currentTick - 2);
						toKCCData   = kcc.GetHistoryData(currentTick - 1);
						fromSample  = GetFixedSample(currentTick - 2);
						toSample    = GetFixedSample(currentTick - 1);
					}
				}
				else
				{
					throw new NotImplementedException(kcc.Settings.InputAuthorityBehavior.ToString());
				}
			}
			else
			{
				useAntiJitter = kcc.IsPredictingInRenderUpdate == false && kcc.Settings.ForcePredictedLookRotation == false;

				if (kcc.Settings.InputAuthorityBehavior == EKCCAuthorityBehavior.PredictFixed_PredictRender)
				{
					if (_lastPredictedRenderFrame < Time.frameCount)
					{
						if (EnableLogs == true)
						{
							kcc.LogError("Missing data for calculation of render position and rotation for current frame. The KCC is set to predict in render, therefore Sample() must be called before this method!");
						}

						renderPosition = default;
						renderRotation = Quaternion.identity;
						return false;
					}

					if (kcc.LastPredictedRenderFrame < Time.frameCount)
					{
						if (EnableLogs == true)
						{
							kcc.LogError("Missing data for calculation of render position and rotation for current frame. The KCC is set to predict in render and must run render update for current frame before calling this method!");
						}

						renderPosition = default;
						renderRotation = Quaternion.identity;
						return false;
					}

					fromKCCData = kcc.FixedData;
					toKCCData   = kcc.RenderData;
					fromSample  = _lastFixedSample;
					toSample    = _lastRenderSample;

					if (renderAlpha > _lastRenderSample.Alpha)
					{
						renderAlpha = 1.0f;
					}
					else if (_lastRenderSample.Alpha > 0.000001f)
					{
						renderAlpha = Mathf.Clamp01(renderAlpha / _lastRenderSample.Alpha);
					}
				}
				else if (kcc.Settings.InputAuthorityBehavior == EKCCAuthorityBehavior.PredictFixed_InterpolateRender)
				{
					int currentTick = kcc.Runner.Tick;

					fromKCCData = kcc.GetHistoryData(currentTick - 1);
					toKCCData   = kcc.GetHistoryData(currentTick);

					if (kcc.Settings.ForcePredictedLookRotation == true)
					{
						if (_lastPredictedRenderFrame < Time.frameCount)
						{
							if (EnableLogs == true)
							{
								kcc.LogError("Missing data for calculation of render position and rotation for current frame. The KCC has Force Predicted Look Rotation enabled, therefore Sample() must be called before this method!");
							}

							renderPosition = default;
							renderRotation = Quaternion.identity;
							return false;
						}

						if (kcc.LastPredictedRenderFrame < Time.frameCount)
						{
							if (EnableLogs == true)
							{
								kcc.LogError("Missing data for calculation of render position and rotation for current frame. The KCC has Force Predicted Look Rotation enabled and must run render update for current frame before calling this method!");
							}

							renderPosition = default;
							renderRotation = Quaternion.identity;
							return false;
						}

						fromSample = _lastFixedSample;
						toSample   = _lastRenderSample;

						if (renderAlpha > _lastRenderSample.Alpha)
						{
							renderAlpha = 1.0f;
						}
						else if (_lastRenderSample.Alpha > 0.000001f)
						{
							renderAlpha = Mathf.Clamp01(renderAlpha / _lastRenderSample.Alpha);
						}
					}
					else
					{
						fromSample = GetFixedSample(currentTick - 1);
						toSample   = GetFixedSample(currentTick);
					}
				}
				else
				{
					throw new NotImplementedException(kcc.Settings.InputAuthorityBehavior.ToString());
				}
			}

			if (fromSample == null || toSample == null)
			{
				if (EnableLogs == true)
				{
					kcc.LogWarning("Missing data for calculation of render position and rotation.");
				}

				renderPosition = default;
				renderRotation = Quaternion.identity;
				return false;
			}

			if (toKCCData != null && toKCCData.HasTeleported == true)
			{
				renderPosition = toSample.Position;
				renderRotation = toSample.Rotation;
			}
			else
			{
				Vector3 targetPosition   = Vector3.Lerp(fromSample.Position, toSample.Position, renderAlpha);
				Vector3 kccPositionDelta = default;

				if (fromKCCData != null && toKCCData != null)
				{
					kccPositionDelta = toKCCData.TargetPosition - fromKCCData.TargetPosition;
				}

				if (useAntiJitter == true && kcc.ActiveFeatures.Has(EKCCFeature.AntiJitter) == true)
				{
					Vector2 maxAntiJitterDistance = kcc.Settings.AntiJitterDistance;
					if (maxAntiJitterDistance.IsZero() == false && kccPositionDelta.IsAlmostZero(0.0001f) == false)
					{
						maxAntiJitterDistance.x = Mathf.Min(Vector3.Magnitude(kccPositionDelta.OnlyXZ()), maxAntiJitterDistance.x);
						maxAntiJitterDistance.y = Mathf.Min(Mathf.Abs(kccPositionDelta.y), maxAntiJitterDistance.y);

						Vector3 fromPosition  = fromSample.Position;
						Vector3 toPosition    = toSample.Position;
						Vector3 positionDelta = Vector3.Lerp(fromPosition, toPosition, renderAlpha) - fromPosition;

						targetPosition = fromPosition;

						float distanceY = Mathf.Abs(positionDelta.y);
						if (distanceY > 0.000001f && distanceY > maxAntiJitterDistance.y)
						{
							targetPosition.y += positionDelta.y * ((distanceY - maxAntiJitterDistance.y) / distanceY);
						}

						Vector3 positionDeltaXZ = positionDelta.OnlyXZ();

						float distanceXZ = Vector3.Magnitude(positionDeltaXZ);
						if (distanceXZ > 0.000001f && distanceXZ > maxAntiJitterDistance.x)
						{
							targetPosition += positionDeltaXZ * ((distanceXZ - maxAntiJitterDistance.x) / distanceXZ);
						}
					}
				}

				if (shiftPosition == true)
				{
					targetPosition -= kccPositionDelta;
				}

				renderPosition = targetPosition;
				renderRotation = Quaternion.Slerp(fromSample.Rotation, toSample.Rotation, renderAlpha);
			}

			return true;
		}

		public void Clear()
		{
			if (_samples != null)
			{
				_samples.Clear();
			}
		}

		// PRIVATE METHODS

		private TransformSample GetFixedSample(int tick)
		{
			if (tick < 0)
				return null;

			TransformSample sample = _samples[tick % HISTORY_SIZE];
			if (sample != null && sample.Tick == tick)
				return sample;

			return null;
		}

		private void SampleFixedUpdate(int tick, Vector3 position, Quaternion rotation)
		{
			TransformSample sample = _samples[tick % HISTORY_SIZE];
			if (sample == null)
			{
				sample = new TransformSample();
				_samples[tick % HISTORY_SIZE] = sample;
			}

			sample.Tick     = tick;
			sample.Alpha    = 1.0f;
			sample.Position = position;
			sample.Rotation = rotation;

			_lastFixedSample.Tick     = sample.Tick;
			_lastFixedSample.Alpha    = sample.Alpha;
			_lastFixedSample.Position = sample.Position;
			_lastFixedSample.Rotation = sample.Rotation;

			_lastPredictedFixedTick = tick;
		}

		private void SampleRenderUpdate(int tick, float renderAlpha, Vector3 position, Quaternion rotation)
		{
			_lastRenderSample.Tick     = tick;
			_lastRenderSample.Alpha    = renderAlpha;
			_lastRenderSample.Position = position;
			_lastRenderSample.Rotation = rotation;

			_lastPredictedRenderFrame = Time.frameCount;
		}

		// DATA STRUCTURES

		private sealed class TransformSample
		{
			public int        Tick;
			public float      Alpha;
			public Vector3    Position;
			public Quaternion Rotation;
		}
	}
}
