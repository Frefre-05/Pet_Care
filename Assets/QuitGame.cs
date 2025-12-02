using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void ExitGame()
    {
        Debug.Log("Quit Game"); // This shows in the editor so you know the button works
        Application.Quit();
    }
}