using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ShopButtons : MonoBehaviour
{
    private const string TotalSpentKey = "CHATBOT_TOTAL_APPLES_SPENT";
    private const string TotalBoughtKey = "CHATBOT_TOTAL_ITEMS_BOUGHT";
    private const string HealthPotionKey = "CHATBOT_BOUGHT_health_potion";
    private const string EnergyPotionKey = "CHATBOT_BOUGHT_energy_potion";
    private const string SmallFoodKey = "CHATBOT_BOUGHT_small_food";
    private const string BigFoodKey = "CHATBOT_BOUGHT_big_food";
    private const string JumpBoostKey = "CHATBOT_BOUGHT_jump_boost";
    private const string SpeedBoostKey = "CHATBOT_BOUGHT_speed_boost";
    private const string CherryBoostKey = "CHATBOT_BOUGHT_cherry_boost";
    private const string TeddyBearKey = "CHATBOT_BOUGHT_teddy_bear";
    private const string VolleyballKey = "CHATBOT_BOUGHT_volleyball";
    private const string JumpBoostUntilKey = "PLAYER_JUMP_BOOST_UNTIL_UTC";
    private const string SpeedBoostUntilKey = "PLAYER_SPEED_BOOST_UNTIL_UTC";

    [Header("Drag your pet / player here (optional)")]
    public PetNeeds petNeeds;

    [Header("Potion costs (in Gold Coins)")]
    public int healthPotionCost = 20;
    public int energyPotionCost = 20;

    [Header("Food costs (in Gold Coins)")]
    public int smallFoodCost = 5;
    public int bigFoodCost = 20;

    [Header("Boost costs and duration")]
    public int jumpBoostCost = 10;
    public int speedBoostCost = 10;
    public float boostDurationMinutes = 10f;

    [Header("Cherry boost")]
    public int cherryBoostCost = 15;
    public float cherryHungerRestore = 10f;
    public float cherryDecayBoostDurationMinutes = 5f;

    [Header("Teddy Bear")]
    public int teddyBearCost = 25;
    public float teddyBearHappinessRestore = 50f;
    public float teddyBearSecretDecaySlowDurationMinutes = 5f;

    [Header("Volleyball")]
    public int volleyballCost = 5;
    public float volleyballHappinessRestore = 5f;
    public float volleyballHappinessDecaySlowDurationMinutes = 1f;

    [Header("Shop feedback")]
    [SerializeField] private float feedbackDuration = 2.2f;
    [SerializeField] private int successFontSize = 24;
    [SerializeField] private int errorFontSize = 34;

    private static ShopButtonsMessageUI messageUI;
    private static bool sceneHookInstalled;
    private GameObject hierarchyWarningPanel;
    private TMP_Text hierarchyWarningText;
    private static GameObject sharedSuccessPanel;
    private static TMP_Text sharedSuccessText;
    private static CanvasGroup sharedSuccessCanvasGroup;
    private static bool sharedSuccessStyled;
    private static int sharedSuccessSceneHandle = int.MinValue;
    private Coroutine warningRoutine;
    private Coroutine warningShowRoutine;
    private Coroutine successRoutine;
    private bool hierarchyWarningStyled;
    private bool cherryBoostButtonWired;
    private bool teddyBearButtonWired;
    private bool volleyballButtonWired;
    private float nextCherryBoostWireAttemptAt;
    private float nextTeddyBearWireAttemptAt;
    private float nextVolleyballWireAttemptAt;

    private PetNeeds GetActivePetNeeds()
    {
        if (IsUsablePetNeeds(petNeeds))
            return petNeeds;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            GameObject player = players[i];
            if (player == null || !player.activeInHierarchy)
                continue;

            PetNeeds playerNeeds = player.GetComponentInChildren<PetNeeds>(true);
            if (IsUsablePetNeeds(playerNeeds))
                return playerNeeds;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        PetNeeds[] all = UnityEngine.Object.FindObjectsByType<PetNeeds>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (PetNeeds pn in all)
        {
            if (IsUsablePetNeeds(pn) && pn.gameObject.scene == activeScene)
                return pn;
        }

        foreach (PetNeeds pn in all)
        {
            if (IsUsablePetNeeds(pn))
                return pn;
        }

        if (petNeeds != null)
            Debug.LogWarning("ShopButtons: assigned PetNeeds is not active, so it was ignored. Assign the spawned player pet or leave the field empty.");

        return null;
    }

    private bool IsUsablePetNeeds(PetNeeds target)
    {
        return target != null && target.gameObject.activeInHierarchy && target.gameObject.scene.IsValid();
    }

    public void BuyHealthPotion()
    {
        PetNeeds target = GetActivePetNeeds();
        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        if (AppleCurrency.Spend(healthPotionCost))
        {
            target.SetHealthToFull();
            RecordPurchase(HealthPotionKey, healthPotionCost, 1);
            PixiePurchaseMemory.RecordPurchase("Health Potion", healthPotionCost, 1, AppleCurrency.Get());
            ShowPurchaseSuccess("health potion");
            Debug.Log("Bought Health Potion (-" + healthPotionCost + " Gold Coins, sickness cured if active)");
        }
        else
        {
            ShowNotEnoughApples();
            Debug.Log("Not enough Gold Coins for Health Potion");
        }
    }

    public void BuyEnergyPotion()
    {
        PetNeeds target = GetActivePetNeeds();
        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        if (AppleCurrency.Spend(energyPotionCost))
        {
            target.SetEnergyToFull();
            RecordPurchase(EnergyPotionKey, energyPotionCost, 1);
            PixiePurchaseMemory.RecordPurchase("Energy Potion", energyPotionCost, 1, AppleCurrency.Get());
            ShowPurchaseSuccess("energy potion");
            Debug.Log("Bought Energy Potion (-" + energyPotionCost + " Gold Coins)");
        }
        else
        {
            ShowNotEnoughApples();
            Debug.Log("Not enough Gold Coins for Energy Potion");
        }
    }

    public void BuySmallFood()
    {
        PetNeeds target = GetActivePetNeeds();
        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        if (AppleCurrency.Spend(smallFoodCost))
        {
            target.AddHunger(25f);
            RecordPurchase(SmallFoodKey, smallFoodCost, 1);
            PixiePurchaseMemory.RecordPurchase("Kiwi", smallFoodCost, 1, AppleCurrency.Get());
            ShowPurchaseSuccess("kiwi");
            Debug.Log("Bought Small Food (-" + smallFoodCost + " Gold Coins, +25 Hunger)");
        }
        else
        {
            ShowNotEnoughApples();
            Debug.Log("Not enough Gold Coins for Small Food");
        }
    }

    public void BuyBigFood()
    {
        PetNeeds target = GetActivePetNeeds();
        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        if (AppleCurrency.Spend(bigFoodCost))
        {
            target.SetHungerToFull();
            RecordPurchase(BigFoodKey, bigFoodCost, 1);
            PixiePurchaseMemory.RecordPurchase("Pineapple", bigFoodCost, 1, AppleCurrency.Get());
            ShowPurchaseSuccess("pineapple");
            Debug.Log("Bought Big Food (-" + bigFoodCost + " Gold Coins, Hunger = 100)");
        }
        else
        {
            ShowNotEnoughApples();
            Debug.Log("Not enough Gold Coins for Big Food");
        }
    }

    public void BuyCherryBoost()
    {
        PetNeeds target = GetActivePetNeeds();
        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        if (AppleCurrency.Spend(cherryBoostCost))
        {
            target.ApplyCherryBoost(cherryHungerRestore, cherryDecayBoostDurationMinutes);
            RecordPurchase(CherryBoostKey, cherryBoostCost, 1);
            PixiePurchaseMemory.RecordPurchase("Cherry", cherryBoostCost, 1, AppleCurrency.Get());
            ShowPurchaseSuccess("cherry");
            Debug.Log("Bought Cherry (-" + cherryBoostCost + " Gold Coins, +" + cherryHungerRestore + " Hunger, 10% slower stat decay for " + cherryDecayBoostDurationMinutes + " minutes)");
        }
        else
        {
            ShowNotEnoughApples();
            Debug.Log("Not enough Gold Coins for Cherry");
        }
    }

    public void BuyTeddyBear()
    {
        PetNeeds target = GetActivePetNeeds();
        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        if (AppleCurrency.Spend(teddyBearCost))
        {
            target.ApplyTeddyBearBoost(teddyBearHappinessRestore, teddyBearSecretDecaySlowDurationMinutes);
            RecordPurchase(TeddyBearKey, teddyBearCost, 1);
            PixiePurchaseMemory.RecordPurchase("Teddy Bear", teddyBearCost, 1, AppleCurrency.Get());
            ShowPurchaseSuccess("teddy bear");
            Debug.Log("Bought Teddy Bear (-" + teddyBearCost + " Gold Coins, +" + teddyBearHappinessRestore + " Happiness)");
        }
        else
        {
            ShowNotEnoughApples();
            Debug.Log("Not enough Gold Coins for Teddy Bear");
        }
    }

    public void BuyVolleyball()
    {
        PetNeeds target = GetActivePetNeeds();
        if (target == null)
        {
            Debug.LogError("ShopButtons: no ACTIVE PetNeeds found in scene!");
            return;
        }

        if (AppleCurrency.Spend(volleyballCost))
        {
            target.ApplyVolleyballBoost(volleyballHappinessRestore, volleyballHappinessDecaySlowDurationMinutes);
            RecordPurchase(VolleyballKey, volleyballCost, 1);
            PixiePurchaseMemory.RecordPurchase("Volleyball", volleyballCost, 1, AppleCurrency.Get());
            ShowPurchaseSuccess("volleyball");
            Debug.Log("Bought Volleyball (-" + volleyballCost + " Gold Coins, +" + volleyballHappinessRestore + " Happiness, slower Happiness loss for " + volleyballHappinessDecaySlowDurationMinutes + " minute)");
        }
        else
        {
            ShowNotEnoughApples();
            Debug.Log("Not enough Gold Coins for Volleyball");
        }
    }

    public void BuyJumpBoost()
    {
        if (AppleCurrency.Spend(jumpBoostCost))
        {
            SetBoostExpiry(JumpBoostUntilKey, boostDurationMinutes);
            RecordPurchase(JumpBoostKey, jumpBoostCost, 1);
            PixiePurchaseMemory.RecordPurchase("Jump Boost", jumpBoostCost, 1, AppleCurrency.Get());
            ShowPurchaseSuccess("jump boost");
            Debug.Log("Bought Jump Boost (-" + jumpBoostCost + " Gold Coins, 10 minutes)");
        }
        else
        {
            ShowNotEnoughApples();
            Debug.Log("Not enough Gold Coins for Jump Boost");
        }
    }

    public void BuySpeedBoost()
    {
        if (AppleCurrency.Spend(speedBoostCost))
        {
            SetBoostExpiry(SpeedBoostUntilKey, boostDurationMinutes);
            RecordPurchase(SpeedBoostKey, speedBoostCost, 1);
            PixiePurchaseMemory.RecordPurchase("Speed Boost", speedBoostCost, 1, AppleCurrency.Get());
            ShowPurchaseSuccess("speed boost");
            Debug.Log("Bought Speed Boost (-" + speedBoostCost + " Gold Coins, 10 minutes)");
        }
        else
        {
            ShowNotEnoughApples();
            Debug.Log("Not enough Gold Coins for Speed Boost");
        }
    }

    private void Awake()
    {
        EnsureSceneHook();
    }

    private void Start()
    {
        EnsureMessageUI();
        FindHierarchyWarningUI();
        EnsureSharedSuccessUI();
        MarkPurchaseButtonsForAudioOverride();
        WireCherryBoostButton();
        WireTeddyBearButton();
        WireVolleyballButton();
    }

    private void OnDisable()
    {
        HidePurchaseSuccessNow();
    }

    private void Update()
    {
        if (!cherryBoostButtonWired && Time.unscaledTime >= nextCherryBoostWireAttemptAt)
        {
            nextCherryBoostWireAttemptAt = Time.unscaledTime + 0.5f;
            WireCherryBoostButton();
        }

        if (!teddyBearButtonWired && Time.unscaledTime >= nextTeddyBearWireAttemptAt)
        {
            nextTeddyBearWireAttemptAt = Time.unscaledTime + 0.5f;
            WireTeddyBearButton();
        }

        if (!volleyballButtonWired && Time.unscaledTime >= nextVolleyballWireAttemptAt)
        {
            nextVolleyballWireAttemptAt = Time.unscaledTime + 0.5f;
            WireVolleyballButton();
        }
    }

    private static void EnsureSceneHook()
    {
        if (sceneHookInstalled)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        sceneHookInstalled = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        messageUI = null;
        sharedSuccessPanel = null;
        sharedSuccessText = null;
        sharedSuccessCanvasGroup = null;
        sharedSuccessStyled = false;
        sharedSuccessSceneHandle = int.MinValue;
    }

    private void WireCherryBoostButton()
    {
        Button button = FindButtonByObjectName("The OnClickOfCherryGoesHere");
        if (button == null)
            button = FindButtonByObjectName("CherryBoost");
        if (button == null)
        {
            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button candidate = buttons[i];
                if (candidate == null || !string.Equals(candidate.gameObject.name, "CherryBoost", StringComparison.Ordinal))
                    continue;

                button = candidate;
                break;
            }
        }

        if (button == null)
            return;

        MarkButtonForAudioOverride(button);
        PrepareNamedClickArea(button, "The OnClickOfCherryGoesHere");
        button.onClick.RemoveListener(BuyCherryBoost);
        button.onClick.AddListener(BuyCherryBoost);
        button.interactable = true;
        ConfigureShopButtonClickFeedback(button, "The OnClickOfCherryGoesHere", "CherryBoost", "Cherry");
        cherryBoostButtonWired = true;
        Debug.Log("ShopButtons: CherryBoost button wired to BuyCherryBoost on " + GetHierarchyPath(button.transform));
    }

    private void WireTeddyBearButton()
    {
        Button button = FindButtonByObjectName("The OnClickOfTeddyBearGoesHere");
        if (button == null)
            button = FindButtonByObjectName("TeddyBearBox");
        if (button == null)
        {
            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button candidate = buttons[i];
                if (candidate == null || !string.Equals(candidate.gameObject.name, "TeddyBearBox", StringComparison.Ordinal))
                    continue;

                button = candidate;
                break;
            }
        }

        if (button == null)
            return;

        MarkButtonForAudioOverride(button);
        PrepareNamedClickArea(button, "The OnClickOfTeddyBearGoesHere");
        button.onClick.RemoveListener(BuyTeddyBear);
        button.onClick.AddListener(BuyTeddyBear);
        button.interactable = true;
        ConfigureShopButtonClickFeedback(button, "The OnClickOfTeddyBearGoesHere", "TeddyBearBox", "Teddy");
        teddyBearButtonWired = true;
        Debug.Log("ShopButtons: Teddy Bear button wired to BuyTeddyBear on " + GetHierarchyPath(button.transform));
    }

    private void WireVolleyballButton()
    {
        Button button = FindButtonByObjectName("The OnClickOfBeachBallGoesHere");
        if (button == null)
            button = FindButtonByObjectName("BeachBallBox");
        if (button == null)
        {
            Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button candidate = buttons[i];
                if (candidate == null || !string.Equals(candidate.gameObject.name, "BeachBallBox", StringComparison.Ordinal))
                    continue;

                button = candidate;
                break;
            }
        }

        if (button == null)
            return;

        MarkButtonForAudioOverride(button);
        PrepareNamedClickArea(button, "The OnClickOfBeachBallGoesHere");
        button.onClick.RemoveListener(BuyVolleyball);
        button.onClick.AddListener(BuyVolleyball);
        button.interactable = true;
        ConfigureShopButtonClickFeedback(button, "The OnClickOfBeachBallGoesHere", "BeachBallBox", "BeachBall");
        volleyballButtonWired = true;
        Debug.Log("ShopButtons: Volleyball button wired to BuyVolleyball on " + GetHierarchyPath(button.transform));
    }

    private Button FindButtonByObjectName(string objectName)
    {
        Transform[] transforms = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform target = transforms[i];
            if (target == null || !string.Equals(target.gameObject.name, objectName, StringComparison.Ordinal))
                continue;

            Button button = target.GetComponent<Button>();
            if (button != null)
                return button;

            button = target.GetComponentInParent<Button>(true);
            if (button != null)
                return button;

            button = target.GetComponentInChildren<Button>(true);
            if (button != null)
                return button;
        }

        return null;
    }

    private string GetHierarchyPath(Transform target)
    {
        if (target == null)
            return string.Empty;

        StringBuilder path = new StringBuilder(target.name);
        Transform current = target.parent;
        while (current != null)
        {
            path.Insert(0, current.name + "/");
            current = current.parent;
        }
        return path.ToString();
    }

    private void ConfigureShopButtonClickFeedback(Button button, string clickAreaName, params string[] visualNames)
    {
        if (button == null)
            return;

        // Cherry uses an invisible click area, so Unity's built-in tint can leave
        // the visible box looking permanently pressed. Use our short flash instead.
        button.transition = Selectable.Transition.None;
        Graphic[] targetGraphics = FindPreferredClickGraphics(button, clickAreaName, visualNames);
        if (targetGraphics.Length > 0)
            button.targetGraphic = targetGraphics[0];

        ColorBlock colors = button.colors;
        if (colors.colorMultiplier <= 0f)
            colors.colorMultiplier = 1f;
        colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        ShopButtonClickFlash flash = button.GetComponent<ShopButtonClickFlash>();
        if (flash == null)
            flash = button.gameObject.AddComponent<ShopButtonClickFlash>();
        flash.Configure(button, targetGraphics);
        flash.SetNormalNow();
    }

    private void PrepareNamedClickArea(Button button, string clickAreaName)
    {
        if (button == null)
            return;

        if (!string.Equals(button.gameObject.name, clickAreaName, StringComparison.Ordinal))
            return;

        Graphic[] clickGraphics = button.GetComponents<Graphic>();
        for (int i = 0; i < clickGraphics.Length; i++)
        {
            Graphic graphic = clickGraphics[i];
            if (graphic == null || graphic is TMP_Text)
                continue;

            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;
            graphic.raycastTarget = true;
        }
    }

    private Graphic[] FindPreferredClickGraphics(Button button, string clickAreaName, params string[] visualNames)
    {
        List<Graphic> preferred = new List<Graphic>();
        if (button == null)
            return preferred.ToArray();

        if (visualNames != null)
        {
            for (int i = 0; i < visualNames.Length; i++)
                AddNamedClickGraphics(preferred, button.transform, visualNames[i]);
        }

        if (button.transform.parent != null)
        {
            if (visualNames != null)
            {
                for (int i = 0; i < visualNames.Length; i++)
                    AddNamedClickGraphics(preferred, button.transform.parent, visualNames[i]);
            }
        }

        Graphic[] graphics = button.GetComponentsInChildren<Graphic>(true);
        for (int pass = 0; pass < 2; pass++)
        {
            for (int i = 0; i < graphics.Length; i++)
            {
                Graphic graphic = graphics[i];
                if (graphic == null || graphic is TMP_Text)
                    continue;
                if (string.Equals(graphic.gameObject.name, clickAreaName, StringComparison.Ordinal))
                    continue;

                string lowerName = graphic.gameObject.name.ToLowerInvariant();
                bool isItemGraphic = lowerName.Contains("box") || lowerName.Contains("apple") || lowerName.Contains("item") || lowerName.Contains("cherry") || lowerName.Contains("teddy") || lowerName.Contains("beachball") || lowerName.Contains("volley");
                if ((pass == 0 && isItemGraphic) || (pass == 1 && preferred.Count == 0))
                    AddClickGraphic(preferred, graphic, clickAreaName);
            }
        }

        if (preferred.Count == 0 && button.targetGraphic != null)
            AddClickGraphic(preferred, button.targetGraphic, clickAreaName);

        return preferred.ToArray();
    }

    private void AddNamedClickGraphics(List<Graphic> graphics, Transform root, string objectName)
    {
        if (root == null)
            return;

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform transform = transforms[i];
            if (transform == null || !string.Equals(transform.gameObject.name, objectName, StringComparison.Ordinal))
                continue;

            Graphic[] namedGraphics = transform.GetComponents<Graphic>();
            for (int g = 0; g < namedGraphics.Length; g++)
                AddClickGraphic(graphics, namedGraphics[g], null);
        }
    }

    private void AddClickGraphic(List<Graphic> graphics, Graphic graphic, string clickAreaName)
    {
        if (graphic == null)
            return;
        if (!string.IsNullOrEmpty(clickAreaName) && string.Equals(graphic.gameObject.name, clickAreaName, StringComparison.Ordinal))
            return;
        if (graphics.Contains(graphic))
            return;
        graphics.Add(graphic);
    }

    private void ShowPurchaseSuccess(string itemDisplayName)
    {
        string itemText = "You bought a " + itemDisplayName + "!";
        ClearNotEnoughGoldCoinsWarning();
        EnsureSharedSuccessUI();
        PlayPurchaseSuccessSfx();
        if (sharedSuccessPanel != null && sharedSuccessText != null)
        {
            if (successRoutine != null)
                StopCoroutine(successRoutine);
            sharedSuccessText.gameObject.SetActive(true);
            sharedSuccessText.text = itemText;
            sharedSuccessPanel.transform.SetAsLastSibling();
            EnsureFrontCanvas(sharedSuccessPanel, 200);
            if (sharedSuccessCanvasGroup != null)
                sharedSuccessCanvasGroup.alpha = 1f;
            sharedSuccessPanel.SetActive(true);
            Canvas.ForceUpdateCanvases();
            RectTransform panelRect = sharedSuccessPanel.transform as RectTransform;
            if (panelRect != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
            successRoutine = StartCoroutine(HideHierarchySuccessAfterDelay(Mathf.Max(0.25f, feedbackDuration)));
            return;
        }

        EnsureMessageUI();
        if (messageUI == null)
            return;

        string line1 = itemText;
        string line2 = "Mask Dude: Thank you.";
        RectTransform targetRect = GetCurrentClickedRect();
        messageUI.ShowAboveTarget(line1 + "\n" + line2, Color.white, successFontSize, feedbackDuration, targetRect);
    }

    private void ShowNotEnoughApples()
    {
        FindHierarchyWarningUI();
        PlayPurchaseFailedSfx();
        if (hierarchyWarningPanel != null && hierarchyWarningText != null)
        {
            if (warningShowRoutine != null)
                StopCoroutine(warningShowRoutine);
            if (warningRoutine != null)
                StopCoroutine(warningRoutine);
            warningShowRoutine = StartCoroutine(ShowHierarchyWarningDelayed(0.08f));
            return;
        }

        EnsureMessageUI();
        if (messageUI != null)
            messageUI.ShowInsideShopPanel("You do not have enough Gold Coins!", Color.red, errorFontSize, feedbackDuration, FindShopPanelRect());
    }

    private void ClearNotEnoughGoldCoinsWarning()
    {
        if (warningShowRoutine != null)
        {
            StopCoroutine(warningShowRoutine);
            warningShowRoutine = null;
        }

        if (warningRoutine != null)
        {
            StopCoroutine(warningRoutine);
            warningRoutine = null;
        }

        if (hierarchyWarningPanel != null)
            hierarchyWarningPanel.SetActive(false);

        if (hierarchyWarningText != null)
            hierarchyWarningText.text = string.Empty;

        if (messageUI != null)
            messageUI.HideNow();
    }

    private void HidePurchaseSuccessNow()
    {
        if (successRoutine != null)
        {
            StopCoroutine(successRoutine);
            successRoutine = null;
        }

        if (sharedSuccessCanvasGroup != null)
            sharedSuccessCanvasGroup.alpha = 0f;

        if (sharedSuccessText != null)
        {
            sharedSuccessText.text = string.Empty;
            sharedSuccessText.gameObject.SetActive(false);
        }

        if (sharedSuccessPanel != null)
            sharedSuccessPanel.SetActive(false);
    }

    private void EnsureMessageUI()
    {
        if (messageUI != null)
            return;

        Canvas canvas = FindBestCanvas();
        if (canvas == null)
            return;

        Transform existing = canvas.transform.Find("ShopButtonsMessageUI");
        if (existing != null)
        {
            messageUI = existing.GetComponent<ShopButtonsMessageUI>();
            if (messageUI != null)
                return;
        }

        GameObject root = new GameObject("ShopButtonsMessageUI", typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(canvas.transform, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(420f, 110f);
        rootRect.anchoredPosition = Vector2.zero;
        root.SetActive(false);

        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image), typeof(Outline));
        backgroundObject.transform.SetParent(root.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image background = backgroundObject.GetComponent<Image>();
        background.color = new Color32(24, 8, 10, 232);
        background.raycastTarget = false;

        Outline outline = backgroundObject.GetComponent<Outline>();
        outline.effectColor = new Color32(255, 211, 117, 210);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject textObject = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(root.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(14f, 10f);
        textRect.offsetMax = new Vector2(-14f, -10f);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;
        text.text = string.Empty;
        text.color = Color.white;
        text.fontSize = successFontSize;
        text.outlineWidth = 0.28f;
        text.outlineColor = new Color32(0, 0, 0, 255);

        messageUI = root.AddComponent<ShopButtonsMessageUI>();
        messageUI.Initialize(text, background);
    }

    private static Canvas FindBestCanvas()
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Canvas fallback = null;
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null || !canvas.isActiveAndEnabled)
                continue;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return canvas;
            if (fallback == null)
                fallback = canvas;
        }

        return fallback;
    }

    private RectTransform GetCurrentClickedRect()
    {
        GameObject clicked = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (clicked == null)
            return null;

        return clicked.transform as RectTransform;
    }

    private RectTransform FindShopPanelRect()
    {
        RectTransform[] rects = FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < rects.Length; i++)
        {
            RectTransform rect = rects[i];
            if (rect == null)
                continue;
            if (string.Equals(rect.gameObject.name, "Shop", StringComparison.Ordinal))
                return rect;
        }

        for (int i = 0; i < rects.Length; i++)
        {
            RectTransform rect = rects[i];
            if (rect == null)
                continue;
            if (string.Equals(rect.gameObject.name, "ShopPanel.", StringComparison.Ordinal))
                return rect;
        }

        return null;
    }

    private void FindHierarchyWarningUI()
    {
        if (hierarchyWarningPanel != null && hierarchyWarningText != null)
            return;


        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Transform warningPanelTransform = null;
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform t = transforms[i];
            if (t == null)
                continue;
            if (string.Equals(t.gameObject.name, "NotEnoughApplesPanel", StringComparison.Ordinal) ||
                string.Equals(t.gameObject.name, "messagePanel", StringComparison.Ordinal))
            {
                warningPanelTransform = t;
                break;
            }
        }

        if (warningPanelTransform == null)
            return;

        hierarchyWarningPanel = warningPanelTransform.gameObject;
        TMP_Text[] texts = hierarchyWarningPanel.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text t = texts[i];
            if (t == null)
                continue;
            if (string.Equals(t.gameObject.name, "NotEnoughApplesText", StringComparison.Ordinal) ||
                string.Equals(t.gameObject.name, "messageText", StringComparison.Ordinal))
            {
                hierarchyWarningText = t;
                break;
            }
        }

        if (hierarchyWarningPanel != null)
        {
            hierarchyWarningPanel.transform.SetAsLastSibling();
            EnsureFrontCanvas(hierarchyWarningPanel, 190);
            DecorateHierarchyWarningUI();
            hierarchyWarningPanel.SetActive(false);
        }
    }

    private void EnsureSharedSuccessUI()
    {
        int activeSceneHandle = SceneManager.GetActiveScene().handle;
        if (sharedSuccessPanel != null && sharedSuccessText != null && sharedSuccessCanvasGroup != null && sharedSuccessSceneHandle == activeSceneHandle)
            return;

        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Transform shopTransform = null;
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform t = transforms[i];
            if (t == null)
                continue;
            if (string.Equals(t.gameObject.name, "PurchaseSuccessPanel", StringComparison.Ordinal))
            {
                if (Application.isPlaying)
                    Destroy(t.gameObject);
                else
                    DestroyImmediate(t.gameObject);
                continue;
            }
            if (shopTransform == null && string.Equals(t.gameObject.name, "Shop", StringComparison.Ordinal))
                shopTransform = t;
        }

        sharedSuccessPanel = null;
        sharedSuccessText = null;
        sharedSuccessCanvasGroup = null;
        sharedSuccessStyled = false;

        if (shopTransform == null)
            return;

        sharedSuccessSceneHandle = activeSceneHandle;

        GameObject panelObject = new GameObject("PurchaseSuccessPanel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        panelObject.transform.SetParent(shopTransform, false);
        RectTransform panelRect = panelObject.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(-70f, 0f);
        panelRect.sizeDelta = new Vector2(520f, 70f);

        GameObject textObject = new GameObject("PurchaseSuccessText", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelObject.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 10f);
        textRect.offsetMax = new Vector2(-18f, -10f);

        sharedSuccessPanel = panelObject;
        sharedSuccessText = textObject.GetComponent<TextMeshProUGUI>();
        sharedSuccessCanvasGroup = panelObject.GetComponent<CanvasGroup>();
        sharedSuccessPanel.transform.SetAsLastSibling();
        EnsureFrontCanvas(sharedSuccessPanel, 200);
        DecorateHierarchySuccessUI();
        sharedSuccessPanel.SetActive(true);
        sharedSuccessCanvasGroup.alpha = 0f;
        sharedSuccessCanvasGroup.interactable = false;
        sharedSuccessCanvasGroup.blocksRaycasts = false;
    }

    private static void EnsureFrontCanvas(GameObject target, int sortingOrder)
    {
        if (target == null)
            return;

        Canvas canvas = target.GetComponent<Canvas>();
        if (canvas == null)
            canvas = target.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = sortingOrder;

        GraphicRaycaster raycaster = target.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
            raycaster = target.AddComponent<GraphicRaycaster>();
        raycaster.enabled = false;
    }

    private System.Collections.IEnumerator HideHierarchyWarningAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (hierarchyWarningPanel != null)
            hierarchyWarningPanel.SetActive(false);
        warningRoutine = null;
    }

    private System.Collections.IEnumerator HideHierarchySuccessAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (sharedSuccessCanvasGroup != null)
            sharedSuccessCanvasGroup.alpha = 0f;
        if (sharedSuccessText != null)
        {
            sharedSuccessText.text = string.Empty;
            sharedSuccessText.gameObject.SetActive(false);
        }
        if (sharedSuccessPanel != null)
            sharedSuccessPanel.SetActive(false);
        successRoutine = null;
    }

    private void DecorateHierarchySuccessUI()
    {
        if (sharedSuccessStyled || sharedSuccessPanel == null)
            return;

        Image panelImage = sharedSuccessPanel.GetComponent<Image>();
        if (panelImage == null)
            panelImage = sharedSuccessPanel.AddComponent<Image>();
        panelImage.color = new Color32(34, 16, 18, 232);
        panelImage.raycastTarget = false;

        CanvasGroup panelCanvasGroup = sharedSuccessPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
            panelCanvasGroup = sharedSuccessPanel.AddComponent<CanvasGroup>();
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;

        Outline outline = sharedSuccessPanel.GetComponent<Outline>();
        if (outline == null)
            outline = sharedSuccessPanel.AddComponent<Outline>();
        outline.effectColor = new Color32(255, 212, 96, 255);
        outline.effectDistance = new Vector2(3f, -3f);

        Shadow shadow = sharedSuccessPanel.GetComponent<Shadow>();
        if (shadow == null)
            shadow = sharedSuccessPanel.AddComponent<Shadow>();
        shadow.effectColor = new Color32(18, 6, 0, 180);
        shadow.effectDistance = new Vector2(6f, -6f);

        RectTransform panelRect = sharedSuccessPanel.transform as RectTransform;
        CreateOrUpdateAccent(panelRect, "SuccessTopAccent", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 8f), Vector2.zero, new Color32(255, 211, 84, 255));
        CreateOrUpdateAccent(panelRect, "SuccessBottomAccent", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), Vector2.zero, new Color32(173, 92, 14, 255));

        if (sharedSuccessText == null)
        {
            GameObject textObject = new GameObject("PurchaseSuccessText", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(sharedSuccessPanel.transform, false);
            sharedSuccessText = textObject.GetComponent<TextMeshProUGUI>();
        }

        RectTransform textRect = sharedSuccessText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 10f);
        textRect.offsetMax = new Vector2(-18f, -10f);

        sharedSuccessText.text = "You bought a health potion!";
        sharedSuccessText.color = Color.white;
        sharedSuccessText.fontSize = 28f;
        sharedSuccessText.alignment = TextAlignmentOptions.Center;
        sharedSuccessText.textWrappingMode = TextWrappingModes.Normal;
        sharedSuccessText.margin = new Vector4(12f, 8f, 12f, 8f);
        sharedSuccessText.outlineWidth = 0.22f;
        sharedSuccessText.outlineColor = new Color32(0, 0, 0, 255);
        sharedSuccessText.raycastTarget = false;

        Graphic[] successGraphics = sharedSuccessPanel.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < successGraphics.Length; i++)
        {
            if (successGraphics[i] == null)
                continue;
            successGraphics[i].raycastTarget = false;
        }

        sharedSuccessStyled = true;
    }

    private System.Collections.IEnumerator ShowHierarchyWarningDelayed(float delay)
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, delay));
        if (hierarchyWarningText != null)
        {
            hierarchyWarningText.text = "You do not have enough Gold Coins!";
            hierarchyWarningText.color = Color.red;
            hierarchyWarningText.fontSize = errorFontSize;
        }
        if (hierarchyWarningPanel != null)
        {
            hierarchyWarningPanel.transform.SetAsLastSibling();
            EnsureFrontCanvas(hierarchyWarningPanel, 190);
            hierarchyWarningPanel.SetActive(true);
        }
        warningRoutine = StartCoroutine(HideHierarchyWarningAfterDelay(Mathf.Max(0.25f, feedbackDuration)));
        warningShowRoutine = null;
    }

    private void DecorateHierarchyWarningUI()
    {
        if (hierarchyWarningStyled || hierarchyWarningPanel == null)
            return;

        Image panelImage = hierarchyWarningPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = new Color32(92, 28, 12, 224);
            panelImage.raycastTarget = false;
        }

        CanvasGroup panelCanvasGroup = hierarchyWarningPanel.GetComponent<CanvasGroup>();
        if (panelCanvasGroup == null)
            panelCanvasGroup = hierarchyWarningPanel.AddComponent<CanvasGroup>();
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;

        Outline outline = hierarchyWarningPanel.GetComponent<Outline>();
        if (outline == null)
            outline = hierarchyWarningPanel.AddComponent<Outline>();
        outline.effectColor = new Color32(255, 214, 84, 255);
        outline.effectDistance = new Vector2(3f, -3f);

        Shadow shadow = hierarchyWarningPanel.GetComponent<Shadow>();
        if (shadow == null)
            shadow = hierarchyWarningPanel.AddComponent<Shadow>();
        shadow.effectColor = new Color32(20, 4, 0, 180);
        shadow.effectDistance = new Vector2(6f, -6f);

        RectTransform panelRect = hierarchyWarningPanel.transform as RectTransform;
        CreateOrUpdateAccent(panelRect, "TopAccent", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 8f), Vector2.zero, new Color32(255, 209, 64, 255));
        CreateOrUpdateAccent(panelRect, "BottomAccent", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), Vector2.zero, new Color32(157, 82, 10, 255));

        if (hierarchyWarningText != null)
        {
            hierarchyWarningText.color = Color.red;
            hierarchyWarningText.alignment = TextAlignmentOptions.Center;
            hierarchyWarningText.margin = new Vector4(18f, 12f, 18f, 12f);
            hierarchyWarningText.outlineWidth = 0.22f;
            hierarchyWarningText.outlineColor = new Color32(26, 0, 0, 255);
            hierarchyWarningText.raycastTarget = false;
        }

        Graphic[] warningGraphics = hierarchyWarningPanel.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < warningGraphics.Length; i++)
        {
            if (warningGraphics[i] == null)
                continue;
            warningGraphics[i].raycastTarget = false;
        }

        hierarchyWarningStyled = true;
    }

    private void CreateOrUpdateAccent(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 sizeDelta, Vector2 anchoredPosition, Color color)
    {
        if (parent == null)
            return;

        Transform existing = parent.Find(name);
        GameObject accentObject;
        if (existing == null)
        {
            accentObject = new GameObject(name, typeof(RectTransform), typeof(Image));
            accentObject.transform.SetParent(parent, false);
            accentObject.transform.SetAsFirstSibling();
        }
        else
        {
            accentObject = existing.gameObject;
        }

        RectTransform accentRect = accentObject.GetComponent<RectTransform>();
        accentRect.anchorMin = anchorMin;
        accentRect.anchorMax = anchorMax;
        accentRect.pivot = pivot;
        accentRect.sizeDelta = sizeDelta;
        accentRect.anchoredPosition = anchoredPosition;

        Image accentImage = accentObject.GetComponent<Image>();
        accentImage.color = color;
        accentImage.raycastTarget = false;
    }

    private void RecordPurchase(string perItemKey, int costPerItem, int quantity)
    {
        if (quantity <= 0) return;
        int safeCost = Mathf.Max(0, costPerItem);

        int currentItemCount = PlayerPrefs.GetInt(perItemKey, 0);
        PlayerPrefs.SetInt(perItemKey, currentItemCount + quantity);

        int currentTotalBought = PlayerPrefs.GetInt(TotalBoughtKey, 0);
        PlayerPrefs.SetInt(TotalBoughtKey, currentTotalBought + quantity);

        int currentTotalSpent = PlayerPrefs.GetInt(TotalSpentKey, 0);
        PlayerPrefs.SetInt(TotalSpentKey, currentTotalSpent + (safeCost * quantity));

        PlayerPrefs.Save();
    }

    private void SetBoostExpiry(string key, float minutes)
    {
        string until = DateTime.UtcNow.AddMinutes(Mathf.Max(0.1f, minutes)).ToString("O");
        PlayerPrefs.SetString(key, until);
        PlayerPrefs.Save();
    }

    private void PlayPurchaseSuccessSfx()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPurchaseSuccessSfx();
    }

    private void PlayPurchaseFailedSfx()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayPurchaseFailedSfx();
    }

    private void MarkPurchaseButtonsForAudioOverride()
    {
        Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
                continue;

            if (CallsThisPurchaseMethod(button))
                MarkButtonForAudioOverride(button);
        }
    }

    private bool CallsThisPurchaseMethod(Button button)
    {
        if (button == null)
            return false;

        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            UnityEngine.Object target = button.onClick.GetPersistentTarget(i);
            string methodName = button.onClick.GetPersistentMethodName(i);
            if (target != this || string.IsNullOrWhiteSpace(methodName))
                continue;

            if (methodName == nameof(BuyHealthPotion) ||
                methodName == nameof(BuyEnergyPotion) ||
                methodName == nameof(BuySmallFood) ||
                methodName == nameof(BuyBigFood) ||
                methodName == nameof(BuyJumpBoost) ||
                methodName == nameof(BuySpeedBoost) ||
                methodName == nameof(BuyCherryBoost) ||
                methodName == nameof(BuyTeddyBear) ||
                methodName == nameof(BuyVolleyball))
            {
                return true;
            }
        }

        return false;
    }

    private void MarkButtonForAudioOverride(Button button)
    {
        if (button == null)
            return;

        if (button.GetComponent<ShopButtonSfxOverride>() == null)
            button.gameObject.AddComponent<ShopButtonSfxOverride>();
    }
}

