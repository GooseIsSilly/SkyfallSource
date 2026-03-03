using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class FixSkyboxColorsFortnite : MonoBehaviour
{
    [Header("Assign Your Skybox Material")]
    public Material skyboxMaterial;
    
    [Header("Click to Apply Fortnite Colors")]
    public bool applyColors = false;

    private void Update()
    {
        if (applyColors)
        {
            applyColors = false;
            ApplyFortniteSkyboxColors();
        }
    }

    private void ApplyFortniteSkyboxColors()
    {
        if (skyboxMaterial == null)
        {
            Debug.LogError("Please assign the skybox material!");
            return;
        }

        skyboxMaterial.SetColor("_TopColor", new Color(0.2f, 0.5f, 0.9f, 1f));
        skyboxMaterial.SetColor("_HorizonColor", new Color(0.5f, 0.7f, 0.95f, 1f));
        skyboxMaterial.SetColor("_BottomColor", new Color(0.6f, 0.75f, 0.9f, 1f));
        
        skyboxMaterial.SetFloat("_HorizonOffset", 0f);
        skyboxMaterial.SetFloat("_HorizonSmoothness", 1f);
        skyboxMaterial.SetFloat("_GradientPower", 1.2f);
        
        skyboxMaterial.SetColor("_SunColor", new Color(1f, 0.95f, 0.85f, 1f));
        skyboxMaterial.SetFloat("_SunIntensity", 1.5f);
        skyboxMaterial.SetFloat("_SunSize", 0.03f);
        skyboxMaterial.SetFloat("_SunSoftness", 0.02f);
        
        skyboxMaterial.SetColor("_CloudColor", new Color(1f, 1f, 1f, 0.7f));
        skyboxMaterial.SetFloat("_CloudCoverage", 0.4f);
        skyboxMaterial.SetFloat("_CloudSoftness", 0.3f);
        skyboxMaterial.SetFloat("_CloudScale", 3f);
        skyboxMaterial.SetFloat("_CloudSpeed", 0.03f);
        
        skyboxMaterial.SetFloat("_Rotation", 0f);

        Debug.Log("Applied Fortnite-style skybox colors! The harsh white should now be a softer blue.");
    }
}
