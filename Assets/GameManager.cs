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
    public int AppleCount => AppleCurrency.Get();

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

        LoadCollectedFruits();

        UpdateAppleUI();
    }

    private void OnEnable()
    {
        AppleCurrency.OnChanged += HandleAppleChanged;
    }

    private void OnDisable()
    {
        AppleCurrency.OnChanged -= HandleAppleChanged;
    }

    // ==================== APPLE MANAGEMENT ====================

    public void AddApple(int amount = 1)
    {
        AppleCurrency.Add(amount);
        UpdateAppleUI();
    }

    public void SetAppleCount(int value)
    {
        AppleCurrency.Set(value);
        UpdateAppleUI();
    }

    private void UpdateAppleUI()
    {
        // Show apple count on legacy UI
        if (appleText != null)
            appleText.text = AppleCurrency.Get().ToString();

        // Show apple count on TextMeshPro UI
        if (appleTMP != null)
            appleTMP.text = AppleCurrency.Get().ToString();
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
        AppleCurrency.Clear();
        UpdateAppleUI();
        Debug.Log("Apple data reset complete.");
    }

    private void HandleAppleChanged(int _)
    {
        UpdateAppleUI();
    }
}
