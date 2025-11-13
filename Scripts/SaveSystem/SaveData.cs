using System;
using System.Collections.Generic;
using UnityEngine;
using JRPGGame.Core;

namespace JRPGGame.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        [Header("Save Info")]
        public string saveName;
        public DateTime saveTime;
        public int saveSlot;
        public float playTime;

        [Header("Player Progress")]
        public int playerLevel;
        public string currentLocation;
        public Vector3 playerPosition;
        public int gold;

        [Header("Party")]
        public List<CharacterSaveData> partyMembers = new List<CharacterSaveData>();
        public List<CharacterSaveData> reserveMembers = new List<CharacterSaveData>();

        [Header("Inventory")]
        public List<ItemSaveData> items = new List<ItemSaveData>();
        public List<EquipmentSaveData> equipment = new List<EquipmentSaveData>();
        public List<MaterialSaveData> materials = new List<MaterialSaveData>();

        [Header("Quests")]
        public List<QuestSaveData> activeQuests = new List<QuestSaveData>();
        public List<string> completedQuestIDs = new List<string>();

        [Header("Flags & Variables")]
        public Dictionary<string, bool> eventFlags = new Dictionary<string, bool>();
        public Dictionary<string, int> gameVariables = new Dictionary<string, int>();

        [Header("World State")]
        public List<string> visitedLocations = new List<string>();
        public List<string> unlockedWaypoints = new List<string>();
    }

    [Serializable]
    public class CharacterSaveData
    {
        public string characterName;
        public string characterClass;
        public int level;
        public int currentHP;
        public int maxHP;
        public int currentMP;
        public int maxMP;
        public int currentShieldBar;
        public int maxShieldBar;
        public int currentExp;

        // Stats
        public int attack;
        public int defense;
        public int magicAttack;
        public int magicDefense;
        public int speed;

        // Equipment
        public string weaponID;
        public string armorID;
        public List<string> accessoryIDs = new List<string>();

        // Skills
        public List<string> learnedSkillIDs = new List<string>();
    }

    [Serializable]
    public class ItemSaveData
    {
        public string itemID;
        public int quantity;
    }

    [Serializable]
    public class EquipmentSaveData
    {
        public string equipmentID;
        public int upgradeLevel;
    }

    [Serializable]
    public class MaterialSaveData
    {
        public string materialID;
        public int quantity;
    }

    [Serializable]
    public class QuestSaveData
    {
        public string questID;
        public QuestStatus status;
        public List<ObjectiveSaveData> objectives = new List<ObjectiveSaveData>();
    }

    [Serializable]
    public class ObjectiveSaveData
    {
        public string objectiveID;
        public int currentAmount;
        public bool isCompleted;
    }
}
