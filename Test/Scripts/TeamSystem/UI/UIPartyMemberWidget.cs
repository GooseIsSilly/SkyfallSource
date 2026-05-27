using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR.UI
{
    public class UIPartyMemberWidget : MonoBehaviour
    {
        private string _userID;
        private Action<string> _onKick;
        
        private Image _background;
        private TextMeshProUGUI _nicknameText;
        private GameObject _leaderIcon;
        private UIButton _kickButton;

        public void Initialize(string userID, string nickname, bool isLeader, bool isLocalPlayer, float height, Action<string> onKick)
        {
            _userID = userID;
            _onKick = onKick;
            
            BuildUI(height, isLocalPlayer);
            UpdateDisplay(nickname, isLeader);
        }

        private void BuildUI(float height, bool isLocalPlayer)
        {
            var rect = GetComponent<RectTransform>();
            
            _background = CreateBackground(rect);
            _nicknameText = CreateNicknameText(rect);
            _leaderIcon = CreateLeaderIcon(rect);
            
            if (!isLocalPlayer)
            {
                _kickButton = CreateKickButton(rect);
            }
        }

        private Image CreateBackground(RectTransform parent)
        {
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(parent, false);
            
            var rect = bgObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var img = bgObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.25f, 0.9f);
            
            return img;
        }

        private TextMeshProUGUI CreateNicknameText(RectTransform parent)
        {
            var textObj = new GameObject("Nickname");
            textObj.transform.SetParent(parent, false);
            
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0);
            rect.anchorMax = new Vector2(0.7f, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 20;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.color = Color.white;
            
            return text;
        }

        private GameObject CreateLeaderIcon(RectTransform parent)
        {
            var iconObj = new GameObject("LeaderIcon");
            iconObj.transform.SetParent(parent, false);
            
            var rect = iconObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0.5f);
            rect.anchorMax = new Vector2(0.05f, 0.5f);
            rect.sizeDelta = new Vector2(25, 25);
            rect.anchoredPosition = Vector2.zero;
            
            var text = iconObj.AddComponent<TextMeshProUGUI>();
            text.text = "★";
            text.fontSize = 22;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.yellow;
            
            iconObj.SetActive(false);
            return iconObj;
        }

        private UIButton CreateKickButton(RectTransform parent)
        {
            var btnObj = new GameObject("KickButton");
            btnObj.transform.SetParent(parent, false);
            
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.8f, 0.5f);
            rect.anchorMax = new Vector2(0.8f, 0.5f);
            rect.sizeDelta = new Vector2(70, 35);
            rect.anchoredPosition = Vector2.zero;
            
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.6f, 0.2f, 0.2f);
            
            var button = btnObj.AddComponent<UIButton>();
            button.onClick.AddListener(OnKickClick);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Kick";
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            
            return button;
        }

        private void UpdateDisplay(string nickname, bool isLeader)
        {
            if (_nicknameText != null)
            {
                _nicknameText.text = nickname;
            }
            
            if (_leaderIcon != null)
            {
                _leaderIcon.SetActive(isLeader);
            }
        }

        private void OnKickClick()
        {
            _onKick?.Invoke(_userID);
        }
    }
}
