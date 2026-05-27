using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

namespace TPSBR
{
    public class SeasonEndSetupHelper : EditorWindow
    {
        [MenuItem("Tools/Season End/Create Complete Season End UI")]
        public static void CreateSeasonEndUI()
        {
            GameObject canvasObj = new GameObject("SeasonEndCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
            
            GameObject backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(canvasObj.transform, false);
            Image bgImage = backgroundObj.AddComponent<Image>();
            bgImage.color = new Color(0.05f, 0.05f, 0.05f, 0.98f);
            RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            
            GameObject containerObj = new GameObject("Container");
            containerObj.transform.SetParent(canvasObj.transform, false);
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.sizeDelta = new Vector2(1200, 800);
            
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(containerObj.transform, false);
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "SEASON ENDED";
            titleText.fontSize = 80;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = new Color(1f, 0.2f, 0.2f);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -50);
            titleRect.sizeDelta = new Vector2(1000, 100);
            
            GameObject countdownLabelObj = new GameObject("CountdownLabel");
            countdownLabelObj.transform.SetParent(containerObj.transform, false);
            TextMeshProUGUI countdownLabel = countdownLabelObj.AddComponent<TextMeshProUGUI>();
            countdownLabel.text = "Next Season In:";
            countdownLabel.fontSize = 42;
            countdownLabel.alignment = TextAlignmentOptions.Center;
            countdownLabel.color = new Color(0.7f, 0.7f, 0.7f);
            RectTransform labelRect = countdownLabelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = new Vector2(0, 100);
            labelRect.sizeDelta = new Vector2(800, 60);
            
            GameObject countdownObj = new GameObject("CountdownText");
            countdownObj.transform.SetParent(containerObj.transform, false);
            TextMeshProUGUI countdownText = countdownObj.AddComponent<TextMeshProUGUI>();
            countdownText.text = "00:00:00:00";
            countdownText.fontSize = 110;
            countdownText.fontStyle = FontStyles.Bold;
            countdownText.alignment = TextAlignmentOptions.Center;
            countdownText.color = new Color(1f, 1f, 1f);
            RectTransform countdownRect = countdownObj.GetComponent<RectTransform>();
            countdownRect.anchorMin = new Vector2(0.5f, 0.5f);
            countdownRect.anchorMax = new Vector2(0.5f, 0.5f);
            countdownRect.pivot = new Vector2(0.5f, 0.5f);
            countdownRect.anchoredPosition = new Vector2(0, 10);
            countdownRect.sizeDelta = new Vector2(1100, 140);
            
            GameObject formatObj = new GameObject("FormatLabel");
            formatObj.transform.SetParent(containerObj.transform, false);
            TextMeshProUGUI formatText = formatObj.AddComponent<TextMeshProUGUI>();
            formatText.text = "DAYS : HOURS : MINUTES : SECONDS";
            formatText.fontSize = 24;
            formatText.alignment = TextAlignmentOptions.Center;
            formatText.color = new Color(0.5f, 0.5f, 0.5f);
            RectTransform formatRect = formatObj.GetComponent<RectTransform>();
            formatRect.anchorMin = new Vector2(0.5f, 0.5f);
            formatRect.anchorMax = new Vector2(0.5f, 0.5f);
            formatRect.pivot = new Vector2(0.5f, 0.5f);
            formatRect.anchoredPosition = new Vector2(0, -90);
            formatRect.sizeDelta = new Vector2(1000, 40);
            
            GameObject messageObj = new GameObject("MessageText");
            messageObj.transform.SetParent(containerObj.transform, false);
            TextMeshProUGUI messageText = messageObj.AddComponent<TextMeshProUGUI>();
            messageText.text = "Please update your game to continue playing";
            messageText.fontSize = 38;
            messageText.alignment = TextAlignmentOptions.Center;
            messageText.color = new Color(0.9f, 0.9f, 0.9f);
            RectTransform messageRect = messageObj.GetComponent<RectTransform>();
            messageRect.anchorMin = new Vector2(0.5f, 0.5f);
            messageRect.anchorMax = new Vector2(0.5f, 0.5f);
            messageRect.pivot = new Vector2(0.5f, 0.5f);
            messageRect.anchoredPosition = new Vector2(0, -180);
            messageRect.sizeDelta = new Vector2(1000, 60);
            
            GameObject buttonObj = new GameObject("ExitButton");
            buttonObj.transform.SetParent(containerObj.transform, false);
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0f);
            buttonRect.anchorMax = new Vector2(0.5f, 0f);
            buttonRect.pivot = new Vector2(0.5f, 0f);
            buttonRect.anchoredPosition = new Vector2(0, 40);
            buttonRect.sizeDelta = new Vector2(350, 80);
            
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
            