public class ShopButtonsMessageUI : MonoBehaviour
{
    private TextMeshProUGUI label;
    private Image background;
    private CanvasGroup canvasGroup;
    private Coroutine activeRoutine;
    private Canvas parentCanvas;
    private RectTransform rootRect;

    public void Initialize(TextMeshProUGUI targetLabel, Image targetBackground)
    {
        label = targetLabel;
        background = targetBackground;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rootRect = transform as RectTransform;
        parentCanvas = GetComponentInParent<Canvas>();
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void ShowAboveTarget(string message, Color color, float fontSize, float duration, RectTransform target)
    {
        if (label == null)
            return;

        gameObject.SetActive(true);
        if (rootRect != null)
        {
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(420f, 110f);
            rootRect.anchoredPosition = GetTargetPosition(target, new Vector2(0f, 95f));
        }

        ApplyVisuals(message, color, fontSize, new Color32(24, 8, 10, 236));

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowRoutine(Mathf.Max(0.25f, duration)));
    }

    public void ShowInsideShopPanel(string message, Color color, float fontSize, float duration, RectTransform shopPanel)
    {
        if (label == null)
            return;

        gameObject.SetActive(true);
        if (rootRect != null)
        {
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = new Vector2(560f, 78f);
            rootRect.anchoredPosition = GetPanelPosition(shopPanel, new Vector2(0f, 80f), new Vector2(0f, -78f));
        }

        ApplyVisuals(message, color, fontSize, new Color32(40, 0, 0, 232));

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowRoutine(Mathf.Max(0.25f, duration)));
    }

    private void ApplyVisuals(string message, Color color, float fontSize, Color backgroundColor)
    {
        label.text = message;
        label.color = color;
        label.fontSize = fontSize;
        if (background != null)
            background.color = backgroundColor;
    }

    public void HideNow()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        if (label != null)
            label.text = string.Empty;
        gameObject.SetActive(false);
    }

    private Vector2 GetTargetPosition(RectTransform target, Vector2 fallbackOffset)
    {
        if (rootRect == null || parentCanvas == null || target == null)
            return fallbackOffset;

        Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        Vector3 worldTopCenter = (corners[1] + corners[2]) * 0.5f;

        RectTransform canvasRect = parentCanvas.transform as RectTransform;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldTopCenter);
        if (canvasRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out Vector2 localPoint))
            return localPoint + new Vector2(0f, 52f);

        return fallbackOffset;
    }

    private Vector2 GetPanelPosition(RectTransform panel, Vector2 fallback, Vector2 offsetFromTopCenter)
    {
        if (rootRect == null || parentCanvas == null || panel == null)
            return fallback;

        Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera;
        Vector3[] corners = new Vector3[4];
        panel.GetWorldCorners(corners);
        Vector3 worldTopCenter = (corners[1] + corners[2]) * 0.5f;

        RectTransform canvasRect = parentCanvas.transform as RectTransform;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldTopCenter);
        if (canvasRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out Vector2 localPoint))
            return localPoint + offsetFromTopCenter;

        return fallback;
    }

    private System.Collections.IEnumerator ShowRoutine(float duration)
    {
        canvasGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(duration);
        canvasGroup.alpha = 0f;
        label.text = string.Empty;
        gameObject.SetActive(false);
        activeRoutine = null;
    }
}

