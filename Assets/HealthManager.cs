using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    [Header("Heart Images (UI)")]
    public Image[] hearts; // drag 3 UI Images here
    public Sprite fullHeart; // red heart
    public Sprite emptyHeart;// outline

    public void SetHearts(int currentHearts, int maxHearts)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHearts) hearts[i].sprite = fullHeart;
            else hearts[i].sprite = emptyHeart;

            hearts[i].enabled = i < maxHearts;
        }
    }
}
