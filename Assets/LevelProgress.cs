using UnityEngine;
using System;

public class LevelProgress : MonoBehaviour
{
    // 0 = Tutorial playable only, 1 = Level1 unlocked, 2 = Level2, 3 = Level3, 4 = Level4
    public static int MaxUnlocked { get; private set; }
    public static event Action OnProgressChanged;

    const string KEY = "MAX_UNLOCKED";

    void Awake()
    {
        // Singleton-ish & persistent
        if (FindObjectOfType<LevelProgress>() != this)
        {
            if (FindObjectOfType<LevelProgress>() != null && FindObjectOfType<LevelProgress>() != this)
            { Destroy(gameObject); return; }
        }
        DontDestroyOnLoad(gameObject);

        MaxUnlocked = PlayerPrefs.GetInt(KEY, 0); // default: only Tutorial
    }

    public static bool CanPlay(int requiredIndex) => MaxUnlocked >= requiredIndex;

    // Call when a level is completed. Pass its index (see mapping below).
    public static void MarkCompleted(int justFinishedIndex)
    {
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
        MaxUnlocked = 0;
        PlayerPrefs.SetInt(KEY, 0);
        PlayerPrefs.Save();
        OnProgressChanged?.Invoke();
    }
}
