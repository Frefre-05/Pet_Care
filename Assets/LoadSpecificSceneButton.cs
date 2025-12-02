using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadSpecificSceneButton : MonoBehaviour
{
    public enum RefType { ByName, ByIndex }
    public RefType reference = RefType.ByName;

    [Header("Pick ONE")]
    public string sceneName; // e.g. "Level 1"
    public int sceneIndex = 0; // e.g. 3 (from Build Settings order)

    [Header("Options")]
    public bool unpauseBeforeLoad = true;

    void Awake()
    {
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(Load);
    }

    public void Load()
    {
        if (unpauseBeforeLoad) Time.timeScale = 1f;

        if (reference == RefType.ByName && !string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            SceneManager.LoadScene(sceneIndex);
    }
}
