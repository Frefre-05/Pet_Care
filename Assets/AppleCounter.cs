using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppleCounter : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI countText; // drag AppleText
    [SerializeField] private Image icon; // drag AppleIcon (optional)

    [Header("Prefs")]
    [SerializeField] private string prefsKey = "APPLE_COUNT";

    [Header("Display")]
    [SerializeField] private bool useLeadingZeros = false;
    [SerializeField] private int minimumDigits = 1;
    [SerializeField] private float refreshEvery = 0.25f; // seconds

    private int cached = -1;
    private Coroutine loop;

    private void OnEnable()
    {
        // Start a small polling loop—safe & simple across scenes
        loop = StartCoroutine(RefreshLoop());
        ForceRefresh();
    }

    private void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
    }

    private IEnumerator RefreshLoop()
    {
        while (true)
        {
            int v = PlayerPrefs.GetInt(prefsKey, 0);
            if (v != cached)
            {
                cached = v;
                UpdateUI(cached);
            }
            yield return new WaitForSeconds(refreshEvery);
        }
    }

    public void ForceRefresh()
    {
        cached = -1; // forces Update next tick
    }

    private void UpdateUI(int value)
    {
        if (countText == null) return;
        string s = useLeadingZeros ? value.ToString(new string('0', Mathf.Max(1, minimumDigits)))
        : value.ToString();
        countText.text = s;
    }

    // ---------- Currency helpers (use now or later) ----------

    // Safely add apples
    public static void AddApples(int amount)
    {
        int cur = PlayerPrefs.GetInt("APPLE_COUNT", 0);
        PlayerPrefs.SetInt("APPLE_COUNT", Mathf.Max(0, cur + amount));
        PlayerPrefs.Save();
    }

    // Try to spend apples for shop etc.
    public static bool TrySpendApples(int amount)
    {
        int cur = PlayerPrefs.GetInt("APPLE_COUNT", 0);
        if (cur < amount) return false;
        PlayerPrefs.SetInt("APPLE_COUNT", cur - amount);
        PlayerPrefs.Save();
        return true;
    }

    // Read apples anywhere
    public static int GetApples() => PlayerPrefs.GetInt("APPLE_COUNT", 0);
}
