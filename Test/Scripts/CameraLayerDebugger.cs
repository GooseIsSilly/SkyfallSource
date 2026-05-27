using UnityEngine;
using UnityEngine.InputSystem;

public class CameraLayerDebugger : MonoBehaviour
{
    private Camera _camera;
    
    void Start()
    {
        _camera = GetComponent<Camera>();
        if (_camera != null)
        {
            LogCullingMask();
        }
    }
    
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f10Key.wasPressedThisFrame)
        {
            LogCullingMask();
        }
    }
    
    void LogCullingMask()
    {
        Debug.Log("===== CAMERA CULLING MASK DEBUG =====");
        Debug.Log($"Camera: {gameObject.name}");
        Debug.Log($"Culling Mask Value: {_camera.cullingMask}");
        
        string[] layerNames = new string[] 
        {
            "Default", "TransparentFX", "Ignore Raycast", "Target", "Water", 
            "UI", "Agent", "Projectile", "FPV", "AgentKCC", "Interaction", "Pickup", "MapOnly"
        };
        
        Debug.Log("Layers being rendered:");
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);
            if (!string.IsNullOrEmpty(layerName))
            {
                bool isRendered = (_camera.cullingMask & (1 << i)) != 0;
                Debug.Log($"  Layer {i} ({layerName}): {(isRendered ? "RENDERED" : "HIDDEN")}");
            }
        }
        
        Debug.Log("===== END DEBUG =====");
    }
}
