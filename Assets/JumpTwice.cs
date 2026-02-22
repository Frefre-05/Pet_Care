using UnityEngine;

[RequireComponent(typeof(PlayerUnstuckHitbox))]
public class JumpTwice : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    [Header("Hunger Speed Penalty (Reversible)")]
    [SerializeField] private bool enableHungerSpeedPenalty = true;
    [Range(0f, 100f)] [SerializeField] private float lowHungerThreshold = 35f;
    [Range(0f, 100f)] [SerializeField] private float emptyHungerThreshold = 5f;
    [Range(0.2f, 1f)] [SerializeField] private float minSpeedMultiplierAtZeroHunger = 0.6f;

    [Header("Jump Settings")]
    public int extraJumps = 1; // 1 = allows double jump
    private int jumpsLeft;

    private Rigidbody2D rb;
    private Animator anim;
    private PetNeeds petNeeds;

    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        jumpsLeft = extraJumps;
        petNeeds = FindAnyObjectByType<PetNeeds>();
    }

    void Update()
    {
        // --- Move ---
        float move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed * GetHungerSpeedMultiplier(), rb.linearVelocity.y);

        // --- Simple ground check (no groundCheck object) ---
        // If player�s vertical speed is almost zero AND player is near ground
        isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;

        // --- Jump ---
        if (Input.GetButtonDown("Jump")) 
        {
            if (isGrounded)
            {
                anim.Play("Jump");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsLeft = extraJumps; // reset extra jumps when on ground
            }
            else if (jumpsLeft > 0)
            {
                anim.Play("DoubleJump");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpsLeft--;
            }
        }

        // --- Animation (optional) ---
        if (anim != null)
        {
            anim.SetBool("isRunning", move != 0);
            anim.SetBool("isGrounded", isGrounded);
        }
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
