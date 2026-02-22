using UnityEngine;
using UnityEngine.UI;

public static class AutoBlackoutFader
{
    private static BlackoutFaders instance;
    private const string CanvasName = "__AutoBlackoutCanvas";
    private const string PanelName = "BlackoutPanel";

    public static BlackoutFaders EnsureInstance()
    {
        if (instance != null) return instance;
        GameObject canvasGo = GameObject.Find(CanvasName);
        if (canvasGo == null)
        {
            canvasGo = new GameObject(CanvasName);
            Object.DontDestroyOnLoad(canvasGo);
        }

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        if (canvas == null) canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        if (canvasGo.GetComponent<CanvasScaler>() == null) canvasGo.AddComponent<CanvasScaler>();
        if (canvasGo.GetComponent<GraphicRaycaster>() == null) canvasGo.AddComponent<GraphicRaycaster>();

        Transform panelTf = canvasGo.transform.Find(PanelName);
        GameObject panel = panelTf != null ? panelTf.gameObject : new GameObject(PanelName);
        if (panelTf == null) panel.transform.SetParent(canvasGo.transform, false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        if (rt == null) rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        if (image == null) image = panel.AddComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = true;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;

        instance = panel.GetComponent<BlackoutFaders>();
        if (instance == null) instance = panel.AddComponent<BlackoutFaders>();
        return instance;
    }
}
