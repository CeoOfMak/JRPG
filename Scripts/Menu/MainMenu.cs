using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using JRPGGame.SaveSystem;

namespace JRPGGame.Menu
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Menu Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject loadGamePanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject creditsPanel;

        [Header("Buttons")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("Game Scenes")]
        [SerializeField] private string firstGameScene = "GameStart";

        private void Start()
        {
            InitializeMenu();
            SetupButtonListeners();
        }

        private void InitializeMenu()
        {
            ShowMainMenu();

            // Enable/disable continue button based on auto-save existence
            if (continueButton != null && SaveManager.Instance != null)
            {
                continueButton.interactable = SaveManager.Instance.SaveExists(-1);
            }
        }

        private void SetupButtonListeners()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGame);

            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinue);

            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(OnLoadGame);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettings);

            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCredits);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuit);
        }

        private void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (loadGamePanel != null) loadGamePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        private void OnNewGame()
        {
            Debug.Log("Starting new game...");
            SceneManager.LoadScene(firstGameScene);
        }

        private void OnContinue()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.SaveExists(-1))
            {
                Debug.Log("Loading auto-save...");
                SaveManager.Instance.LoadGame(-1);
                // The scene would be loaded as part of the save data
            }
        }

        private void OnLoadGame()
        {
            if (loadGamePanel != null)
            {
                mainMenuPanel.SetActive(false);
                loadGamePanel.SetActive(true);
            }
        }

        private void OnSettings()
        {
            if (settingsPanel != null)
            {
                mainMenuPanel.SetActive(false);
                settingsPanel.SetActive(true);
            }
        }

        private void OnCredits()
        {
            if (creditsPanel != null)
            {
                mainMenuPanel.SetActive(false);
                creditsPanel.SetActive(true);
            }
        }

        private void OnQuit()
        {
            Debug.Log("Quitting game...");
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        public void BackToMainMenu()
        {
            ShowMainMenu();
        }
    }
}
