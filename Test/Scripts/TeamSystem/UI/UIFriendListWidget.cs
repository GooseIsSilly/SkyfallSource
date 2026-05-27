using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TPSBR.UI
{
    public class UIFriendListWidget : MonoBehaviour
    {
        private FriendData _friendData;
        private Action<string> _onInvite;
        private Action<string> _onRemove;
        
        private Image _background;
        private TextMeshProUGUI _nicknameText;
        private Image _statusIcon;
        private UIButton _inviteButton;
        private UIButton _removeButton;

        public void Initialize(FriendData friendData, float height, Action<string> onInvite, Action<string> onRemove)
        {
            _friendData = friendData;
            _onInvite = onInvite;
            _onRemove = onRemove;
            
            BuildUI(height);
            UpdateDisplay();
        }

        private void BuildUI(float height)
        {
            var rect = GetComponent<RectTransform>();
            
            _background = CreateBackground(rect);
            _nicknameText = CreateNicknameText(rect);
            _statusIcon = CreateStatusIcon(rect);
            _inviteButton = CreateInviteButton(rect);
            _removeButton = CreateRemoveButton(rect);
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
            img.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            
            return img;
        }

        private TextMeshProUGUI CreateNicknameText(RectTransform parent)
        {
            var textObj = new GameObject("Nickname");
            textObj.transform.SetParent(parent, false);
            
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.05f, 0);
            rect.anchorMax = new Vector2(0.5f, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.color = Color.white;
            
            return text;
        }

        private Image CreateStatusIcon(RectTransform parent)
        {
            var iconObj = new GameObject("StatusIcon");
            iconObj.transform.SetParent(parent, false);
            
            var rect = iconObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.52f, 0.5f);
            rect.anchorMax = new Vector2(0.52f, 0.5f);
            rect.sizeDelta = new Vector2(15, 15);
            rect.anchoredPosition = Vector2.zero;
            
            var img = iconObj.AddComponent<Image>();
            img.color = Color.gray;
            
            return img;
        }

        private UIButton CreateInviteButton(RectTransform parent)
        {
            var btnObj = new GameObject("InviteButton");
            btnObj.transform.SetParent(parent, false);
            
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.6f, 0.5f);
            rect.anchorMax = new Vector2(0.6f, 0.5f);
            rect.sizeDelta = new Vector2(80, 35);
            rect.anchoredPosition = Vector2.zero;
            
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 0.2f);
            
            var button = btnObj.AddComponent<UIButton>();
            button.onClick.AddListener(OnInviteClick);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Invite";
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            
            return button;
        }

        private UIButton CreateRemoveButton(RectTransform parent)
        {
            var btnObj = new GameObject("RemoveButton");
            btnObj.transform.SetParent(parent, false);
            
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.85f, 0.5f);
            rect.anchorMax = new Vector2(0.85f, 0.5f);
            rect.sizeDelta = new Vector2(80, 35);
            rect.anchoredPosition = Vector2.zero;
            
            var img = btnObj.AddComponent<Image>();
            img.color = new Color(0.6f, 0.2f, 0.2f);
            
            var button = btnObj.AddComponent<UIButton>();
            button.onClick.AddListener(OnRemoveClick);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Remove";
            text.fontSize = 14;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            
            return button;
        }

        private void UpdateDisplay()
        {
            if (_nicknameText != null && _friendData != null)
            {
                _nicknameText.text = _friendData.Nickname;
            }
            
            if (_statusIcon != null && _friendData != null)
            {
                _statusIcon.color = _friendData.IsOnline ? Color.green : Color.gray;
            }
            
            if (_inviteButton != null && _friendData != null)
            {
                _inviteButton.interactable = _friendData.IsOnline;
            }
        }

        private void OnInviteClick()
        {
            if (_friendData != null)
            {
                _onInvite?.Invoke(_friendData.UserID);
            }
        }

        private void OnRemoveClick()
        {
            if (_friendData != null)
            {
                _onRemove?.Invoke(_friendData.UserID);
            }
        }
    }
}
