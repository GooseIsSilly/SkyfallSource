using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Manages a stylized Time of Day system specifically for the FortniteSimpleSky shader.
/// Handles sun rotation, skybox color gradients, and ambient lighting synchronization.
/// </summary>
/// <remarks>
/// The time of day is derived from <see cref="Fusion.NetworkRunner.SimulationTime"/>, which is
/// the same shared network clock on every client in a session, instead of a per-client random
/// start value advanced by local <see cref="Time.deltaTime"/>. This keeps day/night in sync for
/// all players; previously each client picked its own random start time and drifted independently,
/// so one player could see night while another saw morning.
/// </remarks>
public class FortniteTimeOfDayManager : MonoBehaviour
{
    [Header("Skybox Material")]
    public Material skyboxMaterial;
    
    [Header("Sun & Light References")]
    public Light sunLight;
    
    [Header("Time Settings")]
    [Range(0f, 24f)]
    public float currentTime = 12f;
    public float timeMultiplier = 0.1f;
    public bool pauseTime = false;

    [Header("Network Sync")]
    [Tooltip("Hour of day (0-24) that corresponds to network simulation time 0, so every client starts from the same point on the cycle.")]
    [Range(0f, 24f)]
    public float startTimeOfDay = 12f;

    private Fusion.NetworkRunner _runner;

    [Header("Sky Colors")]
    public Gradient topColorGradient = GetDefaultTopGradient();
    public Gradient horizonColorGradient = GetDefaultHorizonGradient();
    public Gradient bottomColorGradient = GetDefaultBottomGradient();
    public Gradient sunColorGradient = GetDefaultSunGradient();
    public Gradient cloudColorGradient = GetDefaultCloudGradient();

    [Header("Lighting Settings")]
    public AnimationCurve sunIntensityCurve = GetDefaultSunIntensityCurve();
    public Gradient ambientColorGradient = GetDefaultAmbientGradient();
    public AnimationCurve ambientIntensityCurve = GetDefaultAmbientIntensityCurve();

    [Header("Fog Settings")]
    public bool syncFog = true;
    public Gradient fogColorGradient = GetDefaultFogGradient();
    public AnimationCurve fogDensityCurve = GetDefaultFogDensityCurve();

    private const string TOP_COLOR_PROP = "_TopColor";
    private const string HORIZON_COLOR_PROP = "_HorizonColor";
    private const string BOTTOM_COLOR_PROP = "_BottomColor";
    private const string SUN_COLOR_PROP = "_SunColor";
    private const string SUN_DIRECTION_PROP = "_SunDirection";
    private const string SUN_INTENSITY_PROP = "_SunIntensity";
    private const string CLOUD_COLOR_PROP = "_CloudColor";

