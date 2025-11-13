using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using JRPGGame.Core;
using JRPGGame.Party;
using JRPGGame.Inventory;

namespace JRPGGame.Quest
{
    public class QuestManager : MonoBehaviour
    {
        private static QuestManager instance;
        public static QuestManager Instance => instance;

        [Header("Quest Database")]
        [SerializeField] private List<Quest> allQuests = new List<Quest>();

        [Header("Active Quests")]
        [SerializeField] private List<QuestProgress> activeQuests = new List<QuestProgress>();
        [SerializeField] private List<QuestProgress> completedQuests = new List<QuestProgress>();

        [Header("Tracking")]
        [SerializeField] private Quest trackedQuest;

        // Events
        public event Action<Quest> OnQuestStarted;
        public event Action<Quest> OnQuestCompleted;
        public event Action<Quest> OnQuestFailed;
        public event Action<Quest, QuestObjective> OnObjectiveCompleted;
        public event Action OnQuestLogUpdated;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Quest Management
        public bool StartQuest(Quest quest)
        {
            if (quest == null)
            {
                Debug.LogWarning("Cannot start null quest!");
                return false;
            }

            // Check if quest is already active or completed
            if (IsQuestActive(quest.questID) || IsQuestCompleted(quest.questID))
            {
                Debug.LogWarning($"Quest {quest.questName} is already active or completed!");
                return false;
            }

            // Check prerequisites
            if (!CheckPrerequisites(quest))
            {
                Debug.LogWarning($"Prerequisites not met for quest {quest.questName}!");
                return false;
            }

            // Check level requirement
            if (PartyManager.Instance != null && PartyManager.Instance.GetAverageLevel() < quest.requiredLevel)
            {
                Debug.LogWarning($"Party level too low for quest {quest.questName}!");
                return false;
            }

            // Start the quest
            QuestProgress progress = new QuestProgress(quest);
            activeQuests.Add(progress);

            OnQuestStarted?.Invoke(quest);
            OnQuestLogUpdated?.Invoke();

            Debug.Log($"Started quest: {quest.questName}");
            return true;
        }

        public bool CompleteQuest(Quest quest)
        {
            var progress = GetQuestProgress(quest.questID);
            if (progress == null)
            {
                Debug.LogWarning($"Quest {quest.questName} is not active!");
                return false;
            }

            if (!quest.AreAllObjectivesComplete(progress))
            {
                Debug.LogWarning($"Not all objectives completed for quest {quest.questName}!");
                return false;
            }

            // Move to completed quests
            activeQuests.Remove(progress);
            progress.status = QuestStatus.Completed;
            progress.completionTime = DateTime.Now;
            completedQuests.Add(progress);

            // Grant rewards
            GrantQuestRewards(quest);

            OnQuestCompleted?.Invoke(quest);
            OnQuestLogUpdated?.Invoke();

            Debug.Log($"Completed quest: {quest.questName}!");

            // Untrack if this was the tracked quest
            if (trackedQuest == quest)
            {
                trackedQuest = null;
            }

            return true;
        }

        public bool FailQuest(Quest quest)
        {
            var progress = GetQuestProgress(quest.questID);
            if (progress == null) return false;

            activeQuests.Remove(progress);
            progress.status = QuestStatus.Failed;

            OnQuestFailed?.Invoke(quest);
            OnQuestLogUpdated?.Invoke();

            Debug.Log($"Failed quest: {quest.questName}");

            if (trackedQuest == quest)
            {
                trackedQuest = null;
            }

            return true;
        }

        // Objective Management
        public bool UpdateObjectiveProgress(string questID, string objectiveID, int amount = 1)
        {
            var progress = GetQuestProgress(questID);
            if (progress == null) return false;

            var quest = progress.quest;
            var objective = quest.objectives.FirstOrDefault(o => o.objectiveID == objectiveID);
            if (objective == null) return false;

            var objectiveProgress = progress.GetObjectiveProgress(objectiveID);
            if (objectiveProgress == null)
            {
                // Initialize objective progress
                objectiveProgress = new ObjectiveProgress
                {
                    objectiveID = objectiveID,
                    currentAmount = 0,
                    isCompleted = false
                };
                progress.objectiveProgress.Add(objectiveProgress);
            }

            // Update progress
            objectiveProgress.currentAmount = Mathf.Min(
                objectiveProgress.currentAmount + amount,
                objective.requiredAmount);

            // Check if objective is complete
            if (objectiveProgress.currentAmount >= objective.requiredAmount && !objectiveProgress.isCompleted)
            {
                objectiveProgress.isCompleted = true;
                OnObjectiveCompleted?.Invoke(quest, objective);
                Debug.Log($"Objective completed: {objective.description}");

                // Check if quest is complete
                if (quest.autoComplete && quest.AreAllObjectivesComplete(progress))
                {
                    CompleteQuest(quest);
                }
            }

            OnQuestLogUpdated?.Invoke();
            return true;
        }

        public void TrackObjective(string questID, string objectiveID, string targetID)
        {
            switch (GetObjectiveType(questID, objectiveID))
            {
                case QuestObjectiveType.KillEnemies:
                    UpdateObjectiveProgress(questID, objectiveID, 1);
                    break;

                case QuestObjectiveType.CollectItems:
                    UpdateObjectiveProgress(questID, objectiveID, 1);
                    break;

                case QuestObjectiveType.TalkToNPC:
                    UpdateObjectiveProgress(questID, objectiveID, 1);
                    break;

                case QuestObjectiveType.ReachLocation:
                    UpdateObjectiveProgress(questID, objectiveID, 1);
                    break;

                case QuestObjectiveType.DefeatBoss:
                    UpdateObjectiveProgress(questID, objectiveID, 1);
                    break;
            }
        }

