using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private const string MusicMutedKey = "MUSIC_MUTED";
    private const string SfxMutedKey = "SFX_MUTED";

    // === Singleton ===
    public static AudioManager Instance { get; private set; }

    [Header("----- Audio Source -----")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource SFXSource;
    [SerializeField] private float musicFadeDurationMin = 1.5f;
    [SerializeField] private float musicFadeDurationMax = 1.9f;

    [Header("----- SFX Variation -----")]
    [SerializeField] private bool enableButtonClickVolumeVariation = true;
    [SerializeField] private bool enableJumpVolumeVariation = true;
    [Range(0.5f, 1f)] [SerializeField] private float buttonClickMinVolumeMultiplier = 0.8f;
    [Range(0.5f, 1f)] [SerializeField] private float jumpMinVolumeMultiplier = 0.8f;

    [Header("----- Audio Clip -----")]
    public AudioClip background;
    public AudioClip death;
    public AudioClip checkpoint;
    public AudioClip jump;
    public AudioClip footsteps;
    public AudioClip endpoint;
    public AudioClip foodcollect;
    public AudioClip buttonClick;
    public AudioClip purchaseSuccess;
    public AudioClip purchaseFailed;

    private Coroutine musicFadeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Important for build: scene-local AudioManager gets created, but singleton survives.
            // Copy the new scene's configured music into the persistent instance before destroying this duplicate.
            if (background != null)
            {
                Instance.PlayMusic(background, false, Instance.GetRandomFadeDuration());
            }

            if (buttonClick != null && Instance.buttonClick == null)
            {
                Instance.buttonClick = buttonClick;
            }

            if (jump != null && Instance.jump == null)
            {
                Instance.jump = jump;
            }

            if (purchaseSuccess != null && Instance.purchaseSuccess == null)
            {
                Instance.purchaseSuccess = purchaseSuccess;
            }

            if (purchaseFailed != null && Instance.purchaseFailed == null)
            {
                Instance.purchaseFailed = purchaseFailed;
            }

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        PlayMusic(background, true);
        ApplySavedAudioState();
        RegisterButtonClickHooks();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    public void PlayMusic(AudioClip clip, bool instant = false, float fadeDurationOverride = -1f)
    {
        if (musicSource == null || clip == null)
        {
            return;
        }

        bool pauseRequested = IsMusicPauseRequested();
        bool sameClip = musicSource.clip == clip;
        if (sameClip && (musicSource.isPlaying || pauseRequested))
        {
            return;
        }

        if (musicFadeRoutine != null)
        {
            StopCoroutine(musicFadeRoutine);
            musicFadeRoutine = null;
        }

        if (instant)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.volume = 1f;
            if (pauseRequested)
            {
                musicSource.Play();
                musicSource.Pause();
            }
            else
            {
                musicSource.Play();
            }
            return;
        }

        float fade = fadeDurationOverride >= 0f ? fadeDurationOverride : GetRandomFadeDuration();
        musicFadeRoutine = StartCoroutine(FadeToClip(clip, fade, pauseRequested));
    }

    private float GetRandomFadeDuration()
    {
        float min = Mathf.Max(0.01f, musicFadeDurationMin);
        float max = Mathf.Max(min, musicFadeDurationMax);
        return Random.Range(min, max);
    }

    private IEnumerator FadeToClip(AudioClip newClip, float fadeDuration, bool pauseRequested)
    {
        if (musicSource == null)
        {
            yield break;
        }

        float duration = Mathf.Max(0.01f, fadeDuration);
        float startVolume = Mathf.Clamp01(musicSource.volume <= 0f ? 1f : musicSource.volume);
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            musicSource.volume = Mathf.Lerp(startVolume, 0f, k);
            yield return null;
        }

        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();

        if (pauseRequested)
        {
            musicSource.Pause();
            musicSource.volume = 1f;
            musicFadeRoutine = null;
            yield break;
        }

        t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            musicSource.volume = Mathf.Lerp(0f, 1f, k);
            yield return null;
        }

        musicSource.volume = 1f;
        musicFadeRoutine = null;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || SFXSource == null) return;
        SFXSource.PlayOneShot(clip);
    }

    public void PlayButtonClickSfx()
    {
        if (buttonClick == null)
            return;

        PlaySFXWithRandomizedVolume(buttonClick, enableButtonClickVolumeVariation ? buttonClickMinVolumeMultiplier : 1f);
    }

    public void PlayJumpSfx()
    {
        if (jump == null)
            return;

        PlaySFXWithRandomizedVolume(jump, enableJumpVolumeVariation ? jumpMinVolumeMultiplier : 1f);
    }

    public void PlayPurchaseSuccessSfx()
    {
        if (purchaseSuccess == null)
            return;

        PlaySFX(purchaseSuccess);
    }

    public void PlayPurchaseFailedSfx()
    {
        if (purchaseFailed == null)
            return;

        PlaySFX(purchaseFailed);
    }


    public bool IsMusicMuted => musicSource != null && musicSource.mute;
    public bool IsMusicPaused => musicSource != null && !musicSource.isPlaying && musicSource.clip != null;
    public bool IsSfxMuted => SFXSource != null && SFXSource.mute;

    public void ToggleMusicMute()
    {
        SetMusicPaused(!IsMusicPaused);
    }

    public void SetMusicMuted(bool muted)
    {
        SetMusicPaused(muted);
    }

    public void SetMusicPaused(bool paused)
    {
        if (musicFadeRoutine != null)
        {
            StopCoroutine(musicFadeRoutine);
            musicFadeRoutine = null;
        }

        if (musicSource != null)
        {
            if (paused)
            {
                musicSource.Pause();
            }
            else
            {
                musicSource.UnPause();
                if (!musicSource.isPlaying && musicSource.clip != null)
                {
                    musicSource.Play();
                }
            }

            musicSource.mute = false;
        }

        PlayerPrefs.SetInt(MusicMutedKey, paused ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSfxMute()
    {
        SetSfxMuted(!IsSfxMuted);
    }

    public void SetSfxMuted(bool muted)
    {
        if (SFXSource != null) SFXSource.mute = muted;
        PlayerPrefs.SetInt(SfxMutedKey, muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplySavedAudioState()
    {
        bool musicPaused = PlayerPrefs.GetInt(MusicMutedKey, 0) == 1;
        bool sfxMuted = PlayerPrefs.GetInt(SfxMutedKey, 0) == 1;
        SetMusicPaused(musicPaused);
        if (SFXSource != null) SFXSource.mute = sfxMuted;
    }

    private bool IsMusicPauseRequested()
    {
        return PlayerPrefs.GetInt(MusicMutedKey, 0) == 1;
    }

    private void PlaySFXWithRandomizedVolume(AudioClip clip, float minVolumeMultiplier)
    {
        if (clip == null || SFXSource == null)
            return;

        float minMultiplier = Mathf.Clamp(minVolumeMultiplier, 0.5f, 1f);
        float randomizedVolume = Random.Range(minMultiplier, 1f);
        float originalVolume = SFXSource.volume;
        SFXSource.PlayOneShot(clip, randomizedVolume);
        SFXSource.volume = originalVolume;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterButtonClickHooks();
    }

    private void RegisterButtonClickHooks()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null || !button.gameObject.scene.IsValid() || !button.gameObject.scene.isLoaded)
                continue;

            ButtonClickSfxHook hook = button.GetComponent<ButtonClickSfxHook>();
            if (hook == null)
                hook = button.gameObject.AddComponent<ButtonClickSfxHook>();

            hook.Initialize(button);
        }
    }
}
