using UnityEngine;
using UnityEngine.SceneManagement;

public class PetSelection : MonoBehaviour
{
    
    public void PlayGame()
    {
        SceneManager.LoadSceneAsync(2);
    }
}
