using UnityEngine;

public class WallSlide : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float wallSlideSpeed = 1.5f; // how fast to slide down
    [SerializeField] private Transform wallCheck; // empty object on side of player
    [SerializeField] private float wallCheckDistance = 0.3f;
    [SerializeField] private LayerMask wallLayer; // set in inspector to "Wall" layer

    private bool isTouchingWall;
    private bool isSliding;

    private void Update()
    {
        CheckWall();

        // If touching a wall and falling downward, start sliding
        if (isTouchingWall && rb.linearVelocity.y < 0)
        {
            isSliding = true;
        }
        else
        {
            isSliding = false;
        }

        // Apply controlled slide speed
        if (isSliding)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
    }

    private void CheckWall()
    {
        // Cast a small ray toward the wall in the direction the sprite is facing
        Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;

        RaycastHit2D hit = Physics2D.Raycast(wallCheck.position, direction, wallCheckDistance, wallLayer);
        isTouchingWall = hit.collider != null;

        // (optional) visualize the ray
        Debug.DrawRay(wallCheck.position, direction * wallCheckDistance, Color.red);
    }
}
