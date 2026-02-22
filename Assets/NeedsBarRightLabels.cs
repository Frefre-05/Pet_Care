using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class NeedsBarRightLabels : MonoBehaviour
{
    [SerializeField] private float rightPadding = 18f;
    [SerializeField] private float minLabelWidth = 120f;
    [SerializeField] private int fallbackFontSize = 30;
    [SerializeField] private Color fallbackColor = Color.white;

    private static NeedsBarRightLabels instance;
    private readonly List<Binding> bindings = new List<Binding>();
    private TMP_Text styleSource;

    private readonly string[] barNames = { "HungerBar", "EnergyBar", "HygieneBar", "HappinessBar", "HealthBar" };
    private readonly string[] barTexts = { "Hunger", "Energy", "Hygiene", "Happiness", "Health" };

    private class Binding
    {
        public RectTransform barRect;
        public RectTransform labelRect;
        public TMP_Text label;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        if (instance != null) return;
        GameObject go = new GameObject("__NeedsBarRightLabels");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<NeedsBarRightLabels>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RebuildForScene(SceneManager.GetActiveScene());
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void LateUpdate()
    {
        if (bindings.Count == 0) return;
        for (int i = 0; i < bindings.Count; i++)
            PositionLabel(bindings[i]);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebuildForScene(scene);
    }

    private void RebuildForScene(Scene scene)
    {
        bindings.Clear();
        styleSource = null;

        if (!IsNeedsScene(scene.name))
            return;

        styleSource = FindStyleSourceText();

        for (int i = 0; i < barNames.Length; i++)
        {
            Slider slider = FindSliderByName(barNames[i]);
            if (slider == null) continue;

            RectTransform barRect = slider.GetComponent<RectTransform>();
            if (barRect == null) continue;

            TMP_Text label = GetOrCreateLabel(slider.transform.parent as RectTransform, barNames[i] + "_RightLabel");
            if (label == null) continue;

            label.text = barTexts[i];

            Binding b = new Binding
            {
                barRect = barRect,
                labelRect = label.rectTransform,
                label = label
            };
            bindings.Add(b);
            PositionLabel(b);
        }
    }

    private static bool IsNeedsScene(string sceneName)
    {
        return string.Equals(sceneName, "House", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(sceneName, "Hospital", StringComparison.OrdinalIgnoreCase);
    }

    private Slider FindSliderByName(string objectName)
    {
        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < sliders.Length; i++)
        {
            if (sliders[i] != null && string.Equals(sliders[i].gameObject.name, objectName, StringComparison.Ordinal))
                return sliders[i];
        }
        return null;
    }

    private TMP_Text FindStyleSourceText()
    {
        TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text t = texts[i];
            if (t == null) continue;
            if (!t.gameObject.activeInHierarchy) continue;
            if (t.gameObject.name.Contains("_RightLabel", StringComparison.Ordinal)) continue;
            return t;
        }
        return null;
    }

    private TMP_Text GetOrCreateLabel(RectTransform parent, string name)
    {
        if (parent == null) return null;

        Transform existing = parent.Find(name);
        TMP_Text label;
        if (existing != null)
        {
            label = existing.GetComponent<TMP_Text>();
            if (label != null)
            {
                ApplyStyle(label);
                return label;
            }
        }

        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        label = go.GetComponent<TextMeshProUGUI>();
        ApplyStyle(label);
        return label;
    }

    private void ApplyStyle(TMP_Text label)
    {
        label.raycastTarget = false;
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Overflow;
        label.alignment = TextAlignmentOptions.Left | TextAlignmentOptions.Midline;

        if (styleSource != null)
        {
            label.font = styleSource.font;
            label.fontSharedMaterial = styleSource.fontSharedMaterial;
            label.fontStyle = styleSource.fontStyle;
            label.fontSize = styleSource.fontSize;
            label.color = styleSource.color;
        }
        else
        {
            label.fontSize = fallbackFontSize;
            label.color = fallbackColor;
        }
    }

    private void PositionLabel(Binding b)
    {
        if (b == null || b.barRect == null || b.labelRect == null) return;
        RectTransform parent = b.labelRect.parent as RectTransform;
        if (parent == null) return;

        Vector3 worldCenter = b.barRect.TransformPoint(b.barRect.rect.center);
        Vector3 worldRight = b.barRect.TransformPoint(new Vector3(b.barRect.rect.xMax, b.barRect.rect.center.y, 0f));
        Vector2 localCenter = parent.InverseTransformPoint(worldCenter);
        Vector2 localRight = parent.InverseTransformPoint(worldRight);

        float barHalfWidth = Mathf.Abs(localRight.x - localCenter.x);
        float x = localCenter.x + barHalfWidth + rightPadding;

        b.labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        b.labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        b.labelRect.pivot = new Vector2(0f, 0.5f);
        b.labelRect.anchoredPosition = new Vector2(x, localCenter.y);
        b.labelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minLabelWidth);
        b.labelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(30f, b.barRect.rect.height));
    }
}

