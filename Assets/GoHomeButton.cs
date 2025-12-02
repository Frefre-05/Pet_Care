using UnityEngine;
using UnityEngine.SceneManagement;

public class GoHomeButton : MonoBehaviour
{
    [Header("Name of your home scene")]
    public string homeSceneName = "House"; // <- change in Inspector if needed

    public void GoHome()
    {
        SceneManager.LoadScene(homeSceneName);
    }
}
