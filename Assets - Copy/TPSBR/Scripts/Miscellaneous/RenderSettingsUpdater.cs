namespace TPSBR
{
	using UnityEngine;
	using UnityEngine.Rendering;

	public sealed class RenderSettingsUpdater : MonoBehaviour
	{
		public Material    Skybox;
		public Light       Sun;

		public bool        Fog;
		public FogMode     FogMode;
		public Color       FogColor;
		public float       FogDensity;
		public float       FogStartDistance;
		public float       FogEndDistance;

		public AmbientMode AmbientMode;
		[ColorUsage(true, true)]
		public Color       AmbientLight;
		public float       AmbientIntensity;
		[ColorUsage(true, true)]
		public Color       AmbientEquatorColor;
		[ColorUsage(true, true)]
		public Color       AmbientGroundColor;
		[ColorUsage(true, true)]
		public Color       AmbientSkyColor;

		public Color       SubtractiveShadowColor;

		public void Process()
		{
			RenderSettings.skybox                 = Skybox;
			RenderSettings.sun                    = Sun;

			RenderSettings.fog                    = Fog;
			RenderSettings.fogMode                = FogMode;
			RenderSettings.fogColor               = FogColor;
			RenderSettings.fogDensity             = FogDensity;
			RenderSettings.fogStartDistance       = FogStartDistance;
			RenderSettings.fogEndDistance         = FogEndDistance;

			RenderSettings.ambientMode            = AmbientMode;
			RenderSettings.ambientLight           = AmbientLight;
			RenderSettings.ambientIntensity       = AmbientIntensity;
			RenderSettings.ambientEquatorColor    = AmbientEquatorColor;
			RenderSettings.ambientGroundColor     = AmbientGroundColor;
			RenderSettings.ambientSkyColor        = AmbientSkyColor;

			RenderSettings.subtractiveShadowColor = SubtractiveShadowColor;
		}
	}
}
