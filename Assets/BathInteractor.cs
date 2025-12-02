using System.Collections;
using UnityEngine;
using TMPro;

public class BathInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PetNeeds petNeeds;
    [SerializeField] private KeyCode bathKey = KeyCode.E;
    [SerializeField] private float bathDuration = 2f; // how long to “shower”

    [Header("Prompt (UI)")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string message = "Press E to Shower!";
    [SerializeField] private bool bob = true;
    [SerializeField] private float bobSpeed = 3f;
    [SerializeField] private float bobHeight = 6f;

    private bool canPrompt = false;
    private bool isBathing = false;
    private Vector3 promptBasePos;

    private void Start()
    {
        // Grab PetNeeds from the active player clone
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
        if (canPrompt && !isBathing && Input.GetKeyDown(bathKey))
        {
            StartCoroutine(BathSequence());
        }

        if (promptText != null && bob)
        {
            var rt = promptText.rectTransform;
            Vector3 pos = promptBasePos;
            pos.y += Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            rt.anchoredPosition = pos;
        }
    }

    private IEnumerator BathSequence()
    {
        isBathing = true;

        if (promptText != null)
            promptText.gameObject.SetActive(false);

        // Make sure we’re using the clone PetNeeds
        if (petNeeds == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                petNeeds = player.GetComponentInChildren<PetNeeds>();
        }

        if (petNeeds != null)
        {
            petNeeds.Bath(); // Hygiene = 100
        }

        yield return new WaitForSecondsRealtime(bathDuration);

        isBathing = false;

        if (canPrompt && promptText != null)
            promptText.gameObject.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        canPrompt = true;
        if (promptText != null)
            promptText.gameObject.SetActive(true);

        // ALWAYS grab PetNeeds from this player (the clone)
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
}
