using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class LoadButtonAuto : MonoBehaviour
{
    [Header("Choose one")]
    public bool useSceneName = false;
    public string sceneName; // e.g. "Level 1"
    public int sceneIndex = -1; // e.g. 4 (from Build Settings)

    [Header("Options")]
    public bool unpauseBeforeLoad = true;

    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Load);
    }

    public void Load()
    {
        if (unpauseBeforeLoad) Time.timeScale = 1f;

        if (useSceneName && !string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else if (!useSceneName && sceneIndex >= 0)
            SceneManager.LoadScene(sceneIndex);
        else
            Debug.LogWarning($"{name}: No scene set.");
    }
}
