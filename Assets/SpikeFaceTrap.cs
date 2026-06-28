using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class SpikeFaceTrap : MonoBehaviour
{
    enum TrapState
    {
        Idle,
        Falling,
        PlayingHit,
        Returning
    }

    [Header("Detection")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Vector2 detectionOffset = new Vector2(0f, -2.25f);
    [SerializeField] private Vector2 detectionSize = new Vector2(2f, 7f);

    [Header("Attack")]
    [SerializeField] private int damageHearts = 1;
    [HideInInspector]
    [SerializeField] private BoxCollider2D damageHitbox;
    [SerializeField] private BoxCollider2D[] damageHitboxes = new BoxCollider2D[4];
    [SerializeField] private Vector2 damageHitboxOffset = new Vector2(0f, -0.45f);
    [SerializeField] private Vector2 damageHitboxSize = new Vector2(1f, 0.35f);
    [SerializeField] private float fallSpeed = 8f;
    [SerializeField] private float maxFallDistance = 6f;
    [SerializeField] private Vector2 floorImpactCheckSize = new Vector2(1f, 0.12f);
    [SerializeField] private float floorImpactCheckOffsetY = 0.18f;
    [SerializeField] private float postHitDelay = 0.1f;
    [SerializeField] private float returnSpeed = 10f;

    [Header("Animation")]
    [SerializeField] private string hitStateName = "Hit";
    [SerializeField] private AnimationClip hitAnimation;
    [SerializeField] private Sprite[] hitSprites = new Sprite[0];
    [SerializeField] private float hitSpriteFrameSeconds = 0.12f;
    [SerializeField] private float hitFinalFrameHoldSeconds = 0.15f;
    [SerializeField] private bool debugLogs = true;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D hitCollider;
    private Rigidbody2D rb;
    private SpikeFaceDelayedLoop delayedLoop;
    private AnimationClip resolvedHitAnimation;
    private Sprite idleSprite;
    private Vector3 startPosition;
    private RigidbodyType2D originalBodyType;
    private RigidbodyConstraints2D originalConstraints;
    private float originalGravityScale;
    private TrapState state = TrapState.Idle;
    private bool hasDamagedThisDrop;
    private bool hasImpactedThisDrop;
    private Coroutine resetRoutine;
    private Coroutine hitAnimationRoutine;

    void Awake()
    {
        startPosition = transform.position;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            idleSprite = spriteRenderer.sprite;

        hitCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            originalBodyType = rb.bodyType;
            originalConstraints = rb.constraints;
            originalGravityScale = rb.gravityScale;
            PrepareControlledRigidbody();
        }

        delayedLoop = GetComponent<SpikeFaceDelayedLoop>();

        EnsureDamageHitboxes();
        resolvedHitAnimation = ResolveHitAnimationClip();
    }

    void Update()
    {
        switch (state)
        {
            case TrapState.Idle:
                CheckForPlayerUnderneath();
                break;
            case TrapState.Falling:
                Fall();
                break;
            case TrapState.Returning:
                ReturnToStart();
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other, false);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other, false);
    }

    void CheckForPlayerUnderneath()
    {
        if (IsPlayerPositionUnderspikeface())
        {
            StartAttack();
            return;
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(GetDetectionCenter(), detectionSize, 0f);
        for (int i = 0; i < hits.Length; i++)
        {
            if (GetPlayerHealth(hits[i]) == null)
                continue;

            StartAttack();
            return;
        }
    }

    void StartAttack()
    {
        if (state != TrapState.Idle)
            return;

        SetState(TrapState.Falling);
        hasDamagedThisDrop = false;
        hasImpactedThisDrop = false;

        if (delayedLoop != null)
            delayedLoop.enabled = false;
        if (animator != null)
        {
            animator.speed = 0f;
            animator.enabled = false;
        }
        ForceIdleSprite();

        if (hitAnimationRoutine != null)
            StopCoroutine(hitAnimationRoutine);

        Log("fall started");
    }

    void Fall()
    {
        PrepareControlledRigidbody();
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        if (HasHitFloor())
        {
            StopAndPlayHitThenReturn();
            return;
        }

        if (startPosition.y - transform.position.y >= maxFallDistance)
        {
            StopAndPlayHitThenReturn();
        }
    }

    void ReturnToStart()
    {
        if (state != TrapState.Returning)
            return;

        PrepareControlledRigidbody();
        transform.position = Vector3.MoveTowards(transform.position, startPosition, returnSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, startPosition) <= 0.01f)
        {
            transform.position = startPosition;
            hasDamagedThisDrop = false;
            RestartDelayedIdle();
            SetState(TrapState.Idle);
        }
    }

    void TryDamagePlayer(Collider2D other, bool ignoreTrapState)
    {
        if (!ignoreTrapState && (state != TrapState.Falling || hasDamagedThisDrop))
            return;

        PlayerHealth health = GetPlayerHealth(other);
        if (health == null)
            return;

        if (!ignoreTrapState)
            hasDamagedThisDrop = true;

        PrepareControlledRigidbody();
        health.TakeDamage(damageHearts, transform.position);
    }

    void StopAndPlayHitThenReturn()
    {
        if (state != TrapState.Falling || hasImpactedThisDrop)
            return;

        hasImpactedThisDrop = true;
        SetState(TrapState.PlayingHit);
        Log("impact detected");
        if (resetRoutine != null)
            StopCoroutine(resetRoutine);
        resetRoutine = StartCoroutine(HitThenReturnRoutine());
    }

    IEnumerator HitThenReturnRoutine()
    {
        yield return PlayHitAnimationOnce();
        if (postHitDelay > 0f)
            yield return new WaitForSecondsRealtime(postHitDelay);

        ResetToIdlePoseForReturn();
        FreezeAnimatorUntilReturnCompletes();
        SetState(TrapState.Returning);
        resetRoutine = null;
    }

    IEnumerator PlayHitAnimationOnce()
    {
        if (spriteRenderer != null && hitSprites != null && hitSprites.Length > 0)
        {
            if (delayedLoop != null)
                delayedLoop.enabled = false;
            if (animator != null)
                animator.enabled = false;

            Log("playing hit sprites: " + hitSprites.Length);
            yield return PlayHitSpritesOnce();
            Log("hit animation finished");
            yield break;
        }

        AnimationClip clip = ResolveHitAnimationClip();
        if (clip != null)
        {
            if (delayedLoop != null)
                delayedLoop.enabled = false;
            if (animator != null)
                animator.enabled = false;

            Log("hit clip found: " + clip.name);
            yield return PlayHitClipOnce(clip);
            Log("hit animation finished");
            yield break;
        }

        Log("hit clip missing; using Animator fallback");
        if (animator != null)
        {
            if (delayedLoop != null)
                delayedLoop.enabled = false;

            animator.enabled = true;
            animator.speed = 1f;

            if (HasAnimatorTrigger(hitStateName))
                animator.ResetTrigger(hitStateName);

            animator.Play(hitStateName, 0, 0f);
            animator.Update(0f);

            if (HasAnimatorTrigger(hitStateName))
                animator.ResetTrigger(hitStateName);

            float fallbackEndTime = Time.time + 1f;
            while (Time.time < fallbackEndTime)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                if (state.length > 0.01f && state.IsName(hitStateName) && state.normalizedTime >= 1f && !animator.IsInTransition(0))
                    yield break;

                if (state.length > 0.01f && !state.IsName(hitStateName))
                    fallbackEndTime = Mathf.Min(fallbackEndTime, Time.time + state.length);

                yield return null;
            }

            yield break;
        }
    }

    void FreezeAnimatorUntilReturnCompletes()
    {
        if (animator != null)
        {
            animator.speed = 0f;
            animator.enabled = false;
        }
        ForceIdleSprite();
    }

    void ResetToIdlePoseForReturn()
    {
        if (animator == null)
        {
            ForceIdleSprite();
            return;
        }

        if (delayedLoop != null)
            delayedLoop.enabled = false;

        animator.enabled = true;
        animator.speed = 1f;
        if (HasAnimatorTrigger(hitStateName))
            animator.ResetTrigger(hitStateName);

        animator.Rebind();
        animator.Update(0f);
        animator.Play(0, 0, 0f);
        animator.Update(0f);
        ForceIdleSprite();
    }

    IEnumerator PlayHitClipOnce(AnimationClip clip)
    {
        float length = Mathf.Max(0.01f, clip.length);
        float elapsed = 0f;
        while (elapsed < length)
        {
            clip.SampleAnimation(gameObject, elapsed);
            elapsed += Time.deltaTime;
            yield return null;
        }

        clip.SampleAnimation(gameObject, length);
        hitAnimationRoutine = null;
    }

    IEnumerator PlayHitSpritesOnce()
    {
        float frameSeconds = Mathf.Max(0.01f, hitSpriteFrameSeconds);
        for (int i = 0; i < hitSprites.Length; i++)
        {
            if (hitSprites[i] != null)
                spriteRenderer.sprite = hitSprites[i];

            Log("hit frame " + (i + 1) + "/" + hitSprites.Length);
            yield return new WaitForSecondsRealtime(frameSeconds);
        }

        if (hitFinalFrameHoldSeconds > 0f)
            yield return new WaitForSecondsRealtime(hitFinalFrameHoldSeconds);

        hitAnimationRoutine = null;
    }

    AnimationClip ResolveHitAnimationClip()
    {
        if (hitAnimation != null)
            return hitAnimation;

        if (resolvedHitAnimation != null)
            return resolvedHitAnimation;

        RuntimeAnimatorController controller = animator != null ? animator.runtimeAnimatorController : null;
        if (controller == null)
            return null;

        AnimationClip[] clips = controller.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            if (clip != null && string.Equals(clip.name, hitStateName, System.StringComparison.OrdinalIgnoreCase))
            {
                resolvedHitAnimation = clip;
                return resolvedHitAnimation;
            }
        }

        return null;
    }

    void ForceIdleSprite()
    {
        if (spriteRenderer != null && idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }

    void Log(string message)
    {
        if (debugLogs)
            Debug.Log("[SpikeFaceTrap] " + name + ": " + message, this);
    }

    void SetState(TrapState nextState)
    {
        if (state == nextState)
            return;

        state = nextState;
        Log("state = " + state);
    }

    bool HasAnimatorTrigger(string parameterName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(parameterName))
            return false;

        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == AnimatorControllerParameterType.Trigger && parameters[i].name == parameterName)
                return true;
        }

        return false;
    }

    void RestartDelayedIdle()
    {
        if (animator != null)
        {
            animator.enabled = true;
            animator.speed = 1f;
            if (HasAnimatorTrigger(hitStateName))
                animator.ResetTrigger(hitStateName);

            animator.Rebind();
            animator.Play(0, 0, 0f);
            animator.Update(0f);
            ForceIdleSprite();
        }

        if (delayedLoop != null)
        {
            delayedLoop.enabled = false;
            delayedLoop.enabled = true;
            delayedLoop.RestartLoopFromIdle();
        }

        Log("returned to top; delayed blinking restarted");
    }

    PlayerHealth GetPlayerHealth(Collider2D other)
    {
        if (other == null)
            return null;

        bool tagMatches =
            string.IsNullOrWhiteSpace(playerTag) ||
            other.CompareTag(playerTag) ||
            (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(playerTag));

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
            return health;

        if (other.attachedRigidbody != null)
        {
            health = other.attachedRigidbody.GetComponent<PlayerHealth>();
            if (health != null)
                return health;
        }

        health = other.GetComponentInParent<PlayerHealth>();
        return tagMatches || health != null ? health : null;
    }

    bool IsPlayerPositionUnderspikeface()
    {
        Bounds detection = GetDetectionBounds();
        PlayerHealth[] players = FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        for (int i = 0; i < players.Length; i++)
        {
            PlayerHealth health = players[i];
            if (health == null || !health.gameObject.activeInHierarchy)
                continue;

            if (!string.IsNullOrWhiteSpace(playerTag) && !health.CompareTag(playerTag))
                continue;

            Collider2D playerCollider = health.GetComponent<Collider2D>();
            Bounds playerBounds = playerCollider != null
                ? playerCollider.bounds
                : new Bounds(health.transform.position, new Vector3(0.5f, 1f, 0.1f));

            if (detection.Intersects(playerBounds) && playerBounds.center.y < transform.position.y)
                return true;
        }

        return false;
    }

    void EnsureDamageHitboxes()
    {
        if (damageHitboxes == null || damageHitboxes.Length != 4)
        {
            BoxCollider2D[] previous = damageHitboxes;
            damageHitboxes = new BoxCollider2D[4];
            if (previous != null)
                for (int i = 0; i < Mathf.Min(previous.Length, damageHitboxes.Length); i++)
                    damageHitboxes[i] = previous[i];
        }

        if (damageHitbox != null && damageHitboxes[0] == null)
            damageHitboxes[0] = damageHitbox;

        for (int i = 0; i < damageHitboxes.Length; i++)
        {
            damageHitboxes[i] = EnsureSingleDamageHitbox(i);
            ConfigureDamageHitbox(damageHitboxes[i]);
        }

        damageHitbox = damageHitboxes.Length > 0 ? damageHitboxes[0] : null;
    }

    BoxCollider2D EnsureSingleDamageHitbox(int index)
    {
        BoxCollider2D hitbox = damageHitboxes[index];
        if (hitbox != null)
            return hitbox;

        string hitboxName = index == 0 ? "SpikeFaceDamageHitbox" : "SpikeFaceDamageHitbox" + (index + 1);
        Transform existing = transform.Find(hitboxName);
        GameObject hitboxObject = existing != null ? existing.gameObject : new GameObject(hitboxName);
        hitboxObject.transform.SetParent(transform, false);
        if (existing == null)
        {
            hitboxObject.transform.localPosition = Vector3.zero;
            hitboxObject.transform.localRotation = Quaternion.identity;
            hitboxObject.transform.localScale = Vector3.one;
        }

        hitbox = hitboxObject.GetComponent<BoxCollider2D>();
        if (hitbox == null)
        {
            hitbox = hitboxObject.AddComponent<BoxCollider2D>();
            hitbox.offset = GetDefaultDamageHitboxOffset(index);
            hitbox.size = GetDefaultDamageHitboxSize(index);
        }

        return hitbox;
    }

    Vector2 GetDefaultDamageHitboxOffset(int index)
    {
        switch (index)
        {
            case 0: return new Vector2(-0.29f, 0f);
            case 1: return new Vector2(0f, -0.29f);
            case 2: return new Vector2(0.29f, 0f);
            case 3: return new Vector2(0f, 0.29f);
            default: return damageHitboxOffset;
        }
    }

    Vector2 GetDefaultDamageHitboxSize(int index)
    {
        switch (index)
        {
            case 0:
            case 2:
                return new Vector2(0.1f, 0.48f);
            case 1:
            case 3:
                return new Vector2(0.48f, 0.1f);
            default:
                return damageHitboxSize;
        }
    }

    void ConfigureDamageHitbox(BoxCollider2D hitbox)
    {
        if (hitbox == null)
            return;

        hitbox.isTrigger = true;

        SpikeFaceTrapHitbox relay = hitbox.GetComponent<SpikeFaceTrapHitbox>();
        if (relay == null)
            relay = hitbox.gameObject.AddComponent<SpikeFaceTrapHitbox>();
        relay.SetOwner(this);
    }

    internal void HandleRelayTrigger(Collider2D other)
    {
        TryDamagePlayer(other, true);
    }

    internal void HandleRelayCollision(Collision2D collision)
    {
        if (collision == null)
            return;

        TryDamagePlayer(collision.collider, true);
    }

    Vector2 GetDetectionCenter()
    {
        return (Vector2)transform.position + detectionOffset;
    }

    Bounds GetDetectionBounds()
    {
        Vector2 center = GetDetectionCenter();
        return new Bounds(new Vector3(center.x, center.y, transform.position.z), new Vector3(detectionSize.x, detectionSize.y, 0.1f));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Application.isPlaying ? GetDetectionCenter() : (Vector2)transform.position + detectionOffset, detectionSize);

        Gizmos.color = Color.yellow;
        if (damageHitboxes != null)
        {
            for (int i = 0; i < damageHitboxes.Length; i++)
            {
                BoxCollider2D hitbox = damageHitboxes[i];
                if (hitbox == null)
                    continue;

                Gizmos.DrawWireCube(hitbox.bounds.center, hitbox.bounds.size);
            }
        }

        if ((damageHitboxes == null || damageHitboxes.Length == 0) && damageHitbox == null)
            Gizmos.DrawWireCube(transform.TransformPoint(damageHitboxOffset), new Vector3(damageHitboxSize.x, damageHitboxSize.y, 0.1f));

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(GetFloorImpactCheckCenter(), floorImpactCheckSize);
    }

    void PrepareControlledRigidbody()
    {
        if (rb == null)
            return;

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void OnDisable()
    {
        if (rb == null)
            return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = originalBodyType;
        rb.constraints = originalConstraints;
        rb.gravityScale = originalGravityScale;
    }

    bool HasHitFloor()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(GetFloorImpactCheckCenter(), floorImpactCheckSize, 0f);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i];
            if (c == null || c.isTrigger || c.transform.IsChildOf(transform) || GetPlayerHealth(c) != null)
                continue;

            return true;
        }

        return false;
    }

    Vector2 GetFloorImpactCheckCenter()
    {
        Bounds bounds = hitCollider != null
            ? hitCollider.bounds
            : new Bounds(transform.position, new Vector3(1f, 1f, 0.1f));

        return new Vector2(bounds.center.x, bounds.min.y + floorImpactCheckOffsetY);
    }
}

public static class SpikeFaceTrapInstaller
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallOnStartup()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryInstall(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryInstall(scene);
    }

    static void TryInstall(Scene scene)
    {
        if (!scene.IsValid())
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
            InstallInChildren(roots[i].transform);
    }

    static void InstallInChildren(Transform root)
    {
        if (root == null)
            return;

        Animator animator = root.GetComponent<Animator>();
        if (animator != null && IsSpikeFace(root.gameObject, animator) && root.GetComponent<SpikeFaceTrap>() == null)
            root.gameObject.AddComponent<SpikeFaceTrap>();

        for (int i = 0; i < root.childCount; i++)
            InstallInChildren(root.GetChild(i));
    }

    static bool IsSpikeFace(GameObject candidate, Animator animator)
    {
        string objectName = candidate.name.ToLowerInvariant().Replace(" ", "");
        if (objectName.Contains("spikeface") || objectName.Contains("spikeface"))
            return true;

        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null)
            return false;

        string controllerName = controller.name.ToLowerInvariant().Replace(" ", "");
        return controllerName.Contains("spikeface") || controllerName.Contains("spikeface");
    }
}
