using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using TPSBR;
using TPSBR.UI;

namespace TPSBREditor
{
    public class CreateModernShop
    {
        [MenuItem("TPSBR/🎨 Create Modern Shop UI")]
        public static void CreateModernShopUI()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Exit Play Mode", "Please exit Play Mode first!", "OK");
                return;
            }

            var menuScene = EditorSceneManager.OpenScene("Assets/TPSBR/Scenes/Menu.unity", OpenSceneMode.Single);
            
            if (!menuScene.IsValid())
            {
                Debug.LogError("Could not open Menu scene!");
                return;
            }

            Debug.Log("🎨 Creating Modern Shop UI...");

            var rootObjects = menuScene.GetRootGameObjects();
            GameObject menuUIObj = rootObjects.FirstOrDefault(obj => obj.name == "MenuUI");

            if (menuUIObj == null)
            {
                Debug.LogError("MenuUI not found in scene!");
                return;
            }

            Transform existingShopView = menuUIObj.transform.Find("UIShopView");
            GameObject modernShopObj;

            if (existingShopView != null)
            {
                Debug.Log("Found existing UIShopView, creating ModernShop alongside it...");
                modernShopObj = new GameObject("ModernShop");
                modernShopObj.transform.SetParent(menuUIObj.transform);
            }
            else
            {
                modernShopObj = new GameObject("ModernShop");
                modernShopObj.transform.SetParent(menuUIObj.transform);
            }

            RectTransform shopRect = modernShopObj.AddComponent<RectTransform>();
            shopRect.anchorMin = Vector2.zero;
            shopRect.anchorMax = Vector2.one;
            shopRect.offsetMin = Vector2.zero;
            shopRect.offsetMax = Vector2.zero;
            shopRect.localScale = Vector3.one;

            Canvas canvas = modernShopObj.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 10;
            
            modernShopObj.AddComponent<GraphicRaycaster>();

