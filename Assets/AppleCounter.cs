using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AppleCounter : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI countText; // drag AppleText
    [SerializeField] private Image icon; // drag AppleIcon (optional)

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
            int v = AppleCurrency.Get();
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
        AppleCurrency.Add(amount);
    }

    // Try to spend apples for shop etc.
    public static bool TrySpendApples(int amount)
    {
        return AppleCurrency.Spend(amount);
    }

    // Read apples anywhere
    public static int GetApples() => AppleCurrency.Get();
}
