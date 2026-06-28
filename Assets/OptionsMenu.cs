using TMPro;
using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject settingsPanel;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("SFX Feedback")]
    [SerializeField] private float sfxStatusTopInset = 24f;

    private bool musicMuted = false;
    private bool sfxMuted = false;
    private TextMeshProUGUI sfxStatusText;

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

        RefreshAudioStateCache();
        UpdateSfxStatusText(forceShow: false);
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
            musicMuted = AudioManager.Instance.IsMusicPaused;
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
        }
        else
        {
            sfxMuted = !sfxMuted;
            if (sfxSource != null) sfxSource.mute = sfxMuted;
        }

        UpdateSfxStatusText(forceShow: true);
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

    private void RefreshAudioStateCache()
    {
        if (AudioManager.Instance != null)
        {
            musicMuted = AudioManager.Instance.IsMusicPaused;
            sfxMuted = AudioManager.Instance.IsSfxMuted;
            return;
        }

        if (musicSource != null)
            musicMuted = musicSource.mute;
        if (sfxSource != null)
            sfxMuted = sfxSource.mute;
    }

    private void UpdateSfxStatusText(bool forceShow)
    {
        EnsureSfxStatusText();
        if (sfxStatusText == null)
            return;

        sfxStatusText.text = sfxMuted
            ? "Sound Effects are now disabled"
            : "Sound Effects are now active";
        sfxStatusText.color = sfxMuted
            ? new Color32(255, 196, 96, 255)
            : new Color32(156, 255, 168, 255);
        sfxStatusText.gameObject.SetActive(forceShow || IsSettingsOpen());
    }

    private void EnsureSfxStatusText()
    {
        if (sfxStatusText != null)
            return;

        if (settingsPanel == null)
            settingsPanel = FindSettingsPanelFallback();
        if (settingsPanel == null)
            return;

        Transform existing = settingsPanel.transform.Find("ToggleSfxStatusText");
        if (existing != null)
        {
            sfxStatusText = existing.GetComponent<TextMeshProUGUI>();
            if (sfxStatusText != null)
                return;
        }

        GameObject textObject = new GameObject("ToggleSfxStatusText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.SetParent(settingsPanel.transform, false);
        textRect.SetAsLastSibling();
        textRect.anchorMin = new Vector2(0.5f, 1f);
        textRect.anchorMax = new Vector2(0.5f, 1f);
        textRect.pivot = new Vector2(0.5f, 1f);
        textRect.anchoredPosition = new Vector2(0f, -sfxStatusTopInset);
        textRect.sizeDelta = new Vector2(520f, 42f);

        sfxStatusText = textObject.GetComponent<TextMeshProUGUI>();
        sfxStatusText.alignment = TextAlignmentOptions.Center;
        sfxStatusText.fontSize = 22f;
        sfxStatusText.outlineWidth = 0.22f;
        sfxStatusText.outlineColor = new Color32(0, 0, 0, 255);
        sfxStatusText.raycastTarget = false;
        sfxStatusText.text = string.Empty;
        sfxStatusText.gameObject.SetActive(false);
    }
}
