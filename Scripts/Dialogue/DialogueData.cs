using System;
using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Core;

namespace JRPGGame.Dialogue
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "JRPG/Dialogue/Dialogue")]
    public class DialogueData : ScriptableObject
    {
        [Header("Dialogue Info")]
        public string dialogueID;
        public string dialogueName;

        [Header("Nodes")]
        public List<DialogueNode> nodes = new List<DialogueNode>();

        [Header("Start Node")]
        public string startNodeID;

        public DialogueNode GetNodeByID(string nodeID)
        {
            return nodes.Find(n => n.nodeID == nodeID);
        }

        public DialogueNode GetStartNode()
        {
            return GetNodeByID(startNodeID);
        }
    }

    [Serializable]
    public class DialogueNode
    {
        [Header("Node Info")]
        public string nodeID;
        public string speakerName;
        public Sprite speakerPortrait;

        [Header("Dialogue Text")]
        [TextArea(3, 6)]
        public string dialogueText;

        [Header("Conditions")]
        public List<DialogueCondition> conditions = new List<DialogueCondition>();

        [Header("Choices")]
        public List<DialogueChoice> choices = new List<DialogueChoice>();

        [Header("Events")]
        public List<DialogueEvent> events = new List<DialogueEvent>();

        [Header("Flow")]
        public string nextNodeID; // Auto-continue to this node if no choices

        public bool CheckConditions()
        {
            foreach (var condition in conditions)
            {
                if (!condition.Evaluate())
                {
                    return false;
                }
            }
            return true;
        }
    }

    [Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public string targetNodeID;
        public List<DialogueCondition> conditions = new List<DialogueCondition>();

        public bool IsAvailable()
        {
            foreach (var condition in conditions)
            {
                if (!condition.Evaluate())
                {
                    return false;
                }
            }
            return true;
        }
    }

    [Serializable]
    public class DialogueCondition
    {
        public DialogueConditionType conditionType;
        public string variableName;
        public ComparisonOperator comparisonOperator;
        public int intValue;
        public bool boolValue;
        public string stringValue;

        public bool Evaluate()
        {
            // Conditions are evaluated by the DialogueManager
            return DialogueManager.Instance?.EvaluateCondition(this) ?? true;
        }
    }

    [Serializable]
    public class DialogueEvent
    {
        public DialogueEventType eventType;
        public string eventID;
        public string parameter1;
        public string parameter2;
        public int intParameter;
    }

    public enum DialogueConditionType
    {
        QuestActive,
        QuestCompleted,
        HasItem,
        HasGold,
        LevelGreaterThan,
        FlagSet,
        VariableEquals,
        Custom
    }

    public enum ComparisonOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterOrEqual,
        LessOrEqual
    }
}
