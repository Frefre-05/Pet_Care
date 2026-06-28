using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AppleCollect : MonoBehaviour
{
    [SerializeField] private string fruitId = "apple_1";

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

        //If fruit collected hide
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

        //  CURRENCY ONLY 

        PlayerPrefs.SetInt($"FRUIT_{fruitId}", 1);
        PlayerPrefs.Save();

        if (gm != null)
            gm.AddApple(1); 
        else
            AppleCurrency.Add(1);

        var growth = other.GetComponent<PlayerGrowth>();
        if (growth != null)
        {
            int applesNow = AppleCurrency.Get();
            if (applesNow % 20 == 0) growth.Grow();
        }

        if (anim != null) anim.SetTrigger(pickupTrigger);

        if (col != null) col.enabled = false;
        if (sr != null) sr.enabled = false;
        Destroy(gameObject, 0.2f);

        // For tutorial
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnFirstAppleCollected();
        //Additions
    }
}
