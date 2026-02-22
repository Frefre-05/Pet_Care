using UnityEngine;

[RequireComponent(typeof(PlayerUnstuckHitbox))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
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
        float speedMul = GetHungerSpeedMultiplier();
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
}
