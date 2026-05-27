using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FixSkyboxColors : MonoBehaviour
{
    public Material skyboxMaterial;
    
    public void ApplyFortniteColors()
    {
        if (skyboxMaterial == null)
        {
            Debug.LogError("No skybox material assigned!");
            return;
        }
        
        skyboxMaterial.SetColor("_TopColor", new Color(0.4f, 0.7f, 1.0f, 1f));
        skyboxMaterial.SetColor("_HorizonColor", new Color(0.7f, 0.85f, 1.0f, 1f));
        skyboxMaterial.SetColor("_BottomColor", new Color(0.8f, 0.9f, 1.0f, 1f));
        
        skyboxMaterial.SetFloat("_HorizonOffset", 0f);
        skyboxMaterial.SetFloat("_HorizonSmoothness", 0.5f);
        skyboxMaterial.SetFloat("_GradientPower", 1.5f);
        
        skyboxMaterial.SetColor("_SunColor", new Color(1f, 0.95f, 0.8f, 1f));
        skyboxMaterial.SetVector("_SunDirection", new Vector4(0.3f, 0.6f, 0.5f, 0f));
        skyboxMaterial.SetFloat("_SunSize", 0.05f);
        skyboxMaterial.SetFloat("_SunSoftness", 0.02f);
        skyboxMaterial.SetFloat("_SunIntensity", 2f);
        
        skyboxMaterial.SetColor("_CloudColor", new Color(1f, 1f, 1f, 0.8f));
        skyboxMaterial.SetFloat("_CloudSpeed", 0.05f);
        skyboxMaterial.SetFloat("_CloudScale", 2f);
        skyboxMaterial.SetFloat("_CloudCoverage", 0.5f);
        skyboxMaterial.SetFloat("_CloudSoftness", 0.3f);
        
        Debug.Log("Fortnite skybox colors applied successfully!");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FixSkyboxColors))]
public class FixSkyboxColorsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        FixSkyboxColors fixer = (FixSkyboxColors)target;
        
        EditorGUILayout.Space(10);
        
        if (GUILayout.Button("Apply Fortnite Skybox Colors", GUILayout.Height(40)))
        {
            fixer.ApplyFortniteColors();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "1. Assign your FortniteSky material above\n" +
            "2. Click the button to apply proper colors\n" +
            "3. The skybox should turn blue immediately!", 
            MessageType.Info);
    }
}
#endif
