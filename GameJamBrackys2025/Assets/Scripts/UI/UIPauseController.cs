using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenuController : MonoBehaviour
{
    [Header("References")]
    public GameObject pausePanel;
    public GameObject loadingPanel;

    [Header("Settings")]
    public float loadingDelay = 4f;

    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void RestartGame()
    {
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = false;

        yield return new WaitForSecondsRealtime(loadingDelay);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu(int sceneIndex)
    {
        StartCoroutine(LoadMainMenuRoutine(sceneIndex));
    }

    private IEnumerator LoadMainMenuRoutine(int sceneIndex)
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        Time.timeScale = 0f;
        isPaused = false;

        yield return new WaitForSecondsRealtime(loadingDelay);

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneIndex);
    }
}