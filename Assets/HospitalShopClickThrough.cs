using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[DisallowMultipleComponent]
public class HospitalShopClickThrough : MonoBehaviour
{
    private static readonly string[] ClickableButtonNames =
    {
        "Health",
        "Energy",
        "Pineapple",
        "Kiwi",
        "Speed",
        "Jump"
    };

    private bool applied;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryInstall(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstall(scene);
    }

    private static void TryInstall(Scene scene)
    {
        if (!string.Equals(scene.name, "Hospital", StringComparison.OrdinalIgnoreCase))
            return;

        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null || !canvas.gameObject.scene.IsValid() || canvas.gameObject.scene != scene)
                continue;
            if (canvas.GetComponent<HospitalShopClickThrough>() != null)
                continue;

            canvas.gameObject.AddComponent<HospitalShopClickThrough>();
        }
    }

    private void Start()
    {
        ApplyIfNeeded();
    }

    private void OnEnable()
    {
        ApplyIfNeeded();
    }

    private void ApplyIfNeeded()
    {
        if (applied)
            return;

        if (!string.Equals(SceneManager.GetActiveScene().name, "Hospital", StringComparison.OrdinalIgnoreCase))
            return;

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            return;

        Button[] buttons = FindTargetButtons(canvas.transform);
        if (buttons.Length == 0)
            return;

        Graphic[] graphics = canvas.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            if (button == null)
                continue;

            RectTransform buttonRect = button.transform as RectTransform;
            if (buttonRect == null)
                continue;

            for (int g = 0; g < graphics.Length; g++)
            {
                Graphic graphic = graphics[g];
                if (graphic == null)
                    continue;
                if (graphic.gameObject == button.gameObject)
                    continue;

                RectTransform graphicRect = graphic.transform as RectTransform;
                if (graphicRect == null)
                    continue;

                if (!RectsOverlap(buttonRect, graphicRect, canvas))
                    continue;

                graphic.raycastTarget = false;
            }
            ConfigureVisibleButtonTarget(button, graphics, canvas);
            AttachPressFeedback(button, graphics, canvas);
        }

        applied = true;
    }

    private static Button[] FindTargetButtons(Transform root)
    {
        List<Button> result = new List<Button>();
        for (int i = 0; i < ClickableButtonNames.Length; i++)
        {
            string targetName = ClickableButtonNames[i];
            Button[] buttons = root.GetComponentsInChildren<Button>(true);
            for (int b = 0; b < buttons.Length; b++)
            {
                if (buttons[b] == null)
                    continue;
                if (!string.Equals(buttons[b].gameObject.name, targetName, StringComparison.Ordinal))
                    continue;
                if (!result.Contains(buttons[b]))
                    result.Add(buttons[b]);
            }
        }

        return result.ToArray();
    }

    private static void ConfigureVisibleButtonTarget(Button button, Graphic[] graphics, Canvas canvas)
    {
        RectTransform buttonRect = button.transform as RectTransform;
        if (buttonRect == null)
            return;

        Graphic bestGraphic = null;
        float bestArea = -1f;
        List<Graphic> eligibleGraphics = GetEligibleGraphicsForButton(buttonRect, graphics, canvas);

        for (int i = 0; i < eligibleGraphics.Count; i++)
        {
            Graphic graphic = eligibleGraphics[i];
            RectTransform graphicRect = graphic.transform as RectTransform;
            float overlapArea = GetOverlapArea(buttonRect, graphicRect, canvas);
            if (overlapArea <= bestArea)
                continue;

            bestArea = overlapArea;
            bestGraphic = graphic;
        }

        if (bestGraphic == null)
            return;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.96f, 0.96f, 0.96f, 1f);
        colors.pressedColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        colors.selectedColor = new Color(0.92f, 0.92f, 0.92f, 1f);
        colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
        colors.fadeDuration = 0.06f;

        button.transition = Selectable.Transition.ColorTint;
        button.targetGraphic = bestGraphic;
        button.colors = colors;
    }

    private static void AttachPressFeedback(Button button, Graphic[] graphics, Canvas canvas)
    {
        if (button == null)
            return;

        HospitalShopPressFeedback feedback = button.GetComponent<HospitalShopPressFeedback>();
        if (feedback == null)
            feedback = button.gameObject.AddComponent<HospitalShopPressFeedback>();

        List<Graphic> targets = new List<Graphic>();
        RectTransform buttonRect = button.transform as RectTransform;
        if (buttonRect == null)
        {
            feedback.Configure(targets);
            return;
        }

        List<Graphic> eligibleGraphics = GetEligibleGraphicsForButton(buttonRect, graphics, canvas);
        for (int i = 0; i < eligibleGraphics.Count; i++)
            if (!targets.Contains(eligibleGraphics[i]))
                targets.Add(eligibleGraphics[i]);

        Graphic targetGraphic = button.targetGraphic;
        if (targetGraphic != null && !targets.Contains(targetGraphic))
            targets.Add(targetGraphic);

        feedback.Configure(targets);
    }

    private static List<Graphic> GetEligibleGraphicsForButton(RectTransform buttonRect, Graphic[] graphics, Canvas canvas)
    {
        List<Graphic> eligible = new List<Graphic>();
        if (buttonRect == null || graphics == null || canvas == null)
            return eligible;

        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Rect buttonScreenRect = GetScreenRect(buttonRect, cam);
        Vector2 buttonCenter = buttonScreenRect.center;
        float buttonArea = Mathf.Max(1f, buttonScreenRect.width * buttonScreenRect.height);
        float maxDistance = Mathf.Max(buttonScreenRect.width, buttonScreenRect.height) * 0.45f;

        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            if (graphic == null || !graphic.gameObject.activeInHierarchy)
                continue;
            if (graphic.gameObject == buttonRect.gameObject)
                continue;
            if (graphic.color.a <= 0.01f)
                continue;
            if (graphic is TextMeshProUGUI)
                continue;

            RectTransform graphicRect = graphic.transform as RectTransform;
            if (graphicRect == null)
                continue;
            if (!RectsOverlap(buttonRect, graphicRect, canvas))
                continue;

            Rect graphicScreenRect = GetScreenRect(graphicRect, cam);
            float graphicArea = Mathf.Max(1f, graphicScreenRect.width * graphicScreenRect.height);
            if (graphicArea > buttonArea * 2.6f)
                continue;

            float distance = Vector2.Distance(buttonCenter, graphicScreenRect.center);
            if (distance > maxDistance)
                continue;

            eligible.Add(graphic);
        }

        return eligible;
    }

    private static bool RectsOverlap(RectTransform a, RectTransform b, Canvas canvas)
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector3[] aCorners = new Vector3[4];
        Vector3[] bCorners = new Vector3[4];
        a.GetWorldCorners(aCorners);
        b.GetWorldCorners(bCorners);

        Rect aRect = WorldCornersToScreenRect(aCorners, cam);
        Rect bRect = WorldCornersToScreenRect(bCorners, cam);
        return aRect.Overlaps(bRect, true);
    }

    private static float GetOverlapArea(RectTransform a, RectTransform b, Canvas canvas)
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        Vector3[] aCorners = new Vector3[4];
        Vector3[] bCorners = new Vector3[4];
        a.GetWorldCorners(aCorners);
        b.GetWorldCorners(bCorners);

        Rect aRect = WorldCornersToScreenRect(aCorners, cam);
        Rect bRect = WorldCornersToScreenRect(bCorners, cam);

        float minX = Mathf.Max(aRect.xMin, bRect.xMin);
        float minY = Mathf.Max(aRect.yMin, bRect.yMin);
        float maxX = Mathf.Min(aRect.xMax, bRect.xMax);
        float maxY = Mathf.Min(aRect.yMax, bRect.yMax);

        if (maxX <= minX || maxY <= minY)
            return 0f;

        return (maxX - minX) * (maxY - minY);
    }

    private static Rect WorldCornersToScreenRect(Vector3[] corners, Camera cam)
    {
        Vector2 min = RectTransformUtility.WorldToScreenPoint(cam, corners[0]);
        Vector2 max = min;
        for (int i = 1; i < corners.Length; i++)
        {
            Vector2 p = RectTransformUtility.WorldToScreenPoint(cam, corners[i]);
            min = Vector2.Min(min, p);
            max = Vector2.Max(max, p);
        }

        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static Rect GetScreenRect(RectTransform rect, Camera cam)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        return WorldCornersToScreenRect(corners, cam);
    }
}

