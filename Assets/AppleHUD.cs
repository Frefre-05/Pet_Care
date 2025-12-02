using TMPro;
using UnityEngine;

public class AppleHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI appleText;

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        appleText.text = AppleCurrency.Get().ToString();
    }
}
