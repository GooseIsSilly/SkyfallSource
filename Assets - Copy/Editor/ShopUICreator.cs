using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

namespace TPSBR.UI.Editor
{
    public class ShopUICreator : EditorWindow
    {
        [MenuItem("TPSBR/Create Shop UI")]
        public static void ShowWindow()
        {
            GetWindow<ShopUICreator>("Shop UI Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Shop UI Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool helps you create the basic shop UI structure.\n\n" +
                "IMPORTANT: Make sure you're in the Menu scene before using these buttons!",
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("Create UIShopItem Widget", GUILayout.Height(40)))
            {
                CreateShopItemWidget();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Create UIShopView Panel", GUILayout.Height(40)))
            {
                CreateShopViewPanel();
            }

            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "After creating the UI:\n" +
                "1. Configure the UIShopView component fields\n" +
                "2. Save as prefabs in /Assets/TPSBR/UI/Prefabs/\n" +
                "3. Link UIShopItem prefab to UIShopView's UIList\n" +
                "4. Add a shop button to your main menu",
                MessageType.Warning
            );
        }

        private static void CreateShopItemWidget()
        {
            GameObject shopItem = new GameObject("UIShopItem", typeof(RectTransform));
            
            RectTransform rt = shopItem.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(720, 120);

            Image bg = shopItem.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            HorizontalLayoutGroup hlg = shopItem.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(10, 10, 10, 10);
            hlg.spacing = 15;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlHeight = true;
            hlg.childControlWidth = false;
            hlg.childForceExpandHeight = true;

            GameObject agentIcon = new GameObject("AgentIcon", typeof(RectTransform));
            agentIcon.transform.SetParent(shopItem.transform);
            Image iconImg = agentIcon.AddComponent<Image>();
            iconImg.color = Color.white;
            LayoutElement iconLE = agentIcon.AddComponent<LayoutElement>();
            iconLE.minWidth = 100;
            iconLE.minHeight = 100;
            iconLE.preferredWidth = 100;
            iconLE.preferredHeight = 100;

            GameObject infoContainer = new GameObject("InfoContainer", typeof(RectTransform));
            infoContainer.transform.SetParent(shopItem.transform);
            VerticalLayoutGroup vlg = infoContainer.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 5;
            vlg.childAlignment = TextAnchor.MiddleLeft;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            LayoutElement infoLE = infoContainer.AddComponent<LayoutElement>();
            infoLE.flexibleWidth = 1;

            GameObject agentName = new GameObject("AgentName", typeof(RectTransform));
            agentName.transform.SetParent(infoContainer.transform);
            TextMeshProUGUI nameTMP = agentName.AddComponent<TextMeshProUGUI>();
            nameTMP.text = "Agent Name";
            nameTMP.fontSize = 28;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.alignment = TextAlignmentOptions.Left;

            GameObject costText = new GameObject("CostText", typeof(RectTransform));
            costText.transform.SetParent(infoContainer.transform);
            TextMeshProUGUI costTMP = costText.AddComponent<TextMeshProUGUI>();
            costTMP.text = "500 CloudCoins";
            costTMP.fontSize = 24;
            costTMP.color = new Color(1, 0.9f, 0, 1);
            costTMP.alignment = TextAlignmentOptions.Left;

            GameObject purchaseButton = new GameObject("PurchaseButton", typeof(RectTransform));
            purchaseButton.transform.SetParent(shopItem.transform);
            Image btnImg = purchaseButton.AddComponent<Image>();
            btnImg.color = new Color(0.1f, 0.5f, 0.8f, 1);
            UIButton btn = purchaseButton.AddComponent<UIButton>();
            LayoutElement btnLE = purchaseButton.AddComponent<LayoutElement>();
            btnLE.minWidth = 150;
            btnLE.preferredWidth = 150;
            btnLE.minHeight = 60;

            GameObject buttonText = new GameObject("ButtonText", typeof(RectTransform));
            buttonText.transform.SetParent(purchaseButton.transform);
            RectTransform btnTextRT = buttonText.GetComponent<RectTransform>();
            btnTextRT.anchorMin = Vector2.zero;
            btnTextRT.anchorMax = Vector2.one;
            btnTextRT.sizeDelta = Vector2.zero;
            TextMeshProUGUI btnTMP = buttonText.AddComponent<TextMeshProUGUI>();
            btnTMP.text = "BUY";
            btnTMP.fontSize = 24;
            btnTMP.fontStyle = FontStyles.Bold;
            btnTMP.alignment = TextAlignmentOptions.Center;

            shopItem.AddComponent<UIShopItem>();

            Selection.activeGameObject = shopItem;
            Debug.Log("UIShopItem widget created! Now:\n1. Configure the UIShopItem component fields\n2. Save as prefab to Assets/TPSBR/UI/Prefabs/Widgets/");
        }

