using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraRenderDistanceHelper : MonoBehaviour
{
    [Tooltip("Maximum render distance (far clipping plane)")]
    public float maxRenderDistance = 1000000f;
    
    void Awake()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.farClipPlane = maxRenderDistance;
        }
    }
}
