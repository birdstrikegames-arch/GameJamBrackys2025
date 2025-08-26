using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class UILoadingController : MonoBehaviour
{
    [Header("References")]
    public GameObject loadingPanel;

    [Header("Settings")]
    public float loadingDelay = 5f;

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneRoutine(sceneIndex));
    }

    private IEnumerator LoadSceneRoutine(int sceneIndex)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        yield return new WaitForSeconds(loadingDelay);

        SceneManager.LoadScene(sceneIndex);
    }
}
