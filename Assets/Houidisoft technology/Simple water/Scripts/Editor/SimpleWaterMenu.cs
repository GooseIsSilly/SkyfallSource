

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace Houidisoft.SimpleWater{
public static class SimpleWaterMenu
{
    private const string ShaderName = "Custom/SimpleWaterURP";
    private const string MaterialName = "Water Material Sample";
    private const string ResourcesFolder = "Assets/Resources";
    private const string MaterialAssetPath = ResourcesFolder + "/" + MaterialName + ".mat";

    [MenuItem("GameObject/3D Object/Add Simple Water", false, 10)]
    private static void CreateSimpleWater(MenuCommand menuCommand)
    {
        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "Simple Water";
        StageUtility.PlaceGameObjectInCurrentStage(water);

        // Unity's built-in Plane primitive mesh is 10x10 world units at scale 1,
        // so a scale of 4 on X/Z gives a 40x40 water surface.
        water.transform.localScale = new Vector3(4f, 1f, 4f);

        // Water doesn't need a collider by default — remove the one CreatePrimitive adds.
        Collider planeCollider = water.GetComponent<Collider>();
        if (planeCollider != null)
        {
            Object.DestroyImmediate(planeCollider);
        }

        Renderer renderer = water.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Each water object gets its own material, copied from the sample —
            // so tweaking one pond's color/foam doesn't affect any other pond.
            Material instanceMaterial = new Material(GetOrCreateWaterMaterial());
            instanceMaterial.name = water.name + " Material";
            renderer.sharedMaterial = instanceMaterial;
        }

        // Parent under whatever was right-clicked in the Hierarchy, matching how
        // Unity's own "Create Object" menu items behave.
        GameObjectUtility.SetParentAndAlign(water, menuCommand.context as GameObject);

        // Place it at the Scene view camera's position.
        SceneView sceneView = SceneView.lastActiveSceneView;
        if (sceneView != null && sceneView.camera != null)
        {
            water.transform.position = sceneView.camera.transform.position;
        }

        Undo.RegisterCreatedObjectUndo(water, "Create " + water.name);
        Selection.activeObject = water;
    }

    private static Material GetOrCreateWaterMaterial()
    {
        // Reuse the existing material if one's already been created.
        Material existing = Resources.Load<Material>(MaterialName);
        if (existing != null)
        {
            return existing;
        }

        Shader shader = Shader.Find(ShaderName);
        if (shader == null)
        {
            Debug.LogError($"Simple Water: couldn't find shader \"{ShaderName}\". " +
                "Make sure SimpleWaterURP.shader is somewhere in the project.");
            return new Material(Shader.Find("Universal Render Pipeline/Lit"));
        }

        if (!AssetDatabase.IsValidFolder(ResourcesFolder))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        Material mat = new Material(shader) { name = MaterialName };

        // Explicit defaults so the material always matches the shader's intended
        // look, even if someone changes the shader's own defaults later.
        mat.SetFloat("_WaveSpeed", 1.0f);
        mat.SetFloat("_WaveStrength", 0.15f);
        mat.SetFloat("_WaveScale", 1.0f);

        mat.SetColor("_ShallowColor", new Color(0.42f, 0.75f, 0.75f, 0.55f));
        mat.SetColor("_DeepColor", new Color(0.02f, 0.18f, 0.32f, 0.95f));
        mat.SetFloat("_WaterDepth", 3.0f);

        mat.SetFloat("_NormalTiling", 1.0f);
        mat.SetFloat("_NormalStrength", 0.5f);
        mat.SetFloat("_NormalSpeed", 0.1f);

        mat.SetFloat("_FresnelPower", 3.0f);
        mat.SetFloat("_ReflectionStrength", 0.6f);

        mat.SetColor("_FoamColor", Color.white);
        mat.SetFloat("_FoamDistance", 0.4f);
        mat.SetFloat("_FoamTiling", 1.0f);
        mat.SetFloat("_FoamSpeed", 0.1f);

        mat.SetColor("_SpecularColor", Color.white);
        mat.SetFloat("_Smoothness", 128.0f);
        mat.SetFloat("_SpecularStrength", 1.0f);

        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        AssetDatabase.CreateAsset(mat, MaterialAssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return mat;
    }
}
#endif
}
