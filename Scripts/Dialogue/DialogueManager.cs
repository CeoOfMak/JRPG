using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using JRPGGame.Core;
using JRPGGame.Quest;
using JRPGGame.Party;
using JRPGGame.Inventory;

namespace JRPGGame.Dialogue
{
    public class DialogueManager : MonoBehaviour
    {
        private static DialogueManager instance;
        public static DialogueManager Instance => instance;

        [Header("Current Dialogue")]
        [SerializeField] private DialogueData currentDialogue;
        [SerializeField] private DialogueNode currentNode;
        [SerializeField] private bool isDialogueActive;

        [Header("Variables")]
        [SerializeField] private Dictionary<string, object> dialogueVariables = new Dictionary<string, object>();
        [SerializeField] private Dictionary<string, bool> eventFlags = new Dictionary<string, bool>();

        // Events
        public event Action<DialogueData> OnDialogueStarted;
        public event Action OnDialogueEnded;
        public event Action<DialogueNode> OnNodeChanged;
        public event Action<List<DialogueChoice>> OnChoicesPresented;

        public UnityEvent<string, string, Sprite> OnDialogueDisplayed; // speaker, text, portrait

        public bool IsDialogueActive => isDialogueActive;
        public DialogueNode CurrentNode => currentNode;

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

        // Dialogue Flow
        public void StartDialogue(DialogueData dialogue)
        {
            if (dialogue == null)
            {
                Debug.LogWarning("Cannot start null dialogue!");
                return;
            }

            currentDialogue = dialogue;
            isDialogueActive = true;

            var startNode = dialogue.GetStartNode();
            if (startNode == null)
            {
                Debug.LogError($"Dialogue {dialogue.dialogueName} has no valid start node!");
                EndDialogue();
                return;
            }

            OnDialogueStarted?.Invoke(dialogue);
            ShowNode(startNode);
        }

        public void ShowNode(DialogueNode node)
        {
            if (node == null)
            {
                EndDialogue();
                return;
            }

            // Check node conditions
            if (!node.CheckConditions())
            {
                // Skip to next node if conditions not met
                if (!string.IsNullOrEmpty(node.nextNodeID))
                {
                    var nextNode = currentDialogue.GetNodeByID(node.nextNodeID);
                    ShowNode(nextNode);
                }
                else
                {
                    EndDialogue();
                }
                return;
            }

            currentNode = node;
            OnNodeChanged?.Invoke(node);

            // Display dialogue
            OnDialogueDisplayed?.Invoke(node.speakerName, node.dialogueText, node.speakerPortrait);

            // Execute node events
            ExecuteNodeEvents(node);

            // Present choices or auto-continue
            if (node.choices != null && node.choices.Count > 0)
            {
                var availableChoices = node.choices.FindAll(c => c.IsAvailable());
                OnChoicesPresented?.Invoke(availableChoices);
            }
            else if (!string.IsNullOrEmpty(node.nextNodeID))
            {
                // Auto-continue available - wait for player input to advance
            }
            else
            {
                // No choices and no next node - dialogue ends
                // Wait for player confirmation before ending
            }
        }

        public void SelectChoice(DialogueChoice choice)
        {
            if (choice == null || string.IsNullOrEmpty(choice.targetNodeID))
            {
                EndDialogue();
                return;
            }

            var nextNode = currentDialogue.GetNodeByID(choice.targetNodeID);
            ShowNode(nextNode);
        }

        public void ContinueDialogue()
        {
            if (currentNode == null)
            {
                EndDialogue();
                return;
            }

            // Continue to next node if no choices
            if ((currentNode.choices == null || currentNode.choices.Count == 0) &&
                !string.IsNullOrEmpty(currentNode.nextNodeID))
            {
                var nextNode = currentDialogue.GetNodeByID(currentNode.nextNodeID);
                ShowNode(nextNode);
            }
            else
            {
                EndDialogue();
            }
        }

        public void EndDialogue()
        {
            isDialogueActive = false;
            currentDialogue = null;
            currentNode = null;

            OnDialogueEnded?.Invoke();
        }

        // Event Execution
        private void ExecuteNodeEvents(DialogueNode node)
        {
            foreach (var dialogueEvent in node.events)
            {
                ExecuteEvent(dialogueEvent);
            }
        }

