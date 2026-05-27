using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace TPSBR.UI
{
    public class UIQuestView : UIView
    {
        [Header("UI References")]
        [SerializeField] private Transform _questListContainer;
        [SerializeField] private GameObject _questItemPrefab;
        [SerializeField] private Button _closeButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        private List<UIQuestItem> _questItems = new List<UIQuestItem>();
        private QuestManager _questManager;

        public UIView BackView { get; set; }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(OnCloseButtonClicked);
            }

            if (_titleText != null)
            {
                _titleText.text = "QUESTS";
            }
        }

        protected override void OnOpen()
        {
            base.OnOpen();

            _questManager = QuestManager.Instance;
            if (_questManager != null)
            {
                _questManager.OnQuestsUpdated += RefreshQuestList;
                _questManager.OnQuestProgressUpdated += OnQuestProgressUpdated;
                _questManager.OnQuestCompleted += OnQuestCompleted;
            }

            RefreshQuestList();
        }

        protected override void OnClose()
        {
            if (_questManager != null)
            {
                _questManager.OnQuestsUpdated -= RefreshQuestList;
                _questManager.OnQuestProgressUpdated -= OnQuestProgressUpdated;
                _questManager.OnQuestCompleted -= OnQuestCompleted;
            }

            if (BackView != null && BackView.IsOpen == false)
            {
                BackView.Open();
            }

            base.OnClose();
        }

        protected override bool OnBackAction()
        {
            if (IsInteractable == false)
                return false;

            Close();
            return true;
        }

        private void RefreshQuestList()
        {
            ClearQuestItems();

            if (_questManager == null || _questListContainer == null || _questItemPrefab == null)
                return;

            var activeQuests = _questManager.GetActiveQuests();
            
            foreach (var quest in activeQuests)
            {
                GameObject questItemObj = Instantiate(_questItemPrefab, _questListContainer);
                UIQuestItem questItem = questItemObj.GetComponent<UIQuestItem>();
                
                if (questItem != null)
                {
                    var progress = _questManager.GetQuestProgress(quest.QuestID);
                    questItem.Initialize(quest, progress, OnClaimRewardClicked);
                    _questItems.Add(questItem);
                }
            }
        }

        private void ClearQuestItems()
        {
            foreach (var item in _questItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _questItems.Clear();
        }

        private void OnQuestProgressUpdated(QuestDefinition quest, int currentProgress)
        {
            var questItem = _questItems.Find(item => item.QuestID == quest.QuestID);
            if (questItem != null)
            {
                var progress = _questManager.GetQuestProgress(quest.QuestID);
                questItem.UpdateProgress(progress);
            }
        }

        private void OnQuestCompleted(QuestDefinition quest)
        {
            var questItem = _questItems.Find(item => item.QuestID == quest.QuestID);
            if (questItem != null)
            {
                var progress = _questManager.GetQuestProgress(quest.QuestID);
                questItem.UpdateProgress(progress);
            }
        }

        private void OnClaimRewardClicked(string questID)
        {
            if (_questManager != null)
            {
                _questManager.ClaimQuestReward(questID);
                RefreshQuestList();
            }
        }

        private void OnCloseButtonClicked()
        {
            Close();
        }

        private void OnDestroy()
        {
            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            }
        }
    }
}
