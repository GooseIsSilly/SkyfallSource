using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using TPSBR.UI;

public class FortniteLobbySetupWizard : EditorWindow
{
    private Canvas parentCanvas;
    
    [MenuItem("Tools/Fortnite Lobby/Setup Wizard")]
    public static void ShowWindow()
    {
        GetWindow<FortniteLobbySetupWizard>("Fortnite Lobby Setup");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Fortnite Lobby Setup Wizard", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This wizard will help you create a Fortnite-style lobby UI.\n\n" +
            "It will create:\n" +
            "- Top navigation bar with icon buttons\n" +
            "- Large Play button on the right\n" +
            "- Player info panel on the left\n" +
            "- All necessary components and references",
            MessageType.Info);
        
        GUILayout.Space(10);
        
        parentCanvas = EditorGUILayout.ObjectField("Parent Canvas", parentCanvas, typeof(Canvas), true) as Canvas;
        
        GUILayout.Space(10);
        
        GUI.enabled = parentCanvas != null;
        
        if (GUILayout.Button("Create Fortnite Lobby UI", GUILayout.Height(40)))
        {
            CreateFortniteLobbyUI();
        }
        
        GUI.enabled = true;
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Open Setup Guide"))
        {
            string guidePath = "Assets/Scripts/FORTNITE_LOBBY_SETUP_GUIDE.md";
            var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(guidePath);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }
            else
            {
                EditorUtility.DisplayDialog("Guide Not Found", 
                    "Could not find the setup guide at:\n" + guidePath, "OK");
            }
        }
    }
    
    private void CreateFortniteLobbyUI()
    {
        if (parentCanvas == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a parent Canvas first!", "OK");
            return;
        }
        
        GameObject lobbyRoot = new GameObject("UIFortniteLobbyView");
        lobbyRoot.transform.SetParent(parentCanvas.transform, false);
        
        RectTransform rootRect = lobbyRoot.AddComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;
        
        CanvasGroup canvasGroup = lobbyRoot.AddComponent<CanvasGroup>();
        Canvas canvas = lobbyRoot.AddComponent<Canvas>();
        lobbyRoot.AddComponent<GraphicRaycaster>();
        
        UIFortniteLobbyView lobbyView = lobbyRoot.AddComponent<UIFortniteLobbyView>();
        
        GameObject topBar = CreateTopNavigationBar(lobbyRoot);
        GameObject playButton = CreatePlayButton(lobbyRoot);
        GameObject playerInfo = CreatePlayerInfoPanel(lobbyRoot);
        
        ConnectComponents(lobbyView, topBar, playButton, playerInfo);
        
        Selection.activeGameObject = lobbyRoot;
        EditorUtility.SetDirty(lobbyRoot);
        
        EditorUtility.DisplayDialog("Success!", 
            "Fortnite Lobby UI created successfully!\n\n" +
            "Next steps:\n" +
            "1. Customize button icons and colors\n" +
            "2. Adjust positions and sizes\n" +
            "3. Connect to your UI system\n" +
            "4. Test in Play Mode", "OK");
    }
    
    private GameObject CreateTopNavigationBar(GameObject parent)
    {
        GameObject topBar = new GameObject("TopNavigationBar");
        topBar.transform.SetParent(parent.transform, false);
        
        RectTransform rect = topBar.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(-40, 80);
        rect.anchoredPosition = new Vector2(0, -20);
        
        Image bg = topBar.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        HorizontalLayoutGroup layout = topBar.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 20;
        layout.padding = new RectOffset(20, 20, 10, 10);
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        
        string[] buttonNames = { "ShopButton", "QuestButton", "LockerButton", "BattlePassButton", "SettingsButton" };
        string[] buttonLabels = { "SHOP", "QUEST", "LOCKER", "PASS", "SETTINGS" };
        
        for (int i = 0; i < buttonNames.Length; i++)
        {
            CreateTopBarButton(topBar, buttonNames[i], buttonLabels[i]);
        }
        
        return topBar;
    }
    
    private GameObject CreateTopBarButton(GameObject parent, string name, string label)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent.transform, false);
        
        RectTransform rect = button.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(70, 60);
        
        Image image = button.AddComponent<Image>();
        image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        Button btn = button.AddComponent<Button>();
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = label;
        text.fontSize = 10;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        
        return button;
    }
    
    private GameObject CreatePlayButton(GameObject parent)
    {
        GameObject playButton = new GameObject("PlayButton");
        playButton.transform.SetParent(parent.transform, false);
        
        RectTransform rect = playButton.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.sizeDelta = new Vector2(300, 90);
        rect.anchoredPosition = new Vector2(-50, 50);
        
        Image image = playButton.AddComponent<Image>();
        image.color = new Color(1f, 0.9f, 0f, 1f);
        
        Button btn = playButton.AddComponent<Button>();
        
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(1f, 0.9f, 0f, 1f);
        colors.highlightedColor = new Color(1f, 1f, 0.2f, 1f);
        colors.pressedColor = new Color(0.8f, 0.7f, 0f, 1f);
        btn.colors = colors;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(playButton.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "PLAY";
        text.fontSize = 48;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;
        
        return playButton;
    }
    
    private GameObject CreatePlayerInfoPanel(GameObject parent)
    {
        GameObject playerInfo = new GameObject("PlayerInfoPanel");
        playerInfo.transform.SetParent(parent.transform, false);
        
        RectTransform rect = playerInfo.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 1);
        rect.sizeDelta = new Vector2(300, 150);
        rect.anchoredPosition = new Vector2(20, -120);
        
        Image bg = playerInfo.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        
        GameObject nameText = new GameObject("PlayerNameText");
        nameText.transform.SetParent(playerInfo.transform, false);
        
        RectTransform nameRect = nameText.AddComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.sizeDelta = new Vector2(-20, 40);
        nameRect.anchoredPosition = new Vector2(0, -10);
        
        TextMeshProUGUI name = nameText.AddComponent<TextMeshProUGUI>();
        name.text = "Player Name";
        name.fontSize = 24;
        name.fontStyle = FontStyles.Bold;
        name.alignment = TextAlignmentOptions.Center;
        name.color = Color.white;
        
        GameObject levelText = new GameObject("LevelText");
        levelText.transform.SetParent(playerInfo.transform, false);
        
        RectTransform levelRect = levelText.AddComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0, 1);
        levelRect.anchorMax = new Vector2(1, 1);
        levelRect.pivot = new Vector2(0.5f, 1);
        levelRect.sizeDelta = new Vector2(-20, 30);
        levelRect.anchoredPosition = new Vector2(0, -60);
        
        TextMeshProUGUI level = levelText.AddComponent<TextMeshProUGUI>();
        level.text = "Level 1";
        level.fontSize = 18;
        level.alignment = TextAlignmentOptions.Center;
        level.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        
        return playerInfo;
    }
    
    private void ConnectComponents(UIFortniteLobbyView lobbyView, GameObject topBar, GameObject playButton, GameObject playerInfo)
    {
        SerializedObject so = new SerializedObject(lobbyView);
        
        so.FindProperty("_shopButton").objectReferenceValue = topBar.transform.Find("ShopButton")?.GetComponent<Button>();
        so.FindProperty("_questButton").objectReferenceValue = topBar.transform.Find("QuestButton")?.GetComponent<Button>();
        so.FindProperty("_lockerButton").objectReferenceValue = topBar.transform.Find("LockerButton")?.GetComponent<Button>();
        so.FindProperty("_battlePassButton").objectReferenceValue = topBar.transform.Find("BattlePassButton")?.GetComponent<Button>();
        so.FindProperty("_settingsButton").objectReferenceValue = topBar.transform.Find("SettingsButton")?.GetComponent<Button>();
        
        so.FindProperty("_playButton").objectReferenceValue = playButton.GetComponent<Button>();
        so.FindProperty("_playButtonText").objectReferenceValue = playButton.transform.Find("Text")?.GetComponent<TextMeshProUGUI>();
        
        so.FindProperty("_playerNameText").objectReferenceValue = playerInfo.transform.Find("PlayerNameText")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_levelText").objectReferenceValue = playerInfo.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
        
        so.FindProperty("_searchTimeout").floatValue = 5f;
        so.FindProperty("_maxPlayers").intValue = 100;
        so.FindProperty("_defaultMapScenePath").stringValue = "TPSBR/Scenes/Game";
        
        so.ApplyModifiedProperties();
    }
}
