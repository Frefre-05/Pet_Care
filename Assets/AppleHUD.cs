using TMPro;
using UnityEngine;

public class AppleHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI appleText;

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        AppleCurrency.OnChanged += HandleAppleChanged;
        Refresh();
    }

    private void OnDisable()
    {
        AppleCurrency.OnChanged -= HandleAppleChanged;
    }

    public void Refresh()
    {
        if (appleText == null) return;
        appleText.text = AppleCurrency.Get().ToString();
    }

    private void HandleAppleChanged(int apples)
    {
        Refresh();
    }
}
