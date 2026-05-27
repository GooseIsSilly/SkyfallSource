using UnityEngine;

namespace TPSBR
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerrainToMesh : MonoBehaviour
    {
        [Header("Terrain Settings")]
        [SerializeField] private Terrain _sourceTerrain;
        [SerializeField] private int _resolution = 1;
        [SerializeField] private bool _generateOnStart = false;
        
        [Header("Material Settings")]
        [Tooltip("Optional: Override material instead of using terrain's material")]
        [SerializeField] private Material _overrideMaterial;
        
        private void Start()
        {
            if (_generateOnStart && _sourceTerrain != null)
            {
                GenerateMeshFromTerrain();
            }
        }
        
        [ContextMenu("Generate Mesh From Terrain")]
        public void GenerateMeshFromTerrain()
        {
            if (_sourceTerrain == null)
            {
                Debug.LogError("[TerrainToMesh] Source terrain is not assigned!");
                return;
            }
            
            TerrainData terrainData = _sourceTerrain.terrainData;
            int width = terrainData.heightmapResolution / _resolution;
            int height = terrainData.heightmapResolution / _resolution;
            Vector3 size = terrainData.size;
            
            float[,] heights = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
            
            Vector3[] vertices = new Vector3[width * height];
            Vector2[] uvs = new Vector2[width * height];
            int[] triangles = new int[(width - 1) * (height - 1) * 6];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    
                    int heightX = x * _resolution;
                    int heightY = y * _resolution;
                    
                    float heightValue = heights[heightY, heightX];
                    
                    vertices[index] = new Vector3(
                        (float)x / (width - 1) * size.x,
                        heightValue * size.y,
                        (float)y / (height - 1) * size.z
                    );
                    
                    uvs[index] = new Vector2((float)x / (width - 1), (float)y / (height - 1));
                }
            }
            
            int triIndex = 0;
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int topLeft = y * width + x;
                    int topRight = topLeft + 1;
                    int bottomLeft = (y + 1) * width + x;
                    int bottomRight = bottomLeft + 1;
                    
                    triangles[triIndex++] = topLeft;
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = topRight;
                    
                    triangles[triIndex++] = topRight;
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = bottomRight;
                }
            }
            
            Mesh mesh = new Mesh();
            mesh.name = "TerrainMesh";
            
            if (vertices.Length > 65535)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            
            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;
            
            MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
            
            if (_overrideMaterial != null)
            {
                meshRenderer.sharedMaterial = _overrideMaterial;
                Debug.Log("[TerrainToMesh] Using override material");
            }
            else if (_sourceTerrain.materialTemplate != null)
            {
                meshRenderer.sharedMaterial = _sourceTerrain.materialTemplate;
                Debug.Log($"[TerrainToMesh] Using terrain material template: {_sourceTerrain.materialTemplate.name}");
            }
            else
            {
                Debug.LogWarning("[TerrainToMesh] No material found! Assign a material to the MeshRenderer or set Override Material.");
            }
            
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }
            meshCollider.sharedMesh = mesh;
            
            transform.position = _sourceTerrain.transform.position;
            
            Debug.Log($"[TerrainToMesh] Generated mesh with {vertices.Length} vertices and {triangles.Length / 3} triangles");
        }
    }
}
