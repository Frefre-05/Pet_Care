using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

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
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
}
