using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject startPanel; // rules panel
    [SerializeField] private GameObject applePanel; // second panel

    [Header("Scripts to disable while paused (optional)")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable; // e.g. Movement

    [Header("Start Text Polish")]
    [SerializeField] private TMP_Text startPanelText;
    [SerializeField] private TMP_Text topBodyText;
    [SerializeField] private TMP_Text bottomBodyText;
    [SerializeField] private bool autoFindStartPanelText = true;
    [SerializeField] private bool typewriterStartText = true;
    [SerializeField] private float startTextCharsPerSecond = 45f;
    [SerializeField] private bool lockStartTextPosition = false;
    [TextArea(2, 8)]
    [SerializeField] private string startPanelMessageOverride = "";
    [SerializeField] private bool hideExtraStartTexts = false;
    [SerializeField] private bool useBookLayout = true;
    [SerializeField] private bool autoArrangeBodyTexts = false;
    [SerializeField] private Vector2 bookAnchoredPosition = new Vector2(0f, -95f);
    [SerializeField] private float bookWidth = 860f;
    [SerializeField] private float bookHeight = 240f;
    [SerializeField] private float bookPageGap = 20f;
    [SerializeField] private TextAlignmentOptions bookAlignment = TextAlignmentOptions.TopLeft;
    [SerializeField] private bool forceBodyFontSize = true;
    [SerializeField] private float bodyFontSize = 28f;

    [Header("Start Panel Pop Animation")]
    [SerializeField] private bool animateStartPanelPop = true;
    [SerializeField] private float popDuration = 0.18f;
    [SerializeField] private float popStartScale = 0.88f;

    private bool hasShownStart = false;
    private bool hasShownApple = false;
    private float previousTimeScale = 1f;
    private Coroutine startTextRoutine;
    private RectTransform startTextRect;
    private Vector2 startTextLockedPos;
    private bool isLockingStartTextPos;
    private readonly List<TMP_Text> animatedStartTexts = new List<TMP_Text>();
    private readonly List<TMP_Text> laidOutBodyTexts = new List<TMP_Text>();
    private readonly List<RectTransform> lockedTextRects = new List<RectTransform>();
    private readonly List<Vector2> lockedTextPositions = new List<Vector2>();
    private readonly List<TMP_Text> hiddenExtraTexts = new List<TMP_Text>();
    private Coroutine startPanelPopRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ShowStartPanel();
    }

    private void LateUpdate()
    {
        if (!isLockingStartTextPos) return;
        if (startTextRect != null)
            startTextRect.anchoredPosition = startTextLockedPos;

        for (int i = 0; i < lockedTextRects.Count; i++)
        {
            RectTransform rt = lockedTextRects[i];
            if (rt == null) continue;
            rt.anchoredPosition = lockedTextPositions[i];
        }
    }

    // ====== PUBLIC BUTTON METHODS ======

    // Called by the "OK / Start" button on the first panel
    public void OnClickStartOK()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        ResumeGame();
    }

    // Called by the "OK" button on the apple panel
    public void OnClickAppleOK()
    {
        if (applePanel != null)
            applePanel.SetActive(false);

        ResumeGame();
    }

    // Called from AppleCollect when first apple is picked
    public void OnFirstAppleCollected()
    {
        if (hasShownApple) return; // only once

        hasShownApple = true;
        ShowApplePanel();
    }

    // ====== INTERNAL ======

    private void ShowStartPanel()
    {
        if (hasShownStart) return;
        hasShownStart = true;

        if (startPanel != null)
            startPanel.SetActive(true);

        PlayStartPanelPopAnimation();
        PrepareStartPanelText();
        PauseGame();
    }

    private void ShowApplePanel()
    {
        if (applePanel != null)
            applePanel.SetActive(true);

        PauseGame();
    }

    private void PauseGame()
    {
        previousTimeScale = Time.timeScale;
        if (previousTimeScale <= 0f)
            previousTimeScale = 1f;
        Time.timeScale = 0f;

        if (scriptsToDisable != null)
        {
            foreach (var s in scriptsToDisable)
            {
                if (s != null) s.enabled = false;
            }
        }
    }

    private void ResumeGame()
    {
        if (startTextRoutine != null)
        {
            StopCoroutine(startTextRoutine);
            startTextRoutine = null;
        }
        if (startPanelPopRoutine != null)
        {
            StopCoroutine(startPanelPopRoutine);
            startPanelPopRoutine = null;
        }
        animatedStartTexts.Clear();
        laidOutBodyTexts.Clear();
        lockedTextRects.Clear();
        lockedTextPositions.Clear();
        isLockingStartTextPos = false;

        Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;

        if (scriptsToDisable != null)
        {
            foreach (var s in scriptsToDisable)
            {
                if (s != null) s.enabled = true;
            }
        }

        RestoreHiddenStartTexts();
    }

    private void PrepareStartPanelText()
    {
        if (startPanelText == null && autoFindStartPanelText && startPanel != null)
            startPanelText = FindBestStartPanelText();

        if (startPanelText == null) return;

        if (hideExtraStartTexts)
            HideExtraStartTexts();

        if (!string.IsNullOrWhiteSpace(startPanelMessageOverride))
            startPanelText.text = startPanelMessageOverride.Trim();

        startPanelText.textWrappingMode = TextWrappingModes.Normal;
        startPanelText.alignment = useBookLayout ? bookAlignment : TextAlignmentOptions.Center;
        startPanelText.overflowMode = TextOverflowModes.Overflow;

        startTextRect = startPanelText.rectTransform;
        if (startTextRect != null)
        {
            startTextLockedPos = startTextRect.anchoredPosition;
            isLockingStartTextPos = lockStartTextPosition && autoArrangeBodyTexts;
        }

        if (useBookLayout && autoArrangeBodyTexts)
            ApplyBookLayoutToBodyTexts();

        PrepareAnimatedTextTargets();

        if (startTextRoutine != null)
            StopCoroutine(startTextRoutine);

        if (typewriterStartText && startTextCharsPerSecond > 0f)
            startTextRoutine = StartCoroutine(TypeStartTextRoutine());
        else
        {
            for (int i = 0; i < animatedStartTexts.Count; i++)
                if (animatedStartTexts[i] != null)
                    animatedStartTexts[i].maxVisibleCharacters = int.MaxValue;
        }
    }

    private IEnumerator TypeStartTextRoutine()
    {
        if (animatedStartTexts.Count == 0)
            yield break;

        int[] lengths = new int[animatedStartTexts.Count];
        int longest = 0;
        for (int i = 0; i < animatedStartTexts.Count; i++)
        {
            TMP_Text t = animatedStartTexts[i];
            string text = (t == null || t.text == null) ? string.Empty : t.text;
            lengths[i] = text.Length;
            if (lengths[i] > longest) longest = lengths[i];
            if (t != null) t.maxVisibleCharacters = 0;
        }

        float cps = Mathf.Max(1f, startTextCharsPerSecond);
        int visible = 0;
        while (visible < longest)
        {
            int add = Mathf.Max(1, Mathf.FloorToInt(cps * Time.unscaledDeltaTime));
            visible = Mathf.Min(longest, visible + add);
            for (int i = 0; i < animatedStartTexts.Count; i++)
            {
                TMP_Text t = animatedStartTexts[i];
                if (t == null) continue;
                t.maxVisibleCharacters = Mathf.Min(lengths[i], visible);
            }
            yield return null;
        }

        startTextRoutine = null;
    }

    private TMP_Text FindBestStartPanelText()
    {
        if (startPanel == null) return null;

        TMP_Text[] texts = startPanel.GetComponentsInChildren<TMP_Text>(true);
        if (texts == null || texts.Length == 0) return null;
        if (texts.Length == 1) return texts[0];

        TMP_Text best = null;
        int bestLen = -1;
        float bestArea = -1f;

        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text t = texts[i];
            if (t == null) continue;

            string content = t.text == null ? string.Empty : t.text.Trim();
            int len = content.Length;
            RectTransform rt = t.rectTransform;
            float area = rt == null ? 0f : Mathf.Abs(rt.rect.width * rt.rect.height);

            bool better = len > bestLen || (len == bestLen && area > bestArea);
            if (better)
            {
                best = t;
                bestLen = len;
                bestArea = area;
            }
        }

        return best ?? texts[0];
    }

    private List<TMP_Text> GetBodyTexts()
    {
        List<TMP_Text> body = new List<TMP_Text>();

        if (topBodyText != null) body.Add(topBodyText);
        if (bottomBodyText != null && bottomBodyText != topBodyText) body.Add(bottomBodyText);
        if (body.Count > 0) return body;

        if (startPanel == null) return body;

        TMP_Text[] texts = startPanel.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text t = texts[i];
            if (t == null || !t.enabled) continue;

            // Never treat button labels (like "OK") as tutorial body text.
            if (t.GetComponentInParent<UnityEngine.UI.Button>() != null) continue;
            body.Add(t);
        }

        return body;
    }

    private void ApplyBookLayoutToBodyTexts()
    {
        List<TMP_Text> bodyTexts = GetPrimaryBodyTexts(2);
        if (bodyTexts.Count == 0) return;

        laidOutBodyTexts.Clear();
        lockedTextRects.Clear();
        lockedTextPositions.Clear();

        float targetW = Mathf.Max(360f, bookWidth);
        float targetH = Mathf.Max(140f, bookHeight);

        if (bodyTexts.Count == 1)
        {
            ApplyTextRect(bodyTexts[0], bookAnchoredPosition, targetW, targetH);
            laidOutBodyTexts.Add(bodyTexts[0]);
            return;
        }

        // Stack body text blocks vertically (one on top of the other).
        float eachH = Mathf.Max(100f, (targetH - Mathf.Max(0f, bookPageGap)) * 0.5f);
        float topY = bookAnchoredPosition.y + (eachH * 0.5f) + (bookPageGap * 0.5f);
        float bottomY = bookAnchoredPosition.y - (eachH * 0.5f) - (bookPageGap * 0.5f);

        ApplyTextRect(bodyTexts[0], new Vector2(bookAnchoredPosition.x, topY), targetW, eachH);
        ApplyTextRect(bodyTexts[1], new Vector2(bookAnchoredPosition.x, bottomY), targetW, eachH);
        laidOutBodyTexts.Add(bodyTexts[0]);
        laidOutBodyTexts.Add(bodyTexts[1]);

        // Any extra body text gets placed below.
        for (int i = 2; i < bodyTexts.Count; i++)
        {
            float y = bottomY - (i - 1) * 36f;
            ApplyTextRect(bodyTexts[i], new Vector2(bookAnchoredPosition.x, y), targetW, 36f);
            laidOutBodyTexts.Add(bodyTexts[i]);
        }
    }

    private void ApplyTextRect(TMP_Text t, Vector2 anchoredPos, float width, float height)
    {
        if (t == null) return;
        t.textWrappingMode = TextWrappingModes.Normal;
        t.alignment = bookAlignment;
        t.overflowMode = TextOverflowModes.Masking;
        t.enableAutoSizing = false;
        if (forceBodyFontSize)
            t.fontSize = Mathf.Max(14f, bodyFontSize);
        t.maxVisibleCharacters = int.MaxValue;

        RectTransform rt = t.rectTransform;
        if (rt == null) return;

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(Mathf.Max(120f, width), Mathf.Max(36f, height));
        rt.anchoredPosition = anchoredPos;

        if (lockStartTextPosition && autoArrangeBodyTexts)
        {
            lockedTextRects.Add(rt);
            lockedTextPositions.Add(anchoredPos);
        }
    }

    private List<TMP_Text> GetPrimaryBodyTexts(int maxCount)
    {
        List<TMP_Text> body = GetBodyTexts();
        body.Sort((a, b) =>
        {
            int aLen = (a == null || a.text == null) ? 0 : a.text.Trim().Length;
            int bLen = (b == null || b.text == null) ? 0 : b.text.Trim().Length;
            if (aLen != bLen) return bLen.CompareTo(aLen);

            float aArea = a == null || a.rectTransform == null ? 0f : Mathf.Abs(a.rectTransform.rect.width * a.rectTransform.rect.height);
            float bArea = b == null || b.rectTransform == null ? 0f : Mathf.Abs(b.rectTransform.rect.width * b.rectTransform.rect.height);
            return bArea.CompareTo(aArea);
        });

        if (body.Count > maxCount)
            body.RemoveRange(maxCount, body.Count - maxCount);
        return body;
    }

    private void PrepareAnimatedTextTargets()
    {
        animatedStartTexts.Clear();
        List<TMP_Text> baseTargets = null;

        if (useBookLayout && autoArrangeBodyTexts && laidOutBodyTexts.Count > 0)
        {
            baseTargets = laidOutBodyTexts;
        }
        else
        {
            baseTargets = GetPrimaryBodyTexts(2);
        }

        for (int i = 0; i < baseTargets.Count; i++)
        {
            TMP_Text t = baseTargets[i];
            if (t != null) animatedStartTexts.Add(t);
        }

        if (animatedStartTexts.Count == 0 && startPanelText != null)
            animatedStartTexts.Add(startPanelText);
    }

    private void HideExtraStartTexts()
    {
        RestoreHiddenStartTexts();
        if (startPanel == null || startPanelText == null) return;

        TMP_Text[] texts = startPanel.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text t = texts[i];
            if (t == null || t == startPanelText || !t.enabled) continue;
            if (t.GetComponentInParent<UnityEngine.UI.Button>() != null) continue; // keep button labels like OK
            t.enabled = false;
            hiddenExtraTexts.Add(t);
        }
    }

    private void RestoreHiddenStartTexts()
    {
        if (hiddenExtraTexts.Count == 0) return;
        for (int i = 0; i < hiddenExtraTexts.Count; i++)
        {
            TMP_Text t = hiddenExtraTexts[i];
            if (t != null) t.enabled = true;
        }
        hiddenExtraTexts.Clear();
    }

    private void PlayStartPanelPopAnimation()
    {
        if (!animateStartPanelPop || startPanel == null) return;
        RectTransform rt = startPanel.GetComponent<RectTransform>();
        if (rt == null) return;

        if (startPanelPopRoutine != null)
            StopCoroutine(startPanelPopRoutine);
        startPanelPopRoutine = StartCoroutine(PopRoutine(rt));
    }

    private IEnumerator PopRoutine(RectTransform rt)
    {
        Vector3 end = Vector3.one;
        Vector3 start = Vector3.one * Mathf.Clamp(popStartScale, 0.6f, 1f);
        float dur = Mathf.Max(0.05f, popDuration);
        float t = 0f;
        rt.localScale = start;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            k = 1f - Mathf.Pow(1f - k, 3f); // ease-out cubic
            rt.localScale = Vector3.LerpUnclamped(start, end, k);
            yield return null;
        }

        rt.localScale = end;
        startPanelPopRoutine = null;
    }
}
