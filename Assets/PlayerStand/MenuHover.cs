using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Handles player highlight on hover. Uses a direct physics raycast that
/// ignores the UI layer, bypassing the GraphicRaycaster blocking on the MenuUI canvas.
/// </summary>
public class MenuHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject HoverLight;
    public GameObject Stand;
    public Material m_PlayerStand_Default;
    public Material m_PlayerStand_Glow;

    private Renderer rend;
    private Collider _col;
    private bool _isHighlighted;

    // LayerMask that excludes the UI layer so the raycast ignores canvas blockers
    private static readonly LayerMask NonUILayerMask = ~(1 << 5); // Layer 5 is "UI"

    private void Start()
    {
        _col = GetComponent<Collider>();

        if (Stand != null)
        {
            rend = Stand.GetComponent<Renderer>();
            if (rend == null) rend = Stand.GetComponentInChildren<Renderer>();
        }

        if (rend != null && m_PlayerStand_Default != null)
            rend.material = m_PlayerStand_Default;

        if (HoverLight != null)
            HoverLight.SetActive(false);
    }

    private void Update()
    {
        if (_col == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2 mousePos;
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null) return;
        mousePos = Mouse.current.position.ReadValue();
#else
        mousePos = Input.mousePosition;
#endif

        // Raycast on non-UI layers only — skips the MenuUI GraphicRaycaster blocker
        Ray ray = cam.ScreenPointToRay(mousePos);
        bool hit = Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, NonUILayerMask, QueryTriggerInteraction.Collide);
        bool over = hit && hitInfo.collider == _col;

        if (over && !_isHighlighted)
            SetHighlight(true);
        else if (!over && _isHighlighted)
            SetHighlight(false);
    }

    // Keep these as a secondary path in case EventSystem is active
    public void OnPointerEnter(PointerEventData eventData) => SetHighlight(true);
    public void OnPointerExit(PointerEventData eventData) => SetHighlight(false);

    private void SetHighlight(bool active)
    {
        _isHighlighted = active;

        if (HoverLight != null)
            HoverLight.SetActive(active);

        if (rend != null)
            rend.material = active ? m_PlayerStand_Glow : m_PlayerStand_Default;
    }
}
