using System;
using System.Collections;
using UnityEngine;

public class PetNeeds : MonoBehaviour
{
    [Header("Starting values (0–100)")]
    [Range(0, 100)] public float Hunger = 100f;
    [Range(0, 100)] public float Energy = 100f;
    [Range(0, 100)] public float Hygiene = 100f;
    [Range(0, 100)] public float Happiness = 100f;
    [Range(0, 100)] public float Health = 100f;

    [Header("Decay every tick (seconds)")]
    public float tickSeconds = 5f;
    public float hungerDecay = 1f;
    public float energyDecay = 1f;
    public float hygieneDecay = 0.5f;
    public float happinessDecay = 0.5f;
    public float healthAutoRegen = 0.25f;

    [Header("Low needs")]
    public float lowNeed = 20f;

    [Header("Apples / Currency")]
    [Tooltip("How much hunger is restored per apple when feeding")]
    public float hungerPerApple = 5f;

    // Events – UI / color / other scripts can listen to this
    public event Action OnNeedsChanged;
    public event Action<float> OnHealthChanged;

    private Coroutine decayRoutine;
    private PetColorChanger pcc;

    // === NEW: save keys for PlayerPrefs ===
    private const string HUNGER_KEY = "PET_HUNGER";
    private const string ENERGY_KEY = "PET_ENERGY";
    private const string HYGIENE_KEY = "PET_HYGIENE";
    private const string HAPPINESS_KEY = "PET_HAPPINESS";
    private const string HEALTH_KEY = "PET_HEALTH";

    // ============== LIFECYCLE ==============

    private void Start()
    {
        pcc = GetComponent<PetColorChanger>();
    }

    private void Awake()
    {
        LoadNeeds(); // NEW – load saved values (if any)
        ClampAll();
    }

    private void OnEnable()
    {
        if (decayRoutine == null)
            decayRoutine = StartCoroutine(DecayLoop());
    }

    private void OnDisable()
    {
        if (decayRoutine != null)
        {
            StopCoroutine(decayRoutine);
            decayRoutine = null;
        }

        SaveNeeds(); // NEW – save current values when changing scene / disabling
    }

    // ============== MAIN LOOP ==============

    private IEnumerator DecayLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(tickSeconds);

        while (true)
        {
            yield return wait;
            TickNeeds();
        }
    }

    private void TickNeeds()
    {
        Hunger = Mathf.Clamp(Hunger - hungerDecay, 0f, 100f);
        Energy = Mathf.Clamp(Energy - energyDecay, 0f, 100f);
        Hygiene = Mathf.Clamp(Hygiene - hygieneDecay, 0f, 100f);
        Happiness = Mathf.Clamp(Happiness - happinessDecay, 0f, 100f);

        // Health logic: regen if everything is OK, lose health if any need is low
        bool anyLow = Hunger <= lowNeed ||
        Energy <= lowNeed ||
        Hygiene <= lowNeed ||
        Happiness <= lowNeed;

        if (anyLow)
            Health = Mathf.Clamp(Health - 1f, 0f, 100f);
        else
            Health = Mathf.Clamp(Health + healthAutoRegen, 0f, 100f);

        ClampAll();

        OnNeedsChanged?.Invoke();
        OnHealthChanged?.Invoke(Health);
    }

    private void Update()
    {
        pcc.UpdateColors(Hunger, Energy, Hygiene, Happiness, Health);
    }

    private void ClampAll()
    {
        Hunger = Mathf.Clamp(Hunger, 0f, 100f);
        Energy = Mathf.Clamp(Energy, 0f, 100f);
        Hygiene = Mathf.Clamp(Hygiene, 0f, 100f);
        Happiness = Mathf.Clamp(Happiness, 0f, 100f);
        Health = Mathf.Clamp(Health, 0f, 100f);
    }

    // ============== APPLES / CURRENCY ==============

    /// <summary>
    /// Called when the player picks up apples that feed the pet.
    /// (Used by your AppleCollect script.)
    /// </summary>
    public void FeedApples(int apples)
    {
        float amount = apples * hungerPerApple;
        Hunger = Mathf.Clamp(Hunger + amount, 0f, 100f);
        Happiness = Mathf.Clamp(Happiness + amount * 0.5f, 0f, 100f);

        OnNeedsChanged?.Invoke();
    }

    // Old alias used in older code – safe to keep
    public void FeedByApples(int apples) => FeedApples(apples);

    /// <summary>
    /// Used so other scripts (ShopButtons, levels) can spend apples.
    /// Returns true if the player had enough apples and they were removed.
    /// </summary>
    public bool TrySpendApples(int cost)
    {
        return AppleCurrency.Spend(cost);
    }

    // ============== DAMAGE / HEARTS ==============

    /// <summary>
    /// Used by traps or bad events to hurt the pet's health directly.
    /// </summary>
    public void TakeNeedsDamage(float amount)
    {
        Health = Mathf.Clamp(Health - amount, 0f, 100f);
        OnNeedsChanged?.Invoke();
        OnHealthChanged?.Invoke(Health);
    }

    // ============== ACTIONS FOR BUTTONS / SHOP / BED ==============

    /// <summary> Bathroom / bath button – fully restores Hygiene. </summary>
    public void Bath()
    {
        Hygiene = 100f;
        OnNeedsChanged?.Invoke();
    }

    /// <summary> Health potion button – sets health to 100. </summary>
    public void SetHealthToFull()
    {
        Health = 100f;
        OnNeedsChanged?.Invoke();
        OnHealthChanged?.Invoke(Health);
    }

    /// <summary> Energy potion button – sets energy to 100. </summary>
    public void SetEnergyToFull()
    {
        Energy = 100f;
        OnNeedsChanged?.Invoke();
    }

    /// <summary>
    /// Sleep at bed – full rest.
    /// This overload matches BedSleepInteractor if it calls SleepFill() with no arguments.
    /// </summary>
    public void SleepFill()
    {
        Energy = 100f;

        OnNeedsChanged?.Invoke();
        OnHealthChanged?.Invoke(Health);
    }

    /// <summary>
    /// Sleep at bed with a custom amount.
    /// This overload matches BedSleepInteractor if it calls SleepFill(someFloat).
    /// </summary>
    public void SleepFill(float amount)
    {
        // Add to energy instead of always full, in case you use a smaller amount
        Energy = Mathf.Clamp(Energy + amount, 0f, 100f);

        OnNeedsChanged?.Invoke();
    }

    // ============== NEW: SAVE / LOAD HELPERS ==============

    private void SaveNeeds()
    {
        PlayerPrefs.SetFloat(HUNGER_KEY, Hunger);
        PlayerPrefs.SetFloat(ENERGY_KEY, Energy);
        PlayerPrefs.SetFloat(HYGIENE_KEY, Hygiene);
        PlayerPrefs.SetFloat(HAPPINESS_KEY, Happiness);
        PlayerPrefs.SetFloat(HEALTH_KEY, Health);
        PlayerPrefs.Save();
    }

    private void LoadNeeds()
    {
        // Use current inspector values as defaults if nothing saved yet
        Hunger = PlayerPrefs.GetFloat(HUNGER_KEY, Hunger);
        Energy = PlayerPrefs.GetFloat(ENERGY_KEY, Energy);
        Hygiene = PlayerPrefs.GetFloat(HYGIENE_KEY, Hygiene);
        Happiness = PlayerPrefs.GetFloat(HAPPINESS_KEY, Happiness);
        Health = PlayerPrefs.GetFloat(HEALTH_KEY, Health);
    }
}
