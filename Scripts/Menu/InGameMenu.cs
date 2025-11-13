using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using JRPGGame.SaveSystem;
using JRPGGame.Party;

namespace JRPGGame.Menu
{
    public class InGameMenu : MonoBehaviour
    {
        private static InGameMenu instance;
        public static InGameMenu Instance => instance;

        [Header("Menu Control")]
        [SerializeField] private KeyCode menuKey = KeyCode.Escape;
        [SerializeField] private bool isMenuOpen = false;

        [Header("Main Menu Panel")]
        [SerializeField] private GameObject menuPanel;

        [Header("Sub Panels")]
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private GameObject equipmentPanel;
        [SerializeField] private GameObject skillsPanel;
        [SerializeField] private GameObject questLogPanel;
        [SerializeField] private GameObject mapPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Buttons")]
        [SerializeField] private Button statusButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button equipmentButton;
        [SerializeField] private Button skillsButton;
        [SerializeField] private Button questLogButton;
        [SerializeField] private Button mapButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button returnToTitleButton;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetupButtonListeners();
            CloseMenu();
        }

        private void Update()
        {
            if (Input.GetKeyDown(menuKey))
            {
                ToggleMenu();
            }
        }

        private void SetupButtonListeners()
        {
            if (statusButton != null)
                statusButton.onClick.AddListener(() => OpenSubPanel(statusPanel));

            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(() => OpenSubPanel(inventoryPanel));

            if (equipmentButton != null)
                equipmentButton.onClick.AddListener(() => OpenSubPanel(equipmentPanel));

            if (skillsButton != null)
                skillsButton.onClick.AddListener(() => OpenSubPanel(skillsPanel));

            if (questLogButton != null)
                questLogButton.onClick.AddListener(() => OpenSubPanel(questLogPanel));

            if (mapButton != null)
                mapButton.onClick.AddListener(() => OpenSubPanel(mapPanel));

            if (saveButton != null)
                saveButton.onClick.AddListener(OnSave);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => OpenSubPanel(settingsPanel));

            if (returnToTitleButton != null)
                returnToTitleButton.onClick.AddListener(OnReturnToTitle);
        }

        public void ToggleMenu()
        {
            if (isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        public void OpenMenu()
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(true);
                isMenuOpen = true;
                Time.timeScale = 0f; // Pause game

                // Disable player controls
                if (PlayerController.PlayerController.Instance != null)
                {
                    PlayerController.PlayerController.Instance.SetControlsEnabled(false);
                }

                CloseAllSubPanels();
            }
        }

        public void CloseMenu()
        {
            if (menuPanel != null)
            {
                menuPanel.SetActive(false);
                isMenuOpen = false;
                Time.timeScale = 1f; // Resume game

                // Enable player controls
                if (PlayerController.PlayerController.Instance != null)
                {
                    PlayerController.PlayerController.Instance.SetControlsEnabled(true);
                }

                CloseAllSubPanels();
            }
        }

        private void OpenSubPanel(GameObject panel)
        {
            if (panel == null) return;

            CloseAllSubPanels();
            panel.SetActive(true);
        }

        private void CloseAllSubPanels()
        {
            if (statusPanel != null) statusPanel.SetActive(false);
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
            if (equipmentPanel != null) equipmentPanel.SetActive(false);
            if (skillsPanel != null) skillsPanel.SetActive(false);
            if (questLogPanel != null) questLogPanel.SetActive(false);
            if (mapPanel != null) mapPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void OnSave()
        {
            if (SaveManager.Instance != null)
            {
                // Open save slot selection UI
                // For now, quick save to slot 0
                SaveManager.Instance.SaveGame(0, "Quick Save");
                Debug.Log("Game saved!");
            }
        }

        private void OnReturnToTitle()
        {
            Time.timeScale = 1f; // Make sure to reset time scale
            SceneManager.LoadScene("MainMenu");
        }

        public bool IsMenuOpen()
        {
            return isMenuOpen;
        }
    }
}
