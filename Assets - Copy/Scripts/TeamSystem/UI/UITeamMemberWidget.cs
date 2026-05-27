using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TPSBR.UI
{
    public class UITeamMemberWidget : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _nicknameText;
        [SerializeField]
        private Image _background;
        [SerializeField]
        private Image _statusIcon;
        [SerializeField]
        private Image _healthBar;
        [SerializeField]
        private GameObject _leaderIcon;
        [SerializeField]
        private GameObject _readyIndicator;
        [SerializeField]
        private GameObject _downedIndicator;
        [SerializeField]
        private Color _aliveColor = Color.green;
        [SerializeField]
        private Color _deadColor = Color.red;
        [SerializeField]
        private Color _downedColor = new Color(1f, 0.5f, 0f);
        [SerializeField]
        private Color _notReadyColor = Color.gray;

        private string _userID;
        private bool _isAlive;
        private bool _isReady;
        private bool _isLeader;
        private bool _isDowned;

        public void AutoGenerate(float height, Color aliveColor, Color downedColor, Color deadColor)
        {
            _aliveColor = aliveColor;
            _downedColor = downedColor;
            _deadColor = deadColor;

            var rect = GetComponent<RectTransform>();
            
            _background = CreateBackground(rect, height);
            _nicknameText = CreateNicknameText(rect);
            _statusIcon = CreateStatusIcon(rect);
            _healthBar = CreateHealthBar(rect);
            _downedIndicator = CreateDownedIndicator(rect);
            _leaderIcon = CreateLeaderIcon(rect);
            _readyIndicator = CreateReadyIndicator(rect);
        }

        private Image CreateBackground(RectTransform parent, float height)
        {
            var bgObj = new GameObject("Background");
            bgObj.transform.SetParent(parent, false);
            
            var rect = bgObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var img = bgObj.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            return img;
        }

        private TextMeshProUGUI CreateNicknameText(RectTransform parent)
        {
            var textObj = new GameObject("PlayerName");
            textObj.transform.SetParent(parent, false);
            
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.15f, 0);
            rect.anchorMax = new Vector2(0.85f, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            var text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Player";
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
            rect.anchorMin = new Vector2(0.02f, 0.5f);
            rect.anchorMax = new Vector2(0.02f, 0.5f);
            rect.sizeDelta = new Vector2(20, 20);
            rect.anchoredPosition = Vector2.zero;
            
            var img = iconObj.AddComponent<Image>();
            img.color = _aliveColor;
            
            return img;
        }

        private Image CreateHealthBar(RectTransform parent)
        {
            var barBgObj = new GameObject("HealthBar");
            barBgObj.transform.SetParent(parent, false);
            
            var bgRect = barBgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0.15f, 0.1f);
            bgRect.anchorMax = new Vector2(0.85f, 0.25f);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            var bgImg = barBgObj.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(barBgObj.transform, false);
            
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2, 2);
            fillRect.offsetMax = new Vector2(-2, -2);
            
            var fillImg = fillObj.AddComponent<Image>();
            fillImg.color = Color.green;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            
            return fillImg;
        }

        private GameObject CreateDownedIndicator(RectTransform parent)
        {
            var downedObj = new GameObject("DownedIndicator");
            downedObj.transform.SetParent(parent, false);
            
            var rect = downedObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.88f, 0.5f);
            rect.anchorMax = new Vector2(0.88f, 0.5f);
            rect.sizeDelta = new Vector2(40, 20);
            rect.anchoredPosition = Vector2.zero;
            
            var text = downedObj.AddComponent<TextMeshProUGUI>();
            text.text = "DOWN";
            text.fontSize = 12;
            text.alignment = TextAlignmentOptions.Center;
            text.color = _downedColor;
            text.fontStyle = FontStyles.Bold;
            
            downedObj.SetActive(false);
            return downedObj;
        }

        private GameObject CreateLeaderIcon(RectTransform parent)
        {
            var leaderObj = new GameObject("LeaderIcon");
            leaderObj.transform.SetParent(parent, false);
            
            var rect = leaderObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.88f, 0.7f);
            rect.anchorMax = new Vector2(0.88f, 0.7f);
            rect.sizeDelta = new Vector2(30, 15);
            rect.anchoredPosition = Vector2.zero;
            
            var text = leaderObj.AddComponent<TextMeshProUGUI>();
            text.text = "★";
            text.fontSize = 16;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.yellow;
            
            leaderObj.SetActive(false);
            return leaderObj;
        }

        private GameObject CreateReadyIndicator(RectTransform parent)
        {
            var readyObj = new GameObject("ReadyIndicator");
            readyObj.transform.SetParent(parent, false);
            
            var rect = readyObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.88f, 0.3f);
            rect.anchorMax = new Vector2(0.88f, 0.3f);
            rect.sizeDelta = new Vector2(15, 15);
            rect.anchoredPosition = Vector2.zero;
            
            var img = readyObj.AddComponent<Image>();
            img.color = Color.green;
            
            readyObj.SetActive(false);
            return readyObj;
        }

        public void SetData(string userID, string nickname, bool isAlive, bool isReady, bool isLeader)
        {
            _userID = userID;
            _isAlive = isAlive;
            _isReady = isReady;
            _isLeader = isLeader;

            if (_nicknameText != null)
            {
                _nicknameText.text = nickname;
            }

            if (_leaderIcon != null)
            {
                _leaderIcon.SetActive(isLeader);
            }

            if (_readyIndicator != null)
            {
                _readyIndicator.SetActive(isReady);
            }

            UpdateVisuals();
        }

        public void SetAliveStatus(bool isAlive)
        {
            _isAlive = isAlive;
            UpdateVisuals();
        }

        public void SetDownedStatus(bool isDowned)
        {
            _isDowned = isDowned;
            
            if (_downedIndicator != null)
            {
                _downedIndicator.SetActive(isDowned);
            }
            
            UpdateVisuals();
        }

        public void SetReadyStatus(bool isReady)
        {
            _isReady = isReady;
            if (_readyIndicator != null)
            {
                _readyIndicator.SetActive(isReady);
            }
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            Color statusColor = _aliveColor;
            
            if (_isDowned)
            {
                statusColor = _downedColor;
            }
            else if (!_isAlive)
            {
                statusColor = _deadColor;
            }
            else if (!_isReady)
            {
                statusColor = _notReadyColor;
            }

            if (_statusIcon != null)
            {
                _statusIcon.color = statusColor;
            }

            if (_background != null)
            {
                _background.color = new Color(statusColor.r, statusColor.g, statusColor.b, 0.3f);
            }
        }

        public string GetUserID()
        {
            return _userID;
        }
    }
}
