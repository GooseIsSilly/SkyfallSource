using UnityEngine;

public class GlobalMaterialApplier : MonoBehaviour
{
    [Header("Material to Apply")]
    [Tooltip("This material will be applied to all renderers in the scene")]
    public Material toonMaterial;
    
    [Header("Filter Options")]
    [Tooltip("If enabled, only apply to objects with specific tags")]
    public bool useTagFilter = false;
    public string[] allowedTags = new string[] { "Player", "Agent" };
    
    [Tooltip("If enabled, only apply to objects on specific layers")]
    public bool useLayerFilter = false;
    public LayerMask allowedLayers;
    
    [Header("Settings")]
    [Tooltip("If true, applies material immediately on Start")]
    public bool applyOnStart = true;
    
    [Tooltip("If true, will replace all materials on each renderer")]
    public bool replaceAllMaterials = true;
    
    [Tooltip("If false, only replaces the first material slot")]
    public bool keepOriginalTextures = false;
    
    private void Start()
    {
        if (applyOnStart)
        {
            ApplyMaterialToAll();
        }
    }
    
    public void ApplyMaterialToAll()
    {
        if (toonMaterial == null)
        {
            Debug.LogWarning("No material assigned to GlobalMaterialApplier!");
            return;
        }
        
        int appliedCount = 0;
        
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        
        foreach (Renderer renderer in allRenderers)
        {
            if (ShouldApplyToRenderer(renderer))
            {
                ApplyMaterialToRenderer(renderer);
                appliedCount++;
            }
        }
        
        Debug.Log($"Applied {toonMaterial.name} to {appliedCount} renderers in the scene.");
    }
    
    private bool ShouldApplyToRenderer(Renderer renderer)
    {
        if (useTagFilter)
        {
            bool hasAllowedTag = false;
            foreach (string tag in allowedTags)
            {
                try
                {
                    if (renderer.CompareTag(tag))
                    {
                        hasAllowedTag = true;
                        break;
                    }
                }
                catch (UnityException)
                {
                    Debug.LogWarning($"[GlobalMaterialApplier] Tag '{tag}' is not defined. Skipping.");
                    continue;
                }
            }
            if (!hasAllowedTag) return false;
        }
        
        if (useLayerFilter)
        {
            if ((allowedLayers.value & (1 << renderer.gameObject.layer)) == 0)
            {
                return false;
            }
        }
        
        return true;
    }
    
    private void ApplyMaterialToRenderer(Renderer renderer)
    {
        if (replaceAllMaterials)
        {
            Material[] newMaterials = new Material[renderer.materials.Length];
            
            if (keepOriginalTextures)
            {
                for (int i = 0; i < renderer.materials.Length; i++)
                {
                    Material newMat = new Material(toonMaterial);
                    
                    if (renderer.materials[i].HasProperty("_BaseMap") || renderer.materials[i].HasProperty("_MainTex"))
                    {
                        Texture originalTexture = renderer.materials[i].mainTexture;
                        if (originalTexture != null)
                        {
                            newMat.SetTexture("_BaseMap", originalTexture);
                        }
                    }
                    
                    if (renderer.materials[i].HasProperty("_BaseColor") || renderer.materials[i].HasProperty("_Color"))
                    {
                        Color originalColor = renderer.materials[i].color;
                        newMat.SetColor("_BaseColor", originalColor);
                    }
                    
                    newMaterials[i] = newMat;
                }
            }
            else
            {
                for (int i = 0; i < newMaterials.Length; i++)
                {
                    newMaterials[i] = toonMaterial;
                }
            }
            
            renderer.materials = newMaterials;
        }
        else
        {
            Material[] materials = renderer.materials;
            materials[0] = toonMaterial;
            renderer.materials = materials;
        }
    }
}
