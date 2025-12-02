using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Fruit : MonoBehaviour
{
    [SerializeField] private GameManager gm;
    [SerializeField] private string fruitId = "apple_001"; // make each apple unique!
    [SerializeField] private Animator anim;
    [SerializeField] private string pickupTrigger = "Picked";
    [SerializeField] private float destroyDelay = 0.1f;

    private Collider2D col;
    private SpriteRenderer sr;
    private bool picked;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;
        sr = GetComponent<SpriteRenderer>();

        if (gm == null)
            gm = FindObjectOfType<GameManager>(); // fallback

        // ✅ If already collected in a previous run, hide it immediately
        if (gm != null && gm.IsFruitCollected(fruitId))
        {
            gameObject.SetActive(false);
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (picked || other.CompareTag("Player") == false || gm == null)
            return;

        picked = true;

        // ✅ Add apple to count
        gm.AddApple(1);

        // ✅ Mark fruit collected so it won’t respawn next time
        gm.MarkFruitCollected(fruitId);

        // ✅ Optional animation
        if (anim != null)
            anim.SetTrigger(pickupTrigger);

        if (col != null)
            col.enabled = false;

        if (sr != null)
            sr.enabled = false;

        Destroy(gameObject, destroyDelay);
    }

#if UNITY_EDITOR
// Auto-generate a unique ID if empty
private void OnValidate()
{
if (string.IsNullOrEmpty(fruitId))
fruitId = System.Guid.NewGuid().ToString("N");
}
#endif
}
