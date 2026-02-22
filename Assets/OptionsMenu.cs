using UnityEngine;

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
        if (settingsPanel == null)
            settingsPanel = FindSettingsPanelFallback();

        if (settingsPanel == null)
        {
            Debug.LogWarning("[OptionsMenu] settingsPanel is not assigned.");
            return;
        }

        settingsPanel.SetActive(true);
        CanvasGroup cg = settingsPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
    }

    // Called by UI Button: "Back"
    public void BackToMenu()
    {
        if (settingsPanel == null) return;

        CanvasGroup cg = settingsPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
        settingsPanel.SetActive(false);
    }

    // Called by UI Button: "Toggle Music"
    public void ToggleMusic()
    {
        // If options button was wired here by mistake, open panel first (do not mute).
        if (!IsSettingsOpen())
        {
            OpenSettings();
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusicMute();
            musicMuted = AudioManager.Instance.IsMusicMuted;
            return;
        }

        musicMuted = !musicMuted;
        if (musicSource != null) musicSource.mute = musicMuted;
    }

    // Called by UI Button: "Toggle SFX"
    public void ToggleSFX()
    {
        if (!IsSettingsOpen())
        {
            OpenSettings();
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleSfxMute();
            sfxMuted = AudioManager.Instance.IsSfxMuted;
            return;
        }

        sfxMuted = !sfxMuted;
        if (sfxSource != null) sfxSource.mute = sfxMuted;
    }

    private bool IsSettingsOpen()
    {
        if (settingsPanel == null)
            settingsPanel = FindSettingsPanelFallback();

        if (settingsPanel == null || !settingsPanel.activeInHierarchy)
            return false;

        CanvasGroup cg = settingsPanel.GetComponent<CanvasGroup>();
        if (cg == null) return true;

        return cg.alpha > 0.01f && cg.interactable && cg.blocksRaycasts;
    }

    private GameObject FindSettingsPanelFallback()
    {
        GameObject found = GameObject.Find("SettingsPanel");
        if (found != null) return found;
        found = GameObject.Find("OptionsPanel");
        if (found != null) return found;
        return GameObject.Find("Settings");
    }
}
