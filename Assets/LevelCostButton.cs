using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // If you don't use TextMeshPro, you can remove this and the TMP fields

[RequireComponent(typeof(Button))]
public class LevelCostButton : MonoBehaviour
{
    public enum RefType { ByName, ByIndex }

    [Header("Scene to load")]
    [SerializeField] private RefType reference = RefType.ByName;
    [SerializeField] private string sceneName; // e.g., "Level 1"
    [SerializeField] private int sceneIndex = -1; // or use Build Settings index

    [Header("Currency")]
    [SerializeField] private string applesKey = "Apples"; // PlayerPrefs key
    [SerializeField] private int cost = 0; // Set per button in Inspector

    [Header("Optional visuals")]
    [SerializeField] private GameObject lockIcon; // small padlock image (optional)
    [SerializeField] private CanvasGroup greyOut; // to fade the button if locked (optional)
    [SerializeField] private TMP_Text warningText; // optional TMP text to show "Need X apples"
    [SerializeField] private float warningDuration = 1.5f;

    private Button btn;

    void OnEnable()
    {
        btn = GetComponent<Button>();
        btn.onClick.RemoveListener(HandleClick);
        btn.onClick.AddListener(HandleClick);
        RefreshInteractable();
    }

    /// <summary>Call this if your apple count changes while the menu is open.</summary>
    public void RefreshInteractable()
    {
        int apples = PlayerPrefs.GetInt(applesKey, 0);
        bool canAfford = apples >= cost;

        if (btn) btn.interactable = canAfford;
        if (lockIcon) lockIcon.SetActive(!canAfford);
        if (greyOut) greyOut.alpha = canAfford ? 1f : 0.5f;
    }

    private void HandleClick()
    {
        int apples = PlayerPrefs.GetInt(applesKey, 0);
        if (apples < cost)
        {
            ShowWarning();
            RefreshInteractable();
            return;
        }

        // Deduct cost and save
        apples -= cost;
        PlayerPrefs.SetInt(applesKey, apples);
        PlayerPrefs.Save();

        // Load the scene
        if (reference == RefType.ByName && !string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else if (reference == RefType.ByIndex && sceneIndex >= 0)
            SceneManager.LoadScene(sceneIndex);
        else
            Debug.LogWarning($"{name}: No valid scene target set on LevelCostButton.");
    }

    private void ShowWarning()
    {
        if (warningText)
        {
            warningText.text = $"Need {cost} apples";
            CancelInvoke(nameof(ClearWarning));
            Invoke(nameof(ClearWarning), warningDuration);
        }
        else
        {
            Debug.LogWarning($"Not enough apples. Requires {cost}.");
        }
    }

    private void ClearWarning()
    {
        if (warningText) warningText.text = "";
    }
}
