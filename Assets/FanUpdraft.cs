using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class FanUpdraft : MonoBehaviour
{
    private struct FanContact
    {
        public float lastSeenTime;
        public bool reachedTop;
    }

    private static readonly Dictionary<int, FanContact> ActiveContacts = new Dictionary<int, FanContact>();
    private const float ContactGraceSeconds = 0.4f;

    [Header("Detection")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Vector2 detectionOffset = new Vector2(0f, 3.5f);
    [SerializeField] private Vector2 detectionSize = new Vector2(2.5f, 7f);

    [Header("Lift")]
    [SerializeField] private float liftHeight = 6.5f;
    [SerializeField] private float liftVelocity = 15f;
    [SerializeField] private float topTolerance = 0.2f;

    [Header("Top Float")]
    [SerializeField] private float bobAmplitude = 0.35f;
    [SerializeField] private float bobSpeed = 2.5f;
    [SerializeField] private float bobFollowStrength = 5f;
    [SerializeField] private float bobMaxVelocity = 3f;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;

    void FixedUpdate()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(GetDetectionCenter(), detectionSize, 0f);
        HashSet<PlayerHealth> handledPlayers = new HashSet<PlayerHealth>();

        for (int i = 0; i < hits.Length; i++)
        {
            PlayerHealth player = GetPlayerHealth(hits[i]);
            if (player == null || handledPlayers.Contains(player))
                continue;

            handledPlayers.Add(player);
            ApplyUpdraft(player);
        }
    }

    void LateUpdate()
    {
        if (ActiveContacts.Count == 0)
            return;

        s_RemoveBuffer.Clear();
        foreach (KeyValuePair<int, FanContact> pair in ActiveContacts)
        {
            if (Time.time - pair.Value.lastSeenTime > ContactGraceSeconds)
                s_RemoveBuffer.Add(pair.Key);
        }

        for (int i = 0; i < s_RemoveBuffer.Count; i++)
            ActiveContacts.Remove(s_RemoveBuffer[i]);
    }

    private static readonly List<int> s_RemoveBuffer = new List<int>();

    void ApplyUpdraft(PlayerHealth player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        float topY = transform.position.y + liftHeight;
        bool isAtTopNow = rb.position.y >= topY - topTolerance;
        bool reachedTop = HasReachedTopThisRide(player.transform) || isAtTopNow;
        RecordContact(player.transform, reachedTop);

        Vector2 velocity = rb.linearVelocity;
        if (!reachedTop)
        {
            velocity.y = Mathf.Max(velocity.y, liftVelocity);
        }
        else if (velocity.y <= bobMaxVelocity)
        {
            float targetY = topY + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            velocity.y = Mathf.Clamp((targetY - rb.position.y) * bobFollowStrength, -bobMaxVelocity, bobMaxVelocity);
        }

        rb.linearVelocity = velocity;
    }

    void RecordContact(Transform player, bool reachedTop)
    {
        if (player == null)
            return;

        ActiveContacts[player.GetInstanceID()] = new FanContact
        {
            lastSeenTime = Time.time,
            reachedTop = reachedTop
        };
    }

    bool HasReachedTopThisRide(Transform player)
    {
        return TryGetContact(player, out FanContact contact) && contact.reachedTop;
    }

    PlayerHealth GetPlayerHealth(Collider2D other)
    {
        if (other == null)
            return null;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health == null && other.attachedRigidbody != null)
            health = other.attachedRigidbody.GetComponent<PlayerHealth>();
        if (health == null)
            health = other.GetComponentInParent<PlayerHealth>();
        if (health == null)
            return null;

        bool tagMatches =
            string.IsNullOrWhiteSpace(playerTag) ||
            health.CompareTag(playerTag) ||
            other.CompareTag(playerTag) ||
            (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(playerTag));

        return tagMatches ? health : null;
    }

    Vector2 GetDetectionCenter()
    {
        return (Vector2)transform.position + detectionOffset;
    }

    public static bool IsJumpBlockedUntilLiftTop(Transform player)
    {
        return TryGetContact(player, out FanContact contact) && !contact.reachedTop;
    }

    public static bool IsPlayerAtFanTop(Transform player)
    {
        return TryGetContact(player, out FanContact contact) && contact.reachedTop;
    }

    static bool TryGetContact(Transform player, out FanContact contact)
    {
        contact = default;
        if (player == null)
            return false;

        if (!ActiveContacts.TryGetValue(player.GetInstanceID(), out contact))
            return false;

        return Time.time - contact.lastSeenTime <= ContactGraceSeconds;
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Application.isPlaying ? GetDetectionCenter() : (Vector2)transform.position + detectionOffset, detectionSize);

        Gizmos.color = Color.green;
        Vector3 topCenter = new Vector3(transform.position.x, transform.position.y + liftHeight, transform.position.z);
        Gizmos.DrawWireCube(topCenter, new Vector3(detectionSize.x, 0.12f, 0.1f));
    }
}