public class ShopButtonClickFlash : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
{
    private Graphic[] targetGraphics = new Graphic[0];
    private Color[] normalColors = new Color[0];
    private readonly Color pressedMultiplier = new Color(0.72f, 0.72f, 0.72f, 1f);
    private Coroutine flashRoutine;

    public void Configure(Button button, Graphic[] graphics)
    {
        if (graphics == null || graphics.Length == 0)
        {
            Graphic fallback = button != null ? button.targetGraphic : null;
            if (fallback == null && button != null)
                fallback = button.GetComponent<Graphic>();
            if (fallback == null && button != null)
                fallback = button.GetComponentInChildren<Image>(true);
            graphics = fallback != null ? new[] { fallback } : new Graphic[0];
        }

        targetGraphics = graphics;
        normalColors = new Color[targetGraphics.Length];
        for (int i = 0; i < targetGraphics.Length; i++)
            normalColors[i] = targetGraphics[i] != null ? targetGraphics[i].color : Color.white;

        if (button != null && targetGraphics.Length > 0 && targetGraphics[0] != null)
            button.targetGraphic = targetGraphics[0];
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }
        SetPressed();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetNormal();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetNormal();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // PointerClick fires after release, which made the shop fade feel delayed.
        // The visual response is handled immediately by PointerDown/PointerUp.
    }

    public void SetNormalNow()
    {
        SetNormal();
    }

    private void SetPressed()
    {
        for (int i = 0; i < targetGraphics.Length; i++)
        {
            Graphic graphic = targetGraphics[i];
            if (graphic != null)
            {
                Color normal = i < normalColors.Length ? normalColors[i] : graphic.color;
                graphic.color = new Color(
                    normal.r * pressedMultiplier.r,
                    normal.g * pressedMultiplier.g,
                    normal.b * pressedMultiplier.b,
                    normal.a);
            }
        }
    }

    private void SetNormal()
    {
        for (int i = 0; i < targetGraphics.Length; i++)
        {
            Graphic graphic = targetGraphics[i];
            if (graphic != null && i < normalColors.Length)
                graphic.color = normalColors[i];
        }
    }

}

