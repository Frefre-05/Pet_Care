using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Button))]
public class LevelPurchaseButton : MonoBehaviour
{
    [Header("Cost & Target")]
    public int cost = 0; // set per button in Inspector
    public bool deductCostOnLoad = false;
    public string sceneName = "Level1"; // set per button (or leave empty and use index)
    public int sceneIndex = -1; // optional alternative to name

    [Header("Optional UI")]
    public TMP_Text warningText; // drag a small TMP text under the button (optional)
    public float warningSeconds = 1.2f; // how long the warning shows

    [Header("Energy Gate (Reversible)")]
    [SerializeField] private bool requireMinEnergyToPlay = true;
    [Range(0f, 100f)] [SerializeField] private float minEnergyPercent = 50f;
    [Header("Progress Gate")]
    [SerializeField] private bool requireLevelProgressUnlock = true;

    Button _btn;
    bool _isLoading;
    float _nextRefreshAt;

    void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(TryBuyAndGo);
        RefreshInteractable();
    }

    void OnEnable() => RefreshInteractable();

    void Update()
    {
        if (Time.unscaledTime < _nextRefreshAt) return;
        _nextRefreshAt = Time.unscaledTime + 0.5f;
        RefreshInteractable();
    }

    public void RefreshInteractable()
    {
        bool energyOk = IsEnergyGatePassed();
        bool progressOk = IsProgressGatePassed();
        if (_btn) _btn.interactable = energyOk && progressOk;
    }

    void TryBuyAndGo()
    {
        if (_isLoading) return;

        bool alreadyCompleted = IsTargetLevelAlreadyCompleted();
        if (!alreadyCompleted && AppleCurrency.Get() < cost)
        {
            if (warningText)
            {
                warningText.text = $"Need {cost} Gold Coins";
                CancelInvoke(nameof(ClearWarning));
                Invoke(nameof(ClearWarning), warningSeconds);
            }
            return;
        }

        if (!IsEnergyGatePassed())
        {
            if (warningText)
            {
                warningText.text = $"Need {Mathf.RoundToInt(minEnergyPercent)}% energy";
                CancelInvoke(nameof(ClearWarning));
                Invoke(nameof(ClearWarning), warningSeconds);
            }
            return;
        }

        if (!IsProgressGatePassed())
        {
            if (warningText)
            {
                warningText.text = "Locked: finish previous level first";
                CancelInvoke(nameof(ClearWarning));
                Invoke(nameof(ClearWarning), warningSeconds);
            }
            else
            {
                Debug.LogWarning("Level locked: finish the previous level first.");
            }
            return;
        }

        if (!alreadyCompleted && deductCostOnLoad && !AppleCurrency.Spend(cost))
            return;

        _isLoading = true;

        // Load by name if provided, else by index
        if (!string.IsNullOrEmpty(sceneName))
            SceneTransitionLoader.LoadScene(sceneName);
        else if (sceneIndex >= 0)
            SceneTransitionLoader.LoadScene(sceneIndex);
        else
            Debug.LogWarning($"{name}: No scene target set.");
    }

    void ClearWarning()
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

    private bool IsTargetLevelAlreadyCompleted()
    {
        int required = ResolveRequiredUnlockedIndex();
        if (required < 0)
            return false;
        return LevelProgress.IsCompleted(required);
    }

    private bool IsProgressGatePassed()
    {
        if (!requireLevelProgressUnlock) return true;
        int required = ResolveRequiredUnlockedIndex();
        if (required < 0) return true;
        return LevelProgress.CanPlay(required);
    }

    private int ResolveRequiredUnlockedIndex()
    {
        string targetName = null;

        if (!string.IsNullOrWhiteSpace(sceneName))
        {
            targetName = sceneName;
        }
        else if (sceneIndex >= 0)
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
}
