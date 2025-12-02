using UnityEngine;
using UnityEngine.UI;

public class NeedsUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PetNeeds needs; // will be auto-found
    [SerializeField] private Slider hungerBar;
    [SerializeField] private Slider energyBar;
    [SerializeField] private Slider hygieneBar;
    [SerializeField] private Slider happinessBar;
    [SerializeField] private Slider healthBar;

    bool warned;

    void Awake()
    {
        // Try to find the correct PetNeeds when the scene starts
        FindNeeds();

        Init(hungerBar);
        Init(energyBar);
        Init(hygieneBar);
        Init(happinessBar);
        Init(healthBar);
    }

    void Update()
    {
        // If the old pet was destroyed and a new one spawned, find again
        if (needs == null || !needs.gameObject.activeInHierarchy)
            FindNeeds();

        if (needs == null)
        {
            WarnOnce("[NeedsUI] PetNeeds not set or found.");
            return;
        }

        if (!AllSliders())
        {
            WarnOnce("[NeedsUI] One or more Slider fields are not assigned.");
            return;
        }

        Set(hungerBar, needs.Hunger);
        Set(energyBar, needs.Energy);
        Set(hygieneBar, needs.Hygiene);
        Set(happinessBar, needs.Happiness);
        Set(healthBar, needs.Health);
    }

    // ================== helpers ==================

    // Prefer the PetNeeds on the object tagged "Player" (your active pet clone)
    void FindNeeds()
    {
        // 1) Try by Player tag first
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            var n = p.GetComponent<PetNeeds>();
            if (n != null)
            {
                needs = n;
                break;
            }
        }

        // 2) Fallback: any PetNeeds in the scene
        if (needs == null)
        {
#if UNITY_6000_9_OR_NEWER
needs = Object.FindFirstObjectByType<PetNeeds>();
#else
            needs = FindObjectOfType<PetNeeds>();
#endif
        }
    }

    void Init(Slider s)
    {
        if (s == null) return;
        s.minValue = 0;
        s.maxValue = 100;
    }

    void Set(Slider s, float v)
    {
        if (s == null) return;
        s.value = Mathf.Clamp(v, 0f, 100f);
    }

    bool AllSliders()
    {
        return hungerBar && energyBar && hygieneBar && happinessBar && healthBar;
    }

    void WarnOnce(string m)
    {
        if (warned) return;
        Debug.LogWarning(m);
        warned = true;
    }
}
