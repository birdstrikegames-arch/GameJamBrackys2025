// File: GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nat
{
    /// <summary>
    /// ESC to pause/resume. If Settings (or a deeper panel) is open, ESC returns to the main pause menu first.
    /// Provides UI button hooks to Resume, Open Settings, Back to Pause Menu, Restart, and Go To Main Menu.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("UI Roots")]
        [SerializeField] private GameObject pauseMenuRoot;     // main pause panel
        [SerializeField] private GameObject settingsPanelRoot; // optional: a deeper panel under pause
        [SerializeField] private GameObject overlaysToHideOnResume; // optional: any extra overlays to hide when resuming

        [Header("Scenes")]
        [Tooltip("Name of the Main Menu scene to load when 'Main Menu' is pressed.")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Cursor")]
        [Tooltip("Lock cursor when gameplay is active.")]
        [SerializeField] private bool lockCursorDuringGameplay = true;

        public bool IsPaused { get; private set; }

                // Add this field to GameManager
        private bool pauseInputLocked = false;

        // Public setter so other systems (like Win) can lock/unlock ESC handling
        public void SetPauseInputLocked(bool locked)
        {
            pauseInputLocked = locked;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            // Optional: DontDestroyOnLoad(this.gameObject);

            // Ensure initial UI state
            if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
            if (settingsPanelRoot != null) settingsPanelRoot.SetActive(false);

            ApplyGameplayCursorState();
        }

        private void Update()
        {
            if (pauseInputLocked) return; // <-- Ignore ESC while locked (e.g., during Win)

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!IsPaused)
                {
                    Pause();
                }
                else
                {
                    if (settingsPanelRoot != null && settingsPanelRoot.activeSelf)
                        ShowPauseHome();
                    else
                        Resume();
                }
            }
        }

        // -------- Public API for UI Buttons --------

        public void OnPressResume() => Resume();

        public void OnPressOpenSettings()
        {
            if (!IsPaused) Pause();
            if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
            if (settingsPanelRoot != null) settingsPanelRoot.SetActive(true);
        }

        public void OnPressBackToPauseMenu() => ShowPauseHome();

        public void OnPressRestart() => RestartGame();

        public void OnPressMainMenu() => GoToMainMenu();

        // --------------- Core ----------------------

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;

            if (pauseMenuRoot != null) pauseMenuRoot.SetActive(true);
            if (settingsPanelRoot != null) settingsPanelRoot.SetActive(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;

            if (pauseMenuRoot != null) pauseMenuRoot.SetActive(false);
            if (settingsPanelRoot != null) settingsPanelRoot.SetActive(false);
            if (overlaysToHideOnResume != null) overlaysToHideOnResume.SetActive(false);

            ApplyGameplayCursorState();
        }

        private void ShowPauseHome()
        {
            if (!IsPaused) Pause();
            if (pauseMenuRoot != null) pauseMenuRoot.SetActive(true);
            if (settingsPanelRoot != null) settingsPanelRoot.SetActive(false);
        }

        public void RestartGame()
        {
            // Return to normal time & cursor first
            Time.timeScale = 1f;
            ApplyGameplayCursorState();

            // BoolFlag and other scene-scope state will reset on scene reload
            var scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.buildIndex);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            if (!string.IsNullOrEmpty(mainMenuSceneName))
                SceneManager.LoadScene(mainMenuSceneName);
            else
                Debug.LogWarning("[GameManager] Main menu scene name is not set.");
        }

        private void ApplyGameplayCursorState()
        {
            if (lockCursorDuringGameplay)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}
