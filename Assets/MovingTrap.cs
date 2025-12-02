using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovingTrap : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Transform leftPoint;
    [SerializeField] private Transform rightPoint;
    [SerializeField] private float speed = 3f; // how fast the saw moves

    private int direction = 1; // 1 = right, -1 = left
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic; // so physics doesn't ragdoll it
        }
    }

    private void FixedUpdate()
    {
        if (leftPoint == null || rightPoint == null) return;

        // move horizontally
        float move = direction * speed * Time.fixedDeltaTime;
        Vector2 newPos = new Vector2(transform.position.x + move, transform.position.y);

        if (rb != null)
            rb.MovePosition(newPos);
        else
            transform.position = newPos;

        // bounce at limits
        if (transform.position.x >= rightPoint.position.x && direction > 0)
            direction = -1;
        else if (transform.position.x <= leftPoint.position.x && direction < 0)
            direction = 1;
    }

    // Just to see the path in Scene view (optional)
    private void OnDrawGizmosSelected()
    {
        if (leftPoint != null && rightPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(leftPoint.position, rightPoint.position);
        }
    }
}
