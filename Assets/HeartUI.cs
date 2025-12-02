using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    public Image[] hearts; // drag 3 heart images here
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public void UpdateHearts(int currentHearts)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            hearts[i].sprite = i < currentHearts ? fullHeart : emptyHeart;
        }
    }
}
