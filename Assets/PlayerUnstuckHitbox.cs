using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerUnstuckHitbox : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private float inputThreshold = 0.2f;
    [SerializeField] private float minMovementPerFixedStep = 0.0004f;
    [SerializeField] private float stuckTime = 0.1f;
    [SerializeField] private float unstuckCooldown = 0.05f;

    [Header("Unstuck Push")]
    [SerializeField] private float horizontalNudge = 0.04f;
    [SerializeField] private float verticalNudge = 0.01f;
    [SerializeField] private float pushSpeed = 1.2f;
    [SerializeField] private float maxPushesPerSecond = 10f;
    [SerializeField] private bool requireBlockingSideContact = true;
    [SerializeField] private float sideNormalThreshold = 0.35f;
    [SerializeField] private float contactEscapeBias = 0.03f;

    [Header("Physics")]
    [SerializeField] private bool forceZeroFriction = true;
    [SerializeField] private bool setContinuousCollision = true;
    [SerializeField] private bool setInterpolation = true;
    [SerializeField] private bool smoothBoxColliderEdges = true;
    [SerializeField] private float boxColliderEdgeRadius = 0.01f;
    [SerializeField] private bool centerBoxColliderOnX = true;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 lastPosition;
    private float stuckTimer;
    private float cooldownTimer;
    private float pushTimer;
    private PhysicsMaterial2D runtimeNoFriction;
    private readonly ContactPoint2D[] contactBuffer = new ContactPoint2D[16];

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
        if (smoothBoxColliderEdges)
            SmoothColliderShape();
    }

    private void OnEnable()
    {
        if (rb != null) lastPosition = rb.position;
        stuckTimer = 0f;
        cooldownTimer = 0f;
        pushTimer = 0f;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        float inputX = Input.GetAxisRaw("Horizontal");
        bool tryingToMove = Mathf.Abs(inputX) >= inputThreshold;
        float moved = Vector2.Distance(rb.position, lastPosition);
        int contactCount = rb.GetContacts(contactBuffer);
        bool hasAnyContact = contactCount > 0;

        if (tryingToMove && hasAnyContact && moved <= minMovementPerFixedStep)
            stuckTimer += Time.fixedDeltaTime;
        else
            stuckTimer = 0f;

        cooldownTimer -= Time.fixedDeltaTime;
        pushTimer += Time.fixedDeltaTime;

        float minPushInterval = 1f / Mathf.Max(1f, maxPushesPerSecond);
        float dir = inputX == 0f ? 0f : Mathf.Sign(inputX);
        bool hasBlockingSide = HasBlockingSideContact(dir, contactCount);
        bool canUnstuck = hasAnyContact && (!requireBlockingSideContact || hasBlockingSide || tryingToMove);

        if (stuckTimer >= stuckTime && cooldownTimer <= 0f && pushTimer >= minPushInterval && canUnstuck)
        {
            AttemptUnstuck(dir);
            stuckTimer = 0f;
            cooldownTimer = unstuckCooldown;
            pushTimer = 0f;
        }

        lastPosition = rb.position;
    }

    private void AttemptUnstuck(float direction)
    {
        Vector2 nudge = BuildEscapeNudge(direction);
        rb.MovePosition(rb.position + nudge);

        Vector2 v = rb.linearVelocity;
        if (direction != 0f)
            v.x = direction * Mathf.Max(Mathf.Abs(v.x), pushSpeed);
        rb.linearVelocity = v;
    }

    private Vector2 BuildEscapeNudge(float direction)
    {
        Vector2 escapeFromContacts = Vector2.zero;
        int count = rb.GetContacts(contactBuffer);
        for (int i = 0; i < count; i++)
            escapeFromContacts += contactBuffer[i].normal;

        if (escapeFromContacts.sqrMagnitude > 0.0001f)
            escapeFromContacts.Normalize();

        Vector2 directionalBias = new Vector2(direction * horizontalNudge, verticalNudge);
        Vector2 contactBias = new Vector2(escapeFromContacts.x * contactEscapeBias, Mathf.Abs(escapeFromContacts.y) * verticalNudge);
        Vector2 result = directionalBias + contactBias;

        if (result.y < verticalNudge)
            result.y = verticalNudge;

        if (direction > 0f)
            result.x = Mathf.Max(result.x, horizontalNudge * 0.5f);
        else if (direction < 0f)
            result.x = Mathf.Min(result.x, -horizontalNudge * 0.5f);

        return result;
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

    private void SmoothColliderShape()
    {
        BoxCollider2D box = col as BoxCollider2D;
        if (box == null)
            return;

        if (centerBoxColliderOnX)
        {
            Vector2 offset = box.offset;
            offset.x = 0f;
            box.offset = offset;
        }

        box.edgeRadius = Mathf.Max(0f, boxColliderEdgeRadius);
    }

    private bool HasBlockingSideContact(float direction, int count)
    {
        if (direction == 0f || rb == null) return false;

        for (int i = 0; i < count; i++)
        {
            Vector2 n = contactBuffer[i].normal;
            if (Mathf.Abs(n.x) < sideNormalThreshold)
                continue;

            if (direction > 0f && n.x < -sideNormalThreshold)
                return true;
            if (direction < 0f && n.x > sideNormalThreshold)
                return true;
        }

        return false;
    }

    private void OnDestroy()
    {
        if (runtimeNoFriction != null)
            Destroy(runtimeNoFriction);
    }
}
