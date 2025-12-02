using UnityEngine;

public class JumpTwice : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Jump Settings")]
    public int extraJumps = 1; // 1 = allows double jump
    private int jumpsLeft;

    private Rigidbody2D rb;
    private Animator anim;

    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        jumpsLeft = extraJumps;
    }

    void Update()
    {
        // --- Move ---
        float move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);

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
}
