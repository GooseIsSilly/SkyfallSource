using UnityEngine;

public class StylizedSkyboxController : MonoBehaviour
{
    [Header("Skybox Material")]
    [Tooltip("The skybox material to control")]
    public Material skyboxMaterial;
    
    [Header("Sun Settings")]
    [Tooltip("The directional light that represents the sun")]
    public Light sunLight;
    
    [Tooltip("If enabled, updates sun direction in skybox to match the directional light")]
    public bool syncWithDirectionalLight = true;
    
    [Header("Time of Day")]
    [Tooltip("If enabled, animates the sun position over time")]
    public bool animateSun = false;
    
    [Range(0f, 24f)]
    [Tooltip("Current time of day in hours (0-24)")]
    public float timeOfDay = 12f;
    
    [Tooltip("How fast time passes (1 = real time, 10 = 10x speed)")]
    public float timeSpeed = 0.1f;
    
    private void Start()
    {
        if (skyboxMaterial == null)
        {
            skyboxMaterial = RenderSettings.skybox;
        }
        
        if (sunLight == null && syncWithDirectionalLight)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    sunLight = light;
                    break;
                }
            }
        }
    }
    
    private void Update()
    {
        if (skyboxMaterial == null) return;
        
        if (animateSun)
        {
            timeOfDay += Time.deltaTime * timeSpeed;
            if (timeOfDay >= 24f) timeOfDay = 0f;
            
            float angle = (timeOfDay / 24f) * 360f - 90f;
            Vector3 sunDir = Quaternion.Euler(angle, 0, 0) * Vector3.forward;
            skyboxMaterial.SetVector("_SunDirection", sunDir);
            
            if (sunLight != null)
            {
                sunLight.transform.rotation = Quaternion.LookRotation(sunDir);
            }
        }
        else if (syncWithDirectionalLight && sunLight != null)
        {
            Vector3 sunDir = -sunLight.transform.forward;
            skyboxMaterial.SetVector("_SunDirection", sunDir);
        }
    }
}
