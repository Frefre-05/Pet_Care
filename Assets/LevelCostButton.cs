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
    [SerializeField] private int cost = 0; // Set per button in Inspector
    [SerializeField] private bool deductCostOnLoad = false;

    [Header("Optional visuals")]
    [SerializeField] private GameObject lockIcon; // small padlock image (optional)
    [SerializeField] private CanvasGroup greyOut; // to fade the button if locked (optional)
    [SerializeField] private TMP_Text warningText; // optional TMP text to show "Need X apples"
    [SerializeField] private float warningDuration = 1.5f;

    [Header("Energy Gate (Reversible)")]
    [SerializeField] private bool requireMinEnergyToPlay = true;
    [Range(0f, 100f)] [SerializeField] private float minEnergyPercent = 50f;
    [Header("Progress Gate")]
    [SerializeField] private bool requireLevelProgressUnlock = true;

    private Button btn;
    private bool isLoading;
    private float nextRefreshAt;

    void OnEnable()
    {
        btn = GetComponent<Button>();
        btn.onClick.RemoveListener(HandleClick);
        btn.onClick.AddListener(HandleClick);
        RefreshInteractable();
    }

    void Update()
    {
        if (Time.unscaledTime < nextRefreshAt) return;
        nextRefreshAt = Time.unscaledTime + 0.5f;
        RefreshInteractable();
    }

    /// <summary>Call this if your apple count changes while the menu is open.</summary>
    public void RefreshInteractable()
    {
        int apples = AppleCurrency.Get();
        bool canAfford = apples >= cost;
        bool energyOk = IsEnergyGatePassed();
        bool progressOk = IsProgressGatePassed();
        bool canUse = canAfford && energyOk && progressOk;

        if (btn) btn.interactable = canUse;
        if (lockIcon) lockIcon.SetActive(!canUse);
        if (greyOut) greyOut.alpha = canUse ? 1f : 0.5f;
    }

    private void HandleClick()
    {
        if (isLoading) return;

        int apples = AppleCurrency.Get();
        if (apples < cost)
        {
            ShowWarning();
            RefreshInteractable();
            return;
        }

        if (!IsEnergyGatePassed())
        {
            ShowEnergyWarning();
            RefreshInteractable();
            return;
        }

        if (!IsProgressGatePassed())
        {
            ShowProgressWarning();
            RefreshInteractable();
            return;
        }

        // Optional deduction. Disabled by default to keep apple count consistent across scenes.
        if (deductCostOnLoad)
            AppleCurrency.Set(apples - cost);
        isLoading = true;

        // Load the scene
        if (reference == RefType.ByName && !string.IsNullOrEmpty(sceneName))
            SceneTransitionLoader.LoadScene(sceneName);
        else if (reference == RefType.ByIndex && sceneIndex >= 0)
            SceneTransitionLoader.LoadScene(sceneIndex);
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

    private bool IsEnergyGatePassed()
    {
        if (!requireMinEnergyToPlay) return true;
        PetNeeds petNeeds = FindAnyObjectByType<PetNeeds>();
        if (petNeeds == null) return true;
        return petNeeds.Energy >= minEnergyPercent;
    }

    private bool IsProgressGatePassed()
    {
        if (!requireLevelProgressUnlock) return true;
        int required = ResolveRequiredUnlockedIndex();
        if (required < 0) return true; // non-level scenes
        return LevelProgress.CanPlay(required);
    }

    private int ResolveRequiredUnlockedIndex()
    {
        string targetName = null;

        if (reference == RefType.ByName && !string.IsNullOrWhiteSpace(sceneName))
        {
            targetName = sceneName;
        }
        else if (reference == RefType.ByIndex && sceneIndex >= 0)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            if (!string.IsNullOrWhiteSpace(path))
                targetName = System.IO.Path.GetFileNameWithoutExtension(path);
        }

        string s = (targetName ?? string.Empty).Trim().ToLowerInvariant().Replace(" ", "");
        if (s.Contains("tutorial")) return 0;
        if (s.Contains("level1")) return 1;
        if (s.Contains("level2")) return 2;
        if (s.Contains("level3")) return 3;
        if (s.Contains("level4")) return 4;
        return -1;
    }

    private void ShowEnergyWarning()
    {
        if (warningText)
        {
            warningText.text = $"Need {Mathf.RoundToInt(minEnergyPercent)}% energy";
            CancelInvoke(nameof(ClearWarning));
            Invoke(nameof(ClearWarning), warningDuration);
        }
        else
        {
            Debug.LogWarning($"Not enough energy. Requires at least {minEnergyPercent}%.");
        }
    }

    private void ShowProgressWarning()
    {
        if (warningText)
        {
            warningText.text = "Locked: finish previous level first";
            CancelInvoke(nameof(ClearWarning));
            Invoke(nameof(ClearWarning), warningDuration);
        }
        else
        {
            Debug.LogWarning("Level locked: finish the previous level first.");
        }
    }
}