    private static Gradient GetDefaultTopGradient()
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.02f, 0.05f, 0.1f), 0f),
                new GradientColorKey(new Color(0.2f, 0.4f, 0.8f), 0.25f),
                new GradientColorKey(new Color(0f, 0.67f, 1f), 0.5f),
                new GradientColorKey(new Color(0.1f, 0.2f, 0.4f), 0.75f),
                new GradientColorKey(new Color(0.02f, 0.05f, 0.1f), 1f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        return g;
    }

    private static Gradient GetDefaultHorizonGradient()
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.05f, 0.1f, 0.2f), 0f),
                new GradientColorKey(new Color(0.85f, 0.75f, 0.7f), 0.25f), // Softer Peach
                new GradientColorKey(new Color(0.7f, 0.85f, 1f), 0.5f),
                new GradientColorKey(new Color(0.8f, 0.65f, 0.6f), 0.75f),  // Softer Sunset
                new GradientColorKey(new Color(0.05f, 0.1f, 0.2f), 1f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        return g;
    }

    private static Gradient GetDefaultBottomGradient()
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.02f, 0.02f, 0.05f), 0f),
                new GradientColorKey(new Color(0.8f, 0.9f, 1f), 0.5f),
                new GradientColorKey(new Color(0.02f, 0.02f, 0.05f), 1f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        return g;
    }

    private static Gradient GetDefaultSunGradient()
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.9f, 0.8f, 0.7f), 0.25f), // Less orange sun
                new GradientColorKey(new Color(1f, 0.95f, 0.8f), 0.5f),
                new GradientColorKey(new Color(0.8f, 0.7f, 0.6f), 0.75f)  // Less orange sun
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        return g;
    }

    private static Gradient GetDefaultCloudGradient()
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.1f, 0.15f, 0.3f), 0f),     // Dark at night
                new GradientColorKey(new Color(1f, 1f, 1f), 0.5f),         // White at noon
                new GradientColorKey(new Color(0.1f, 0.15f, 0.3f), 1f)      // Dark at night
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0f), 
                new GradientAlphaKey(0.8f, 1f) 
            }
        );
        return g;
    }

    private static Gradient GetDefaultAmbientGradient()
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.1f, 0.15f, 0.25f), 0f),
                new GradientColorKey(new Color(0.6f, 0.7f, 0.85f), 0.5f),
                new GradientColorKey(new Color(0.1f, 0.15f, 0.25f), 1f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        return g;
    }

    private static Gradient GetDefaultFogGradient()
    {
        Gradient g = new Gradient();
        g.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.05f, 0.1f, 0.2f), 0f),
                new GradientColorKey(new Color(0.85f, 0.75f, 0.7f), 0.25f), // Softer Fog
                new GradientColorKey(new Color(0.7f, 0.85f, 1f), 0.5f),
                new GradientColorKey(new Color(0.8f, 0.65f, 0.6f), 0.75f),  // Softer Fog
                new GradientColorKey(new Color(0.05f, 0.1f, 0.2f), 1f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        return g;
    }

    private static AnimationCurve GetDefaultSunIntensityCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.2f, 0f),
            new Keyframe(0.3f, 1.2f),
            new Keyframe(0.5f, 1.5f),
            new Keyframe(0.7f, 1.2f),
            new Keyframe(0.8f, 0f),
            new Keyframe(1f, 0f)
        );
    }

    private static AnimationCurve GetDefaultAmbientIntensityCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0.4f),
            new Keyframe(0.5f, 1.0f),
            new Keyframe(1f, 0.4f)
        );
    }

    private static AnimationCurve GetDefaultFogDensityCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 0.004f),
            new Keyframe(0.25f, 0.001f),
            new Keyframe(0.5f, 0.0005f),
            new Keyframe(0.75f, 0.001f),
            new Keyframe(1f, 0.004f)
        );
    }

    private void Start()
    {
        // Start at the configured hour; the actual synced value is recomputed every frame in
        // Update() from the shared network clock once a NetworkRunner is available.
        currentTime = startTimeOfDay;

        if (skyboxMaterial == null)
            skyboxMaterial = RenderSettings.skybox;

        if (sunLight == null)
            sunLight = RenderSettings.sun;

        // Apply immediately on start
        ApplyTimeOfDay();
    }

    private void Update()
    {
        if (!pauseTime)
        {
            if (TryGetSyncedNetworkTime(out double simulationTime))
            {
                // Every client's NetworkRunner shares the same simulation clock, so deriving
                // currentTime from it (instead of accumulating local Time.deltaTime from a random
                // per-client start) guarantees all players see the same time of day.
                double hoursElapsed = simulationTime * timeMultiplier;
                currentTime = (float)(((hoursElapsed + startTimeOfDay) % 24.0 + 24.0) % 24.0);
            }
            else
            {
                // No active network session (e.g. offline testing) - fall back to local advancement.
                currentTime += Time.deltaTime * timeMultiplier;
                if (currentTime >= 24f) currentTime = 0f;
            }
        }

        ApplyTimeOfDay();
    }

    /// <summary>Finds the running NetworkRunner (caching it) and returns its shared simulation time.</summary>
    private bool TryGetSyncedNetworkTime(out double simulationTime)
    {
        if (_runner == null || _runner.IsRunning == false)
        {
            _runner = null;

            if (Fusion.NetworkRunner.Instances != null)
            {
                foreach (Fusion.NetworkRunner runner in Fusion.NetworkRunner.Instances)
                {
                    if (runner != null && runner.IsRunning == true)
                    {
                        _runner = runner;
                        break;
                    }
                }
            }
        }

        if (_runner != null && _runner.IsRunning == true)
        {
            simulationTime = _runner.SimulationTime;
            return true;
        }

        simulationTime = 0.0;
        return false;
    }

    private void ApplyTimeOfDay()
    {
        float timePercent = currentTime / 24f;

        // 1. Calculate Sun Rotation
        // Sun rises at 6:00 (90 deg) and sets at 18:00 (270 deg)
        float sunAngle = (timePercent * 360f) - 90f;
        sunLight.transform.localRotation = Quaternion.Euler(sunAngle, -170f, 0f);

        // 2. Update Skybox Material
        if (skyboxMaterial != null)
        {
            skyboxMaterial.SetColor(TOP_COLOR_PROP, topColorGradient.Evaluate(timePercent));
            skyboxMaterial.SetColor(HORIZON_COLOR_PROP, horizonColorGradient.Evaluate(timePercent));
            skyboxMaterial.SetColor(BOTTOM_COLOR_PROP, bottomColorGradient.Evaluate(timePercent));
            skyboxMaterial.SetColor(SUN_COLOR_PROP, sunColorGradient.Evaluate(timePercent));
            skyboxMaterial.SetColor(CLOUD_COLOR_PROP, cloudColorGradient.Evaluate(timePercent));
            
            // Sync sun direction in shader
            Vector3 sunDir = -sunLight.transform.forward;
            skyboxMaterial.SetVector(SUN_DIRECTION_PROP, sunDir);
            
            // Optional: Sun Intensity in shader if it exists
            float sunInt = sunIntensityCurve.Evaluate(timePercent);
            if (skyboxMaterial.HasProperty(SUN_INTENSITY_PROP))
                skyboxMaterial.SetFloat(SUN_INTENSITY_PROP, sunInt);
        }

        // 3. Update Light Components
        sunLight.color = sunColorGradient.Evaluate(timePercent);
        sunLight.intensity = sunIntensityCurve.Evaluate(timePercent);

        // 4. Update Ambient Lighting
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ambientColorGradient.Evaluate(timePercent);
        RenderSettings.ambientIntensity = ambientIntensityCurve.Evaluate(timePercent);
        
        // Ensure standard RenderSettings colors are updated
        RenderSettings.subtractiveShadowColor = new Color(0.1f, 0.15f, 0.25f);

        // 5. Update Fog
        if (syncFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColorGradient.Evaluate(timePercent);
            RenderSettings.fogDensity = fogDensityCurve.Evaluate(timePercent);
        }

        // Force GI Update for ambient changes
        DynamicGI.UpdateEnvironment();
    }

    private void SetupDefaultGradients()
    {
        // Simple procedural setup if not configured
        topColorGradient = new Gradient();
        horizonColorGradient = new Gradient();
        bottomColorGradient = new Gradient();
        sunColorGradient = new Gradient();
        ambientColorGradient = new Gradient();

        // Noon = 0.5, Night = 0, Sunrise = 0.25, Sunset = 0.75
        
        // Top Color (Blue -> Dark)
        topColorGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.02f, 0.05f, 0.1f), 0f),    // Midnight
                new GradientColorKey(new Color(0.2f, 0.4f, 0.8f), 0.25f),   // Morning
                new GradientColorKey(new Color(0f, 0.67f, 1f), 0.5f),       // Noon
                new GradientColorKey(new Color(0.1f, 0.2f, 0.4f), 0.75f),   // Evening
                new GradientColorKey(new Color(0.02f, 0.05f, 0.1f), 1f)     // Midnight
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        // Horizon Color (Light Blue -> Orange -> Dark)
        horizonColorGradient.SetKeys(
            new GradientColorKey[] {
                new GradientColorKey(new Color(0.05f, 0.1f, 0.2f), 0f),
                new GradientColorKey(new Color(1f, 0.6f, 0.3f), 0.25f),
                new GradientColorKey(new Color(0.7f, 0.85f, 1f), 0.5f),
                new GradientColorKey(new Color(1f, 0.4f, 0.2f), 0.75f),
                new GradientColorKey(new Color(0.05f, 0.1f, 0.2f), 1f)
            },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );

        // Sun Intensity Curve
        sunIntensityCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.2f, 0f),
            new Keyframe(0.3f, 1.5f),
            new Keyframe(0.5f, 2.0f),
            new Keyframe(0.7f, 1.5f),
            new Keyframe(0.8f, 0f),
            new Keyframe(1f, 0f)
        );
        
        // Ambient Intensity Curve
        ambientIntensityCurve = new AnimationCurve(
            new Keyframe(0f, 0.2f),
            new Keyframe(0.5f, 1.2f),
            new Keyframe(1f, 0.2f)
        );

        // Fog Density
        fogDensityCurve = new AnimationCurve(
            new Keyframe(0f, 0.01f),
            new Keyframe(0.5f, 0.002f),
            new Keyframe(1f, 0.01f)
        );
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        if (sunLight != null) ApplyTimeOfDay();
    }
}
