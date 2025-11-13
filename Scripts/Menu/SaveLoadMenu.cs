using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JRPGGame.SaveSystem;
using JRPGGame.Core;

namespace JRPGGame.Menu
{
    public class SaveLoadMenu : MonoBehaviour
    {
        [Header("Save Slot Prefab")]
        [SerializeField] private GameObject saveSlotPrefab;
        [SerializeField] private Transform saveSlotContainer;

        [Header("Mode")]
        [SerializeField] private bool isSaveMode = true; // true = save, false = load

        [Header("Buttons")]
        [SerializeField] private Button backButton;

        private List<SaveSlotUI> saveSlots = new List<SaveSlotUI>();

        private void Start()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBack);
            }

            RefreshSaveSlots();
        }

        private void RefreshSaveSlots()
        {
            // Clear existing slots
            foreach (var slot in saveSlots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            saveSlots.Clear();

            // Create save slots
            for (int i = 0; i < GameConstants.MAX_SAVE_SLOTS; i++)
            {
                CreateSaveSlot(i);
            }
        }

        private void CreateSaveSlot(int slotIndex)
        {
            if (saveSlotPrefab == null || saveSlotContainer == null)
                return;

            GameObject slotObj = Instantiate(saveSlotPrefab, saveSlotContainer);
            SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();

            if (slotUI != null)
            {
                SaveData saveData = SaveManager.Instance?.GetSaveInfo(slotIndex);

                if (saveData != null)
                {
                    slotUI.Initialize(slotIndex, saveData, isSaveMode);
                }
                else
                {
                    slotUI.InitializeEmpty(slotIndex, isSaveMode);
                }

                slotUI.OnSlotClicked += (slot) => OnSaveSlotClicked(slot);
                saveSlots.Add(slotUI);
            }
        }

        private void OnSaveSlotClicked(int slotIndex)
        {
            if (SaveManager.Instance == null) return;

            if (isSaveMode)
            {
                // Save game
                SaveManager.Instance.SaveGame(slotIndex, $"Save {slotIndex + 1}");
                Debug.Log($"Game saved to slot {slotIndex}");
                RefreshSaveSlots();
            }
            else
            {
                // Load game
                if (SaveManager.Instance.SaveExists(slotIndex))
                {
                    SaveManager.Instance.LoadGame(slotIndex);
                    Debug.Log($"Game loaded from slot {slotIndex}");
                }
                else
                {
                    Debug.LogWarning($"No save data in slot {slotIndex}");
                }
            }
        }

        private void OnBack()
        {
            gameObject.SetActive(false);
        }

        public void SetMode(bool saveMode)
        {
            isSaveMode = saveMode;
            RefreshSaveSlots();
        }
    }

    public class SaveSlotUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI slotNumberText;
        [SerializeField] private TextMeshProUGUI saveNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI playTimeText;
        [SerializeField] private TextMeshProUGUI saveTimeText;
        [SerializeField] private GameObject emptySlotIndicator;
        [SerializeField] private Button slotButton;

        private int slotIndex;
        private bool isEmpty;

        public event System.Action<int> OnSlotClicked;

        private void Start()
        {
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(() => OnSlotClicked?.Invoke(slotIndex));
            }
        }

        public void Initialize(int index, SaveData saveData, bool isSaveMode)
        {
            slotIndex = index;
            isEmpty = false;

            if (slotNumberText != null)
                slotNumberText.text = $"Slot {index + 1}";

            if (saveNameText != null)
                saveNameText.text = saveData.saveName;

            if (levelText != null)
                levelText.text = $"Lv. {saveData.playerLevel}";

            if (playTimeText != null)
            {
                System.TimeSpan time = System.TimeSpan.FromSeconds(saveData.playTime);
                playTimeText.text = $"{time.Hours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            }

            if (saveTimeText != null)
                saveTimeText.text = saveData.saveTime.ToString("MM/dd/yyyy HH:mm");

            if (emptySlotIndicator != null)
                emptySlotIndicator.SetActive(false);
        }

        public void InitializeEmpty(int index, bool isSaveMode)
        {
            slotIndex = index;
            isEmpty = true;

            if (slotNumberText != null)
                slotNumberText.text = $"Slot {index + 1}";

            if (saveNameText != null)
                saveNameText.text = isSaveMode ? "New Save" : "Empty";

            if (levelText != null)
                levelText.text = "";

            if (playTimeText != null)
                playTimeText.text = "";

            if (saveTimeText != null)
                saveTimeText.text = "";

            if (emptySlotIndicator != null)
                emptySlotIndicator.SetActive(true);

            // Disable button in load mode if slot is empty
            if (slotButton != null && !isSaveMode)
            {
                slotButton.interactable = false;
            }
        }
    }
}
