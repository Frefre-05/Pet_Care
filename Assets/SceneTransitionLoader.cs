using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class SceneTransitionLoader : MonoBehaviour
{
    public static SceneTransitionLoader Instance { get; private set; }

    [Header("Scene Fade")]
    [SerializeField] private bool enableSceneFade = true;
    [SerializeField] private float fadeOutDuration = 0.75f;
    [SerializeField] private float fadeInDuration = 1.25f;

    private static bool isLoading;

    // Reversible switch: set to false to disable all transition fades without removing calls.
    public static bool ForceDisableTransitions { get; set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoBootstrap()
    {
        EnsureInstance();
    }

    public static SceneTransitionLoader EnsureInstance()
    {
        if (Instance != null) return Instance;

        GameObject go = new GameObject("__SceneTransitionLoader");
        DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideInHierarchy;
        Instance = go.AddComponent<SceneTransitionLoader>();
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        EnsureInstance().StartCoroutine(EnsureInstance().LoadRoutine(() => SceneManager.LoadScene(sceneName), -1f, -1f));
    }

    public static void LoadScene(int sceneIndex)
    {
        EnsureInstance().StartCoroutine(EnsureInstance().LoadRoutine(() => SceneManager.LoadScene(sceneIndex), -1f, -1f));
    }

    public static void LoadScene(string sceneName, float fadeOut, float fadeIn)
    {
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        EnsureInstance().StartCoroutine(EnsureInstance().LoadRoutine(() => SceneManager.LoadScene(sceneName), fadeOut, fadeIn));
    }

    public static void LoadScene(int sceneIndex, float fadeOut, float fadeIn)
    {
        EnsureInstance().StartCoroutine(EnsureInstance().LoadRoutine(() => SceneManager.LoadScene(sceneIndex), fadeOut, fadeIn));
    }

    public static bool IsTransitioning()
    {
        return isLoading;
    }

    private IEnumerator LoadRoutine(Action loadAction, float fadeOutOverride, float fadeInOverride)
    {
        if (isLoading || loadAction == null) yield break;
        isLoading = true;

        float outDuration = fadeOutOverride >= 0f ? fadeOutOverride : fadeOutDuration;
        float inDuration = fadeInOverride >= 0f ? fadeInOverride : fadeInDuration;
        _pendingFadeInDuration = inDuration;

        if (ShouldFade())
        {
            BlackoutFaders fader = AutoBlackoutFader.EnsureInstance();
            if (fader != null)
            {
                yield return fader.FadeOut(outDuration);
            }
        }

        loadAction.Invoke();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        yield return null;

        if (ShouldFade())
        {
            BlackoutFaders fader = AutoBlackoutFader.EnsureInstance();
            if (fader != null)
            {
                yield return fader.FadeIn(_pendingFadeInDuration);
            }
        }

        isLoading = false;
    }

    private bool ShouldFade()
    {
        return enableSceneFade && !ForceDisableTransitions;
    }

    private float _pendingFadeInDuration = 1.25f;
}
