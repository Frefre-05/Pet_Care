using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AppleCollect : MonoBehaviour
{
    // *** ADD: unique id per apple (set a unique value in Inspector: apple_1, apple_2, ...) ***
    [SerializeField] private string fruitId = "apple_1";

    // your existing fields
    [SerializeField] private Animator anim; // optional
    [SerializeField] private string pickupTrigger = "picked";
    [SerializeField] private GameManager gm; // optional manager for AddApple()

    private Collider2D col;
    private SpriteRenderer sr;
    private bool picked = false;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;

        sr = GetComponent<SpriteRenderer>();

        // *** NEW: If this fruit was already collected in the past, hide it immediately ***
        if (PlayerPrefs.GetInt($"FRUIT_{fruitId}", 0) == 1)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (picked || !other.CompareTag("Player")) return;
        picked = true;

        // === CURRENCY ONLY ===

        // 1) Mark this fruit collected so it stays gone next time
        PlayerPrefs.SetInt($"FRUIT_{fruitId}", 1);
        PlayerPrefs.Save();

        // 2) Add to total apples (currency)
        if (gm != null)
            gm.AddApple(1); // uses your GameManager if present
        else
        {
            int apples = PlayerPrefs.GetInt("APPLE_COUNT", 0);
            PlayerPrefs.SetInt("APPLE_COUNT", apples + 1); // PlayerPrefs fallback
            PlayerPrefs.Save();
        }

        // 3) Optional player growth (keep your logic)
        var growth = other.GetComponent<PlayerGrowth>();
        if (growth != null)
        {
            int applesNow = (gm != null) ? gm.AppleCount : PlayerPrefs.GetInt("APPLE_COUNT", 0);
            if (applesNow % 20 == 0) growth.Grow();
        }

        // 4) Optional collect animation
        if (anim != null) anim.SetTrigger(pickupTrigger);

        // 5) Make it disappear after small delay
        if (col != null) col.enabled = false;
        if (sr != null) sr.enabled = false;
        Destroy(gameObject, 0.2f);

        // <<< ADDED FOR TUTORIAL (DO NOT REMOVE OR MODIFY ANYTHING ABOVE) >>>
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnFirstAppleCollected();
        // <<< END ADDITIONS >>>
    }
}
