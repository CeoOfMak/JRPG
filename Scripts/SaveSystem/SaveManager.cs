using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using JRPGGame.Core;
using JRPGGame.Party;
using JRPGGame.Inventory;
using JRPGGame.Quest;

namespace JRPGGame.SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager instance;
        public static SaveManager Instance => instance;

        [Header("Auto-Save")]
        [SerializeField] private bool autoSaveEnabled = true;
        [SerializeField] private float autoSaveInterval = GameConstants.AUTOSAVE_INTERVAL;
        private float autoSaveTimer = 0f;

        [Header("Current Save")]
        [SerializeField] private int currentSaveSlot = -1;
        [SerializeField] private float currentPlayTime = 0f;

        private string saveDirectory;

        // Events
        public event Action<int> OnGameSaved;
        public event Action<int> OnGameLoaded;
        public event Action OnAutoSave;

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
                return;
            }

            saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
            }
        }

        private void Update()
        {
            // Track play time
            currentPlayTime += Time.deltaTime;

            // Auto-save timer
            if (autoSaveEnabled)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    autoSaveTimer = 0f;
                    AutoSave();
                }
            }
        }

        // Save Operations
        public bool SaveGame(int slot, string saveName = "")
        {
            try
            {
                SaveData saveData = CreateSaveData(slot, saveName);
                string json = JsonUtility.ToJson(saveData, true);
                string filePath = GetSaveFilePath(slot);

                File.WriteAllText(filePath, json);

                currentSaveSlot = slot;
                OnGameSaved?.Invoke(slot);

                Debug.Log($"Game saved to slot {slot} at {filePath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
                return false;
            }
        }

        public bool LoadGame(int slot)
        {
            try
            {
                string filePath = GetSaveFilePath(slot);

                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"Save file not found at slot {slot}");
                    return false;
                }

                string json = File.ReadAllText(filePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                ApplySaveData(saveData);

                currentSaveSlot = slot;
                currentPlayTime = saveData.playTime;

                OnGameLoaded?.Invoke(slot);

                Debug.Log($"Game loaded from slot {slot}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                return false;
            }
        }

        public void AutoSave()
        {
            if (currentSaveSlot >= 0)
            {
                SaveGame(currentSaveSlot);
                OnAutoSave?.Invoke();
                Debug.Log("Auto-save completed");
            }
            else
            {
                // Save to auto-save slot
                SaveGame(-1, GameConstants.AUTOSAVE_FILE_NAME);
                OnAutoSave?.Invoke();
            }
        }

        public bool DeleteSave(int slot)
        {
            try
            {
                string filePath = GetSaveFilePath(slot);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"Deleted save at slot {slot}");
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save: {e.Message}");
                return false;
            }
        }

        // Save Data Creation
        private SaveData CreateSaveData(int slot, string saveName)
        {
            SaveData saveData = new SaveData
            {
                saveName = string.IsNullOrEmpty(saveName) ? $"Save {slot}" : saveName,
                saveTime = DateTime.Now,
                saveSlot = slot,
                playTime = currentPlayTime
            };

            // Party data
            if (PartyManager.Instance != null)
            {
                saveData.gold = PartyManager.Instance.Gold;
                saveData.playerLevel = PartyManager.Instance.GetAverageLevel();

                foreach (var character in PartyManager.Instance.ActiveParty)
                {
                    saveData.partyMembers.Add(CreateCharacterSaveData(character));
                }

                foreach (var character in PartyManager.Instance.Reserves)
                {
                    saveData.reserveMembers.Add(CreateCharacterSaveData(character));
                }
            }

            // Inventory data
            if (InventoryManager.Instance != null)
            {
                foreach (var slot in InventoryManager.Instance.GetAllItems())
                {
                    if (slot.item != null)
                    {
                        saveData.items.Add(new ItemSaveData
                        {
                            itemID = slot.item.name,
                            quantity = slot.quantity
                        });
                    }
                }

                foreach (var equipment in InventoryManager.Instance.GetAllEquipment())
                {
                    saveData.equipment.Add(new EquipmentSaveData
                    {
                        equipmentID = equipment.name,
                        upgradeLevel = equipment.upgradeLevel
                    });
                }
            }

            // Quest data
            if (QuestManager.Instance != null)
            {
                foreach (var questProgress in QuestManager.Instance.GetActiveQuests())
                {
                    QuestSaveData questSave = new QuestSaveData
                    {
                        questID = questProgress.quest.questID,
                        status = questProgress.status
                    };

                    foreach (var objProgress in questProgress.objectiveProgress)
                    {
                        questSave.objectives.Add(new ObjectiveSaveData
                        {
                            objectiveID = objProgress.objectiveID,
                            currentAmount = objProgress.currentAmount,
                            isCompleted = objProgress.isCompleted
                        });
                    }

                    saveData.activeQuests.Add(questSave);
                }

                foreach (var completedQuest in QuestManager.Instance.GetCompletedQuests())
                {
                    saveData.completedQuestIDs.Add(completedQuest.quest.questID);
                }
            }

            // World data
            if (WorldNavigator.WorldNavigator.Instance != null)
            {
                var currentLocation = WorldNavigator.WorldNavigator.Instance.GetCurrentLocation();
                if (currentLocation != null)
                {
                    saveData.currentLocation = currentLocation.locationID;
                }
            }

            // Player position
            if (PlayerController.PlayerController.Instance != null)
            {
                saveData.playerPosition = PlayerController.PlayerController.Instance.transform.position;
            }

            return saveData;
        }

        private CharacterSaveData CreateCharacterSaveData(Progression.CharacterProgression character)
        {
            CharacterSaveData charData = new CharacterSaveData
            {
                characterName = character.characterName,
                characterClass = character.characterClass,
                level = character.stats.Level,
                currentHP = character.stats.CurrentHP,
                maxHP = character.stats.MaxHP,
                currentMP = character.stats.CurrentMP,
                maxMP = character.stats.MaxMP,
                currentShieldBar = character.stats.CurrentShieldBar,
                maxShieldBar = character.stats.MaxShieldBar,
                currentExp = character.stats.CurrentExp,
                attack = character.stats.Attack,
                defense = character.stats.Defense,
                magicAttack = character.stats.MagicAttack,
                magicDefense = character.stats.MagicDefense,
                speed = character.stats.Speed
            };

            // Equipment
            if (character.equipment.weapon != null)
                charData.weaponID = character.equipment.weapon.name;
            if (character.equipment.armor != null)
                charData.armorID = character.equipment.armor.name;

            if (character.equipment.accessory1 != null)
                charData.accessoryIDs.Add(character.equipment.accessory1.name);
            if (character.equipment.accessory2 != null)
                charData.accessoryIDs.Add(character.equipment.accessory2.name);
            if (character.equipment.accessory3 != null)
                charData.accessoryIDs.Add(character.equipment.accessory3.name);
            if (character.equipment.accessory4 != null)
                charData.accessoryIDs.Add(character.equipment.accessory4.name);

            // Skills
            foreach (var skill in character.learnedSkills)
            {
                charData.learnedSkillIDs.Add(skill.name);
            }

            return charData;
        }

        // Apply Save Data
        private void ApplySaveData(SaveData saveData)
        {
            // Apply party data
            if (PartyManager.Instance != null)
            {
                // Clear existing party
                foreach (var character in PartyManager.Instance.GetAllCharacters())
                {
                    PartyManager.Instance.RemoveCharacterFromParty(character);
                }

                // Load saved party members
                // This would require instantiating characters from data
                // Implementation depends on your character creation system

                // Set gold
                PartyManager.Instance.AddGold(saveData.gold - PartyManager.Instance.Gold);
            }

            // Apply inventory data
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ClearInventory();

                // Load items, equipment, materials
                // Implementation depends on your resource loading system
            }

            // Apply quest data
            if (QuestManager.Instance != null)
            {
                // Restore active quests
                // Implementation depends on your quest system
            }

            // Apply world state
            if (WorldNavigator.WorldNavigator.Instance != null && !string.IsNullOrEmpty(saveData.currentLocation))
            {
                // Restore location
                WorldNavigator.WorldNavigator.Instance.TravelToLocationByID(saveData.currentLocation);
            }

            // Restore player position
            if (PlayerController.PlayerController.Instance != null)
            {
                WorldNavigator.WorldNavigator.Instance.Teleport(saveData.playerPosition);
            }

            Debug.Log("Save data applied successfully");
        }

        // Utility Methods
        public bool SaveExists(int slot)
        {
            string filePath = GetSaveFilePath(slot);
            return File.Exists(filePath);
        }

        public SaveData GetSaveInfo(int slot)
        {
            try
            {
                string filePath = GetSaveFilePath(slot);

                if (!File.Exists(filePath))
                {
                    return null;
                }

                string json = File.ReadAllText(filePath);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to get save info: {e.Message}");
                return null;
            }
        }

        public List<SaveData> GetAllSaves()
        {
            List<SaveData> saves = new List<SaveData>();

            for (int i = 0; i < GameConstants.MAX_SAVE_SLOTS; i++)
            {
                SaveData saveInfo = GetSaveInfo(i);
                if (saveInfo != null)
                {
                    saves.Add(saveInfo);
                }
            }

            return saves;
        }

        private string GetSaveFilePath(int slot)
        {
            string fileName = slot < 0
                ? $"{GameConstants.AUTOSAVE_FILE_NAME}{GameConstants.SAVE_FILE_EXTENSION}"
                : $"{GameConstants.SAVE_FILE_PREFIX}{slot}{GameConstants.SAVE_FILE_EXTENSION}";

            return Path.Combine(saveDirectory, fileName);
        }

        public void SetAutoSaveEnabled(bool enabled)
        {
            autoSaveEnabled = enabled;
        }

        public float GetPlayTime()
        {
            return currentPlayTime;
        }

        public string GetPlayTimeFormatted()
        {
            TimeSpan time = TimeSpan.FromSeconds(currentPlayTime);
            return $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
        }
    }
}
