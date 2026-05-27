using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class StylizedMaterialSetup : MonoBehaviour
{
    [Header("Fortnite-Style Presets")]
    [Tooltip("Choose a preset configuration for quick setup")]
    public ToonPreset preset = ToonPreset.FortniteBright;
    
    [Header("Target Material")]
    public Material targetMaterial;
    
    public enum ToonPreset
    {
        FortniteBright,
        FortniteNatural,
        Custom
    }
    
    public void ApplyPreset()
    {
        if (targetMaterial == null)
        {
            Debug.LogWarning("No target material assigned!");
            return;
        }
        
        switch (preset)
        {
            case ToonPreset.FortniteBright:
                ApplyFortniteBrightPreset();
                break;
            case ToonPreset.FortniteNatural:
                ApplyFortniteNaturalPreset();
                break;
        }
        
        Debug.Log($"Applied {preset} preset to {targetMaterial.name}");
    }
    
    private void ApplyFortniteBrightPreset()
    {
        targetMaterial.SetColor("_ShadowColor", new Color(0.4f, 0.4f, 0.55f, 1f));
        targetMaterial.SetFloat("_ShadowThreshold", 0.55f);
        targetMaterial.SetFloat("_ShadowSmoothness", 0.08f);
        
        targetMaterial.SetColor("_RimColor", new Color(1f, 1f, 1f, 1f));
        targetMaterial.SetFloat("_RimPower", 4.0f);
        targetMaterial.SetFloat("_RimIntensity", 0.6f);
        
        targetMaterial.SetFloat("_Smoothness", 0.6f);
        targetMaterial.SetFloat("_SpecularIntensity", 0.4f);
        targetMaterial.SetFloat("_SpecularThreshold", 0.92f);
        targetMaterial.SetFloat("_SpecularSmoothness", 0.05f);
        
        targetMaterial.SetFloat("_AmbientIntensity", 0.4f);
    }
    
    private void ApplyFortniteNaturalPreset()
    {
        targetMaterial.SetColor("_ShadowColor", new Color(0.35f, 0.35f, 0.5f, 1f));
        targetMaterial.SetFloat("_ShadowThreshold", 0.5f);
        targetMaterial.SetFloat("_ShadowSmoothness", 0.05f);
        
        targetMaterial.SetColor("_RimColor", new Color(0.95f, 0.95f, 1f, 1f));
        targetMaterial.SetFloat("_RimPower", 3.5f);
        targetMaterial.SetFloat("_RimIntensity", 0.5f);
        
        targetMaterial.SetFloat("_Smoothness", 0.5f);
        targetMaterial.SetFloat("_SpecularIntensity", 0.3f);
        targetMaterial.SetFloat("_SpecularThreshold", 0.9f);
        targetMaterial.SetFloat("_SpecularSmoothness", 0.05f);
        
        targetMaterial.SetFloat("_AmbientIntensity", 0.3f);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(StylizedMaterialSetup))]
public class StylizedMaterialSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        StylizedMaterialSetup setup = (StylizedMaterialSetup)target;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Apply Preset to Material", GUILayout.Height(30)))
        {
            setup.ApplyPreset();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "1. Assign your material to 'Target Material'\n" +
            "2. Choose a preset (Fortnite Bright recommended)\n" +
            "3. Click 'Apply Preset to Material'", 
            MessageType.Info);
    }
}
#endif