public class HospitalShopPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private readonly List<Graphic> graphics = new List<Graphic>();
    private readonly List<Color> originalColors = new List<Color>();
    private bool pressed;

    public void Configure(List<Graphic> targets)
    {
        graphics.Clear();
        originalColors.Clear();

        if (targets == null)
            return;

        for (int i = 0; i < targets.Count; i++)
        {
            Graphic g = targets[i];
            if (g == null)
                continue;
            graphics.Add(g);
            originalColors.Add(g.color);
        }

        Restore();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
        ApplyMultiplier(0.72f);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
        Restore();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!pressed)
            Restore();
    }

    private void OnDisable()
    {
        pressed = false;
        Restore();
    }

    private void ApplyMultiplier(float multiplier)
    {
        for (int i = 0; i < graphics.Count; i++)
        {
            Graphic g = graphics[i];
            if (g == null)
                continue;

            Color baseColor = i < originalColors.Count ? originalColors[i] : g.color;
            g.color = new Color(baseColor.r * multiplier, baseColor.g * multiplier, baseColor.b * multiplier, baseColor.a);
        }
    }

    private void Restore()
    {
        for (int i = 0; i < graphics.Count; i++)
        {
            Graphic g = graphics[i];
            if (g == null)
                continue;

            Color baseColor = i < originalColors.Count ? originalColors[i] : g.color;
            g.color = baseColor;
        }
    }
}
