using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace TPSBREditor
{
    public class QuestUIGenerator : EditorWindow
    {
        [MenuItem("TPSBR/Generate Quest UI")]
        public static void GenerateQuestUI()
        {
            if (!EditorUtility.DisplayDialog("Generate Quest UI", 
                "This will create a complete Quest UI in the Menu scene.\n\n" +
                "Make sure the Menu scene is currently open.", 
                "Generate", "Cancel"))
            {
                return;
            }

            GameObject menuUI = GameObject.Find("MenuUI");
            if (menuUI == null)
            {
                EditorUtility.DisplayDialog("Error", 
                    "Could not find MenuUI GameObject. Please open the Menu scene first.", 
                    "OK");
                return;
            }

            CreateQuestView(menuUI.transform);
            CreateQuestButton();
            CreateQuestItemPrefab();
            CreateQuestSystemInitializer();
            AssignAllReferences();

            EditorUtility.DisplayDialog("Success!", 
                "Quest UI has been generated successfully!\n\n" +
                "Components created:\n" +
                "- UIQuestView in MenuUI\n" +
                "- Quests Button in UIMainMenuView\n" +
                "- QuestItemPrefab in /Assets/Prefabs/\n" +
                "- QuestSystemInitializer in Menu scene\n\n" +
                "⚠️ IMPORTANT: Save the scene now (Ctrl+S)!", 
                "OK");

            Debug.Log("[Quest UI Generator] Quest UI created successfully!");
        }

        private static void CreateQuestView(Transform menuUIParent)
        {
            GameObject existingQuestView = GameObject.Find("UIQuestView");
            if (existingQuestView != null)
            {
                if (!EditorUtility.DisplayDialog("UIQuestView Exists", 
                    "UIQuestView already exists. Replace it?", 
                    "Yes", "No"))
                {
                    return;
                }
                DestroyImmediate(existingQuestView);
            }

            GameObject shopView = GameObject.Find("UIShopView");
            if (shopView == null)
            {
                Debug.LogError("Could not find UIShopView to use as template!");
                return;
            }

            GameObject questView = Instantiate(shopView, menuUIParent);
            questView.name = "UIQuestView";
            questView.SetActive(false);

            DestroyImmediate(questView.GetComponent<TPSBR.UI.UIShopView>());

            Transform titleText = questView.transform.Find("Content/TitleText");
            if (titleText != null)
            {
                var tmp = titleText.GetComponent<TextMeshProUGUI>();
                if (tmp != null) tmp.text = "QUESTS";
            }

            Transform cloudCoinsText = questView.transform.Find("Content/CloudCoinsText");
            if (cloudCoinsText != null)
            {
                cloudCoinsText.gameObject.SetActive(false);
            }

            Transform shopItemsList = questView.transform.Find("Content/ShopItemsList");
            if (shopItemsList != null)
            {
                shopItemsList.name = "QuestListContainer";
                
                foreach (Transform child in shopItemsList)
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            var questViewComponent = questView.AddComponent<TPSBR.UI.UIQuestView>();

            Transform closeButton = questView.transform.Find("CloseButton");
            Transform content = questView.transform.Find("Content");
            Transform questListContainer = questView.transform.Find("Content/QuestListContainer");
            Transform title = questView.transform.Find("Content/TitleText");

            var serializedObject = new SerializedObject(questViewComponent);
            
            if (closeButton != null)
            {
                var closeButtonProp = serializedObject.FindProperty("_closeButton");
                if (closeButtonProp != null)
                    closeButtonProp.objectReferenceValue = closeButton.GetComponent<Button>();
            }

            if (questListContainer != null)
            {
                var containerProp = serializedObject.FindProperty("_questListContainer");
                if (containerProp != null)
                    containerProp.objectReferenceValue = questListContainer;
            }

            if (title != null)
            {
                var titleProp = serializedObject.FindProperty("_titleText");
                if (titleProp != null)
                    titleProp.objectReferenceValue = title.GetComponent<TextMeshProUGUI>();
            }

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(questView);
            Debug.Log("[Quest UI Generator] UIQuestView created!");
        }

        private static void CreateQuestButton()
        {
            GameObject mainMenuView = GameObject.Find("UIMainMenuView");
            if (mainMenuView == null)
            {
                Debug.LogWarning("Could not find UIMainMenuView to add Quests button");
                return;
            }

            GameObject existingButton = mainMenuView.transform.Find("QuestsButton")?.gameObject;
            if (existingButton != null)
            {
                DestroyImmediate(existingButton);
            }

            GameObject shopButton = mainMenuView.transform.Find("ShopButton")?.gameObject;
            if (shopButton == null)
            {
                Debug.LogWarning("Could not find ShopButton to use as template");
                return;
            }

            GameObject questsButton = Instantiate(shopButton, mainMenuView.transform);
            questsButton.name = "QuestsButton";

            int shopIndex = shopButton.transform.GetSiblingIndex();
            questsButton.transform.SetSiblingIndex(shopIndex + 1);

            Transform labelTransform = questsButton.transform.Find("BG/Label");
            if (labelTransform != null)
            {
                var label = labelTransform.GetComponent<TextMeshProUGUI>();
                if (label != null) label.text = "QUESTS";
            }

            var button = questsButton.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }

            var mainMenuViewComponent = mainMenuView.GetComponent<TPSBR.UI.UIMainMenuView>();
            if (mainMenuViewComponent != null)
            {
                var serializedObject = new SerializedObject(mainMenuViewComponent);
                var questsButtonProp = serializedObject.FindProperty("_questsButton");
                if (questsButtonProp != null)
                {
                    var uiButton = questsButton.GetComponent<TPSBR.UI.UIButton>();
                    if (uiButton != null)
                    {
                        questsButtonProp.objectReferenceValue = uiButton;
                        serializedObject.ApplyModifiedProperties();
                        Debug.Log("[Quest UI Generator] Quests button reference assigned to UIMainMenuView!");
                    }
                }
            }

            EditorUtility.SetDirty(mainMenuView);
            Debug.Log("[Quest UI Generator] Quests button created!");
        }

        private static void CreateQuestItemPrefab()
        {
            string prefabPath = "Assets/Prefabs/QuestItemPrefab.prefab";

            if (System.IO.File.Exists(prefabPath))
            {
                if (!EditorUtility.DisplayDialog("Prefab Exists", 
                    "QuestItemPrefab.prefab already exists. Replace it?", 
                    "Yes", "No"))
                {
                    return;
                }
            }

            if (!System.IO.Directory.Exists("Assets/Prefabs"))
            {
                System.IO.Directory.CreateDirectory("Assets/Prefabs");
            }

            GameObject questItemPrefab = new GameObject("QuestItemPrefab");

            RectTransform rt = questItemPrefab.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(600, 120);

            Image bg = questItemPrefab.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

            GameObject nameTextObj = new GameObject("QuestNameText");
            nameTextObj.transform.SetParent(questItemPrefab.transform);
            RectTransform nameRT = nameTextObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.7f);
            nameRT.anchorMax = new Vector2(0.7f, 1);
            nameRT.offsetMin = new Vector2(15, 0);
            nameRT.offsetMax = new Vector2(-15, -5);
            var nameText = nameTextObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Quest Name";
            nameText.fontSize = 20;
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.white;

            GameObject descTextObj = new GameObject("QuestDescriptionText");
            descTextObj.transform.SetParent(questItemPrefab.transform);
            RectTransform descRT = descTextObj.AddComponent<RectTransform>();
            descRT.anchorMin = new Vector2(0, 0.35f);
            descRT.anchorMax = new Vector2(0.7f, 0.7f);
            descRT.offsetMin = new Vector2(15, 0);
            descRT.offsetMax = new Vector2(-15, 0);
            var descText = descTextObj.AddComponent<TextMeshProUGUI>();
            descText.text = "Quest Description";
            descText.fontSize = 14;
            descText.color = new Color(0.8f, 0.8f, 0.8f, 1f);

            GameObject progressBarObj = new GameObject("ProgressBar");
            progressBarObj.transform.SetParent(questItemPrefab.transform);
            RectTransform progRT = progressBarObj.AddComponent<RectTransform>();
            progRT.anchorMin = new Vector2(0, 0.1f);
            progRT.anchorMax = new Vector2(0.6f, 0.3f);
            progRT.offsetMin = new Vector2(15, 0);
            progRT.offsetMax = new Vector2(-15, 0);
            
            Slider progressBar = progressBarObj.AddComponent<Slider>();
            progressBar.minValue = 0;
            progressBar.maxValue = 100;
            progressBar.value = 0;
            progressBar.interactable = false;

            GameObject bgSlider = new GameObject("Background");
            bgSlider.transform.SetParent(progressBarObj.transform);
            RectTransform bgSliderRT = bgSlider.AddComponent<RectTransform>();
            bgSliderRT.anchorMin = Vector2.zero;
            bgSliderRT.anchorMax = Vector2.one;
            bgSliderRT.offsetMin = Vector2.zero;
            bgSliderRT.offsetMax = Vector2.zero;
            Image bgSliderImg = bgSlider.AddComponent<Image>();
            bgSliderImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(progressBarObj.transform);
            RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.offsetMin = Vector2.zero;
            fillAreaRT.offsetMax = Vector2.zero;

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform);
            RectTransform fillRT = fill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = new Color(0.3f, 0.6f, 1f, 1f);

            progressBar.targetGraphic = fillImg;
            progressBar.fillRect = fillRT;

            GameObject progressTextObj = new GameObject("ProgressText");
            progressTextObj.transform.SetParent(questItemPrefab.transform);
            RectTransform progTextRT = progressTextObj.AddComponent<RectTransform>();
            progTextRT.anchorMin = new Vector2(0.6f, 0.1f);
            progTextRT.anchorMax = new Vector2(0.75f, 0.3f);
            progTextRT.offsetMin = new Vector2(5, 0);
            progTextRT.offsetMax = new Vector2(-5, 0);
            var progressText = progressTextObj.AddComponent<TextMeshProUGUI>();
            progressText.text = "0/100";
            progressText.fontSize = 14;
            progressText.color = Color.white;
            progressText.alignment = TextAlignmentOptions.Center;

            GameObject rewardTextObj = new GameObject("RewardText");
            rewardTextObj.transform.SetParent(questItemPrefab.transform);
            RectTransform rewardRT = rewardTextObj.AddComponent<RectTransform>();
            rewardRT.anchorMin = new Vector2(0.75f, 0.65f);
            rewardRT.anchorMax = new Vector2(1, 0.95f);
            rewardRT.offsetMin = new Vector2(5, 0);
            rewardRT.offsetMax = new Vector2(-10, 0);
            var rewardText = rewardTextObj.AddComponent<TextMeshProUGUI>();
            rewardText.text = "💰 100";
            rewardText.fontSize = 18;
            rewardText.fontStyle = FontStyles.Bold;
            rewardText.color = new Color(1f, 0.85f, 0f, 1f);
            rewardText.alignment = TextAlignmentOptions.Center;

            GameObject claimButtonObj = new GameObject("ClaimButton");
            claimButtonObj.transform.SetParent(questItemPrefab.transform);
            RectTransform claimRT = claimButtonObj.AddComponent<RectTransform>();
            claimRT.anchorMin = new Vector2(0.75f, 0.1f);
            claimRT.anchorMax = new Vector2(0.98f, 0.55f);
            claimRT.offsetMin = new Vector2(5, 5);
            claimRT.offsetMax = new Vector2(-5, -5);
            
            Image claimBg = claimButtonObj.AddComponent<Image>();
            claimBg.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            
            Button claimButton = claimButtonObj.AddComponent<Button>();
            claimButton.targetGraphic = claimBg;

            GameObject claimTextObj = new GameObject("Text");
            claimTextObj.transform.SetParent(claimButtonObj.transform);
            RectTransform claimTextRT = claimTextObj.AddComponent<RectTransform>();
            claimTextRT.anchorMin = Vector2.zero;
            claimTextRT.anchorMax = Vector2.one;
            claimTextRT.offsetMin = Vector2.zero;
            claimTextRT.offsetMax = Vector2.zero;
            var claimText = claimTextObj.AddComponent<TextMeshProUGUI>();
            claimText.text = "CLAIM";
            claimText.fontSize = 16;
            claimText.fontStyle = FontStyles.Bold;
            claimText.color = Color.white;
            claimText.alignment = TextAlignmentOptions.Center;

            var questItemComponent = questItemPrefab.AddComponent<TPSBR.UI.UIQuestItem>();
            var serializedObject = new SerializedObject(questItemComponent);
            
            serializedObject.FindProperty("_questNameText").objectReferenceValue = nameText;
            serializedObject.FindProperty("_questDescriptionText").objectReferenceValue = descText;
            serializedObject.FindProperty("_progressBar").objectReferenceValue = progressBar;
            serializedObject.FindProperty("_progressText").objectReferenceValue = progressText;
            serializedObject.FindProperty("_rewardText").objectReferenceValue = rewardText;
            serializedObject.FindProperty("_claimButton").objectReferenceValue = claimButton;
            
            serializedObject.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(questItemPrefab, prefabPath);
            DestroyImmediate(questItemPrefab);

            GameObject questView = GameObject.Find("UIQuestView");
            if (questView != null)
            {
                var questViewComponent = questView.GetComponent<TPSBR.UI.UIQuestView>();
                if (questViewComponent != null)
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    var so = new SerializedObject(questViewComponent);
                    so.FindProperty("_questItemPrefab").objectReferenceValue = prefab;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(questView);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("[Quest UI Generator] QuestItemPrefab created at: " + prefabPath);
        }

        private static void CreateQuestSystemInitializer()
        {
            GameObject existing = GameObject.Find("QuestSystemInitializer");
            if (existing != null)
            {
                Debug.Log("[Quest UI Generator] QuestSystemInitializer already exists - skipping creation");
                return;
            }

            GameObject initializer = new GameObject("QuestSystemInitializer");
            initializer.AddComponent<TPSBR.QuestSystemInitializer>();

            EditorUtility.SetDirty(initializer);
            Debug.Log("[Quest UI Generator] QuestSystemInitializer created! Quests will now initialize on startup.");
        }

        private static void AssignAllReferences()
        {
            GameObject questView = GameObject.Find("UIQuestView");
            if (questView == null)
            {
                Debug.LogWarning("[Quest UI Generator] UIQuestView not found - skipping reference assignment");
                return;
            }

            var questViewComponent = questView.GetComponent<TPSBR.UI.UIQuestView>();
            if (questViewComponent == null)
            {
                Debug.LogWarning("[Quest UI Generator] UIQuestView component not found");
                return;
            }

            var so = new SerializedObject(questViewComponent);

            Transform closeButton = questView.transform.Find("CloseButton");
            if (closeButton != null)
            {
                var button = closeButton.GetComponent<Button>();
                if (button != null)
                {
                    so.FindProperty("_closeButton").objectReferenceValue = button;
                    Debug.Log("[Quest UI Generator] Close button assigned");
                }
            }

            Transform titleText = questView.transform.Find("Content/TitleText");
            if (titleText != null)
            {
                var text = titleText.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    so.FindProperty("_titleText").objectReferenceValue = text;
                    Debug.Log("[Quest UI Generator] Title text assigned");
                }
            }

            Transform questListContainer = questView.transform.Find("Content/QuestListContainer");
            if (questListContainer != null)
            {
                so.FindProperty("_questListContainer").objectReferenceValue = questListContainer;
                Debug.Log("[Quest UI Generator] Quest list container assigned");
            }

            string prefabPath = "Assets/Prefabs/QuestItemPrefab.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null)
            {
                so.FindProperty("_questItemPrefab").objectReferenceValue = prefab;
                Debug.Log("[Quest UI Generator] Quest item prefab assigned");
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(questView);
            Debug.Log("[Quest UI Generator] All references assigned successfully!");
        }
    }
}
