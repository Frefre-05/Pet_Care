using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private bool musicMuted = false;
    private bool sfxMuted = false;

    // Called by UI Button: "Options"
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // Called by UI Button: "Back"
    public void BackToMenu()
    {
        settingsPanel.SetActive(false);
        SceneManager.LoadScene("MainMenu"); // change to your menu scene name
    }

    // Called by UI Button: "Toggle Music"
    public void ToggleMusic()
    {
        musicMuted = !musicMuted;
        musicSource.mute = musicMuted;
    }

    // Called by UI Button: "Toggle SFX"
    public void ToggleSFX()
    {
        sfxMuted = !sfxMuted;
        sfxSource.mute = sfxMuted;
    }
}
