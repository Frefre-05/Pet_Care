using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class EndPoint : MonoBehaviour
{
    [Header("Progress")]
    [Tooltip("0=Tutorial, 1=Level1, 2=Level2, 3=Level3, 4=Level4")]
    [SerializeField] int thisLevelIndex = 0;

    [Header("Where to go next")]
    [SerializeField] string sceneToLoad = "House";

    [Header("Fade (optional)")]
    [SerializeField] Image fadeImage; // full-screen black UI Image
    [SerializeField] float fadeDuration = 0.6f;

    [Header("Player filter")]
    [SerializeField] string playerTag = "Player";

    [Header("Apple / Energy System")]
    [Tooltip("PlayerPrefs key for saved apples/energy count")]
    [SerializeField] string appleKey = "Apples";
    [Tooltip("Cost for each level (index = 0=Tutorial, 1=Level1, etc.)")]
    [SerializeField] int[] levelAppleCosts = { 0, 3, 5, 8, 10 };

    bool isTransitioning = false;

    // === ADDED: helper to get the ACTIVE PetNeeds (the clone) ===
    private PetNeeds FindActivePetNeeds()
    {
        var all = FindObjectsOfType<PetNeeds>();
        foreach (var p in all)
        {
            if (p != null && p.isActiveAndEnabled && p.gameObject.activeInHierarchy)
                return p; // this will be your spawned clone
        }
        return null;
    }
    // ===========================================================

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning) return;
        if (!other.CompareTag(playerTag)) return;

        StartCoroutine(TransitionSequence());
    }

    IEnumerator TransitionSequence()
    {
        isTransitioning = true;

        // 1) Unlock next level
        LevelProgress.MarkCompleted(thisLevelIndex);

        // 2) Subtract apples depending on current level
        int apples = PlayerPrefs.GetInt(appleKey, 0);
        int cost = 0;

        if (thisLevelIndex >= 0 && thisLevelIndex < levelAppleCosts.Length)
        {
            cost = levelAppleCosts[thisLevelIndex];
        }

        apples -= cost;
        if (apples < 0) apples = 0; // prevent negative apples
        PlayerPrefs.SetInt(appleKey, apples);
        PlayerPrefs.Save();

        Debug.Log($"Level {thisLevelIndex} completed! Cost: {cost} apples. Remaining apples: {apples}");

        // === ADDED: make the ACTIVE pet happy when teleporting ===
        var pet = FindActivePetNeeds();
        if (pet != null)
        {
            pet.Happiness = 100f; // set bar to full for the clone
        }
        // =========================================================

        // 3) Fade to black (optional)
        if (fadeImage != null && fadeDuration > 0f)
        {
            var c = fadeImage.color;
            c.a = 0f;
            fadeImage.color = c;
            fadeImage.gameObject.SetActive(true);

            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                c.a = Mathf.InverseLerp(0f, fadeDuration, t);
                fadeImage.color = c;
                yield return null;
            }
        }

        yield return new WaitForSecondsRealtime(0.1f);

        // 4) Load next scene
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
