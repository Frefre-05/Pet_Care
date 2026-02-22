using System.Collections;
using TMPro;
using UnityEngine;


public class BedSleepInteractor : MonoBehaviour
{
    private const string LastBedUseKey = "PIXIE_AI_LAST_BED_USE_UTC";

    [Header("References")]
    [SerializeField] private PetNeeds petNeeds;
    [SerializeField] private BlackoutFaders blackoutFader;
    [SerializeField] private KeyCode sleepKey = KeyCode.E;
    [SerializeField] private float sleepDuration = 3f; // how long to "sleep"
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("Prompt (UI)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string message = "Press E to Sleep";
    [SerializeField] private bool bob = true;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float bobHeight = 6f;

    private bool canPrompt;
    private bool isSleeping;
    private Vector3 promptBasePos;

    private void Start()
    {
        blackoutFader = AutoBlackoutFader.EnsureInstance();

        // Always try to grab the PetNeeds from the active player clone
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            petNeeds = player.GetComponentInChildren<PetNeeds>();

        if (promptText != null)
        {
            promptText.text = message;
            promptBasePos = promptText.rectTransform.anchoredPosition;
            promptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (canPrompt && !isSleeping && Input.GetKeyDown(sleepKey))
            StartCoroutine(SleepSequence());

        if (promptText != null && bob)
        {
            var rt = promptText.rectTransform;
            Vector3 pos = promptBasePos;
            pos.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            rt.anchoredPosition = pos;
        }
    }

    private IEnumerator SleepSequence()
    {
        isSleeping = true;

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        // Make sure we're using the clone PetNeeds
        if (petNeeds == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                petNeeds = player.GetComponentInChildren<PetNeeds>();
        }

        if (blackoutFader == null)
            blackoutFader = AutoBlackoutFader.EnsureInstance();

        if (blackoutFader != null)
            yield return blackoutFader.FadeOut(fadeDuration);

        if (petNeeds != null)
        {
            petNeeds.SleepFill(); // fills energy and records bed usage
            PlayerPrefs.SetInt(LastBedUseKey, NowUnix());
            PlayerPrefs.Save();
        }

        yield return new WaitForSecondsRealtime(sleepDuration);

        if (blackoutFader != null)
            yield return blackoutFader.FadeIn(fadeDuration);

        isSleeping = false;

        if (canPrompt && promptText != null)
            promptText.gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        canPrompt = true;
        if (promptText != null)
            promptText.gameObject.SetActive(true);

        // ALWAYS grab PetNeeds from the player that entered (the clone)
        var pn = other.GetComponentInChildren<PetNeeds>();
        if (pn != null)
            petNeeds = pn;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        canPrompt = false;
        if (promptText != null)
            promptText.gameObject.SetActive(false);
    }

    private int NowUnix()
    {
        long unix = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (unix > int.MaxValue) return int.MaxValue;
        if (unix < int.MinValue) return int.MinValue;
        return (int)unix;
    }
}
