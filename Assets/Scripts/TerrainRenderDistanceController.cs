using UnityEngine;

public class TerrainRenderDistanceController : MonoBehaviour
{
    [Header("Terrain Render Settings")]
    [SerializeField]
    [Tooltip("Distance at which grass and terrain details are visible (default: 80)")]
    private float _detailDistance = 100000f;

    [SerializeField]
    [Tooltip("Distance at which trees start to billboard (default: 50)")]
    private float _treeBillboardStart = 500000f;

    [SerializeField]
    [Tooltip("Maximum distance for tree rendering (default: 5000)")]
    private float _treeDistance = 1000000f;

    [SerializeField]
    [Tooltip("Detail density scale (default: 1)")]
    private float _detailDensity = 1f;

    [Header("LOD Settings")]
    [SerializeField]
    [Tooltip("LOD bias - higher values keep higher quality models at distance (default: 1)")]
    private float _lodBias = 10f;

    [SerializeField]
    [Tooltip("Apply settings to all terrain instances in the scene")]
    private bool _applyToAllTerrains = true;

    private void Start()
    {
        ApplyTerrainSettings();
    }

    private void ApplyTerrainSettings()
    {
        QualitySettings.terrainQualityOverrides = 
            TerrainQualityOverrides.DetailDistance | 
            TerrainQualityOverrides.BillboardStart | 
            TerrainQualityOverrides.TreeDistance;

        QualitySettings.terrainDetailDistance = _detailDistance;
        QualitySettings.terrainBillboardStart = _treeBillboardStart;
        QualitySettings.terrainTreeDistance = _treeDistance;
        QualitySettings.terrainDetailDensityScale = _detailDensity;
        QualitySettings.lodBias = _lodBias;

        if (_applyToAllTerrains)
        {
            Terrain[] terrains = FindObjectsByType<Terrain>(FindObjectsSortMode.None);
            foreach (Terrain terrain in terrains)
            {
                terrain.detailObjectDistance = _detailDistance;
                terrain.treeBillboardDistance = _treeBillboardStart;
                terrain.treeDistance = _treeDistance;
            }
            Debug.Log($"Applied terrain settings to {terrains.Length} terrain(s)");
        }

        Debug.Log($"Terrain render distances updated - Detail: {_detailDistance}, Tree Billboard: {_treeBillboardStart}, Tree Distance: {_treeDistance}, LOD Bias: {_lodBias}");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyTerrainSettings();
        }
    }
#endif
}
