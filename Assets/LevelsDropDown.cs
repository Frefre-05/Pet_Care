using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelsDropDown : MonoBehaviour
{
    [Header("Menu Parts")]
    [SerializeField] private GameObject levelsMenu; // the panel that contains all the level buttons
    [SerializeField] private Image dimBackground; // full-screen Image behind the panel (optional)

    [Header("Fade / Dim")]
    [SerializeField] private float dimAlpha = 0.5f;

    [Header("Scene Names (match Build Settings)")]
    [SerializeField] private string sceneTutorial = "Tutorial";
    [SerializeField] private string sceneLevel1 = "Level1";
    [SerializeField] private string sceneLevel2 = "Level2";
    [SerializeField] private string sceneLevel3 = "Level3";
    [SerializeField] private string sceneLevel4 = "Level4";
    [SerializeField] private string sceneHospital = "Hospital";
    [SerializeField] private string sceneStore = "Store";

    CanvasGroup menuCg;
    CanvasGroup dimCg;

    void Awake()
    {
        // --- get/add CanvasGroup on the TARGETS (never on the button) ---
        if (levelsMenu == null)
        {
            Debug.LogError("LevelsDropDown: Assign the LevelsMenu (panel) in the Inspector.");
        }
        else
        {
            menuCg = levelsMenu.GetComponent<CanvasGroup>();
            if (menuCg == null) menuCg = levelsMenu.AddComponent<CanvasGroup>();
        }

        if (dimBackground != null)
        {
            dimCg = dimBackground.GetComponent<CanvasGroup>();
            if (dimCg == null) dimCg = dimBackground.gameObject.AddComponent<CanvasGroup>();
            dimBackground.raycastTarget = true; // blocks clicks behind the menu when visible
        }

        HideMenuImmediate();
    }

    // --- Public API for your OnClick on the green "LEVELS" button ---
    public void OpenMenu()
    {
        if (menuCg == null) return;

        if (!levelsMenu.activeSelf) levelsMenu.SetActive(true);
        menuCg.alpha = 1f;
        menuCg.interactable = true;
        menuCg.blocksRaycasts = true;

        if (dimCg != null)
        {
            if (!dimBackground.gameObject.activeSelf) dimBackground.gameObject.SetActive(true);
            dimCg.alpha = dimAlpha;
            dimCg.blocksRaycasts = true;
        }

        // Time.timeScale = 0f; // pause game
    }

    public void CloseMenu()
    {
        if (menuCg == null) return;

        menuCg.alpha = 0f;
        menuCg.interactable = false;
        menuCg.blocksRaycasts = false;
        levelsMenu.SetActive(false);

        if (dimCg != null)
        {
            dimCg.alpha = 0f;
            dimCg.blocksRaycasts = false;
            dimBackground.gameObject.SetActive(false);
        }

        Time.timeScale = 1f; // resume game
    }

    void HideMenuImmediate()
    {
        if (levelsMenu != null)
        {
            if (!levelsMenu.activeSelf) levelsMenu.SetActive(false);
            if (menuCg != null)
            {
                menuCg.alpha = 0f;
                menuCg.interactable = false;
                menuCg.blocksRaycasts = false;
            }
        }

        if (dimBackground != null)
        {
            if (dimCg != null)
            {
                dimCg.alpha = 0f;
                dimCg.blocksRaycasts = false;
            }
            dimBackground.gameObject.SetActive(false);
        }
    }

    // --- Button hooks (assign directly in each button's OnClick) ---
    public void LoadTutorial() => LoadSceneByName(sceneTutorial);
    public void LoadLevel1() => LoadSceneByName(sceneLevel1);
    public void LoadLevel2() => LoadSceneByName(sceneLevel2);
    public void LoadLevel3() => LoadSceneByName(sceneLevel3);
    public void LoadLevel4() => LoadSceneByName(sceneLevel4);
    public void LoadHospital() => LoadSceneByName(sceneHospital);
    public void LoadStore() => LoadSceneByName(sceneStore);

    public void LoadSceneByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(name);
    }
}
