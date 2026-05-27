namespace TPSBR
{
	using UnityEngine;
	using Fusion.Addons.KCC;

	public static partial class InputUtility
	{
		// CONSTANTS

		private const float INCH_TO_CM = 2.54f;

		// PUBLIC METHODS

		public static Vector2 GetSmoothLookRotationDelta(SmoothVector2 smoothVector, Vector2 lookRotationDelta, float sensitivity, float responsivity)
		{
			lookRotationDelta *= sensitivity;

			// If the look rotation responsivity is enabled, calculate average delta instead.
			if (responsivity > 0.0f)
			{
				// Kill any rotation in opposite direction for instant direction flip.
				smoothVector.FilterValues(lookRotationDelta.x < 0.0f, lookRotationDelta.x > 0.0f, lookRotationDelta.y < 0.0f, lookRotationDelta.y > 0.0f);

				// Add or update value for current frame.
				smoothVector.AddValue(Time.frameCount, Time.unscaledDeltaTime, lookRotationDelta);

				// Calculate smooth look rotation delta.
				lookRotationDelta = smoothVector.CalculateSmoothValue(responsivity, Time.unscaledDeltaTime);
			}

			return lookRotationDelta;
		}

		public static float PixelsToCentimeters(float pixels)
		{
			return (pixels * INCH_TO_CM) / Screen.dpi;
		}

		public static Vector2 PixelsToCentimeters(Vector2 pixels)
		{
			return (pixels * INCH_TO_CM) / Screen.dpi;
		}
	}
}
