using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class FallWhenTouched : MonoBehaviour
{
    static readonly System.Collections.Generic.List<FallWhenTouched> ActivePlatforms = new System.Collections.Generic.List<FallWhenTouched>();
    static readonly System.Collections.Generic.List<Transform> RidingPlayers = new System.Collections.Generic.List<Transform>();
    static readonly System.Collections.Generic.Dictionary<Transform, float> RecentRideTimes = new System.Collections.Generic.Dictionary<Transform, float>();

    [Header("Falling")]
    [SerializeField] float startingFallSpeed = 0.15f;
    [SerializeField] float fallAcceleration = 2.5f;
    [SerializeField] float maxFallSpeed = 8f;
    [SerializeField] float stopFallingAtY = -4.55f;

    [Header("Reset")]
    [SerializeField] float veryFarResetDistance = 45f;
    [SerializeField] bool resetWhenPlayerVeryFar = false;
    [SerializeField] string playerTag = "Player";
    [SerializeField] bool fitColliderToRendererOnAwake = true;
    [SerializeField] bool debugLogs = true;

    Vector3 startPosition;
    Quaternion startRotation;
    Animator animator;
    Rigidbody2D rb;
    BoxCollider2D boxCollider;
    Collider2D[] platformColliders;
    bool[] platformColliderStartEnabled;
    readonly System.Collections.Generic.List<Collider2D> extraSolidColliders = new System.Collections.Generic.List<Collider2D>();
    readonly System.Collections.Generic.List<bool> extraSolidColliderStartEnabled = new System.Collections.Generic.List<bool>();
    readonly System.Collections.Generic.List<Vector3> extraSolidColliderStartPositions = new System.Collections.Generic.List<Vector3>();
    Transform player;
    Transform ridingPlayer;
    bool isFalling;
    bool hasFallen;
    float currentFallSpeed;
    RigidbodyType2D originalBodyType;
    RigidbodyConstraints2D originalConstraints;
    float originalGravityScale;
    float nextFallDebugLogAt;
    bool hadRigidbodyOnAwake;

    void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        hadRigidbodyOnAwake = rb != null;
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        originalBodyType = rb.bodyType;
        originalConstraints = rb.constraints;
        originalGravityScale = rb.gravityScale;

        boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider == null)
            boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.enabled = true;
        FitBoxColliderToRendererIfNeeded();
        CachePlatformColliders();
        Log("Ready. Collider size: " + boxCollider.size + ", offset: " + boxCollider.offset + ", isTrigger: " + boxCollider.isTrigger + ", Rigidbody2D: " + rb.bodyType + ", added Rigidbody2D: " + !hadRigidbodyOnAwake);
    }

    void OnEnable()
    {
        if (!ActivePlatforms.Contains(this))
            ActivePlatforms.Add(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        ActivePlatforms.Remove(this);
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (!isFalling)
            return;

        if (resetWhenPlayerVeryFar && IsPlayerVeryFarAway())
            ResetPlatform();
    }

    void FixedUpdate()
    {
        if (!isFalling)
            return;

        currentFallSpeed = Mathf.Min(maxFallSpeed, currentFallSpeed + fallAcceleration * Time.fixedDeltaTime);
        Vector3 beforeMove = transform.position;
        Vector3 afterMove = transform.position + Vector3.down * currentFallSpeed * Time.fixedDeltaTime;
        if (afterMove.y <= stopFallingAtY)
        {
            Vector3 stoppedPosition = afterMove;
            stoppedPosition.y = stopFallingAtY;
            MovePlatformAndExtraHitboxes(stoppedPosition - beforeMove);
            isFalling = false;
            currentFallSpeed = 0f;
            Log("Reached stop Y " + stopFallingAtY.ToString("0.00") + ". Waiting there until reset.");
            return;
        }

        MovePlatformAndExtraHitboxes(afterMove - beforeMove);

        if (debugLogs && Time.time >= nextFallDebugLogAt)
        {
            nextFallDebugLogAt = Time.time + 0.5f;
            Log("Falling. Y: " + transform.position.y.ToString("0.00") + ", speed: " + currentFallSpeed.ToString("0.00"));
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerContact(collision.collider);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        HandlePlayerContact(collision.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandlePlayerContact(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        HandlePlayerContact(other);
    }

    void HandlePlayerContact(Collider2D other)
    {
        if (!IsPlayerCollider(other))
            return;

        if (IsPlayerJumpingUp(other))
        {
            ReleaseRidingPlayerIfMatching(other);
            return;
        }

        player = other.transform;
        SetRidingPlayer(other);

        if (isFalling || hasFallen)
            return;

        if (animator != null)
            animator.enabled = false;

        CacheOverlappingSolidColliders();
        PrepareRigidbodyForScriptedFall();
        currentFallSpeed = Mathf.Max(0f, startingFallSpeed);
        isFalling = true;
        hasFallen = true;
        Log("Player touch detected from '" + other.name + "'. Falling started. Animator disabled: " + (animator != null));
    }

    bool IsPlayerCollider(Collider2D other)
    {
        if (other == null)
            return false;

        if (other.CompareTag(playerTag))
            return true;

        Transform root = other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform.root;
        bool isPlayer = root != null && root.CompareTag(playerTag);
        if (!isPlayer)
            Log("Touched by '" + other.name + "' but it is not tagged '" + playerTag + "'. Root: " + (root != null ? root.name : "none"));
        return isPlayer;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        ReleaseRidingPlayerIfMatching(collision.collider);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        ReleaseRidingPlayerIfMatching(other);
    }

    bool IsPlayerVeryFarAway()
    {
        if (player == null)
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag(playerTag);
            if (foundPlayer != null)
                player = foundPlayer.transform;
        }

        if (player == null)
            return false;

        return Vector2.Distance(startPosition, player.position) >= veryFarResetDistance;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.Equals(scene.name, "House", System.StringComparison.OrdinalIgnoreCase))
            ResetPlatform();
    }

    public void ResetPlatform()
    {
        isFalling = false;
        hasFallen = false;
        ClearRidingPlayer();
        currentFallSpeed = 0f;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = hadRigidbodyOnAwake ? originalBodyType : RigidbodyType2D.Kinematic;
            rb.constraints = originalConstraints;
            rb.gravityScale = originalGravityScale;
        }

        transform.SetPositionAndRotation(startPosition, startRotation);
        RestorePlatformColliders();

        if (animator != null)
        {
            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
        }

        Log("Platform reset to start position.");
    }

    void CachePlatformColliders()
    {
        platformColliders = GetComponentsInChildren<Collider2D>();
        platformColliderStartEnabled = new bool[platformColliders.Length];
        for (int i = 0; i < platformColliders.Length; i++)
            platformColliderStartEnabled[i] = platformColliders[i] != null && platformColliders[i].enabled;
    }

    void RestorePlatformColliders()
    {
        if (platformColliders == null || platformColliderStartEnabled == null)
            return;

        int count = Mathf.Min(platformColliders.Length, platformColliderStartEnabled.Length);
        for (int i = 0; i < count; i++)
        {
            if (platformColliders[i] != null)
                platformColliders[i].enabled = platformColliderStartEnabled[i];
        }

        int extraCount = Mathf.Min(extraSolidColliders.Count, extraSolidColliderStartEnabled.Count);
        for (int i = 0; i < extraCount; i++)
        {
            if (extraSolidColliders[i] != null)
                extraSolidColliders[i].enabled = extraSolidColliderStartEnabled[i];
        }

        int extraPositionCount = Mathf.Min(extraSolidColliders.Count, extraSolidColliderStartPositions.Count);
        for (int i = 0; i < extraPositionCount; i++)
        {
            if (extraSolidColliders[i] != null)
                extraSolidColliders[i].transform.position = extraSolidColliderStartPositions[i];
        }
    }

    void CacheOverlappingSolidColliders()
    {
        if (boxCollider == null)
            return;

        Bounds bounds = boxCollider.bounds;
        bounds.Expand(new Vector3(0.4f, 0.4f, 0f));
        Collider2D[] hits = Physics2D.OverlapBoxAll(bounds.center, bounds.size, 0f);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i];
            if (c == null || c.isTrigger || IsPlayerCollider(c) || IsCachedDirectPlatformCollider(c) || extraSolidColliders.Contains(c))
                continue;

            extraSolidColliders.Add(c);
            extraSolidColliderStartEnabled.Add(c.enabled);
            extraSolidColliderStartPositions.Add(c.transform.position);
            Log("Found extra overlapping solid hitbox to move with platform: '" + c.name + "'.");
        }
    }

    void MovePlatformAndExtraHitboxes(Vector3 delta)
    {
        transform.position += delta;

        for (int i = 0; i < extraSolidColliders.Count; i++)
        {
            Collider2D c = extraSolidColliders[i];
            if (c == null)
                continue;

            c.transform.position += delta;
        }

    }

    void SetRidingPlayer(Collider2D other)
    {
        if (other == null)
            return;

        Transform root = other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform.root;
        if (root == null)
            root = other.transform;

        bool isNewRider = ridingPlayer != root;
        ridingPlayer = root;
        if (!RidingPlayers.Contains(ridingPlayer))
            RidingPlayers.Add(ridingPlayer);
        RecentRideTimes[ridingPlayer] = Time.time;
        if (isNewRider)
            Log("Player is riding the falling platform: '" + ridingPlayer.name + "'.");
    }

    void ReleaseRidingPlayerIfMatching(Collider2D other)
    {
        if (other == null || ridingPlayer == null)
            return;

        Transform root = other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform.root;
        if (root == null)
            root = other.transform;

        if (root == ridingPlayer)
        {
            RecentRideTimes[ridingPlayer] = Time.time;
            ClearRidingPlayer();
            Log("Player left the falling platform.");
        }
    }

    void ClearRidingPlayer()
    {
        if (ridingPlayer != null)
            RidingPlayers.Remove(ridingPlayer);
        ridingPlayer = null;
    }

    bool IsPlayerJumpingUp(Collider2D other)
    {
        if (other == null || other.attachedRigidbody == null)
            return false;

        return other.attachedRigidbody.linearVelocity.y > 0.05f;
    }

    public static bool IsRidingFallingPlatform(Transform candidate)
    {
        if (candidate == null)
            return false;

        for (int i = RidingPlayers.Count - 1; i >= 0; i--)
        {
            if (RidingPlayers[i] == null)
            {
                RidingPlayers.RemoveAt(i);
                continue;
            }

            if (RidingPlayers[i] == candidate)
                return true;
        }

        return false;
    }

    public static bool WasRecentlyRidingFallingPlatform(Transform candidate, float graceSeconds)
    {
        if (candidate == null)
            return false;

        if (IsRidingFallingPlatform(candidate))
            return true;

        return RecentRideTimes.TryGetValue(candidate, out float lastRideTime) &&
               Time.time - lastRideTime <= Mathf.Max(0f, graceSeconds);
    }

    public static void NotifyPlayerJumpedFromFallingPlatform(Transform candidate)
    {
        if (candidate == null)
            return;

        RidingPlayers.Remove(candidate);
        RecentRideTimes.Remove(candidate);
    }

    bool IsCachedDirectPlatformCollider(Collider2D colliderToCheck)
    {
        if (platformColliders == null)
            return false;

        for (int i = 0; i < platformColliders.Length; i++)
        {
            if (platformColliders[i] == colliderToCheck)
                return true;
        }

        return false;
    }

    void FitBoxColliderToRendererIfNeeded()
    {
        if (!fitColliderToRendererOnAwake || boxCollider == null)
            return;

        if (boxCollider.size != Vector2.one || boxCollider.offset != Vector2.zero)
            return;

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        if (renderers == null || renderers.Length == 0)
            return;

        Bounds worldBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            worldBounds.Encapsulate(renderers[i].bounds);

        Vector3 localMin = transform.InverseTransformPoint(worldBounds.min);
        Vector3 localMax = transform.InverseTransformPoint(worldBounds.max);
        Vector2 size = new Vector2(Mathf.Abs(localMax.x - localMin.x), Mathf.Abs(localMax.y - localMin.y));

        if (size.x <= 0.01f || size.y <= 0.01f)
            return;

        boxCollider.offset = (Vector2)((localMin + localMax) * 0.5f);
        boxCollider.size = size;
        Log("Auto-fitted BoxCollider2D to renderer. New size: " + boxCollider.size + ", offset: " + boxCollider.offset);
    }

    void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[FallWhenTouched] " + name + ": " + message, this);
    }

    void PrepareRigidbodyForScriptedFall()
    {
        if (rb == null)
            return;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public static void ResetAllPlatforms()
    {
        for (int i = ActivePlatforms.Count - 1; i >= 0; i--)
        {
            if (ActivePlatforms[i] != null)
                ActivePlatforms[i].ResetPlatform();
        }
    }
}
