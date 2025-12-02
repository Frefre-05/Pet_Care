using UnityEngine;
using System;

public static class AppleCurrency
{
    // PlayerPrefs key
    private const string Key = "APPLE_COUNT";

    // Fired whenever the apple amount changes (after Set / Add / Spend)
    public static event Action<int> OnChanged;

    // Current amount
    public static int Get() => PlayerPrefs.GetInt(Key, 0);

    // Set exact amount (never below 0)
    public static void Set(int value)
    {
        if (value < 0) value = 0;

        int old = Get();
        if (old == value)
        {
            // still save so it exists in prefs
            PlayerPrefs.SetInt(Key, value);
            PlayerPrefs.Save();
            return;
        }

        PlayerPrefs.SetInt(Key, value);
        PlayerPrefs.Save();

        OnChanged?.Invoke(value); // notify HUD, etc.
    }

    // Add apples (can be negative if you really want)
    public static void Add(int amount) => Set(Get() + amount);

    // Check if player has enough
    public static bool Has(int amount) => Get() >= amount;

    // Spend apples, returns true if it worked
    public static bool Spend(int amount)
    {
        if (amount <= 0) return true; // spending 0 or less is always ok

        int have = Get();
        if (have < amount) return false; // not enough

        Set(have - amount);
        return true;
    }

    // Reset to 0 (you can call this from a reset-save button)
    public static void Clear() => Set(0);

#if UNITY_EDITOR
// Small editor helper: menu item to reset apples while testing
[UnityEditor.MenuItem("Debug/Currency/Reset Apples")]
private static void DebugResetApples()
{
Clear();
Debug.Log("AppleCurrency: reset apples to 0");
}
#endif
}
