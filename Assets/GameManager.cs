using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI (Assign One or Both)")]
    [SerializeField] private Text appleText; // Legacy UI.Text
    [SerializeField] private TMP_Text appleTMP; // TextMeshPro TMP_Text (recommended)

    // --- Apple tracking ---
    public int AppleCount { get; private set; } = 0;

    // --- PlayerPrefs Keys ---
    private const string APPLE_COUNT_KEY = "APPLE_COUNT";
    private const string COLLECTED_FRUITS_KEY = "COLLECTED_FRUITS";

    // --- Tracking collected fruits (by ID) ---
    private HashSet<string> collected = new HashSet<string>();

    private void Start()
    {
        // TEMPORARY: comment this out once you want progress to persist
        // PlayerPrefs.DeleteKey(APPLE_COUNT_KEY);
        // PlayerPrefs.DeleteKey(COLLECTED_FRUITS_KEY);

        // Load saved data
        LoadAppleCount();
        LoadCollectedFruits();

        UpdateAppleUI();
    }

    // ==================== APPLE MANAGEMENT ====================

    public void AddApple(int amount = 1)
    {
        AppleCount += amount;
        UpdateAppleUI();
        SaveAppleCount();
    }

    public void SetAppleCount(int value)
    {
        AppleCount = value;
        UpdateAppleUI();
        SaveAppleCount();
    }

    private void UpdateAppleUI()
    {
        // Show apple count on legacy UI
        if (appleText != null)
            appleText.text = AppleCount.ToString();

        // Show apple count on TextMeshPro UI
        if (appleTMP != null)
            appleTMP.text = AppleCount.ToString();
    }

    private void SaveAppleCount()
    {
        PlayerPrefs.SetInt(APPLE_COUNT_KEY, AppleCount);
        PlayerPrefs.Save();
    }

    private void LoadAppleCount()
    {
        AppleCount = PlayerPrefs.GetInt(APPLE_COUNT_KEY, 0);
    }

    // ==================== FRUIT COLLECTOR MANAGEMENT ====================

    public void MarkFruitCollected(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        collected.Add(id);
        SaveCollectedFruits();
    }

    public bool IsFruitCollected(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return collected.Contains(id);
    }

    private void SaveCollectedFruits()
    {
        string joined = string.Join(",", collected);
        PlayerPrefs.SetString(COLLECTED_FRUITS_KEY, joined);
        PlayerPrefs.Save();
    }

    private void LoadCollectedFruits()
    {
        string data = PlayerPrefs.GetString(COLLECTED_FRUITS_KEY, "");
        collected.Clear();

        if (!string.IsNullOrEmpty(data))
        {
            string[] parts = data.Split(',');
            foreach (string p in parts)
            {
                if (!string.IsNullOrEmpty(p))
                    collected.Add(p);
            }
        }
    }

    // ==================== RESET TOOLS ====================

    [ContextMenu("Reset Apple Data")]
    public void ResetAppleData()
    {
        PlayerPrefs.DeleteKey(APPLE_COUNT_KEY);
        PlayerPrefs.DeleteKey(COLLECTED_FRUITS_KEY);
        PlayerPrefs.Save();
        collected.Clear();
        AppleCount = 0;
        UpdateAppleUI();
        Debug.Log("Apple data reset complete.");
    }
}
