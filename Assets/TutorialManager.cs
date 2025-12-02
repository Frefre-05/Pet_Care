using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject startPanel; // rules panel
    [SerializeField] private GameObject applePanel; // second panel

    [Header("Scripts to disable while paused (optional)")]
    [SerializeField] private MonoBehaviour[] scriptsToDisable; // e.g. Movement

    private bool hasShownStart = false;
    private bool hasShownApple = false;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ShowStartPanel();
    }

    // ====== PUBLIC BUTTON METHODS ======

    // Called by the "OK / Start" button on the first panel
    public void OnClickStartOK()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        ResumeGame();
    }

    // Called by the "OK" button on the apple panel
    public void OnClickAppleOK()
    {
        if (applePanel != null)
            applePanel.SetActive(false);

        ResumeGame();
    }

    // Called from AppleCollect when first apple is picked
    public void OnFirstAppleCollected()
    {
        if (hasShownApple) return; // only once

        hasShownApple = true;
        ShowApplePanel();
    }

    // ====== INTERNAL ======

    private void ShowStartPanel()
    {
        if (hasShownStart) return;
        hasShownStart = true;

        if (startPanel != null)
            startPanel.SetActive(true);

        PauseGame();
    }

    private void ShowApplePanel()
    {
        if (applePanel != null)
            applePanel.SetActive(true);

        PauseGame();
    }

    private void PauseGame()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (scriptsToDisable != null)
        {
            foreach (var s in scriptsToDisable)
            {
                if (s != null) s.enabled = false;
            }
        }
    }

    private void ResumeGame()
    {
        Time.timeScale = previousTimeScale;

        if (scriptsToDisable != null)
        {
            foreach (var s in scriptsToDisable)
            {
                if (s != null) s.enabled = true;
            }
        }
    }
}
