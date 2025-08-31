// File: GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nat
{
    /// <summary>
    /// ESC to pause/resume. If Settings (or deeper panel) is open, ESC returns to the main pause panel first.
    /// Has separate Pause Root (entire overlay/canvas) and Pause Panel (home panel).
    /// Provides UI button hooks to Resume, Open Settings, Back to Pause Menu, Restart, and Go To Main Menu.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("UI Roots")]
        [Tooltip("Top-level pause overlay/canvas that's enabled when pausing.")]
        [SerializeField] private GameObject pauseRoot;          // NEW: overall pause canvas/overlay
        [Tooltip("The main/home panel inside the pause overlay.")]
        [SerializeField] private GameObject pauseMenuRoot;      // main pause panel (home)
        [Tooltip("Settings (or any deeper) panel inside the pause overlay.")]
        [SerializeField] private GameObject settingsPanelRoot;  // deeper panel (e.g., settings)
        [SerializeField] private GameObject overlaysToHideOnResume; // optional extra overlays to hide on resume

        [Header("Scenes")]
        [Tooltip("Name of the Main Menu scene to load when 'Main Menu' is pressed.")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";

        [Header("Cursor")]
        [Tooltip("Lock cursor when gameplay is active.")]
        [SerializeField] private bool lockCursorDuringGameplay = true;

        public bool IsPaused { get; private set; }

        // Pause input lock (used by Win/Death/etc.)
        private bool pauseInputLocked = false;

        // Public setter so other systems (like Win/Death) can lock/unlock ESC handling
        public void SetPauseInputLocked(bool locked) => pauseInputLocked = locked;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            // Optional: DontDestroyOnLoad(gameObject);

            // Ensure initial UI state
            SafeSetActive(pauseRoot, false);
            SafeSetActive(pauseMenuRoot, false);
            SafeSetActive(settingsPanelRoot, false);

            ApplyGameplayCursorState();
        }

        private void Update()
        {
            if (pauseInputLocked) return; // Ignore ESC while locked (e.g., during Win/Death)

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!IsPaused)
                {
                    Pause();
                }
                else
                {
                    // If a deeper panel (like Settings) is open, go back to main pause panel first
                    if (settingsPanelRoot != null && settingsPanelRoot.activeSelf)
                    {
                        ShowPauseHome();
                    }
                    else
                    {
                        Resume();
                    }
                }
            }
        }

        // -------- Public API for UI Buttons --------

        public void OnPressResume() => Resume();

        public void OnPressOpenSettings()
        {
            if (!IsPaused) Pause();
            SafeSetActive(pauseRoot, true);
            SafeSetActive(pauseMenuRoot, false);
            SafeSetActive(settingsPanelRoot, true);
        }

        public void OnPressBackToPauseMenu() => ShowPauseHome();

        public void OnPressRestart() => RestartGame();

        public void OnPressMainMenu() => GoToMainMenu();

        // --------------- Core ----------------------

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;

            SafeSetActive(pauseRoot, true);
            SafeSetActive(pauseMenuRoot, true);
            SafeSetActive(settingsPanelRoot, false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;

            SafeSetActive(pauseRoot, false);
            SafeSetActive(pauseMenuRoot, false);
            SafeSetActive(settingsPanelRoot, false);
            SafeSetActive(overlaysToHideOnResume, false);

            ApplyGameplayCursorState();
        }

        private void ShowPauseHome()
        {
            if (!IsPaused) Pause();
            SafeSetActive(pauseRoot, true);
            SafeSetActive(pauseMenuRoot, true);
            SafeSetActive(settingsPanelRoot, false);
        }

        public void RestartGame()
        {
            // Return to normal time & cursor first
            Time.timeScale = 1f;
            ApplyGameplayCursorState();

            // Reload current scene (BoolFlags and scene-scope state reset on load)
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

        private static void SafeSetActive(GameObject go, bool state)
        {
            if (go != null && go.activeSelf != state) go.SetActive(state);
        }
    }
}
