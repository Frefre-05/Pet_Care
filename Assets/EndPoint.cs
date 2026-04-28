using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class EndPoint : MonoBehaviour
{
    const string TutorialCompletedKey = "TutorialCompleted";
    const float PetSicknessChanceOnLevelComplete = 0.10f;
    const float PetSicknessHealthLoss = 25f;
    static readonly float HappinessRewardOnEndpoint = 0f;

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
        if (finishedIndex == 1) GamesChatBotStats.RecordEvent("complete_level_1");
        else if (finishedIndex == 2) GamesChatBotStats.RecordEvent("complete_level_2");
        else if (finishedIndex == 3) GamesChatBotStats.RecordEvent("complete_level_3");
        else if (finishedIndex == 4) GamesChatBotStats.RecordEvent("complete_level_4");
        if (finishedIndex == 0)
        {
            PlayerPrefs.SetInt(TutorialCompletedKey, 1);
            PlayerPrefs.Save();
        }

        // 2) Do not charge Gold Coins here.
        // Entry buttons/shop already handle spending; charging in EndPoint causes double-deduction.
        Debug.Log($"Level {finishedIndex} completed. Gold Coins unchanged: {AppleCurrency.Get()}");

        var pet = FindActivePetNeeds();
        if (pet != null)
        {
            ApplyEndpointHappinessReward(pet);
            TryApplyLevelCompletionSickness(pet);
        }

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

    private void ApplyEndpointHappinessReward(PetNeeds pet)
    {
        if (pet == null)
            return;

        if (HappinessRewardOnEndpoint <= 0f)
            return;

        pet.Happiness = Mathf.Clamp(pet.Happiness + HappinessRewardOnEndpoint, 0f, 100f);
    }

    private void TryApplyLevelCompletionSickness(PetNeeds pet)
    {
        if (pet == null)
            return;

        if (IsHospitalScene())
            return;

        if (Random.value > PetSicknessChanceOnLevelComplete)
            return;

        pet.ApplySickness();
        pet.TakeNeedsDamage(PetSicknessHealthLoss);

        string petName = PlayerPrefs.GetString("PlayerName", string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(petName))
            petName = "Your pet";

        string levelName = SceneManager.GetActiveScene().name;
        if (string.IsNullOrWhiteSpace(levelName))
            levelName = "this level";

        GamesChatBot.QueueLevelSicknessWarning(petName, levelName);
        Debug.Log($"[EndPoint] {petName} got sick after finishing {levelName}, lost {PetSicknessHealthLoss}% health, and now has faster health drain until cured.");
    }

    private bool IsHospitalScene()
    {
        return string.Equals(SceneManager.GetActiveScene().name, "Hospital", System.StringComparison.OrdinalIgnoreCase);
    }
}