            Button button = buttonObj.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.8f, 0.2f, 0.2f, 1f);
            colors.highlightedColor = new Color(1f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.6f, 0.1f, 0.1f, 1f);
            button.colors = colors;
            
            GameObject buttonTextObj = new GameObject("Text");
            buttonTextObj.transform.SetParent(buttonObj.transform, false);
            TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
            buttonText.text = "EXIT TO LOBBY";
            buttonText.fontSize = 36;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = Color.white;
            RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
            buttonTextRect.anchorMin = Vector2.zero;
            buttonTextRect.anchorMax = Vector2.one;
            buttonTextRect.sizeDelta = Vector2.zero;
            
            SeasonEndUI seasonEndUI = canvasObj.AddComponent<SeasonEndUI>();
            
            SerializedObject so = new SerializedObject(seasonEndUI);
            so.FindProperty("_canvas").objectReferenceValue = canvas;
            so.FindProperty("_titleText").objectReferenceValue = titleText;
            so.FindProperty("_countdownText").objectReferenceValue = countdownText;
            so.FindProperty("_messageText").objectReferenceValue = messageText;
            so.FindProperty("_exitButton").objectReferenceValue = button;
            so.FindProperty("_exitButtonText").objectReferenceValue = buttonText;
            so.FindProperty("_backgroundImage").objectReferenceValue = bgImage;
            so.ApplyModifiedProperties();
            
            canvas.enabled = false;
            
            PrefabUtility.SaveAsPrefabAsset(canvasObj, "Assets/Prefabs/SeasonEndUI.prefab");
            
            Selection.activeGameObject = canvasObj;
            
            EditorUtility.DisplayDialog("Season End UI Created!", 
                "Season End UI has been created and saved as a prefab!\n\n" +
                "Next Steps:\n" +
                "1. Assign this UI to your SeasonEndController\n" +
                "2. Set the countdown time in SeasonEndController\n" +
                "3. Mark your LiveEvent as 'Is Season End Event'", 
                "OK");
        }
        
        [MenuItem("Tools/Season End/Create Fade Overlay")]
        public static void CreateFadeOverlay()
        {
            GameObject canvasObj = new GameObject("FadeOverlayCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            CanvasGroup canvasGroup = canvasObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
            
            GameObject fadeObj = new GameObject("FadeImage");
            fadeObj.transform.SetParent(canvasObj.transform, false);
            Image fadeImage = fadeObj.AddComponent<Image>();
            fadeImage.color = Color.black;
            RectTransform fadeRect = fadeObj.GetComponent<RectTransform>();
            fadeRect.anchorMin = Vector2.zero;
            fadeRect.anchorMax = Vector2.one;
            fadeRect.sizeDelta = Vector2.zero;
            
            canvasObj.SetActive(false);
            
            PrefabUtility.SaveAsPrefabAsset(canvasObj, "Assets/Prefabs/FadeOverlay.prefab");
            
            Selection.activeGameObject = canvasObj;
            
            EditorUtility.DisplayDialog("Fade Overlay Created!", 
                "Fade Overlay has been created and saved as a prefab!\n\n" +
                "Assign the CanvasGroup component to SeasonEndController's 'Fade Overlay' field.", 
                "OK");
        }
    }
}
