using UnityEngine;

[RequireComponent(typeof(PlayerUnstuckHitbox))]
public class PlayerMovement : MonoBehaviour
{
    private const string SpeedBoostUntilKey = "PLAYER_SPEED_BOOST_UNTIL_UTC";

    public float moveSpeed = 5f;
    [Header("Shop Boosts")]
    [SerializeField] private float speedBoostMultiplier = 1.25f;
    [Header("Hunger Speed Penalty (Reversible)")]
    [SerializeField] private bool enableHungerSpeedPenalty = true;
    [Range(0f, 100f)] [SerializeField] private float lowHungerThreshold = 35f;
    [Range(0f, 100f)] [SerializeField] private float emptyHungerThreshold = 5f;
    [Range(0.2f, 1f)] [SerializeField] private float minSpeedMultiplierAtZeroHunger = 0.6f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private PetNeeds petNeeds;

    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        petNeeds = FindAnyObjectByType<PetNeeds>();

        rb.freezeRotation = true; // fix ragdoll
    }

    void Update()
    {
        if (LevelsDropDown.IsMenuOpen)
        {
            moveInput = 0f;
            if (anim != null)
                anim.SetFloat("Speed", 0f);
            return;
        }

        moveInput = Input.GetAxisRaw("Horizontal");

        // Animation
        anim.SetFloat("Speed", Mathf.Abs(moveInput));

        // Flip
        if (moveInput > 0)
            sr.flipX = false;
        else if (moveInput < 0)
            sr.flipX = true;
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        if (LevelsDropDown.IsMenuOpen)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float speedMul = GetHungerSpeedMultiplier() * GetShopSpeedMultiplier();
        rb.linearVelocity = new Vector2(moveInput * moveSpeed * speedMul, rb.linearVelocity.y);
    }

    private float GetHungerSpeedMultiplier()
    {
        if (!enableHungerSpeedPenalty) return 1f;
        if (petNeeds == null || !petNeeds.gameObject.activeInHierarchy) petNeeds = FindAnyObjectByType<PetNeeds>();
        if (petNeeds == null) return 1f;

        float hunger = petNeeds.Hunger;
        if (hunger >= lowHungerThreshold) return 1f;
        float t = Mathf.InverseLerp(lowHungerThreshold, emptyHungerThreshold, hunger);
        return Mathf.Lerp(1f, minSpeedMultiplierAtZeroHunger, t);
    }

    private float GetShopSpeedMultiplier()
    {
        if (!IsBoostActive(SpeedBoostUntilKey))
            return 1f;

        return Mathf.Max(1f, speedBoostMultiplier);
    }

    private bool IsBoostActive(string key)
    {
        string raw = PlayerPrefs.GetString(key, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (!System.DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out System.DateTime untilUtc))
            return false;

        return System.DateTime.UtcNow < untilUtc.ToUniversalTime();
    }
}
