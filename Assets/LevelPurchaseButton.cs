using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Button))]
public class LevelPurchaseButton : MonoBehaviour
{
    [Header("Cost & Target")]
    public int cost = 0; // set per button in Inspector
    public string sceneName = "Level1"; // set per button (or leave empty and use index)
    public int sceneIndex = -1; // optional alternative to name

    [Header("Optional UI")]
    public TMP_Text warningText; // drag a small TMP text under the button (optional)
    public float warningSeconds = 1.2f; // how long the warning shows

    Button _btn;

    void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(TryBuyAndGo);
        RefreshInteractable();
    }

    void OnEnable() => RefreshInteractable();

    public void RefreshInteractable()
    {
        // Grey out if player can't afford (purely visual)
        if (_btn) _btn.interactable = AppleCurrency.Get() >= cost;
    }

    void TryBuyAndGo()
    {
        if (!AppleCurrency.Spend(cost))
        {
            if (warningText)
            {
                warningText.text = $"Need {cost} apples";
                CancelInvoke(nameof(ClearWarning));
                Invoke(nameof(ClearWarning), warningSeconds);
            }
            return;
        }

        // Load by name if provided, else by index
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
        else if (sceneIndex >= 0)
            SceneManager.LoadScene(sceneIndex);
        else
            Debug.LogWarning($"{name}: No scene target set.");
    }

    void ClearWarning()
    {
        if (warningText) warningText.text = "";
    }
}
