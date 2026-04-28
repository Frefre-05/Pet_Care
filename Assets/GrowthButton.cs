using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GrowthButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Costs & Requirements")]
    [SerializeField] private int appleCost = 20;
    [SerializeField, Range(0, 100)] private float requiredNeed = 70f;
    [SerializeField] private float maxScale = 11f;
    [SerializeField] private float scaleStep = 1f;
    [SerializeField] private float growAnimationDuration = 0.25f;

    [Header("References")]
    [SerializeField] private PetNeeds petNeeds;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private float feedbackDuration = 2f;

    private Transform petTransform;
    private static GrowthFeedbackUI feedbackUI;
    private Coroutine growRoutine;

    private void Awake()
    {
        if (tooltipText != null)
            tooltipText.gameObject.SetActive(false);

        EnsureFeedbackUI();
    }

    private void RefreshPetReference()
    {
        if (petNeeds != null && petNeeds.gameObject.activeInHierarchy)
        {
            petTransform = petNeeds.transform;
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        petNeeds = player.GetComponentInChildren<PetNeeds>();
        if (petNeeds != null)
            petTransform = petNeeds.transform;
    }

    private bool AllNeedsHighEnough(PetNeeds pn)
    {
        return pn.Hunger >= requiredNeed &&
               pn.Energy >= requiredNeed &&
               pn.Hygiene >= requiredNeed &&
               pn.Happiness >= requiredNeed &&
               pn.Health >= requiredNeed;
    }

    public void TryGrow()
    {
        RefreshPetReference();

        if (petNeeds == null || petTransform == null)
        {
            Debug.LogError("GrowthButton: no PetNeeds found on the active Player clone.");
            ShowFailureFeedback();
            return;
        }

        if (!AllNeedsHighEnough(petNeeds))
        {
            Debug.Log("GrowthButton: needs too low, cannot grow.");
            ShowFailureFeedback();
            return;
        }

        float currentScale = petTransform.localScale.x;
        if (currentScale >= maxScale)
        {
            Debug.Log("GrowthButton: already at max size.");
            ShowFeedback("Your pet is already at maximum size.", new Color32(255, 238, 170, 255));
            return;
        }

        if (!AppleCurrency.Spend(appleCost))
        {
            Debug.Log("GrowthButton: not enough Gold Coins to grow.");
            ShowFailureFeedback();
            return;
        }

        float newScale = Mathf.Min(currentScale + scaleStep, maxScale);
        if (growRoutine != null)
            StopCoroutine(growRoutine);
        growRoutine = StartCoroutine(AnimateGrowth(currentScale, newScale));

        PlayerPrefs.SetFloat("PET_SCALE", newScale);
        PlayerPrefs.Save();

        Debug.Log($"GrowthButton: grew CLONE to {newScale}, Gold Coins spent: {appleCost}");
        ShowFeedback("Your pet has gotten bigger!", Color.white);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipText == null)
            return;

        tooltipText.text = $"Requires all bars above {requiredNeed} and {appleCost} Gold Coins";
        tooltipText.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipText != null)
            tooltipText.gameObject.SetActive(false);
    }

    private void ShowFailureFeedback()
    {
        ShowFeedback("Your bars arent full, or you dont have enough Gold Coins, sorry!", new Color32(255, 120, 120, 255));
    }

    private IEnumerator AnimateGrowth(float startScale, float targetScale)
    {
        if (petTransform == null)
            yield break;

        float duration = Mathf.Max(0.05f, growAnimationDuration);
        float elapsed = 0f;
        Vector3 current = petTransform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            float scale = Mathf.Lerp(startScale, targetScale, eased);
            petTransform.localScale = new Vector3(scale, scale, current.z);
            yield return null;
        }

        petTransform.localScale = new Vector3(targetScale, targetScale, current.z);
        growRoutine = null;
    }

    private void ShowFeedback(string message, Color textColor)
    {
        EnsureFeedbackUI();
        if (feedbackUI != null)
            feedbackUI.Show(message, textColor, Mathf.Max(0.25f, feedbackDuration));
    }

    private static void EnsureFeedbackUI()
    {
        if (feedbackUI != null)
        {
            ApplyFeedbackLayout(feedbackUI.transform as RectTransform);
            return;
        }

        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Canvas bestCanvas = null;
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null || !canvas.gameObject.activeInHierarchy)
                continue;
            bestCanvas = canvas;
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                break;
        }

        if (bestCanvas == null)
            return;

        Transform existing = bestCanvas.transform.Find("GrowthFeedbackUI");
        if (existing != null)
        {
            feedbackUI = existing.GetComponent<GrowthFeedbackUI>();
            if (feedbackUI != null)
            {
                ApplyFeedbackLayout(existing as RectTransform);
                return;
            }
        }

        GameObject root = new GameObject("GrowthFeedbackUI", typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(bestCanvas.transform, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        ApplyFeedbackLayout(rootRect);

        GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
        backgroundObject.transform.SetParent(root.transform, false);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        Image background = backgroundObject.GetComponent<Image>();
        background.color = new Color32(34, 16, 18, 236);
        background.raycastTarget = false;

        Outline outline = backgroundObject.GetComponent<Outline>();
        outline.effectColor = new Color32(255, 212, 96, 255);
        outline.effectDistance = new Vector2(3f, -3f);

        Shadow shadow = backgroundObject.GetComponent<Shadow>();
        shadow.effectColor = new Color32(18, 6, 0, 180);
        shadow.effectDistance = new Vector2(6f, -6f);

        GameObject topAccent = new GameObject("TopAccent", typeof(RectTransform), typeof(Image));
        topAccent.transform.SetParent(root.transform, false);
        RectTransform topAccentRect = topAccent.GetComponent<RectTransform>();
        topAccentRect.anchorMin = new Vector2(0f, 1f);
        topAccentRect.anchorMax = new Vector2(1f, 1f);
        topAccentRect.pivot = new Vector2(0.5f, 1f);
        topAccentRect.sizeDelta = new Vector2(0f, 8f);
        topAccentRect.anchoredPosition = Vector2.zero;
        Image topAccentImage = topAccent.GetComponent<Image>();
        topAccentImage.color = new Color32(255, 211, 84, 255);
        topAccentImage.raycastTarget = false;

        GameObject bottomAccent = new GameObject("BottomAccent", typeof(RectTransform), typeof(Image));
        bottomAccent.transform.SetParent(root.transform, false);
        RectTransform bottomAccentRect = bottomAccent.GetComponent<RectTransform>();
        bottomAccentRect.anchorMin = new Vector2(0f, 0f);
        bottomAccentRect.anchorMax = new Vector2(1f, 0f);
        bottomAccentRect.pivot = new Vector2(0.5f, 0f);
        bottomAccentRect.sizeDelta = new Vector2(0f, 6f);
        bottomAccentRect.anchoredPosition = Vector2.zero;
        Image bottomAccentImage = bottomAccent.GetComponent<Image>();
        bottomAccentImage.color = new Color32(173, 92, 14, 255);
        bottomAccentImage.raycastTarget = false;

        GameObject textObject = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(root.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 10f);
        textRect.offsetMax = new Vector2(-18f, -10f);

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.Normal;
        label.fontSize = 26f;
        label.outlineWidth = 0.22f;
        label.outlineColor = Color.black;
        label.raycastTarget = false;

        feedbackUI = root.AddComponent<GrowthFeedbackUI>();
        feedbackUI.Initialize(label);
        root.SetActive(false);
    }

    private static void ApplyFeedbackLayout(RectTransform rootRect)
    {
        if (rootRect == null)
            return;

        rootRect.anchorMin = new Vector2(0.5f, 1f);
        rootRect.anchorMax = new Vector2(0.5f, 1f);
        rootRect.pivot = new Vector2(0.5f, 1f);
        rootRect.sizeDelta = new Vector2(620f, 92f);
        rootRect.anchoredPosition = new Vector2(0f, -210f);
    }
}

public class GrowthFeedbackUI : MonoBehaviour
{
    private TextMeshProUGUI label;
    private CanvasGroup canvasGroup;
    private Coroutine activeRoutine;

    public void Initialize(TextMeshProUGUI targetLabel)
    {
        label = targetLabel;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public void Show(string message, Color color, float duration)
    {
        if (label == null)
            return;

        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        label.text = message;
        label.color = color;

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(ShowRoutine(duration));
    }

    private IEnumerator ShowRoutine(float duration)
    {
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(duration);
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
        activeRoutine = null;
    }
}
