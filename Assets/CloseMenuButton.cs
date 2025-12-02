using UnityEngine;

public class CloseMenuButton : MonoBehaviour
{
    [Header("Assign the menu you want to close")]
    [SerializeField] private GameObject menuToClose; // Your LevelsMenu panel
    [SerializeField] private GameObject dimBackground; // Optional background overlay

    [Header("Optional CanvasGroup (for fading & raycasts)")]
    [SerializeField] private CanvasGroup menuCanvasGroup;

    public void CloseMenu()
    {
        // Hide the menu panel
        if (menuToClose != null)
            menuToClose.SetActive(false);

        // Hide the dim background if it exists
        if (dimBackground != null)
            dimBackground.SetActive(false);

        // Disable raycasts if using a CanvasGroup
        if (menuCanvasGroup != null)
        {
            menuCanvasGroup.alpha = 0f;
            menuCanvasGroup.interactable = false;
            menuCanvasGroup.blocksRaycasts = false;
        }

        Debug.Log("Menu closed successfully!");
    }
}
