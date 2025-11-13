using System;
using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Core;
using JRPGGame.Inventory;

namespace JRPGGame.Quest
{
    [CreateAssetMenu(fileName = "New Quest", menuName = "JRPG/Quest/Quest")]
    public class Quest : ScriptableObject
    {
        [Header("Basic Info")]
        public string questID;
        public string questName;
        [TextArea(3, 5)]
        public string questDescription;
        public QuestType questType;

        [Header("Requirements")]
        public int requiredLevel = 1;
        public List<string> prerequisiteQuestIDs = new List<string>();

        [Header("Objectives")]
        public List<QuestObjective> objectives = new List<QuestObjective>();

        [Header("Rewards")]
        public int experienceReward;
        public int goldReward;
        public List<ItemReward> itemRewards = new List<ItemReward>();

        [Header("Quest Flow")]
        public string questGiverNPC;
        public string questCompleteNPC;
        public bool autoComplete = false;

        [Header("Tracking")]
        public bool canTrack = true;
        public bool showInJournal = true;

        public bool AreAllObjectivesComplete(QuestProgress progress)
        {
            foreach (var objective in objectives)
            {
                var objectiveProgress = progress.GetObjectiveProgress(objective.objectiveID);
                if (objectiveProgress == null || !objectiveProgress.isCompleted)
                {
                    return false;
                }
            }
            return true;
        }

        public float GetCompletionPercentage(QuestProgress progress)
        {
            if (objectives.Count == 0) return 0f;

            int completedObjectives = 0;
            foreach (var objective in objectives)
            {
                var objectiveProgress = progress.GetObjectiveProgress(objective.objectiveID);
                if (objectiveProgress != null && objectiveProgress.isCompleted)
                {
                    completedObjectives++;
                }
            }

            return (float)completedObjectives / objectives.Count * 100f;
        }
    }

    [Serializable]
    public class QuestObjective
    {
        public string objectiveID;
        public string description;
        public QuestObjectiveType objectiveType;

        [Header("Target")]
        public string targetID; // Enemy ID, Item ID, NPC ID, Location ID, etc.
        public int requiredAmount = 1;

        [Header("Optional")]
        public bool optional = false;
        public bool hidden = false;
    }

    [Serializable]
    public class ItemReward
    {
        public Item item;
        public int quantity = 1;
    }

    public enum QuestObjectiveType
    {
        KillEnemies,
        CollectItems,
        TalkToNPC,
        ReachLocation,
        DefeatBoss,
        EscortNPC,
        Custom
    }
}
