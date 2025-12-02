using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    // Called by the button OnClick()
    public void RestartLevel()
    {
        // Reset saved apple data (so all apples reappear)
        PlayerPrefs.DeleteAll(); // or use PlayerPrefs.DeleteKey("FRUIT_...") if you want selective clearing
        PlayerPrefs.Save();

        // Reload current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