        private void ExecuteEvent(DialogueEvent dialogueEvent)
        {
            switch (dialogueEvent.eventType)
            {
                case DialogueEventType.StartQuest:
                    if (QuestManager.Instance != null)
                    {
                        var quest = QuestManager.Instance.GetQuestByID(dialogueEvent.eventID);
                        if (quest != null)
                        {
                            QuestManager.Instance.StartQuest(quest);
                        }
                    }
                    break;

                case DialogueEventType.CompleteQuest:
                    if (QuestManager.Instance != null)
                    {
                        var quest = QuestManager.Instance.GetQuestByID(dialogueEvent.eventID);
                        if (quest != null)
                        {
                            QuestManager.Instance.CompleteQuest(quest);
                        }
                    }
                    break;

                case DialogueEventType.GiveItem:
                    if (InventoryManager.Instance != null)
                    {
                        // You would load the item from resources or a database
                        Debug.Log($"Give item: {dialogueEvent.eventID}");
                    }
                    break;

                case DialogueEventType.TakeItem:
                    if (InventoryManager.Instance != null)
                    {
                        Debug.Log($"Take item: {dialogueEvent.eventID}");
                    }
                    break;

                case DialogueEventType.SetFlag:
                    SetFlag(dialogueEvent.eventID, true);
                    break;

                case DialogueEventType.StartBattle:
                    Debug.Log($"Start battle: {dialogueEvent.eventID}");
                    // Battle start logic would be handled by BattleManager
                    break;

                case DialogueEventType.Teleport:
                    Debug.Log($"Teleport to: {dialogueEvent.eventID}");
                    // Teleport logic would be handled by WorldNavigator
                    break;

                case DialogueEventType.Custom:
                    // Custom event handling
                    Debug.Log($"Custom event: {dialogueEvent.eventID}");
                    break;
            }
        }

        // Condition Evaluation
        public bool EvaluateCondition(DialogueCondition condition)
        {
            switch (condition.conditionType)
            {
                case DialogueConditionType.QuestActive:
                    return QuestManager.Instance?.IsQuestActive(condition.stringValue) ?? false;

                case DialogueConditionType.QuestCompleted:
                    return QuestManager.Instance?.IsQuestCompleted(condition.stringValue) ?? false;

                case DialogueConditionType.HasItem:
                    // Would check inventory
                    return true;

                case DialogueConditionType.HasGold:
                    return PartyManager.Instance?.HasGold(condition.intValue) ?? false;

                case DialogueConditionType.LevelGreaterThan:
                    return PartyManager.Instance?.GetAverageLevel() > condition.intValue ?? false;

                case DialogueConditionType.FlagSet:
                    return GetFlag(condition.stringValue);

                case DialogueConditionType.VariableEquals:
                    return EvaluateVariableCondition(condition);

                case DialogueConditionType.Custom:
                    return true;

                default:
                    return true;
            }
        }

        private bool EvaluateVariableCondition(DialogueCondition condition)
        {
            if (!dialogueVariables.ContainsKey(condition.variableName))
            {
                return false;
            }

            var value = dialogueVariables[condition.variableName];

            if (value is int intVal)
            {
                return CompareInt(intVal, condition.intValue, condition.comparisonOperator);
            }
            else if (value is bool boolVal)
            {
                return boolVal == condition.boolValue;
            }
            else if (value is string stringVal)
            {
                return stringVal == condition.stringValue;
            }

            return false;
        }

        private bool CompareInt(int a, int b, ComparisonOperator op)
        {
            switch (op)
            {
                case ComparisonOperator.Equals: return a == b;
                case ComparisonOperator.NotEquals: return a != b;
                case ComparisonOperator.GreaterThan: return a > b;
                case ComparisonOperator.LessThan: return a < b;
                case ComparisonOperator.GreaterOrEqual: return a >= b;
                case ComparisonOperator.LessOrEqual: return a <= b;
                default: return false;
            }
        }

        // Variable Management
        public void SetVariable(string variableName, object value)
        {
            if (dialogueVariables.ContainsKey(variableName))
            {
                dialogueVariables[variableName] = value;
            }
            else
            {
                dialogueVariables.Add(variableName, value);
            }
        }

        public object GetVariable(string variableName)
        {
            return dialogueVariables.ContainsKey(variableName) ? dialogueVariables[variableName] : null;
        }

        public void SetFlag(string flagName, bool value)
        {
            if (eventFlags.ContainsKey(flagName))
            {
                eventFlags[flagName] = value;
            }
            else
            {
                eventFlags.Add(flagName, value);
            }
        }

        public bool GetFlag(string flagName)
        {
            return eventFlags.ContainsKey(flagName) && eventFlags[flagName];
        }

        public void ClearAllVariables()
        {
            dialogueVariables.Clear();
            eventFlags.Clear();
        }
    }
}