        // Quest Queries
        public bool IsQuestActive(string questID)
        {
            return activeQuests.Any(q => q.quest.questID == questID);
        }

        public bool IsQuestCompleted(string questID)
        {
            return completedQuests.Any(q => q.quest.questID == questID);
        }

        public QuestProgress GetQuestProgress(string questID)
        {
            return activeQuests.FirstOrDefault(q => q.quest.questID == questID);
        }

        public Quest GetQuestByID(string questID)
        {
            return allQuests.FirstOrDefault(q => q.questID == questID);
        }

        public List<Quest> GetAvailableQuests()
        {
            return allQuests.Where(q =>
                !IsQuestActive(q.questID) &&
                !IsQuestCompleted(q.questID) &&
                CheckPrerequisites(q)).ToList();
        }

        public List<QuestProgress> GetActiveQuests()
        {
            return new List<QuestProgress>(activeQuests);
        }

        public List<QuestProgress> GetCompletedQuests()
        {
            return new List<QuestProgress>(completedQuests);
        }

        // Quest Tracking
        public void SetTrackedQuest(Quest quest)
        {
            if (IsQuestActive(quest.questID))
            {
                trackedQuest = quest;
                Debug.Log($"Now tracking: {quest.questName}");
            }
        }

        public void UntrackQuest()
        {
            trackedQuest = null;
        }

        public Quest GetTrackedQuest()
        {
            return trackedQuest;
        }

        // Helper Methods
        private bool CheckPrerequisites(Quest quest)
        {
            foreach (var prereqID in quest.prerequisiteQuestIDs)
            {
                if (!IsQuestCompleted(prereqID))
                {
                    return false;
                }
            }
            return true;
        }

        private void GrantQuestRewards(Quest quest)
        {
            // Grant experience
            if (quest.experienceReward > 0 && PartyManager.Instance != null)
            {
                foreach (var character in PartyManager.Instance.ActiveParty)
                {
                    character.stats.AddExperience(quest.experienceReward);
                }
                Debug.Log($"Gained {quest.experienceReward} EXP!");
            }

            // Grant gold
            if (quest.goldReward > 0 && PartyManager.Instance != null)
            {
                PartyManager.Instance.AddGold(quest.goldReward);
                Debug.Log($"Gained {quest.goldReward} gold!");
            }

            // Grant items
            if (InventoryManager.Instance != null)
            {
                foreach (var itemReward in quest.itemRewards)
                {
                    InventoryManager.Instance.AddItem(itemReward.item, itemReward.quantity);
                    Debug.Log($"Received {itemReward.item.itemName} x{itemReward.quantity}!");
                }
            }
        }

        private QuestObjectiveType GetObjectiveType(string questID, string objectiveID)
        {
            var quest = GetQuestByID(questID);
            if (quest == null) return QuestObjectiveType.Custom;

            var objective = quest.objectives.FirstOrDefault(o => o.objectiveID == objectiveID);
            return objective?.objectiveType ?? QuestObjectiveType.Custom;
        }

        // Event Handlers (called by game systems)
        public void OnEnemyDefeated(string enemyID)
        {
            foreach (var progress in activeQuests)
            {
                foreach (var objective in progress.quest.objectives)
                {
                    if (objective.objectiveType == QuestObjectiveType.KillEnemies &&
                        objective.targetID == enemyID)
                    {
                        UpdateObjectiveProgress(progress.quest.questID, objective.objectiveID, 1);
                    }
                }
            }
        }

        public void OnItemCollected(string itemID)
        {
            foreach (var progress in activeQuests)
            {
                foreach (var objective in progress.quest.objectives)
                {
                    if (objective.objectiveType == QuestObjectiveType.CollectItems &&
                        objective.targetID == itemID)
                    {
                        UpdateObjectiveProgress(progress.quest.questID, objective.objectiveID, 1);
                    }
                }
            }
        }

        public void OnLocationReached(string locationID)
        {
            foreach (var progress in activeQuests)
            {
                foreach (var objective in progress.quest.objectives)
                {
                    if (objective.objectiveType == QuestObjectiveType.ReachLocation &&
                        objective.targetID == locationID)
                    {
                        UpdateObjectiveProgress(progress.quest.questID, objective.objectiveID, 1);
                    }
                }
            }
        }

        public void OnNPCTalkedTo(string npcID)
        {
            foreach (var progress in activeQuests)
            {
                foreach (var objective in progress.quest.objectives)
                {
                    if (objective.objectiveType == QuestObjectiveType.TalkToNPC &&
                        objective.targetID == npcID)
                    {
                        UpdateObjectiveProgress(progress.quest.questID, objective.objectiveID, 1);
                    }
                }
            }
        }
    }

    [Serializable]
    public class QuestProgress
    {
        public Quest quest;
        public QuestStatus status;
        public DateTime startTime;
        public DateTime completionTime;
        public List<ObjectiveProgress> objectiveProgress = new List<ObjectiveProgress>();

        public QuestProgress(Quest quest)
        {
            this.quest = quest;
            this.status = QuestStatus.InProgress;
            this.startTime = DateTime.Now;

            // Initialize objective progress
            foreach (var objective in quest.objectives)
            {
                objectiveProgress.Add(new ObjectiveProgress
                {
                    objectiveID = objective.objectiveID,
                    currentAmount = 0,
                    isCompleted = false
                });
            }
        }

        public ObjectiveProgress GetObjectiveProgress(string objectiveID)
        {
            return objectiveProgress.FirstOrDefault(op => op.objectiveID == objectiveID);
        }
    }

    [Serializable]
    public class ObjectiveProgress
    {
        public string objectiveID;
        public int currentAmount;
        public bool isCompleted;
    }
}
