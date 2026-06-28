using UnityEngine;

public class TogglePanel : MonoBehaviour
{
    public GameObject panel;
    public GameObject shopWelcomeText;

    public void Toggle()
    {
        if (panel == null)
            return;

        bool willOpen = !panel.activeSelf;
        panel.SetActive(willOpen);

        if (shopWelcomeText != null)
            shopWelcomeText.SetActive(!willOpen);
    }

    public void ClosePanel()
    {
        if (panel != null)
            panel.SetActive(false);

        if (shopWelcomeText != null)
            shopWelcomeText.SetActive(true);
    }

    private void OnDisable()
    {
        ClosePanel();
    }
}
