using UnityEngine;
using System;

public class LevelProgress : MonoBehaviour
{
    // 0 = Tutorial playable only, 1 = Level1 unlocked, 2 = Level2, 3 = Level3, 4 = Level4
    public static int MaxUnlocked { get; private set; }
    public static event Action OnProgressChanged;

    const string KEY = "MAX_UNLOCKED";
    static string CompletedKey(int levelIndex) => "LEVEL_COMPLETED_" + Mathf.Clamp(levelIndex, 0, 4);
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
        requiredIndex = Mathf.Clamp(requiredIndex, 0, 4);
        if (MaxUnlocked >= requiredIndex)
            return true;

        // Never relock a level the player has already completed.
        return IsCompleted(requiredIndex);
    }

    // Call when a level is completed. Pass its index (see mapping below).
    public static void MarkCompleted(int justFinishedIndex)
    {
        EnsureInitialized();
        justFinishedIndex = Mathf.Clamp(justFinishedIndex, 0, 4);
        PlayerPrefs.SetInt(CompletedKey(justFinishedIndex), 1);

        // Only allow sequential progression: you can only complete a level that is currently unlocked.
        // This prevents accidental "unlock all" if an endpoint is misconfigured with a wrong index.
        if (justFinishedIndex > MaxUnlocked)
        {
            PlayerPrefs.Save();
            OnProgressChanged?.Invoke();
            Debug.LogWarning($"[LevelProgress] Ignored out-of-order completion index {justFinishedIndex}. Current MaxUnlocked is {MaxUnlocked}.");
            return;
        }

        // Unlock next one
        int next = Mathf.Clamp(justFinishedIndex + 1, 0, 4);
        if (next > MaxUnlocked)
        {
            MaxUnlocked = next;
            PlayerPrefs.SetInt(KEY, MaxUnlocked);
        }

        PlayerPrefs.Save();
        OnProgressChanged?.Invoke();
    }

    // For debugging, you can add a reset method if you want.
    public static void ResetAll()
    {
        EnsureInitialized();
        MaxUnlocked = 0;
        PlayerPrefs.SetInt(KEY, 0);
        for (int i = 0; i <= 4; i++)
            PlayerPrefs.DeleteKey(CompletedKey(i));
        PlayerPrefs.Save();
        OnProgressChanged?.Invoke();
    }

    public static bool IsCompleted(int levelIndex)
    {
        EnsureInitialized();
        levelIndex = Mathf.Clamp(levelIndex, 0, 4);
        return PlayerPrefs.GetInt(CompletedKey(levelIndex), 0) == 1;
    }
}