            CanvasGroup canvasGroup = modernShopObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            GameObject backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(modernShopObj.transform);
            RectTransform bgRect = backgroundObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            Image bgImage = backgroundObj.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            GameObject contentObj = new GameObject("Content");
            contentObj.transform.SetParent(modernShopObj.transform);
            RectTransform contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.1f, 0.1f);
            contentRect.anchorMax = new Vector2(0.9f, 0.9f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            GameObject headerObj = new GameObject("Header");
            headerObj.transform.SetParent(contentObj.transform);
            RectTransform headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(1, 1);
            headerRect.sizeDelta = new Vector2(0, 100);
            headerRect.anchoredPosition = new Vector2(0, -50);

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(headerObj.transform);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.sizeDelta = new Vector2(400, 60);
            titleRect.anchoredPosition = new Vector2(200, 0);
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "AGENT SHOP";
            titleText.fontSize = 48;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = Color.white;

            GameObject coinsObj = new GameObject("CoinsDisplay");
            coinsObj.transform.SetParent(headerObj.transform);
            RectTransform coinsRect = coinsObj.AddComponent<RectTransform>();
            coinsRect.anchorMin = new Vector2(1, 0.5f);
            coinsRect.anchorMax = new Vector2(1, 0.5f);
            coinsRect.sizeDelta = new Vector2(300, 60);
            coinsRect.anchoredPosition = new Vector2(-150, 0);
            
            TextMeshProUGUI coinsText = coinsObj.AddComponent<TextMeshProUGUI>();
            coinsText.text = "CloudCoins: 0";
            coinsText.fontSize = 32;
            coinsText.fontStyle = FontStyles.Bold;
            coinsText.alignment = TextAlignmentOptions.Right;
            coinsText.color = new Color(1f, 0.8f, 0.2f);

            GameObject closeButtonObj = new GameObject("CloseButton");
            closeButtonObj.transform.SetParent(headerObj.transform);
            RectTransform closeRect = closeButtonObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1, 1);
            closeRect.anchorMax = new Vector2(1, 1);
            closeRect.sizeDelta = new Vector2(60, 60);
            closeRect.anchoredPosition = new Vector2(-30, -30);
            
            Image closeImg = closeButtonObj.AddComponent<Image>();
            closeImg.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            
            UIButton closeButton = closeButtonObj.AddComponent<UIButton>();
            
            GameObject closeTextObj = new GameObject("Text");
            closeTextObj.transform.SetParent(closeButtonObj.transform);
            RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
            closeTextRect.anchorMin = Vector2.zero;
            closeTextRect.anchorMax = Vector2.one;
            closeTextRect.offsetMin = Vector2.zero;
            closeTextRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI closeText = closeTextObj.AddComponent<TextMeshProUGUI>();
            closeText.text = "✕";
            closeText.fontSize = 36;
            closeText.alignment = TextAlignmentOptions.Center;
            closeText.color = Color.white;

            GameObject scrollViewObj = new GameObject("ScrollView");
            scrollViewObj.transform.SetParent(contentObj.transform);
            RectTransform scrollRect = scrollViewObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.offsetMin = new Vector2(0, 0);
            scrollRect.offsetMax = new Vector2(0, -120);

            Image scrollBg = scrollViewObj.AddComponent<Image>();
            scrollBg.color = new Color(0.15f, 0.15f, 0.2f, 0.5f);

            ScrollRect scroll = scrollViewObj.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;

            GameObject viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollViewObj.transform);
            RectTransform viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = new Vector2(10, 10);
            viewportRect.offsetMax = new Vector2(-10, -10);
            
            viewportObj.AddComponent<RectMask2D>();

            GameObject gridObj = new GameObject("Grid");
            gridObj.transform.SetParent(viewportObj.transform);
            RectTransform gridRect = gridObj.AddComponent<RectTransform>();
            gridRect.anchorMin = new Vector2(0, 1);
            gridRect.anchorMax = new Vector2(1, 1);
            gridRect.pivot = new Vector2(0.5f, 1);
            gridRect.anchoredPosition = Vector2.zero;
            gridRect.sizeDelta = new Vector2(0, 600);

            GridLayoutGroup grid = gridObj.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(280, 400);
            grid.spacing = new Vector2(20, 20);
            grid.padding = new RectOffset(20, 20, 20, 20);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.childAlignment = TextAnchor.UpperCenter;

            ContentSizeFitter fitter = gridObj.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = gridRect;
            scroll.viewport = viewportRect;

            GameObject shopCardPrefab = CreateShopCardPrefab();

            ModernShopManager shopManager = modernShopObj.AddComponent<ModernShopManager>();
            
            SerializedObject so = new SerializedObject(shopManager);
            so.FindProperty("_shopDatabase").objectReferenceValue = AssetDatabase.LoadAssetAtPath<ShopDatabase>("Assets/Scripts/ShopDatabase.asset");
            so.FindProperty("_shopItemsContainer").objectReferenceValue = gridObj.transform;
            so.FindProperty("_shopCardPrefab").objectReferenceValue = shopCardPrefab;
            so.FindProperty("_coinsText").objectReferenceValue = coinsText;
            so.FindProperty("_closeButton").objectReferenceValue = closeButton;
            so.FindProperty("_coinsFormat").stringValue = "CloudCoins: {0}";
            so.ApplyModifiedPropertiesWithoutUndo();

            modernShopObj.SetActive(false);

            EditorUtility.SetDirty(modernShopObj);
            EditorSceneManager.MarkSceneDirty(menuScene);
            EditorSceneManager.SaveScene(menuScene);

            Selection.activeGameObject = modernShopObj;

            Debug.Log("✅ Modern Shop UI created successfully!");
            Debug.Log($"✓ Shop Manager configured");
            Debug.Log($"✓ Card prefab created at: Assets/Prefabs/ModernShopCard.prefab");
            Debug.Log($"✓ Shop starts inactive (UIView will show/hide it)");
            Debug.Log($"");
            Debug.Log("📝 Click SHOP button in main menu to open");
            Debug.Log("📝 Shop cards will auto-populate from ShopDatabase");
            
            EditorUtility.DisplayDialog("✅ Modern Shop Created!", 
                "The modern shop UI has been created!\n\n" +
                "✓ Sleek card-based design\n" +
                "✓ Rarity colors and effects\n" +
                "✓ Auto-populated from ShopDatabase\n\n" +
                "Check the ModernShop GameObject in the scene!", 
                "Awesome!");
        }

        private static GameObject CreateShopCardPrefab()
        {
            GameObject cardObj = new GameObject("ModernShopCard");

            RectTransform cardRect = cardObj.AddComponent<RectTransform>();
            cardRect.sizeDelta = new Vector2(280, 400);

            Image cardBg = cardObj.AddComponent<Image>();
            cardBg.color = new Color(0.2f, 0.2f, 0.25f, 1f);

            GameObject borderObj = new GameObject("RarityBorder");
            borderObj.transform.SetParent(cardObj.transform);
            RectTransform borderRect = borderObj.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            
            Image borderImage = borderObj.AddComponent<Image>();
            borderImage.color = Color.white;

            GameObject glowObj = new GameObject("RarityGlow");
            glowObj.transform.SetParent(cardObj.transform);
            RectTransform glowRect = glowObj.AddComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-10, -10);
            glowRect.offsetMax = new Vector2(10, 10);
            
            Image glowImage = glowObj.AddComponent<Image>();
            glowImage.color = new Color(1, 1, 1, 0.3f);

            GameObject iconObj = new GameObject("CharacterIcon");
            iconObj.transform.SetParent(cardObj.transform);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1);
            iconRect.anchorMax = new Vector2(0.5f, 1);
            iconRect.sizeDelta = new Vector2(220, 220);
            iconRect.anchoredPosition = new Vector2(0, -130);
            
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.color = Color.white;

            GameObject nameObj = new GameObject("CharacterName");
            nameObj.transform.SetParent(cardObj.transform);
            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.5f, 0);
            nameRect.anchorMax = new Vector2(0.5f, 0);
            nameRect.sizeDelta = new Vector2(260, 40);
            nameRect.anchoredPosition = new Vector2(0, 150);
            
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Character Name";
            nameText.fontSize = 24;
            nameText.fontStyle = FontStyles.Bold;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.color = Color.white;

            GameObject rarityTextObj = new GameObject("RarityText");
            rarityTextObj.transform.SetParent(cardObj.transform);
            RectTransform rarityRect = rarityTextObj.AddComponent<RectTransform>();
            rarityRect.anchorMin = new Vector2(0.5f, 0);
            rarityRect.anchorMax = new Vector2(0.5f, 0);
            rarityRect.sizeDelta = new Vector2(260, 30);
            rarityRect.anchoredPosition = new Vector2(0, 115);
            
            TextMeshProUGUI rarityText = rarityTextObj.AddComponent<TextMeshProUGUI>();
            rarityText.text = "COMMON";
            rarityText.fontSize = 18;
            rarityText.fontStyle = FontStyles.Bold;
            rarityText.alignment = TextAlignmentOptions.Center;
            rarityText.color = new Color(0.7f, 0.7f, 0.7f);

            GameObject priceObj = new GameObject("PriceText");
            priceObj.transform.SetParent(cardObj.transform);
            RectTransform priceRect = priceObj.AddComponent<RectTransform>();
            priceRect.anchorMin = new Vector2(0.5f, 0);
            priceRect.anchorMax = new Vector2(0.5f, 0);
            priceRect.sizeDelta = new Vector2(260, 35);
            priceRect.anchoredPosition = new Vector2(0, 75);
            
            TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
            priceText.text = "500";
            priceText.fontSize = 28;
            priceText.fontStyle = FontStyles.Bold;
            priceText.alignment = TextAlignmentOptions.Center;
            priceText.color = new Color(1f, 0.8f, 0.2f);

            GameObject buttonObj = new GameObject("ActionButton");
            buttonObj.transform.SetParent(cardObj.transform);
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0);
            buttonRect.anchorMax = new Vector2(0.5f, 0);
            buttonRect.sizeDelta = new Vector2(240, 50);
            buttonRect.anchoredPosition = new Vector2(0, 35);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.6f, 1f, 1f);
            
            Button button = buttonObj.AddComponent<Button>();

            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform);
            RectTransform buttonTextRect = buttonTextObj.AddComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.offsetMin = Vector2.zero;
            buttonTextRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "BUY";
            buttonText.fontSize = 24;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;

            GameObject selectedObj = new GameObject("SelectedIndicator");
            selectedObj.transform.SetParent(cardObj.transform);
            RectTransform selectedRect = selectedObj.AddComponent<RectTransform>();
            selectedRect.anchorMin = new Vector2(1, 1);
            selectedRect.anchorMax = new Vector2(1, 1);
            selectedRect.sizeDelta = new Vector2(80, 80);
            selectedRect.anchoredPosition = new Vector2(-40, -40);
            
            Image selectedImage = selectedObj.AddComponent<Image>();
            selectedImage.color = new Color(0.2f, 1f, 0.3f, 1f);
            selectedObj.SetActive(false);

            GameObject selectedTextObj = new GameObject("Text");
            selectedTextObj.transform.SetParent(selectedObj.transform);
            RectTransform selectedTextRect = selectedTextObj.AddComponent<RectTransform>();
            selectedTextRect.anchorMin = Vector2.zero;
            selectedTextRect.anchorMax = Vector2.one;
            selectedTextRect.offsetMin = Vector2.zero;
            selectedTextRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI selectedText = selectedTextObj.AddComponent<TextMeshProUGUI>();
            selectedText.text = "✓";
            selectedText.fontSize = 48;
            selectedText.fontStyle = FontStyles.Bold;
            selectedText.alignment = TextAlignmentOptions.Center;
            selectedText.color = Color.white;

            GameObject ownedObj = new GameObject("OwnedBadge");
            ownedObj.transform.SetParent(cardObj.transform);
            RectTransform ownedRect = ownedObj.AddComponent<RectTransform>();
            ownedRect.anchorMin = new Vector2(0, 1);
            ownedRect.anchorMax = new Vector2(0, 1);
            ownedRect.sizeDelta = new Vector2(100, 40);
            ownedRect.anchoredPosition = new Vector2(50, -20);
            
            Image ownedImage = ownedObj.AddComponent<Image>();
            ownedImage.color = new Color(0.2f, 0.8f, 0.3f, 0.9f);
            ownedObj.SetActive(false);

            GameObject ownedTextObj = new GameObject("Text");
            ownedTextObj.transform.SetParent(ownedObj.transform);
            RectTransform ownedTextRect = ownedTextObj.AddComponent<RectTransform>();
            ownedTextRect.anchorMin = Vector2.zero;
            ownedTextRect.anchorMax = Vector2.one;
            ownedTextRect.offsetMin = Vector2.zero;
            ownedTextRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI ownedText = ownedTextObj.AddComponent<TextMeshProUGUI>();
            ownedText.text = "OWNED";
            ownedText.fontSize = 16;
            ownedText.fontStyle = FontStyles.Bold;
            ownedText.alignment = TextAlignmentOptions.Center;
            ownedText.color = Color.white;

            ModernShopCard cardScript = cardObj.AddComponent<ModernShopCard>();
            
            SerializedObject cardSO = new SerializedObject(cardScript);
            cardSO.FindProperty("_characterIcon").objectReferenceValue = iconImage;
            cardSO.FindProperty("_rarityBorder").objectReferenceValue = borderImage;
            cardSO.FindProperty("_rarityGlow").objectReferenceValue = glowImage;
            cardSO.FindProperty("_characterName").objectReferenceValue = nameText;
            cardSO.FindProperty("_rarityText").objectReferenceValue = rarityText;
            cardSO.FindProperty("_priceText").objectReferenceValue = priceText;
            cardSO.FindProperty("_actionButton").objectReferenceValue = button;
            cardSO.FindProperty("_actionButtonText").objectReferenceValue = buttonText;
            cardSO.FindProperty("_selectedIndicator").objectReferenceValue = selectedObj;
            cardSO.FindProperty("_ownedBadge").objectReferenceValue = ownedObj;
            cardSO.FindProperty("_glowIntensity").floatValue = 0.3f;
            cardSO.ApplyModifiedPropertiesWithoutUndo();

            string prefabFolder = "Assets/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            string prefabPath = $"{prefabFolder}/ModernShopCard.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cardObj, prefabPath);
            
            Object.DestroyImmediate(cardObj);

            Debug.Log($"✓ Created shop card prefab: {prefabPath}");
            
            return prefab;
        }
    }
}
