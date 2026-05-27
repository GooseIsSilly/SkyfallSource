using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace TPSBR.UI
{
    public class UIQuestProgressHUD : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform _questProgressContainer;
        [SerializeField] private GameObject _questProgressItemPrefab;
        [SerializeField] private int _maxVisibleQuests = 3;
        [SerializeField] private bool _showOnlyIncompleteQuests = true;

        private List<UIQuestProgressItem> _progressItems = new List<UIQuestProgressItem>();
        private QuestManager _questManager;

        private void Start()
        {
            _questManager = QuestManager.Instance;
            
            if (_questManager != null)
            {
                _questManager.OnQuestsUpdated += RefreshQuestProgress;
                _questManager.OnQuestProgressUpdated += OnQuestProgressUpdated;
                _questManager.OnQuestCompleted += OnQuestCompleted;
            }

            RefreshQuestProgress();
        }

        private void RefreshQuestProgress()
        {
            ClearProgressItems();

            if (_questManager == null || _questProgressContainer == null || _questProgressItemPrefab == null)
                return;

            var activeQuests = _questManager.GetActiveQuests();
            var questsToShow = activeQuests
                .Where(q => !_showOnlyIncompleteQuests || !_questManager.GetQuestProgress(q.QuestID)?.IsCompleted == true)
                .Take(_maxVisibleQuests)
                .ToList();

            foreach (var quest in questsToShow)
            {
                GameObject itemObj = Instantiate(_questProgressItemPrefab, _questProgressContainer);
                UIQuestProgressItem progressItem = itemObj.GetComponent<UIQuestProgressItem>();
                
                if (progressItem != null)
                {
                    var progress = _questManager.GetQuestProgress(quest.QuestID);
                    progressItem.Initialize(quest, progress);
                    _progressItems.Add(progressItem);
                }
            }
        }

        private void ClearProgressItems()
        {
            foreach (var item in _progressItems)
            {
                if (item != null && item.gameObject != null)
                {
                    Destroy(item.gameObject);
                }
            }
            _progressItems.Clear();
        }

        private void OnQuestProgressUpdated(QuestDefinition quest, int currentProgress)
        {
            var progressItem = _progressItems.Find(item => item.QuestID == quest.QuestID);
            if (progressItem != null)
            {
                var progress = _questManager.GetQuestProgress(quest.QuestID);
                progressItem.UpdateProgress(progress);
            }
        }

        private void OnQuestCompleted(QuestDefinition quest)
        {
            if (_showOnlyIncompleteQuests)
            {
                RefreshQuestProgress();
            }
        }

        private void OnDestroy()
        {
            if (_questManager != null)
            {
                _questManager.OnQuestsUpdated -= RefreshQuestProgress;
                _questManager.OnQuestProgressUpdated -= OnQuestProgressUpdated;
                _questManager.OnQuestCompleted -= OnQuestCompleted;
            }
        }
    }

    public class UIQuestProgressItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _questNameText;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Slider _progressBar;

        public string QuestID { get; private set; }
        private QuestDefinition _questDefinition;

        public void Initialize(QuestDefinition quest, QuestProgress progress)
        {
            _questDefinition = quest;
            QuestID = quest.QuestID;

            if (_questNameText != null)
            {
                _questNameText.text = quest.QuestName;
            }

            UpdateProgress(progress);
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
        }
    }
}