        private static void CreateShopViewPanel()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                EditorUtility.DisplayDialog("Error", "No Canvas found in scene. Please open the Menu scene first.", "OK");
                return;
            }

            GameObject shopView = new GameObject("UIShopView", typeof(RectTransform));
            shopView.transform.SetParent(canvas.transform, false);
            
            RectTransform rt = shopView.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;

            shopView.AddComponent<CanvasGroup>();
            Canvas subCanvas = shopView.AddComponent<Canvas>();
            subCanvas.overrideSorting = false;
            shopView.AddComponent<GraphicRaycaster>();

            GameObject background = new GameObject("Background", typeof(RectTransform));
            background.transform.SetParent(shopView.transform, false);
            RectTransform bgRT = background.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            Image bgImg = background.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(shopView.transform, false);
            RectTransform contentRT = content.GetComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0.5f, 0.5f);
            contentRT.anchorMax = new Vector2(0.5f, 0.5f);
            contentRT.sizeDelta = new Vector2(800, 600);
            contentRT.anchoredPosition = Vector2.zero;

            GameObject titleText = new GameObject("TitleText", typeof(RectTransform));
            titleText.transform.SetParent(content.transform, false);
            RectTransform titleRT = titleText.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(1, 1);
            titleRT.sizeDelta = new Vector2(-40, 50);
            titleRT.anchoredPosition = new Vector2(0, -25);
            TextMeshProUGUI titleTMP = titleText.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "AGENT SHOP";
            titleTMP.fontSize = 48;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;

            GameObject cloudCoinsText = new GameObject("CloudCoinsText", typeof(RectTransform));
            cloudCoinsText.transform.SetParent(content.transform, false);
            RectTransform coinsRT = cloudCoinsText.GetComponent<RectTransform>();
            coinsRT.anchorMin = new Vector2(0, 1);
            coinsRT.anchorMax = new Vector2(1, 1);
            coinsRT.sizeDelta = new Vector2(-40, 40);
            coinsRT.anchoredPosition = new Vector2(0, -80);
            TextMeshProUGUI coinsTMP = cloudCoinsText.AddComponent<TextMeshProUGUI>();
            coinsTMP.text = "CloudCoins: 0";
            coinsTMP.fontSize = 36;
            coinsTMP.fontStyle = FontStyles.Bold;
            coinsTMP.alignment = TextAlignmentOptions.Center;

            GameObject scrollView = new GameObject("ShopItemsList", typeof(RectTransform));
            scrollView.transform.SetParent(content.transform, false);
            RectTransform scrollRT = scrollView.GetComponent<RectTransform>();
            scrollRT.anchorMin = Vector2.zero;
            scrollRT.anchorMax = Vector2.one;
            scrollRT.offsetMin = new Vector2(20, 20);
            scrollRT.offsetMax = new Vector2(-20, -140);
            scrollView.AddComponent<UIList>();
            VerticalLayoutGroup vlg = scrollView.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            scrollView.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            shopView.AddComponent<UIShopView>();

            Selection.activeGameObject = shopView;
            Debug.Log("UIShopView created! Now:\n1. Configure the UIShopView component (link ShopItemsList and CloudCoinsText)\n2. Link UIShopItem prefab to ShopItemsList's UIList component\n3. Save as prefab to Assets/TPSBR/UI/Prefabs/MenuViews/");
        }
    }
}
