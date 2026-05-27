using UnityEngine;
using UnityEngine.UI;

namespace TPSBR
{
    public class FortniteMapSetupHelper : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform mapDisplayParent;
        [SerializeField] private SimpleMapSystem mapSystem;

        [Header("Sprites")]
        [SerializeField] private Sprite circleSprite;
        [SerializeField] private Sprite lineSprite;

        [ContextMenu("Setup Fortnite-Style Map")]
        public void SetupFortniteMap()
        {
            if (mapDisplayParent == null)
            {
                Debug.LogError("Map Display Parent not assigned!");
                return;
            }

            CreateDangerZoneOverlay();
            CreateGuidanceLine();
            SetupZoneCircles();

            Debug.Log("Fortnite-style map setup complete!");
        }

        private void CreateDangerZoneOverlay()
        {
            Transform existingOverlay = mapDisplayParent.Find("DangerZoneOverlay");
            GameObject overlayObj;

            if (existingOverlay != null)
            {
                overlayObj = existingOverlay.gameObject;
            }
            else
            {
                overlayObj = new GameObject("DangerZoneOverlay");
                overlayObj.transform.SetParent(mapDisplayParent, false);
                overlayObj.transform.SetSiblingIndex(1);
            }

            RectTransform rect = overlayObj.GetComponent<RectTransform>();
            if (rect == null)
                rect = overlayObj.AddComponent<RectTransform>();

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);

            Image image = overlayObj.GetComponent<Image>();
            if (image == null)
                image = overlayObj.AddComponent<Image>();

            image.color = Color.white;
            image.raycastTarget = false;

            Debug.Log("Danger Zone Overlay created/updated");
        }

        private void CreateGuidanceLine()
        {
            Transform existingLine = mapDisplayParent.Find("GuidanceLine");
            GameObject lineObj;

            if (existingLine != null)
            {
                lineObj = existingLine.gameObject;
            }
            else
            {
                lineObj = new GameObject("GuidanceLine");
                lineObj.transform.SetParent(mapDisplayParent, false);
            }

            RectTransform rect = lineObj.GetComponent<RectTransform>();
            if (rect == null)
                rect = lineObj.AddComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(100f, 3f);

            Image image = lineObj.GetComponent<Image>();
            if (image == null)
                image = lineObj.AddComponent<Image>();

            image.color = new Color(1f, 1f, 1f, 0.8f);
            image.raycastTarget = false;

            if (lineSprite != null)
            {
                image.sprite = lineSprite;
            }

            Debug.Log("Guidance Line created/updated");
        }

        private void SetupZoneCircles()
        {
            SetupCircle("CurrentZoneCircle", new Color(1f, 0f, 0f, 0.3f), 1);
            SetupCircle("NextZoneCircle", new Color(1f, 1f, 1f, 0.8f), 2);
        }

        private void SetupCircle(string circleName, Color color, int siblingIndex)
        {
            Transform existingCircle = mapDisplayParent.Find(circleName);
            GameObject circleObj;

            if (existingCircle != null)
            {
                circleObj = existingCircle.gameObject;
            }
            else
            {
                circleObj = new GameObject(circleName);
                circleObj.transform.SetParent(mapDisplayParent, false);
            }

            circleObj.transform.SetSiblingIndex(siblingIndex);

            RectTransform rect = circleObj.GetComponent<RectTransform>();
            if (rect == null)
                rect = circleObj.AddComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(100f, 100f);

            Image image = circleObj.GetComponent<Image>();
            if (image == null)
                image = circleObj.AddComponent<Image>();

            image.color = color;
            image.raycastTarget = false;

            if (circleSprite != null)
            {
                image.sprite = circleSprite;
                image.type = Image.Type.Simple;
            }
            else
            {
                image.type = Image.Type.Simple;
            }

            Debug.Log($"{circleName} created/updated with color {color}");
        }

        [ContextMenu("Auto-Assign to SimpleMapSystem")]
        public void AutoAssignToMapSystem()
        {
            if (mapSystem == null)
            {
                mapSystem = FindFirstObjectByType<SimpleMapSystem>();
                if (mapSystem == null)
                {
                    Debug.LogError("SimpleMapSystem not found in scene!");
                    return;
                }
            }

            if (mapDisplayParent == null)
            {
                Debug.LogError("Map Display Parent not assigned!");
                return;
            }

            Debug.Log("Auto-assigning UI elements to SimpleMapSystem...");
        }
    }
}
