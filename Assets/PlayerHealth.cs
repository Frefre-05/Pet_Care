using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class PlayerHealth : MonoBehaviour
{
    [Header("Lives")]
    public int maxHearts = 3;
    public int currentHearts;

    [Header("Respawn Settings")]
    public float respawnDelay = 0.2f;
    public bool freezeVelocityOnRespawn = true;
    [SerializeField] private float fullDeathFallDelay = 0.65f;
    [SerializeField] private float fullDeathJumpVelocity = 6f;
    [SerializeField] private float fullDeathJumpHoldDuration = 0.18f;
    [SerializeField] private float fullDeathFallVelocity = -7f;

    [Header("Hit Feedback")]
    [SerializeField] private string hitTriggerName = "Pet1_Hit";
    [SerializeField] private float damageImmunitySeconds = 0.7f;
    [SerializeField] private Vector2 damageKnockbackVelocity = new Vector2(-4f, 2f);
    [SerializeField] private float damageKnockbackControlLock = 0.2f;

    [SerializeField] private HealthManager hud; // Optional, assign if you have heart UI

    private Vector3 respawnPoint;
    private Vector3 initialRespawnPoint;
    private Animator animator;
    private bool isDead;
    private bool isRespawning;
    private float immuneUntilTime;
    private Vector3 lastDamageSourcePosition;
    private bool hasDamageSourcePosition;
    private Coroutine knockbackRoutine;
    private Coroutine respawnRoutine;
    private Coroutine fullDeathRoutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (hud == null)
            hud = FindFirstObjectByType<HealthManager>();
        currentHearts = maxHearts;
        if (hud != null)
            hud.SetHearts(currentHearts, maxHearts);
        respawnPoint = transform.position;
        initialRespawnPoint = respawnPoint;
    }

    public void SetCheckpoint(Vector3 pos)
    {
        respawnPoint = pos;
    }

    public void TakeDamageAndRespawn()
    {
        ApplyDamage(1, true, false);
    }

    public void TakeDamage(int amount)
    {
        ApplyDamage(amount, false, true);
    }

    public void TakeDamage(int amount, Vector3 damageSourcePosition)
    {
        lastDamageSourcePosition = damageSourcePosition;
        hasDamageSourcePosition = true;
        ApplyDamage(amount, false, true);
    }

    private void ApplyDamage(int amount, bool respawnIfAlive, bool playHitFeedback)
    {
        if (isDead || isRespawning)
            return;

        if (IsDamageImmune())
            return;

        int damage = Mathf.Max(1, amount);
        currentHearts = Mathf.Max(0, currentHearts - damage);
        immuneUntilTime = Time.time + Mathf.Max(0f, damageImmunitySeconds);
        if (playHitFeedback)
            PlayHitFeedback();

        if (hud != null) hud.SetHearts(currentHearts, maxHearts);

        if (currentHearts <= 0)
        {
            if (IsTutorialScene())
            {
                StartCoroutine(TutorialRestartRoutine());
                return;
            }

            StartFullDeathRestart();
            return;
        }

        if (respawnIfAlive)
            StartVoidRespawn();
        else
            ApplyDamageKnockback();

        hasDamageSourcePosition = false;
    }

    public void HealToFull()
    {
        currentHearts = maxHearts;
        if (hud != null) hud.SetHearts(currentHearts, maxHearts);
    }

    private bool IsTutorialScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        return sceneName.Trim().ToLowerInvariant().Replace(" ", string.Empty).Contains("tutorial");
    }

    private IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        var rb = GetComponent<Rigidbody2D>();
        var cam = Camera.main != null ? Camera.main.GetComponent<CameraFollowing2>() : null;
        if (freezeVelocityOnRespawn && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(respawnDelay);
        FallWhenTouched.ResetAllPlatforms();
        transform.position = respawnPoint;
        if (cam != null) cam.SnapToTarget();
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.RestoreGameplayIfPanelsClosed();

        isRespawning = false;
        respawnRoutine = null;
    }

    private void StartVoidRespawn()
    {
        if (isDead || respawnRoutine != null)
            return;

        respawnRoutine = StartCoroutine(RespawnRoutine());
    }

    private void StartFullDeathRestart()
    {
        if (fullDeathRoutine != null)
            return;

        if (respawnRoutine != null)
        {
            StopCoroutine(respawnRoutine);
            respawnRoutine = null;
            isRespawning = false;
        }

        fullDeathRoutine = StartCoroutine(FullDeathRestartRoutine());
    }

    private IEnumerator FullDeathRestartRoutine()
    {
        isDead = true;

        var rb = GetComponent<Rigidbody2D>();
        DisablePlayerControlForDeathFall();
        DisableSolidCollidersForDeathFall();

        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = new Vector2(0f, fullDeathJumpVelocity);
            rb.angularVelocity = 0f;
        }

        LevelDeathPenalty.TryApply();
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, fullDeathJumpHoldDuration));

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, fullDeathFallVelocity);

        yield return new WaitForSecondsRealtime(Mathf.Max(0f, fullDeathFallDelay));

        SceneTransitionLoader.LoadScene(SceneManager.GetActiveScene().buildIndex);

        while (SceneTransitionLoader.IsTransitioning())
            yield return null;

        fullDeathRoutine = null;
    }

    private void DisablePlayerControlForDeathFall()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        JumpTwice jump = GetComponent<JumpTwice>();
        if (jump != null)
            jump.enabled = false;

        WallSlide wallSlide = GetComponent<WallSlide>();
        if (wallSlide != null)
            wallSlide.enabled = false;

        PlayerUnstuckHitbox unstuck = GetComponent<PlayerUnstuckHitbox>();
        if (unstuck != null)
            unstuck.enabled = false;
    }

    private void DisableSolidCollidersForDeathFall()
    {
        Collider2D[] colliders = GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider2D playerCollider = colliders[i];
            if (playerCollider != null && !playerCollider.isTrigger)
                playerCollider.enabled = false;
        }
    }

    private void PlayHitFeedback()
    {
        if (animator == null)
            return;

        string triggerToUse = ResolveHitTriggerName();
        if (string.IsNullOrWhiteSpace(triggerToUse))
            return;

        animator.ResetTrigger(triggerToUse);
        animator.SetTrigger(triggerToUse);
    }

    private string ResolveHitTriggerName()
    {
        if (!string.IsNullOrWhiteSpace(hitTriggerName) && HasAnimatorTrigger(hitTriggerName))
            return hitTriggerName;

        if (HasAnimatorTrigger("Pet1_Hit"))
            return "Pet1_Hit";

        if (HasAnimatorTrigger("Pet2_Hit"))
            return "Pet2_Hit";

        if (HasAnimatorTrigger("Pet3_Hit"))
            return "Pet3_Hit";

        return string.Empty;
    }

    private void ApplyDamageKnockback()
    {
        if (knockbackRoutine != null)
            StopCoroutine(knockbackRoutine);

        knockbackRoutine = StartCoroutine(DamageKnockbackRoutine());
    }

    private IEnumerator DamageKnockbackRoutine()
    {
        PlayerMovement movement = GetComponent<PlayerMovement>();
        JumpTwice jump = GetComponent<JumpTwice>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        Vector2 knockbackVelocity = GetDamageKnockbackVelocity();

        bool hadMovement = movement != null && movement.enabled;
        bool hadJump = jump != null && jump.enabled;

        if (movement != null)
            movement.enabled = false;
        if (jump != null)
            jump.enabled = false;

        if (rb != null)
            rb.linearVelocity = knockbackVelocity;

        yield return new WaitForSeconds(Mathf.Max(0f, damageKnockbackControlLock));

        if (!isDead)
        {
            if (movement != null)
                movement.enabled = hadMovement;
            if (jump != null)
                jump.enabled = hadJump;
        }

        knockbackRoutine = null;
    }

    private Vector2 GetDamageKnockbackVelocity()
    {
        float xForce = Mathf.Abs(damageKnockbackVelocity.x);
        float yForce = damageKnockbackVelocity.y;

        if (!hasDamageSourcePosition)
            return new Vector2(-xForce, yForce);

        float direction = transform.position.x >= lastDamageSourcePosition.x ? 1f : -1f;
        return new Vector2(direction * xForce, yForce);
    }

    private bool HasAnimatorTrigger(string triggerName)
    {
        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == AnimatorControllerParameterType.Trigger && parameters[i].name == triggerName)
                return true;
        }

        return false;
    }

    private bool IsDamageImmune()
    {
        return Time.time < immuneUntilTime;
    }

    private IEnumerator TutorialRestartRoutine()
    {
        var rb = GetComponent<Rigidbody2D>();
        var cam = Camera.main != null ? Camera.main.GetComponent<CameraFollowing2>() : null;
        var respawnOnFall = GetComponent<RespawnOnFall>();
        BlackoutFaders fader = AutoBlackoutFader.EnsureInstance();

        if (freezeVelocityOnRespawn && rb != null)
            rb.linearVelocity = Vector2.zero;

        if (fader != null)
            yield return fader.FadeOut(0.25f);
        else
            yield return new WaitForSecondsRealtime(respawnDelay);

        respawnPoint = initialRespawnPoint;
        if (respawnOnFall != null)
            respawnOnFall.SetRespawnPosition(initialRespawnPoint);

        FallWhenTouched.ResetAllPlatforms();

        currentHearts = maxHearts;
        if (hud != null)
            hud.SetHearts(currentHearts, maxHearts);

        transform.position = initialRespawnPoint;
        if (cam != null)
            cam.SnapToTarget();

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.RestoreGameplayIfPanelsClosed();

        if (fader != null)
            yield return fader.FadeIn(0.35f);
    }
}
