using UnityEngine;
using System.Collections;

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

    [Header("----- Audio Clip -----")]
    public AudioClip background;
    public AudioClip death;
    public AudioClip checkpoint;
    public AudioClip jump;
    public AudioClip footsteps;
    public AudioClip endpoint;
    public AudioClip foodcollect;

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

            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayMusic(background, true);
        ApplySavedAudioState();
    }

    public void PlayMusic(AudioClip clip, bool instant = false, float fadeDurationOverride = -1f)
    {
        if (musicSource == null || clip == null)
        {
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
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
            musicSource.Play();
            return;
        }

        float fade = fadeDurationOverride >= 0f ? fadeDurationOverride : GetRandomFadeDuration();
        musicFadeRoutine = StartCoroutine(FadeToClip(clip, fade));
    }

    private float GetRandomFadeDuration()
    {
        float min = Mathf.Max(0.01f, musicFadeDurationMin);
        float max = Mathf.Max(min, musicFadeDurationMax);
        return Random.Range(min, max);
    }

    private IEnumerator FadeToClip(AudioClip newClip, float fadeDuration)
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

    public bool IsMusicMuted => musicSource != null && musicSource.mute;
    public bool IsSfxMuted => SFXSource != null && SFXSource.mute;

    public void ToggleMusicMute()
    {
        SetMusicMuted(!IsMusicMuted);
    }

    public void SetMusicMuted(bool muted)
    {
        if (musicSource != null) musicSource.mute = muted;
        PlayerPrefs.SetInt(MusicMutedKey, muted ? 1 : 0);
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
        bool musicMuted = PlayerPrefs.GetInt(MusicMutedKey, 0) == 1;
        bool sfxMuted = PlayerPrefs.GetInt(SfxMutedKey, 0) == 1;
        if (musicSource != null) musicSource.mute = musicMuted;
        if (SFXSource != null) SFXSource.mute = sfxMuted;
    }
}
