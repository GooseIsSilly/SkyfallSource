using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR.UI
{
    public class ReviveUIBuilder : MonoBehaviour
    {
        private static bool _uiCreated = false;

        private void Start()
        {
            if (_uiCreated)
                return;

            CreateReviveUI();
            _uiCreated = true;
            Debug.Log("[ReviveUIBuilder] Revive UI created successfully");
        }

        private void CreateReviveUI()
        {
            Canvas canvas = FindCanvasOrCreate();

            CreateRevivePromptUI(canvas);
            CreateDownedStateUI(canvas);
        }

        private Canvas FindCanvasOrCreate()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            
            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("ReviveCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                canvasObj.AddComponent<GraphicRaycaster>();
                
                Debug.Log("[ReviveUIBuilder] Created new Canvas");
            }

            return canvas;
        }

        private void CreateRevivePromptUI(Canvas canvas)
        {
            GameObject promptPanel = new GameObject("RevivePrompt");
            promptPanel.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = promptPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.3f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(400, 120);

            Image background = promptPanel.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.7f);

            GameObject promptTextObj = new GameObject("PromptText");
            promptTextObj.transform.SetParent(promptPanel.transform, false);
            
            RectTransform promptTextRect = promptTextObj.AddComponent<RectTransform>();
            promptTextRect.anchorMin = new Vector2(0, 0.5f);
            promptTextRect.anchorMax = new Vector2(1, 1);
            promptTextRect.pivot = new Vector2(0.5f, 0.5f);
            promptTextRect.offsetMin = new Vector2(20, 0);
            promptTextRect.offsetMax = new Vector2(-20, -10);

            TextMeshProUGUI promptText = promptTextObj.AddComponent<TextMeshProUGUI>();
            promptText.text = "Hold [U] to Revive";
            promptText.fontSize = 24;
            promptText.alignment = TextAlignmentOptions.Center;
            promptText.color = Color.white;

            GameObject playerNameTextObj = new GameObject("PlayerNameText");
            playerNameTextObj.transform.SetParent(promptPanel.transform, false);
            
            RectTransform playerNameRect = playerNameTextObj.AddComponent<RectTransform>();
            playerNameRect.anchorMin = new Vector2(0, 0);
            playerNameRect.anchorMax = new Vector2(1, 0.5f);
            playerNameRect.pivot = new Vector2(0.5f, 0.5f);
            playerNameRect.offsetMin = new Vector2(20, 10);
            playerNameRect.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI playerNameText = playerNameTextObj.AddComponent<TextMeshProUGUI>();
            playerNameText.text = "";
            playerNameText.fontSize = 20;
            playerNameText.alignment = TextAlignmentOptions.Center;
            playerNameText.color = new Color(1, 1, 0.5f);

            GameObject progressBarObj = new GameObject("ProgressBar");
            progressBarObj.transform.SetParent(promptPanel.transform, false);
            
            RectTransform progressRect = progressBarObj.AddComponent<RectTransform>();
            progressRect.anchorMin = new Vector2(0.1f, 0.1f);
            progressRect.anchorMax = new Vector2(0.9f, 0.25f);
            progressRect.pivot = new Vector2(0.5f, 0.5f);

            Image progressBg = progressBarObj.AddComponent<Image>();
            progressBg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            GameObject progressFillObj = new GameObject("Fill");
            progressFillObj.transform.SetParent(progressBarObj.transform, false);
            
            RectTransform fillRect = progressFillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image progressFill = progressFillObj.AddComponent<Image>();
            progressFill.color = new Color(0, 1, 0, 0.8f);
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillAmount = 0;

            UIRevivePrompt uiPrompt = promptPanel.AddComponent<UIRevivePrompt>();
            typeof(UIRevivePrompt).GetField("_promptText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiPrompt, promptText);
            typeof(UIRevivePrompt).GetField("_playerNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiPrompt, playerNameText);
            typeof(UIRevivePrompt).GetField("_progressBar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiPrompt, progressFill);

            promptPanel.SetActive(true);
            
            Debug.Log("[ReviveUIBuilder] Created Revive Prompt UI");
        }

        private void CreateDownedStateUI(Canvas canvas)
        {
            GameObject downedPanel = new GameObject("DownedStatePanel");
            downedPanel.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = downedPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.7f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.7f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(500, 120);

            Image background = downedPanel.AddComponent<Image>();
            background.color = new Color(0.5f, 0, 0, 0.7f);

            GameObject downedTextObj = new GameObject("DownedText");
            downedTextObj.transform.SetParent(downedPanel.transform, false);
            
            RectTransform downedTextRect = downedTextObj.AddComponent<RectTransform>();
            downedTextRect.anchorMin = new Vector2(0, 0.5f);
            downedTextRect.anchorMax = new Vector2(1, 1);
            downedTextRect.pivot = new Vector2(0.5f, 0.5f);
            downedTextRect.offsetMin = new Vector2(20, 0);
            downedTextRect.offsetMax = new Vector2(-20, -10);

            TextMeshProUGUI downedText = downedTextObj.AddComponent<TextMeshProUGUI>();
            downedText.text = "YOU ARE DOWNED";
            downedText.fontSize = 32;
            downedText.fontStyle = FontStyles.Bold;
            downedText.alignment = TextAlignmentOptions.Center;
            downedText.color = Color.red;

            GameObject timerTextObj = new GameObject("TimerText");
            timerTextObj.transform.SetParent(downedPanel.transform, false);
            
            RectTransform timerTextRect = timerTextObj.AddComponent<RectTransform>();
            timerTextRect.anchorMin = new Vector2(0, 0);
            timerTextRect.anchorMax = new Vector2(1, 0.5f);
            timerTextRect.pivot = new Vector2(0.5f, 0.5f);
            timerTextRect.offsetMin = new Vector2(20, 10);
            timerTextRect.offsetMax = new Vector2(-20, 0);

            TextMeshProUGUI timerText = timerTextObj.AddComponent<TextMeshProUGUI>();
            timerText.text = "30s";
            timerText.fontSize = 28;
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.color = Color.white;

            UIDownedState uiDownedState = downedPanel.AddComponent<UIDownedState>();
            typeof(UIDownedState).GetField("_downedText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiDownedState, downedText);
            typeof(UIDownedState).GetField("_timerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiDownedState, timerText);

            downedPanel.SetActive(true);
            
            Debug.Log("[ReviveUIBuilder] Created Downed State UI");
        }
    }
}
