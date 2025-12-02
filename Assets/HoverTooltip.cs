using UnityEngine;
using TMPro; // if your text is TextMeshPro

public class HoverTooltip : MonoBehaviour,
UnityEngine.EventSystems.IPointerEnterHandler,
UnityEngine.EventSystems.IPointerExitHandler
{
    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private string tooltipMessage = "Requires all bars above 70%";
    [SerializeField] private TextMeshProUGUI tooltipText;

    private void Awake()
    {
        if (tooltipObject != null)
            tooltipObject.SetActive(false);
    }

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(true);
            if (tooltipText != null)
                tooltipText.text = tooltipMessage;
        }
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (tooltipObject != null)
            tooltipObject.SetActive(false);
    }
}
