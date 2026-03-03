using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace TPSBR.UI
{
    public class UIQuestItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _questNameText;
        [SerializeField] private TextMeshProUGUI _questDescriptionText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _rewardText;
        [SerializeField] private Slider _progressBar;
        [SerializeField] private Button _claimButton;
        [SerializeField] private GameObject _completedIndicator;
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Image _typeIcon;

        [Header("Colors")]
        [SerializeField] private Color _dailyColor = new Color(0.3f, 0.6f, 1f);
        [SerializeField] private Color _combatColor = new Color(1f, 0.3f, 0.3f);
        [SerializeField] private Color _weeklyColor = new Color(0.8f, 0.4f, 1f);
        [SerializeField] private Color _progressionColor = new Color(1f, 0.8f, 0.2f);
        [SerializeField] private Color _specialColor = new Color(1f, 0.5f, 0f);

        public string QuestID { get; private set; }

        private Action<string> _onClaimCallback;
        private QuestDefinition _questDefinition;

        public void Initialize(QuestDefinition quest, QuestProgress progress, Action<string> onClaimCallback)
        {
            _questDefinition = quest;
            QuestID = quest.QuestID;
            _onClaimCallback = onClaimCallback;

            if (_questNameText != null)
            {
                _questNameText.text = quest.QuestName;
            }

            if (_questDescriptionText != null)
            {
                _questDescriptionText.text = quest.QuestDescription;
            }

            if (_rewardText != null)
            {
                _rewardText.text = $"{quest.CoinReward} CloudCoins";
            }

            SetQuestTypeColor(quest.Type);

            UpdateProgress(progress);

            if (_claimButton != null)
            {
                _claimButton.onClick.AddListener(OnClaimButtonClicked);
            }
        }

        public void UpdateProgress(QuestProgress progress)
        {
            if (progress == null || _questDefinition == null)
                return;

            int currentProgress = Mathf.Min(progress.CurrentProgress, _questDefinition.RequiredAmount);
            int requiredAmount = _questDefinition.RequiredAmount;

            if (_progressText != null)
            {
                _progressText.text = $"{currentProgress}/{requiredAmount}";
            }

            if (_progressBar != null)
            {
                _progressBar.maxValue = requiredAmount;
                _progressBar.value = currentProgress;
            }

            bool isCompleted = progress.IsCompleted;
            bool canClaim = isCompleted && !progress.RewardClaimed;

            if (_claimButton != null)
            {
                _claimButton.gameObject.SetActive(canClaim);
                _claimButton.interactable = canClaim;
            }

            if (_completedIndicator != null)
            {
                _completedIndicator.SetActive(progress.RewardClaimed);
            }
        }

        private void SetQuestTypeColor(QuestType type)
        {
            Color selectedColor = _dailyColor;

            switch (type)
            {
                case QuestType.Daily:
                    selectedColor = _dailyColor;
                    break;
                case QuestType.Combat:
                    selectedColor = _combatColor;
                    break;
                case QuestType.Weekly:
                    selectedColor = _weeklyColor;
                    break;
                case QuestType.Progression:
                    selectedColor = _progressionColor;
                    break;
                case QuestType.Special:
                    selectedColor = _specialColor;
                    break;
            }

            if (_backgroundImage != null)
            {
                _backgroundImage.color = selectedColor * 0.3f;
            }

            if (_typeIcon != null)
            {
                _typeIcon.color = selectedColor;
            }
        }

        private void OnClaimButtonClicked()
        {
            _onClaimCallback?.Invoke(QuestID);
        }

        private void OnDestroy()
        {
            if (_claimButton != null)
            {
                _claimButton.onClick.RemoveListener(OnClaimButtonClicked);
            }
        }
    }
}
