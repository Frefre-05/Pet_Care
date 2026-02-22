using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndPoint : MonoBehaviour
{
    [Header("Progress")]
    [Tooltip("0=Tutorial, 1=Level1, 2=Level2, 3=Level3, 4=Level4")]
    [SerializeField] int thisLevelIndex = 0;

    [Header("Where to go next")]
    [SerializeField] string sceneToLoad = "House";

    [Header("Player filter")]
    [SerializeField] string playerTag = "Player";

    bool isTransitioning = false;

    // === ADDED: helper to get the ACTIVE PetNeeds (the clone) ===
    private PetNeeds FindActivePetNeeds()
    {
        var all = Object.FindObjectsByType<PetNeeds>(FindObjectsSortMode.None);
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
        int finishedIndex = ResolveFinishedLevelIndex();
        LevelProgress.MarkCompleted(finishedIndex);

        // 2) Do not charge apples here.
        // Entry buttons/shop already handle spending; charging in EndPoint causes double-deduction.
        Debug.Log($"Level {finishedIndex} completed. Apples unchanged: {AppleCurrency.Get()}");

        // === ADDED: make the ACTIVE pet happy when teleporting ===
        var pet = FindActivePetNeeds();
        if (pet != null)
        {
            pet.Happiness = 100f; // set bar to full for the clone
        }
        // =========================================================

        // 3) Global scene transition handles fade-out/in.
        yield return new WaitForSecondsRealtime(0.05f);

        // 4) Load next scene
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            // Keep trophy transition at previous slower speed.
            SceneTransitionLoader.LoadScene(sceneToLoad, 1.7f, 1.7f);
        }
    }

    private int ResolveFinishedLevelIndex()
    {
        string scene = SceneManager.GetActiveScene().name;
        string s = scene == null ? string.Empty : scene.ToLowerInvariant().Replace(" ", "");

        // Robust fallback to scene-name mapping to avoid inspector misconfiguration.
        if (s.Contains("tutorial")) return 0;
        if (s.Contains("level1")) return 1;
        if (s.Contains("level2")) return 2;
        if (s.Contains("level3")) return 3;
        if (s.Contains("level4")) return 4;

        return Mathf.Clamp(thisLevelIndex, 0, 4);
    }
}