[Serializable]
public class PixiePurchaseEntry
{
    public string isoTime;
    public string item;
    public int quantity;
    public int cost;
    public int remainingApples;
}

public static class PixiePurchaseMemory
{
    private const string LastItemKey = "PIXIE_LAST_PURCHASE_ITEM";
    private const string LastCostKey = "PIXIE_LAST_PURCHASE_COST";
    private const string LastQtyKey = "PIXIE_LAST_PURCHASE_QTY";
    private const string LastRemainingKey = "PIXIE_LAST_PURCHASE_REMAINING";
    private const string LastTimeKey = "PIXIE_LAST_PURCHASE_TIME";
    private const string LogKey = "PIXIE_PURCHASE_LOG";
    private const int MaxLogEntries = 120;

    public static void RecordPurchase(string item, int cost, int quantity, int remainingApples)
    {
        if (string.IsNullOrWhiteSpace(item) || quantity <= 0) return;

        string nowIso = DateTime.UtcNow.ToString("O");
        PlayerPrefs.SetString(LastItemKey, item);
        PlayerPrefs.SetInt(LastCostKey, Mathf.Max(0, cost));
        PlayerPrefs.SetInt(LastQtyKey, quantity);
        PlayerPrefs.SetInt(LastRemainingKey, Mathf.Max(0, remainingApples));
        PlayerPrefs.SetString(LastTimeKey, nowIso);

        List<PixiePurchaseEntry> entries = GetPurchaseLog(200);
        entries.Add(new PixiePurchaseEntry
        {
            isoTime = nowIso,
            item = item,
            quantity = quantity,
            cost = Mathf.Max(0, cost),
            remainingApples = Mathf.Max(0, remainingApples)
        });

        if (entries.Count > MaxLogEntries)
            entries.RemoveRange(0, entries.Count - MaxLogEntries);

        SaveEntries(entries);
        PlayerPrefs.Save();
    }

