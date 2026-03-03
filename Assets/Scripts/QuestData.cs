using System;
using UnityEngine;

namespace TPSBR
{
    public enum QuestType
    {
        Daily,
        Combat,
        Weekly,
        Progression,
        Special
    }

    public enum QuestRequirementType
    {
        PlayMatches,
        SurviveTime,
        LootBoxes,
        FinishTopPercent,
        TravelDistance,
        SurviveStormCircles,
        GetEliminations,
        DealDamage,
        CloseRangeElimination,
        HeadshotEliminations,
        WinMatch,
        FinishTop10,
        SurviveFinalCircle,
        TakeStormDamage,
        EliminationsWithDifferentWeapons,
        LandInDifferentLocations,
        WinWithoutStormDamage,
        UseHealingItems
    }

    [Serializable]
    public class QuestDefinition
    {
        public string QuestID;
        public string QuestName;
        public string QuestDescription;
        public QuestType Type;
        public QuestRequirementType RequirementType;
        public int RequiredAmount;
        public int CoinReward;
        public bool IsActive;

        public QuestDefinition(string id, string name, string description, QuestType type, QuestRequirementType requirementType, int requiredAmount, int coinReward)
        {
            QuestID = id;
            QuestName = name;
            QuestDescription = description;
            Type = type;
            RequirementType = requirementType;
            RequiredAmount = requiredAmount;
            CoinReward = coinReward;
            IsActive = true;
        }
    }

    [Serializable]
    public class QuestProgress
    {
        public string QuestID;
        public int CurrentProgress;
        public bool IsCompleted;
        public bool RewardClaimed;

        public QuestProgress(string questID)
        {
            QuestID = questID;
            CurrentProgress = 0;
            IsCompleted = false;
            RewardClaimed = false;
        }
    }

    [Serializable]
    public class PlayerQuestData
    {
        public long LastDailyResetTime;
        public long LastWeeklyResetTime;
        public System.Collections.Generic.List<QuestProgress> ActiveQuests = new System.Collections.Generic.List<QuestProgress>();
        public System.Collections.Generic.List<string> CompletedQuestIDs = new System.Collections.Generic.List<string>();
    }
}
