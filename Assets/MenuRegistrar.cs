using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class MenuRegistrar : MonoBehaviour
{
    [Header("Drag ALL menu buttons here")]
    [SerializeField] private Button[] buttons;

    [Header("Optional dim overlay (Image)")]
    [SerializeField] private Image dimBackground; // can be null

    [Header("Fade/Dim")]
    [Range(0f, 1f)] public float dimAlpha = 0.5f;
    public bool pauseGame = true;

    CanvasGroup menuCg;

    void Reset()
    {
        // Auto-fill on Add Component
        buttons = GetComponentsInChildren<Button>(true);
        menuCg = GetComponent<CanvasGroup>();
        if (!menuCg) menuCg = gameObject.AddComponent<CanvasGroup>();
    }

    void Awake()
    {
        if (!menuCg) menuCg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        // start hidden/non-interactive
        menuCg.alpha = 0f;
        menuCg.interactable = false;
        menuCg.blocksRaycasts = false;
        gameObject.SetActive(false);

        if (dimBackground) dimBackground.gameObject.SetActive(false);

        // EventSystem safety
        if (EventSystem.current == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.hideFlags = HideFlags.DontSave;
        }
    }

    // Call from the big "LEVELS" button
    public void OpenMenu()
    {
        // allow ALL parent CanvasGroups to interact
        EnableCanvasGroupChain(true);

        gameObject.SetActive(true);
        menuCg.alpha = 1f;
        menuCg.interactable = true;
        menuCg.blocksRaycasts = true;

        // force every button to be clickable
        FixButtons();

        if (dimBackground)
        {
            dimBackground.gameObject.SetActive(true);
            // make sure the dim actually catches clicks behind
            var bgCg = dimBackground.GetComponent<CanvasGroup>() ?? dimBackground.gameObject.AddComponent<CanvasGroup>();
            bgCg.alpha = dimAlpha;
            bgCg.blocksRaycasts = true;
        }

        if (pauseGame) Time.timeScale = 0f;
    }

    // Call from the Close button
    public void CloseMenu()
    {
        menuCg.interactable = false;
        menuCg.blocksRaycasts = false;
        menuCg.alpha = 0f;
        gameObject.SetActive(false);

        if (dimBackground) dimBackground.gameObject.SetActive(false);

        if (pauseGame) Time.timeScale = 1f;
    }

    void FixButtons()
    {
        if (buttons == null) return;
        foreach (var b in buttons)
        {
            if (!b) continue;
            b.interactable = true;
            if (b.targetGraphic) b.targetGraphic.raycastTarget = true;
            // also ensure parent graphics allow raycasts
            var img = b.GetComponent<Image>();
            if (img) img.raycastTarget = true;
        }
    }

    void EnableCanvasGroupChain(bool on)
    {
        // walk up parents and flip any CanvasGroup that would grey children
        Transform t = transform;
        while (t != null)
        {
            var cg = t.GetComponent<CanvasGroup>();
            if (cg)
            {
                cg.interactable = on;
                cg.blocksRaycasts = on;
                if (on && cg.alpha < 1f) cg.alpha = 1f;
            }
            t = t.parent;
        }
    }
}