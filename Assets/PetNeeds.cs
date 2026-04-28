using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PetNeeds : MonoBehaviour
{
    private const string LastBedUseKey = "PIXIE_AI_LAST_BED_USE_UTC";
    private const string LastShowerUseKey = "PIXIE_AI_LAST_SHOWER_USE_UTC";

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

    [Header("Global Decay Tuning (Reversible)")]
    [SerializeField] private bool enableSlowerOverallDecay = true;
    [Range(0.1f, 1f)] [SerializeField] private float overallDecayMultiplier = 0.7f;

    [Header("Need Decay Tuning")]
    [Range(0.5f, 1f)] [SerializeField] private float otherNeedsSlowMultiplier = 0.9f;
    [SerializeField] private float minGapAboveHappiness = 0.1f;

    [Header("Low needs")]
    public float lowNeed = 20f;

    [Header("Gold Coins / Currency")]
    [Tooltip("How much hunger is restored per apple when feeding")]
    public float hungerPerApple = 5f;

    [Header("Sickness")]
    [SerializeField] private float sicknessHealthDrainPerTick = 0.75f;
    public bool IsSick { get; private set; }

    // Events – UI / color / other scripts can listen to this
    public event Action OnNeedsChanged;
    public event Action<float> OnHealthChanged;

    private Coroutine decayRoutine;
    private PetColorChanger pcc;
    private static bool sessionNeedsInitialized;
    private static bool firstSpawnHealthNormalized;
    private bool healthRecoveryArmed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeStatics()
    {
        // Ensures a true fresh start each time the game boots/play mode starts,
        // even when domain reload is disabled in editor settings.
        sessionNeedsInitialized = false;
        firstSpawnHealthNormalized = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetRuntimeStaticsBeforeScene()
    {
        // Extra safety for projects/editor setups where SubsystemRegistration can be skipped.
        sessionNeedsInitialized = false;
        firstSpawnHealthNormalized = false;
    }

    // === NEW: save keys for PlayerPrefs ===
    private const string HUNGER_KEY = "PET_HUNGER";
    private const string ENERGY_KEY = "PET_ENERGY";
    private const string HYGIENE_KEY = "PET_HYGIENE";
    private const string HAPPINESS_KEY = "PET_HAPPINESS";
    private const string HEALTH_KEY = "PET_HEALTH";
    private const string SICK_KEY = "PET_IS_SICK";
    private const string CHERRY_DECAY_BOOST_UNTIL_KEY = "PET_CHERRY_DECAY_BOOST_UNTIL_UTC";
    private const string TEDDY_HEALTH_DECAY_BOOST_UNTIL_KEY = "PET_TEDDY_HEALTH_DECAY_BOOST_UNTIL_UTC";
    private const string VOLLEYBALL_HAPPINESS_DECAY_BOOST_UNTIL_KEY = "PET_VOLLEYBALL_HAPPINESS_DECAY_BOOST_UNTIL_UTC";

    // ============== LIFECYCLE ==============

    private void Start()
    {
        pcc = GetComponent<PetColorChanger>();
    }

    private void Awake()
    {
        if (!sessionNeedsInitialized)
        {
            // Start each fresh run with full bars.
            Hunger = 100f;
            Energy = 100f;
            Hygiene = 100f;
            Happiness = 100f;
            Health = 100f;
            SaveNeeds();
            sessionNeedsInitialized = true;
        }
        else
        {
            LoadNeeds(); // keep continuity during the same run across scene switches
        }

        // Single fix: guarantee first runtime spawn starts with full health.
        if (!firstSpawnHealthNormalized)
        {
            Health = 100f;
            SaveNeeds();
            firstSpawnHealthNormalized = true;
        }
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

        if (!SaveData.IsHardResetInProgress)
            SaveNeeds(); // save current values when changing scene / disabling
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
        float decayScale = enableSlowerOverallDecay ? Mathf.Clamp(overallDecayMultiplier, 0.1f, 1f) : 1f;
        if (IsCherryDecayBoostActive())
            decayScale *= 0.9f;

        float happyDecay = Mathf.Max(0.02f, happinessDecay);
        float minOtherDecay = happyDecay + Mathf.Max(0.01f, minGapAboveHappiness);

        float hungerStep = Mathf.Max(minOtherDecay, hungerDecay * Mathf.Clamp(otherNeedsSlowMultiplier, 0.5f, 1f));
        float energyStep = Mathf.Max(minOtherDecay, energyDecay * Mathf.Clamp(otherNeedsSlowMultiplier, 0.5f, 1f));
        float hygieneStep = Mathf.Max(minOtherDecay, hygieneDecay * Mathf.Clamp(otherNeedsSlowMultiplier, 0.5f, 1f));

        float happinessDecayScale = IsVolleyballHappinessDecayBoostActive() ? decayScale * 0.9f : decayScale;

        Hunger = Mathf.Clamp(Hunger - (hungerStep * decayScale), 0f, 100f);
        Energy = Mathf.Clamp(Energy - (energyStep * decayScale), 0f, 100f);
        Hygiene = Mathf.Clamp(Hygiene - (hygieneStep * decayScale), 0f, 100f);
        Happiness = Mathf.Clamp(Happiness - (happinessDecay * happinessDecayScale), 0f, 100f);

        // Health logic: regen if everything is OK, lose health if any need is low
        bool anyLow = Hunger <= lowNeed ||
        Energy <= lowNeed ||
        Hygiene <= lowNeed ||
        Happiness <= lowNeed;

        if (anyLow)
        {
            // Health drop speed exactly matches Happiness drop speed.
            float healthLoss = ApplyTeddyHealthDecayReduction(Mathf.Max(0.01f, happinessDecay * decayScale));
            Health = Mathf.Clamp(Health - healthLoss, 0f, 100f);
        }
        else
            Health = Mathf.Clamp(Health + healthAutoRegen, 0f, 100f);

        if (IsSick)
        {
            float sicknessLoss = ApplyTeddyHealthDecayReduction(Mathf.Max(0.01f, sicknessHealthDrainPerTick * decayScale));
            Health = Mathf.Clamp(Health - sicknessLoss, 0f, 100f);
        }

        ClampAll();
        TrackHealthRecoveryState();

        OnNeedsChanged?.Invoke();
        OnHealthChanged?.Invoke(Health);

    }

    private void Update()
    {
        if (pcc != null) pcc.UpdateColors(Hunger, Energy, Hygiene, Happiness, Health);
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

        TrackHealthRecoveryState();
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
        TrackHealthRecoveryState();
        OnNeedsChanged?.Invoke();
        OnHealthChanged?.Invoke(Health);
    }

    public void ApplySickness()
    {
        if (IsSick)
            return;

        IsSick = true;
        OnNeedsChanged?.Invoke();
        OnHealthChanged?.Invoke(Health);
    }

    public void CureSickness()
    {
        if (!IsSick)
            return;

        IsSick = false;
        OnNeedsChanged?.Invoke();
        OnHealthChanged?.Invoke(Health);
    }

    public void ApplyLevelCompletionHealthPenalty()
    {
        // Intentionally disabled: no level-end health penalty.
    }

    // ============== ACTIONS FOR BUTTONS / SHOP / BED ==============

    /// <summary> Bathroom / bath button – fully restores Hygiene. </summary>
    public void Bath()
    {
        Hygiene = 100f;
        PlayerPrefs.SetInt(LastShowerUseKey, NowUnix());
        PlayerPrefs.Save();
        GamesChatBotStats.RecordEvent("bath");
        TrackHealthRecoveryState();
        OnNeedsChanged?.Invoke();
    }

    /// <summary> Health potion button – sets health to 100. </summary>
    public void SetHealthToFull()
    {
        CureSickness();
        Health = 100f;
        TrackHealthRecoveryState();
        SaveNeeds();
        OnNeedsChanged?.Invoke();
        OnHealthChanged?.Invoke(Health);
    }

    /// <summary> Energy potion button – sets energy to 100. </summary>
    public void SetEnergyToFull()
    {
        Energy = 100f;
        TrackHealthRecoveryState();
        SaveNeeds();
        OnNeedsChanged?.Invoke();
    }

    public void AddHunger(float amount)
    {
        Hunger = Mathf.Clamp(Hunger + Mathf.Max(0f, amount), 0f, 100f);
        TrackHealthRecoveryState();
        SaveNeeds();
        OnNeedsChanged?.Invoke();
    }

    public void SetHungerToFull()
    {
        Hunger = 100f;
        TrackHealthRecoveryState();
        SaveNeeds();
        OnNeedsChanged?.Invoke();
    }

    public void ApplyCherryBoost(float hungerRestore, float durationMinutes)
    {
        Hunger = Mathf.Clamp(Hunger + Mathf.Max(0f, hungerRestore), 0f, 100f);
        string until = DateTime.UtcNow.AddMinutes(Mathf.Max(0.1f, durationMinutes)).ToString("O");
        PlayerPrefs.SetString(CHERRY_DECAY_BOOST_UNTIL_KEY, until);
        TrackHealthRecoveryState();
        SaveNeeds();
        OnNeedsChanged?.Invoke();
    }

    public void ApplyTeddyBearBoost(float happinessRestore, float durationMinutes)
    {
        Happiness = Mathf.Clamp(Happiness + Mathf.Max(0f, happinessRestore), 0f, 100f);
        string until = DateTime.UtcNow.AddMinutes(Mathf.Max(0.1f, durationMinutes)).ToString("O");
        PlayerPrefs.SetString(TEDDY_HEALTH_DECAY_BOOST_UNTIL_KEY, until);
        TrackHealthRecoveryState();
        SaveNeeds();
        OnNeedsChanged?.Invoke();
    }

    public void ApplyVolleyballBoost(float happinessRestore, float durationMinutes)
    {
        Happiness = Mathf.Clamp(Happiness + Mathf.Max(0f, happinessRestore), 0f, 100f);
        string until = DateTime.UtcNow.AddMinutes(Mathf.Max(0.1f, durationMinutes)).ToString("O");
        PlayerPrefs.SetString(VOLLEYBALL_HAPPINESS_DECAY_BOOST_UNTIL_KEY, until);
        TrackHealthRecoveryState();
        SaveNeeds();
        OnNeedsChanged?.Invoke();
    }

    /// <summary>
    /// Sleep at bed – full rest.
    /// This overload matches BedSleepInteractor if it calls SleepFill() with no arguments.
    /// </summary>
    public void SleepFill()
    {
        Energy = 100f;
        PlayerPrefs.SetInt(LastBedUseKey, NowUnix());
        PlayerPrefs.Save();
        GamesChatBotStats.RecordEvent("sleep");

        TrackHealthRecoveryState();
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
        PlayerPrefs.SetInt(LastBedUseKey, NowUnix());
        PlayerPrefs.Save();
        GamesChatBotStats.RecordEvent("sleep");

        TrackHealthRecoveryState();
        OnNeedsChanged?.Invoke();
    }

    private void TrackHealthRecoveryState()
    {
        if (Health <= 25f)
            healthRecoveryArmed = true;
        else if (healthRecoveryArmed && Health >= 100f)
        {
            GamesChatBotStats.RecordEvent("recover_health_low_to_full");
            healthRecoveryArmed = false;
        }
    }

    // ============== NEW: SAVE / LOAD HELPERS ==============

    private void SaveNeeds()
    {
        PlayerPrefs.SetFloat(HUNGER_KEY, Hunger);
        PlayerPrefs.SetFloat(ENERGY_KEY, Energy);
        PlayerPrefs.SetFloat(HYGIENE_KEY, Hygiene);
        PlayerPrefs.SetFloat(HAPPINESS_KEY, Happiness);
        PlayerPrefs.SetFloat(HEALTH_KEY, Health);
        PlayerPrefs.SetInt(SICK_KEY, IsSick ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadNeeds()
    {
        // Always default missing keys to full values.
        Hunger = PlayerPrefs.GetFloat(HUNGER_KEY, 100f);
        Energy = PlayerPrefs.GetFloat(ENERGY_KEY, 100f);
        Hygiene = PlayerPrefs.GetFloat(HYGIENE_KEY, 100f);
        Happiness = PlayerPrefs.GetFloat(HAPPINESS_KEY, 100f);
        Health = PlayerPrefs.GetFloat(HEALTH_KEY, 100f);
        IsSick = PlayerPrefs.GetInt(SICK_KEY, 0) == 1;
    }

    private int NowUnix()
    {
        long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (unix > int.MaxValue) return int.MaxValue;
        if (unix < int.MinValue) return int.MinValue;
        return (int)unix;
    }

    private bool IsCherryDecayBoostActive()
    {
        string raw = PlayerPrefs.GetString(CHERRY_DECAY_BOOST_UNTIL_KEY, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (!DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime untilUtc))
            return false;

        return untilUtc.ToUniversalTime() > DateTime.UtcNow;
    }

    private float ApplyTeddyHealthDecayReduction(float healthLoss)
    {
        if (!IsTeddyHealthDecayBoostActive())
            return healthLoss;

        return Mathf.Max(0.01f, healthLoss - 0.1f);
    }

    private bool IsTeddyHealthDecayBoostActive()
    {
        string raw = PlayerPrefs.GetString(TEDDY_HEALTH_DECAY_BOOST_UNTIL_KEY, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (!DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime untilUtc))
            return false;

        return untilUtc.ToUniversalTime() > DateTime.UtcNow;
    }

    private bool IsVolleyballHappinessDecayBoostActive()
    {
        string raw = PlayerPrefs.GetString(VOLLEYBALL_HAPPINESS_DECAY_BOOST_UNTIL_KEY, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (!DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out DateTime untilUtc))
            return false;

        return untilUtc.ToUniversalTime() > DateTime.UtcNow;
    }

    public static void ResetSessionInitialization()
    {
        sessionNeedsInitialized = false;
    }
}
