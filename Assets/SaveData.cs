using System.Collections.Generic;
using UnityEngine;

public static class SaveData
{
    // PlayerPrefs keys
    const string APPLE_COUNT_KEY = "APPLE_COUNT";
    const string FRUITS_KEY = "COLLECTED_FRUITS"; // CSV string
    const string ENERGY_FLAG_KEY = "RESTORE_ENERGY_ON_LOAD";

    // In-memory cache
    static bool loaded = false;
    static HashSet<string> collected = new HashSet<string>();
    static int appleCountCache = 0;

    static void EnsureLoaded()
    {
        if (loaded) return;
        appleCountCache = PlayerPrefs.GetInt(APPLE_COUNT_KEY, 0);

        collected.Clear();
        var csv = PlayerPrefs.GetString(FRUITS_KEY, "");
        if (!string.IsNullOrEmpty(csv))
        {
            var parts = csv.Split(',');
            foreach (var p in parts) if (!string.IsNullOrWhiteSpace(p)) collected.Add(p);
        }
        loaded = true;
    }

    public static int AppleCount
    {
        get { EnsureLoaded(); return appleCountCache; }
        set { EnsureLoaded(); appleCountCache = Mathf.Max(0, value); }
    }

    public static bool IsFruitCollected(string id)
    {
        EnsureLoaded();
        return !string.IsNullOrEmpty(id) && collected.Contains(id);
    }

    public static void MarkFruitCollected(string id)
    {
        EnsureLoaded();
        if (string.IsNullOrEmpty(id)) return;
        if (collected.Add(id)) SaveCollectedOnly();
    }

    public static void AddApples(int amount)
    {
        EnsureLoaded();
        appleCountCache += Mathf.Max(0, amount);
        PlayerPrefs.SetInt(APPLE_COUNT_KEY, appleCountCache);
        PlayerPrefs.Save();
    }

    static void SaveCollectedOnly()
    {
        PlayerPrefs.SetString(FRUITS_KEY, string.Join(",", collected));
        PlayerPrefs.Save();
    }

    // --- Energy restore on next Home load ---
    public static void FlagRestoreEnergyOnNextLoad() // call in LevelExit
    {
        PlayerPrefs.SetInt(ENERGY_FLAG_KEY, 1);
        PlayerPrefs.Save();
    }

    public static bool ConsumeRestoreEnergyFlag() // call in PetNeeds.Start (Home scene)
    {
        var v = PlayerPrefs.GetInt(ENERGY_FLAG_KEY, 0) == 1;
        if (v) { PlayerPrefs.DeleteKey(ENERGY_FLAG_KEY); PlayerPrefs.Save(); }
        return v;
    }
}
