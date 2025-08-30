using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneSwitcher : MonoBehaviour
{
    [Header("Scene Switch Settings")]
    [Tooltip("Time in seconds before switching scenes.")]
    public float delayInSeconds = 5f;

    [Tooltip("The name of the scene to switch to.")]
    public string sceneToLoad;

    private void Start()
    {
        // Start the coroutine when the object loads
        StartCoroutine(SwitchSceneAfterDelay());
    }

    private System.Collections.IEnumerator SwitchSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayInSeconds);
        LoadScene();
    }

    /// <summary>
    /// Public function that can be called from a UI Button
    /// to load the scene immediately.
    /// </summary>
    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("SceneSwitcher: No scene name set in the Inspector.");
        }
    }
}
