using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TPSBR
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        public event Action OnQuestsUpdated;
        public event Action<QuestDefinition, int> OnQuestProgressUpdated;
        public event Action<QuestDefinition> OnQuestCompleted;

        private const string QUEST_SAVE_KEY = "PlayerQuestData";
        private const int DAILY_QUESTS_PER_DAY = 5;

        private List<QuestDefinition> _allQuests = new List<QuestDefinition>();
        private PlayerQuestData _playerData = new PlayerQuestData();
        private CloudCoinSystem _coinSystem;

        private Dictionary<string, int> _sessionProgress = new Dictionary<string, int>();
        private HashSet<string> _weaponTypesUsed = new HashSet<string>();
        private HashSet<string> _locationsVisited = new HashSet<string>();
        private bool _tookStormDamageThisMatch = false;
        private bool _tookStormDamageThisSession = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeQuests();
            LoadQuestData();
        }

        private void Start()
        {
            GameObject coinSystemObject = GameObject.Find("CloudCoinManager");
            if (coinSystemObject != null)
            {
                var coinManager = coinSystemObject.GetComponent<CloudCoinManager>();
                if (coinManager != null)
                {
                    _coinSystem = coinManager.CoinSystem;
                }
            }

            CheckAndResetQuests();
        }

        private void InitializeQuests()
        {
            _allQuests.Clear();

            _allQuests.Add(new QuestDefinition("daily_first_drop", "First Drop", "Play 1 match", QuestType.Daily, QuestRequirementType.PlayMatches, 1, 25));
            _allQuests.Add(new QuestDefinition("daily_survivor", "Survivor", "Survive for 5 minutes in a match", QuestType.Daily, QuestRequirementType.SurviveTime, 300, 50));
            _allQuests.Add(new QuestDefinition("daily_scavenger", "Scavenger", "Land and loot 3 different item boxes", QuestType.Daily, QuestRequirementType.LootBoxes, 3, 40));
            _allQuests.Add(new QuestDefinition("daily_top_half", "Top Half", "Finish in top 50% of players", QuestType.Daily, QuestRequirementType.FinishTopPercent, 50, 60));
            _allQuests.Add(new QuestDefinition("daily_distance_walker", "Distance Walker", "Travel 1000 meters on foot", QuestType.Daily, QuestRequirementType.TravelDistance, 1000, 35));
            _allQuests.Add(new QuestDefinition("daily_zone_runner", "Zone Runner", "Survive 3 storm circles", QuestType.Daily, QuestRequirementType.SurviveStormCircles, 3, 75));

            _allQuests.Add(new QuestDefinition("combat_first_blood", "First Blood", "Get your first elimination", QuestType.Combat, QuestRequirementType.GetEliminations, 1, 100));
            _allQuests.Add(new QuestDefinition("combat_marksman", "Marksman", "Deal 200 damage to enemies", QuestType.Combat, QuestRequirementType.DealDamage, 200, 125));
            _allQuests.Add(new QuestDefinition("combat_close_combat", "Close Combat", "Get 1 elimination within 10 meters", QuestType.Combat, QuestRequirementType.CloseRangeElimination, 1, 150));
            _allQuests.Add(new QuestDefinition("combat_headhunter", "Headhunter", "Get 2 headshot eliminations", QuestType.Combat, QuestRequirementType.HeadshotEliminations, 2, 175));

            _allQuests.Add(new QuestDefinition("weekly_victory_royale", "Victory Royale", "Win 1 match", QuestType.Weekly, QuestRequirementType.WinMatch, 1, 500));
            _allQuests.Add(new QuestDefinition("weekly_top_10_streak", "Top 10 Streak", "Finish top 10 in 3 matches", QuestType.Weekly, QuestRequirementType.FinishTop10, 3, 250));
            _allQuests.Add(new QuestDefinition("weekly_elimination_spree", "Elimination Spree", "Get 5 total eliminations across all matches", QuestType.Weekly, QuestRequirementType.GetEliminations, 5, 200));
            _allQuests.Add(new QuestDefinition("weekly_zone_master", "Zone Master", "Survive to final circle 2 times", QuestType.Weekly, QuestRequirementType.SurviveFinalCircle, 2, 300));

            _allQuests.Add(new QuestDefinition("progression_veteran", "Battle Royale Veteran", "Play 10 total matches", QuestType.Progression, QuestRequirementType.PlayMatches, 10, 300));
            _allQuests.Add(new QuestDefinition("progression_storm_survivor", "Storm Survivor", "Take storm damage and survive 5 times", QuestType.Progression, QuestRequirementType.TakeStormDamage, 5, 400));
            _allQuests.Add(new QuestDefinition("progression_weapon_master", "Weapon Master", "Get eliminations with 3 different weapon types", QuestType.Progression, QuestRequirementType.EliminationsWithDifferentWeapons, 3, 350));
            _allQuests.Add(new QuestDefinition("progression_explorer", "Explorer", "Land in 5 different named locations", QuestType.Progression, QuestRequirementType.LandInDifferentLocations, 5, 250));

            _allQuests.Add(new QuestDefinition("special_weekly_champion", "Weekly Champion", "Win 3 matches this week", QuestType.Special, QuestRequirementType.WinMatch, 3, 1000));
            _allQuests.Add(new QuestDefinition("special_perfect_game", "Perfect Game", "Win without taking storm damage", QuestType.Special, QuestRequirementType.WinWithoutStormDamage, 1, 750));
            _allQuests.Add(new QuestDefinition("special_medic", "Medic", "Use healing items 10 times", QuestType.Special, QuestRequirementType.UseHealingItems, 10, 100));
        }

        private void CheckAndResetQuests()
        {
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long oneDayInSeconds = 86400;
            long oneWeekInSeconds = 604800;

            bool shouldResetDaily = (currentTime - _playerData.LastDailyResetTime) >= oneDayInSeconds;
            bool shouldResetWeekly = (currentTime - _playerData.LastWeeklyResetTime) >= oneWeekInSeconds;

            if (shouldResetDaily)
            {
                ResetDailyQuests();
                _playerData.LastDailyResetTime = currentTime;
            }

            if (shouldResetWeekly)
            {
                ResetWeeklyQuests();
                _playerData.LastWeeklyResetTime = currentTime;
            }

            if (_playerData.ActiveQuests.Count == 0)
            {
                AssignRandomDailyQuests();
            }

            SaveQuestData();
        }

        private void ResetDailyQuests()
        {
            _playerData.ActiveQuests.RemoveAll(q =>
            {
                var questDef = GetQuestDefinition(q.QuestID);
                return questDef != null && questDef.Type == QuestType.Daily;
            });

            AssignRandomDailyQuests();
        }

        private void ResetWeeklyQuests()
        {
            _playerData.ActiveQuests.RemoveAll(q =>
            {
                var questDef = GetQuestDefinition(q.QuestID);
                return questDef != null && (questDef.Type == QuestType.Weekly || questDef.Type == QuestType.Special);
            });

            AssignWeeklyQuests();
        }

        private void AssignRandomDailyQuests()
        {
            var dailyQuests = _allQuests.Where(q => q.Type == QuestType.Daily).ToList();
            var combatQuests = _allQuests.Where(q => q.Type == QuestType.Combat).ToList();
            var allAvailable = new List<QuestDefinition>();
            allAvailable.AddRange(dailyQuests);
            allAvailable.AddRange(combatQuests);

            var selectedQuests = new List<QuestDefinition>();
            System.Random random = new System.Random();

            while (selectedQuests.Count < DAILY_QUESTS_PER_DAY && allAvailable.Count > 0)
            {
                int index = random.Next(allAvailable.Count);
                selectedQuests.Add(allAvailable[index]);
                allAvailable.RemoveAt(index);
            }

            foreach (var quest in selectedQuests)
            {
                if (!_playerData.ActiveQuests.Any(q => q.QuestID == quest.QuestID))
                {
                    _playerData.ActiveQuests.Add(new QuestProgress(quest.QuestID));
                }
            }
        }

        private void AssignWeeklyQuests()
        {
            var weeklyQuests = _allQuests.Where(q => q.Type == QuestType.Weekly || q.Type == QuestType.Progression).ToList();

            foreach (var quest in weeklyQuests)
            {
                if (!_playerData.ActiveQuests.Any(q => q.QuestID == quest.QuestID))
                {
                    _playerData.ActiveQuests.Add(new QuestProgress(quest.QuestID));
                }
            }
        }

        public void UpdateQuestProgress(QuestRequirementType requirementType, int amount, object additionalData = null)
        {
            var activeQuests = _playerData.ActiveQuests.Where(q => !q.IsCompleted).ToList();

            foreach (var questProgress in activeQuests)
            {
                var questDef = GetQuestDefinition(questProgress.QuestID);
                if (questDef == null || questDef.RequirementType != requirementType)
                    continue;

                bool shouldUpdate = true;

                if (requirementType == QuestRequirementType.EliminationsWithDifferentWeapons && additionalData is string weaponType)
                {
                    _weaponTypesUsed.Add(weaponType);
                    questProgress.CurrentProgress = _weaponTypesUsed.Count;
                    shouldUpdate = false;
                }
                else if (requirementType == QuestRequirementType.LandInDifferentLocations && additionalData is string location)
                {
                    _locationsVisited.Add(location);
                    questProgress.CurrentProgress = _locationsVisited.Count;
                    shouldUpdate = false;
                }

                if (shouldUpdate)
                {
                    questProgress.CurrentProgress += amount;
                }

                questProgress.CurrentProgress = Mathf.Min(questProgress.CurrentProgress, questDef.RequiredAmount);

                OnQuestProgressUpdated?.Invoke(questDef, questProgress.CurrentProgress);

                if (questProgress.CurrentProgress >= questDef.RequiredAmount && !questProgress.IsCompleted)
                {
                    CompleteQuest(questProgress, questDef);
                }
            }

            SaveQuestData();
            OnQuestsUpdated?.Invoke();
        }

        private void CompleteQuest(QuestProgress progress, QuestDefinition definition)
        {
            progress.IsCompleted = true;
            OnQuestCompleted?.Invoke(definition);

            Debug.Log($"Quest Completed: {definition.QuestName} - Reward: {definition.CoinReward} CloudCoins");
        }

        public void ClaimQuestReward(string questID)
        {
            var questProgress = _playerData.ActiveQuests.FirstOrDefault(q => q.QuestID == questID);
            if (questProgress == null || !questProgress.IsCompleted || questProgress.RewardClaimed)
                return;

            var questDef = GetQuestDefinition(questID);
            if (questDef == null)
                return;

            if (_coinSystem != null)
            {
                _coinSystem.AddCoins(questDef.CoinReward);
                Debug.Log($"Claimed {questDef.CoinReward} CloudCoins from quest: {questDef.QuestName}");
            }

            questProgress.RewardClaimed = true;
            _playerData.CompletedQuestIDs.Add(questID);

            SaveQuestData();
            OnQuestsUpdated?.Invoke();
        }

        public List<QuestDefinition> GetActiveQuests()
        {
            var activeQuests = new List<QuestDefinition>();
            foreach (var progress in _playerData.ActiveQuests)
            {
                var questDef = GetQuestDefinition(progress.QuestID);
                if (questDef != null && !progress.RewardClaimed)
                {
                    activeQuests.Add(questDef);
                }
            }
            return activeQuests;
        }

        public QuestProgress GetQuestProgress(string questID)
        {
            return _playerData.ActiveQuests.FirstOrDefault(q => q.QuestID == questID);
        }

        private QuestDefinition GetQuestDefinition(string questID)
        {
            return _allQuests.FirstOrDefault(q => q.QuestID == questID);
        }

        public void OnMatchStarted()
        {
            _sessionProgress.Clear();
            _tookStormDamageThisMatch = false;
        }

        public void OnMatchEnded(int playerPosition, int totalPlayers, bool isWinner)
        {
            UpdateQuestProgress(QuestRequirementType.PlayMatches, 1);

            if (isWinner)
            {
                UpdateQuestProgress(QuestRequirementType.WinMatch, 1);

                if (!_tookStormDamageThisMatch)
                {
                    UpdateQuestProgress(QuestRequirementType.WinWithoutStormDamage, 1);
                }
            }

            float topPercent = ((float)playerPosition / totalPlayers) * 100f;
            if (topPercent <= 50f)
            {
                UpdateQuestProgress(QuestRequirementType.FinishTopPercent, 1);
            }

            if (playerPosition <= 10)
            {
                UpdateQuestProgress(QuestRequirementType.FinishTop10, 1);
            }

            if (_tookStormDamageThisMatch)
            {
                _tookStormDamageThisSession = true;
            }
        }

        public void OnPlayerSurvived()
        {
            if (_tookStormDamageThisSession)
            {
                UpdateQuestProgress(QuestRequirementType.TakeStormDamage, 1);
                _tookStormDamageThisSession = false;
            }
        }

        public void OnEliminationObtained(bool isHeadshot, float distance, string weaponType)
        {
            UpdateQuestProgress(QuestRequirementType.GetEliminations, 1);

            if (isHeadshot)
            {
                UpdateQuestProgress(QuestRequirementType.HeadshotEliminations, 1);
            }

            if (distance <= 10f)
            {
                UpdateQuestProgress(QuestRequirementType.CloseRangeElimination, 1);
            }

            if (!string.IsNullOrEmpty(weaponType))
            {
                UpdateQuestProgress(QuestRequirementType.EliminationsWithDifferentWeapons, 1, weaponType);
            }
        }

        public void OnDamageDealt(int damage)
        {
            UpdateQuestProgress(QuestRequirementType.DealDamage, damage);
        }

        public void OnStormDamageTaken()
        {
            _tookStormDamageThisMatch = true;
        }

        public void OnItemLooted()
        {
            UpdateQuestProgress(QuestRequirementType.LootBoxes, 1);
        }

        public void OnHealingItemUsed()
        {
            UpdateQuestProgress(QuestRequirementType.UseHealingItems, 1);
        }

        public void OnPlayerLanded(string locationName)
        {
            if (!string.IsNullOrEmpty(locationName))
            {
                UpdateQuestProgress(QuestRequirementType.LandInDifferentLocations, 1, locationName);
            }
        }

        public void OnStormCircleSurvived()
        {
            UpdateQuestProgress(QuestRequirementType.SurviveStormCircles, 1);
        }

        public void OnFinalCircleReached()
        {
            UpdateQuestProgress(QuestRequirementType.SurviveFinalCircle, 1);
        }

        public void UpdateSurvivalTime(float seconds)
        {
            UpdateQuestProgress(QuestRequirementType.SurviveTime, Mathf.FloorToInt(seconds));
        }

        public void UpdateTravelDistance(float meters)
        {
            UpdateQuestProgress(QuestRequirementType.TravelDistance, Mathf.FloorToInt(meters));
        }

        private void SaveQuestData()
        {
            string json = JsonUtility.ToJson(_playerData);
            PlayerPrefs.SetString(QUEST_SAVE_KEY, json);
            PlayerPrefs.Save();
        }

        private void LoadQuestData()
        {
            if (PlayerPrefs.HasKey(QUEST_SAVE_KEY))
            {
                string json = PlayerPrefs.GetString(QUEST_SAVE_KEY);
                _playerData = JsonUtility.FromJson<PlayerQuestData>(json);
            }
            else
            {
                _playerData = new PlayerQuestData
                {
                    LastDailyResetTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    LastWeeklyResetTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
            }
        }
    }
}
