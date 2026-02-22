using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerUnstuckHitbox : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float inputThreshold = 0.2f;
    [SerializeField] private float minMovementPerFixedStep = 0.001f;
    [SerializeField] private float stuckTime = 0.2f;
    [SerializeField] private float unstuckCooldown = 0.25f;

    [Header("Unstuck Push")]
    [SerializeField] private float horizontalNudge = 0.08f;
    [SerializeField] private float verticalNudge = 0.08f;
    [SerializeField] private float pushSpeed = 2.5f;

    [Header("Physics")]
    [SerializeField] private bool forceZeroFriction = true;
    [SerializeField] private bool setContinuousCollision = true;
    [SerializeField] private bool setInterpolation = true;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 lastPosition;
    private float stuckTimer;
    private float cooldownTimer;
    private PhysicsMaterial2D runtimeNoFriction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (setContinuousCollision)
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (setInterpolation)
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        if (forceZeroFriction)
            ApplyNoFrictionMaterial();
    }

    private void OnEnable()
    {
        if (rb != null) lastPosition = rb.position;
        stuckTimer = 0f;
        cooldownTimer = 0f;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        float inputX = Input.GetAxisRaw("Horizontal");
        bool tryingToMove = Mathf.Abs(inputX) >= inputThreshold;
        float moved = Vector2.Distance(rb.position, lastPosition);

        if (tryingToMove && moved <= minMovementPerFixedStep)
            stuckTimer += Time.fixedDeltaTime;
        else
            stuckTimer = 0f;

        cooldownTimer -= Time.fixedDeltaTime;

        if (stuckTimer >= stuckTime && cooldownTimer <= 0f)
        {
            float dir = inputX == 0f ? 0f : Mathf.Sign(inputX);
            AttemptUnstuck(dir);
            stuckTimer = 0f;
            cooldownTimer = unstuckCooldown;
        }

        lastPosition = rb.position;
    }

    private void AttemptUnstuck(float direction)
    {
        Vector2 nudge = new Vector2(direction * horizontalNudge, verticalNudge);
        rb.position += nudge;

        Vector2 v = rb.linearVelocity;
        if (direction != 0f)
            v.x = direction * Mathf.Max(Mathf.Abs(v.x), pushSpeed);
        rb.linearVelocity = v;
    }

    private void ApplyNoFrictionMaterial()
    {
        if (col == null) return;

        runtimeNoFriction = new PhysicsMaterial2D("Pixie_NoFriction_Runtime")
        {
            friction = 0f,
            bounciness = 0f
        };
        col.sharedMaterial = runtimeNoFriction;
    }

    private void OnDestroy()
    {
        if (runtimeNoFriction != null)
            Destroy(runtimeNoFriction);
    }
}
