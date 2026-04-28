using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShopNPC : MonoBehaviour
{
    public GameObject shopPanel;
    public TextMeshProUGUI dialogueText;
    [Header("Render Order")]
    [SerializeField] private int npcSortingOrder = 20;
    [SerializeField] private bool keepNpcVisibleInFront = true;
    [SerializeField] private string welcomeMessage = "Mask Dude: Welcome to the magical potion shop. Please choose the item you would like to buy.";
    private SpriteRenderer npcRenderer;
    private Collider2D npcCollider;
    private int touchingPlayers;

    private void Awake()
    {
        npcRenderer = GetComponent<SpriteRenderer>();
        npcCollider = GetComponent<Collider2D>();
        ApplyRenderSettings();
        EnsureDialogueStyle();
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void Start()
    {
        ApplyRenderSettings();
    }

    private void LateUpdate()
    {
        if (keepNpcVisibleInFront)
            ApplyRenderSettings();
    }

    private void ApplyRenderSettings()
    {
        if (npcRenderer == null) return;
        if (!npcRenderer.enabled) npcRenderer.enabled = true;
        if (npcRenderer.sortingOrder != npcSortingOrder)
            npcRenderer.sortingOrder = npcSortingOrder;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayerCollider(other))
            return;

        touchingPlayers++;
        if (shopPanel != null)
            shopPanel.SetActive(true);
        if (dialogueText != null)
            dialogueText.text = welcomeMessage;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayerCollider(other))
            return;

        touchingPlayers = Mathf.Max(0, touchingPlayers - 1);
        if (touchingPlayers == 0 && shopPanel != null)
            shopPanel.SetActive(false);
    }

    private bool IsPlayerCollider(Collider2D other)
    {
        if (other == null)
            return false;

        if (other.CompareTag("Player"))
            return true;
        if (other.GetComponent<PetNeeds>() != null)
            return true;
        if (other.GetComponentInParent<PetNeeds>() != null)
            return true;
        if (other.GetComponent<PlayerMovement>() != null)
            return true;
        if (other.GetComponentInParent<PlayerMovement>() != null)
            return true;
        return false;
    }

    private void EnsureDialogueStyle()
    {
        if (dialogueText == null)
            return;

        dialogueText.enableAutoSizing = false;
        dialogueText.fontSize = 22f;
        dialogueText.alignment = TextAlignmentOptions.Center;
        dialogueText.textWrappingMode = TextWrappingModes.Normal;
        dialogueText.overflowMode = TextOverflowModes.Overflow;
        dialogueText.outlineWidth = 0.25f;
        dialogueText.outlineColor = new Color32(20, 4, 6, 255);
        dialogueText.color = Color.white;
        dialogueText.margin = new Vector4(18f, 14f, 18f, 14f);

        RectTransform textRect = dialogueText.rectTransform;
        if (textRect != null)
        {
            textRect.anchorMin = new Vector2(0.5f, 1f);
            textRect.anchorMax = new Vector2(0.5f, 1f);
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.sizeDelta = new Vector2(700f, 84f);
            textRect.anchoredPosition = new Vector2(0f, -18f);
        }

        Transform existing = dialogueText.transform.parent != null ? dialogueText.transform.parent.Find("DialogueBackdrop") : null;
        if (existing != null)
            return;

        GameObject backdropObject = new GameObject("DialogueBackdrop", typeof(RectTransform), typeof(Image), typeof(Outline));
        backdropObject.transform.SetParent(dialogueText.transform, false);
        backdropObject.transform.SetAsFirstSibling();

        RectTransform backdropRect = backdropObject.GetComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = new Vector2(-10f, -10f);
        backdropRect.offsetMax = new Vector2(10f, 10f);

        Image backdrop = backdropObject.GetComponent<Image>();
        backdrop.color = new Color32(38, 8, 12, 220);
        backdrop.raycastTarget = false;

        Outline outline = backdropObject.GetComponent<Outline>();
        outline.effectColor = new Color32(255, 210, 120, 220);
        outline.effectDistance = new Vector2(2f, -2f);

        GameObject accentObject = new GameObject("TopAccent", typeof(RectTransform), typeof(Image));
        accentObject.transform.SetParent(backdropObject.transform, false);
        RectTransform accentRect = accentObject.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.sizeDelta = new Vector2(0f, 6f);
        accentRect.anchoredPosition = Vector2.zero;

        Image accent = accentObject.GetComponent<Image>();
        accent.color = new Color32(255, 177, 78, 255);
        accent.raycastTarget = false;
    }
}