    public static string GetLastPurchaseSummary()
    {
        string item = PlayerPrefs.GetString(LastItemKey, string.Empty);
        if (string.IsNullOrWhiteSpace(item))
            return "No purchase history yet.";

        int cost = PlayerPrefs.GetInt(LastCostKey, 0);
        int qty = PlayerPrefs.GetInt(LastQtyKey, 1);
        int remaining = PlayerPrefs.GetInt(LastRemainingKey, 0);
        string isoTime = PlayerPrefs.GetString(LastTimeKey, string.Empty);

        string whenText = string.Empty;
        if (DateTime.TryParse(isoTime, out DateTime dt))
            whenText = dt.ToLocalTime().ToString("g");

        if (!string.IsNullOrEmpty(whenText))
            return "Last purchase: " + item + " x" + qty + " for " + cost + " Gold Coins at " + whenText + ". Remaining Gold Coins: " + remaining + ".";

        return "Last purchase: " + item + " x" + qty + " for " + cost + " Gold Coins. Remaining Gold Coins: " + remaining + ".";
    }

    public static List<PixiePurchaseEntry> GetPurchaseLog(int maxEntries)
    {
        string raw = PlayerPrefs.GetString(LogKey, string.Empty);
        List<PixiePurchaseEntry> list = new List<PixiePurchaseEntry>();

        if (!string.IsNullOrWhiteSpace(raw))
        {
            string[] lines = raw.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] p = line.Split('|');
                if (p.Length < 5) continue;
                if (!int.TryParse(p[2], out int qty)) qty = 1;
                if (!int.TryParse(p[3], out int cost)) cost = 0;
                if (!int.TryParse(p[4], out int remaining)) remaining = 0;

                list.Add(new PixiePurchaseEntry
                {
                    isoTime = p[0],
                    item = p[1],
                    quantity = qty,
                    cost = cost,
                    remainingApples = remaining
                });
            }
        }

        if (maxEntries > 0 && list.Count > maxEntries)
            list.RemoveRange(0, list.Count - maxEntries);

        return list;
    }

    public static string GetPurchaseLogSummary(int maxEntries)
    {
        List<PixiePurchaseEntry> entries = GetPurchaseLog(maxEntries);
        if (entries.Count == 0) return "No purchases logged.";

        StringBuilder sb = new StringBuilder();
        int start = Mathf.Max(0, entries.Count - maxEntries);
        for (int i = start; i < entries.Count; i++)
        {
            PixiePurchaseEntry e = entries[i];
            sb.Append(e.item).Append(" x").Append(e.quantity).Append(" (").Append(e.cost).Append(" Gold Coins)");
            if (i < entries.Count - 1) sb.Append(" | ");
        }

        return sb.ToString();
    }

    private static void SaveEntries(List<PixiePurchaseEntry> entries)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < entries.Count; i++)
        {
            PixiePurchaseEntry e = entries[i];
            sb.Append(Safe(e.isoTime)).Append('|')
              .Append(Safe(e.item)).Append('|')
              .Append(e.quantity).Append('|')
              .Append(e.cost).Append('|')
              .Append(e.remainingApples);
            if (i < entries.Count - 1) sb.Append('\n');
        }

        PlayerPrefs.SetString(LogKey, sb.ToString());
    }

    private static string Safe(string value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : value.Replace('|', '/').Replace('\n', ' ');
    }
}

