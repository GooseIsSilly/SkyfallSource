using UnityEngine;

namespace TPSBR
{
    /// <summary>
    /// Fortnite-style rocket trail with glowing effects
    /// Attach to rocket projectile prefab for automatic trail setup
    /// </summary>
    public class FortniteRocketTrail : MonoBehaviour
    {
        [Header("Trail Renderer")]
        [SerializeField] private TrailRenderer trail;
        [SerializeField] private float trailTime = 0.5f;
        [SerializeField] private float trailWidthStart = 0.3f;
        [SerializeField] private float trailWidthEnd = 0.05f;
        [SerializeField] private Color trailColorStart = new Color(1f, 0.6f, 0f, 1f); // Orange
        [SerializeField] private Color trailColorEnd = new Color(1f, 0.2f, 0f, 0f); // Red fading
        [SerializeField] private Material trailMaterial;
        
        [Header("Glow Light")]
        [SerializeField] private Light rocketLight;
        [SerializeField] private bool createLight = true;
        [SerializeField] private float glowRange = 5f;
        [SerializeField] private float glowIntensity = 2f;
        [SerializeField] private Color glowColor = new Color(1f, 0.6f, 0f); // Orange
        
        [Header("Particle Trail (Optional)")]
        [SerializeField] private ParticleSystem smokeTrail;
        [SerializeField] private bool createSmokeTrail = false;

        private void Start()
        {
            SetupTrailRenderer();
            
            if (createLight)
            {
                SetupGlowLight();
            }
            
            if (createSmokeTrail && smokeTrail == null)
            {
                SetupSmokeTrail();
            }
        }

        private void SetupTrailRenderer()
        {
            // Create trail renderer if not assigned
            if (trail == null)
            {
                trail = GetComponent<TrailRenderer>();
                if (trail == null)
                {
                    trail = gameObject.AddComponent<TrailRenderer>();
                }
            }
            
            // Configure trail
            trail.time = trailTime;
            trail.minVertexDistance = 0.1f;
            trail.autodestruct = false;
            trail.emitting = true;
            
            // Width curve (starts wide, gets narrow)
            trail.widthCurve = AnimationCurve.Linear(0, trailWidthStart, 1, trailWidthEnd);
            
            // Color gradient (orange to red, fading out)
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(trailColorStart, 0f),
                    new GradientColorKey(trailColorEnd, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(trailColorStart.a, 0f),
                    new GradientAlphaKey(trailColorEnd.a, 1f)
                }
            );
            trail.colorGradient = gradient;
            
            // Material
            if (trailMaterial != null)
            {
                trail.material = trailMaterial;
            }
            else
            {
                // Create basic additive material
                Material mat = new Material(Shader.Find("Particles/Additive"));
                mat.color = trailColorStart;
                trail.material = mat;
            }
            
            // Rendering settings
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            trail.receiveShadows = false;
            
            Debug.Log("[FortniteRocketTrail] Trail renderer configured!");
        }

        private void SetupGlowLight()
        {
            // Create light if not assigned
            if (rocketLight == null)
            {
                rocketLight = GetComponent<Light>();
                if (rocketLight == null)
                {
                    rocketLight = gameObject.AddComponent<Light>();
                }
            }
            
            // Configure light
            rocketLight.type = LightType.Point;
            rocketLight.color = glowColor;
            rocketLight.range = glowRange;
            rocketLight.intensity = glowIntensity;
            rocketLight.shadows = LightShadows.None;
            rocketLight.renderMode = LightRenderMode.ForcePixel;
            
            Debug.Log("[FortniteRocketTrail] Glow light configured!");
        }

        private void SetupSmokeTrail()
        {
            // Create smoke trail particle system
            GameObject smokeObj = new GameObject("SmokeTrail");
            smokeObj.transform.SetParent(transform);
            smokeObj.transform.localPosition = Vector3.zero;
            smokeObj.transform.localRotation = Quaternion.identity;
            
            smokeTrail = smokeObj.AddComponent<ParticleSystem>();
            
            // Main module
            var main = smokeTrail.main;
            main.duration = 1f;
            main.loop = true;
            main.startLifetime = 0.5f;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.4f);
            main.startColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            // Emission
            var emission = smokeTrail.emission;
            emission.rateOverTime = 30f;
            
            // Shape
            var shape = smokeTrail.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;
            
            // Color over lifetime (fade out)
            var colorOverLifetime = smokeTrail.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(Color.gray, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0.3f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);
            
            // Size over lifetime (grow)
            var sizeOverLifetime = smokeTrail.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 2));
            
            // Renderer
            var renderer = smokeTrail.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            
            Debug.Log("[FortniteRocketTrail] Smoke trail particle system created!");
        }

        private void OnEnable()
        {
            // Reset trail when rocket is spawned
            if (trail != null)
            {
                trail.Clear();
                trail.emitting = true;
            }
        }

        private void OnDisable()
        {
            // Stop emitting when rocket is despawned
            if (trail != null)
            {
                trail.emitting = false;
            }
        }
    }
}
