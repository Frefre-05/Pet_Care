using UnityEngine;
using System;

public class LevelProgress : MonoBehaviour
{
    // 0 = Tutorial playable only, 1 = Level1 unlocked, 2 = Level2, 3 = Level3, 4 = Level4
    public static int MaxUnlocked { get; private set; }
    public static event Action OnProgressChanged;

    const string KEY = "MAX_UNLOCKED";
    static bool initialized;
    static LevelProgress instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetRuntimeStatics()
    {
        initialized = false;
        instance = null;
        MaxUnlocked = 0;
    }

    void Awake()
    {
        // Singleton + persistent
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        MaxUnlocked = Mathf.Clamp(PlayerPrefs.GetInt(KEY, 0), 0, 4); // default: only Tutorial
        initialized = true;
    }

    static void EnsureInitialized()
    {
        if (initialized) return;
        MaxUnlocked = Mathf.Clamp(PlayerPrefs.GetInt(KEY, 0), 0, 4);
        initialized = true;
    }

    public static bool CanPlay(int requiredIndex)
    {
        EnsureInitialized();
        return MaxUnlocked >= requiredIndex;
    }

    // Call when a level is completed. Pass its index (see mapping below).
    public static void MarkCompleted(int justFinishedIndex)
    {
        EnsureInitialized();

        // Only allow sequential progression: you can only complete a level that is currently unlocked.
        // This prevents accidental "unlock all" if an endpoint is misconfigured with a wrong index.
        if (justFinishedIndex > MaxUnlocked)
        {
            Debug.LogWarning($"[LevelProgress] Ignored out-of-order completion index {justFinishedIndex}. Current MaxUnlocked is {MaxUnlocked}.");
            return;
        }

        // Unlock next one
        int next = Mathf.Clamp(justFinishedIndex + 1, 0, 4);
        if (next > MaxUnlocked)
        {
            MaxUnlocked = next;
            PlayerPrefs.SetInt(KEY, MaxUnlocked);
            PlayerPrefs.Save();
            OnProgressChanged?.Invoke();
        }
    }

    // For debugging, you can add a reset method if you want.
    public static void ResetAll()
    {
        EnsureInitialized();
        MaxUnlocked = 0;
        PlayerPrefs.SetInt(KEY, 0);
        PlayerPrefs.Save();
        OnProgressChanged?.Invoke();
    }
}
