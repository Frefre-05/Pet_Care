using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public void PlayGame()
    {
        SceneTransitionLoader.LoadScene(1);
    }
}
