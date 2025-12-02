using UnityEngine;
using TMPro;

public class ShopNPC : MonoBehaviour
{
    public GameObject shopPanel;
    public TextMeshProUGUI dialogueText;

    private void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            shopPanel.SetActive(true);
            if (dialogueText != null)
                dialogueText.text = "Hey, what medicine do you want to buy?";
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            shopPanel.SetActive(false);
        }
    }
}
