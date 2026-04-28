using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerUnstuckHitbox))]
public class JumpTwice : MonoBehaviour
{
    private static readonly string[] GroundJumpStates = { "Jump", "Jump1", "Jump2" };
    private static readonly string[] AirJumpStates = { "DoubleJump", "DoubleJump1", "DoubleJump2" };
    private const string JumpBoostUntilKey = "PLAYER_JUMP_BOOST_UNTIL_UTC";

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    [Header("Shop Boosts")]
    [SerializeField] private float jumpBoostMultiplier = 1.18f;
    [Header("Hunger Speed Penalty (Reversible)")]
    [SerializeField] private bool enableHungerSpeedPenalty = true;
    [Range(0f, 100f)] [SerializeField] private float lowHungerThreshold = 35f;
    [Range(0f, 100f)] [SerializeField] private float emptyHungerThreshold = 5f;
    [Range(0.2f, 1f)] [SerializeField] private float minSpeedMultiplierAtZeroHunger = 0.6f;

    [Header("Jump Settings")]
    public int extraJumps = 1; // 1 = allows double jump
    [SerializeField] private float fallingPlatformGroundGraceSeconds = 0.35f;
    private int jumpsLeft;

    private Rigidbody2D rb;
    private Animator anim;
    private PetNeeds petNeeds;

    private bool isGrounded;
    private bool wasAtFanTop;
    private bool animatorHasIsRunning;
    private bool animatorHasIsGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        jumpsLeft = Mathf.Max(0, extraJumps);
        petNeeds = FindAnyObjectByType<PetNeeds>();
        CacheAnimatorParameters();

        if (rb == null)
        {
            Debug.LogError("[JumpTwice] Missing Rigidbody2D on " + name + ".", this);
            enabled = false;
        }
    }

    void Update()
    {
        if (rb == null)
            return;

        if (LevelsDropDown.IsMenuOpen)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            if (anim != null)
            {
                SetAnimatorBool("isRunning", false);
                SetAnimatorBool("isGrounded", Mathf.Abs(rb.linearVelocity.y) < 0.01f);
            }
            return;
        }

        // --- Move ---
        float move = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(move * moveSpeed * GetHungerSpeedMultiplier(), rb.linearVelocity.y);

        // --- Simple ground check (no groundCheck object) ---
        // If player's vertical speed is almost zero AND player is near ground
        isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f || FallWhenTouched.WasRecentlyRidingFallingPlatform(transform, fallingPlatformGroundGraceSeconds);

        bool fanBlocksNormalJump = FanUpdraft.IsJumpBlockedUntilLiftTop(transform);
        bool fanAllowsOnlyAirJump = FanUpdraft.IsPlayerAtFanTop(transform);
        if (fanAllowsOnlyAirJump && !wasAtFanTop)
            jumpsLeft = Mathf.Max(jumpsLeft, Mathf.Max(0, extraJumps));
        wasAtFanTop = fanAllowsOnlyAirJump;

        // --- Jump ---
        if (Input.GetButtonDown("Jump"))
        {
            if (fanBlocksNormalJump)
                return;

            if (fanAllowsOnlyAirJump)
            {
                if (jumpsLeft > 0)
                {
                    PlayFirstAvailableState(AirJumpStates);
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, GetCurrentJumpForce());
                    jumpsLeft--;
                }

                return;
            }

            if (isGrounded)
            {
                PlayFirstAvailableState(GroundJumpStates);
                FallWhenTouched.NotifyPlayerJumpedFromFallingPlatform(transform);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, GetCurrentJumpForce());
                jumpsLeft = Mathf.Max(0, extraJumps); // reset extra jumps when on ground
            }
            else if (jumpsLeft > 0)
            {
                PlayFirstAvailableState(AirJumpStates);
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, GetCurrentJumpForce());
                jumpsLeft--;
            }
        }

        // --- Animation (optional) ---
        if (anim != null)
        {
            SetAnimatorBool("isRunning", move != 0);
            SetAnimatorBool("isGrounded", isGrounded);
        }
    }

    private float GetHungerSpeedMultiplier()
    {
        if (!enableHungerSpeedPenalty) return 1f;
        if (petNeeds == null || !petNeeds.gameObject.activeInHierarchy) petNeeds = FindAnyObjectByType<PetNeeds>();
        if (petNeeds == null) return 1f;

        float hunger = petNeeds.Hunger;
        if (hunger >= lowHungerThreshold) return 1f;
        float t = Mathf.InverseLerp(lowHungerThreshold, emptyHungerThreshold, hunger);
        return Mathf.Lerp(1f, minSpeedMultiplierAtZeroHunger, t);
    }

    private void PlayFirstAvailableState(string[] stateNames)
    {
        if (anim == null || stateNames == null)
            return;

        for (int i = 0; i < stateNames.Length; i++)
        {
            string stateName = stateNames[i];
            if (string.IsNullOrWhiteSpace(stateName))
                continue;

            if (anim.HasState(0, Animator.StringToHash(stateName)))
            {
                anim.Play(stateName);
                return;
            }
        }
    }

    private float GetCurrentJumpForce()
    {
        float baseForce = jumpForce;
        if (!IsBoostActive(JumpBoostUntilKey))
            return baseForce;

        return baseForce * Mathf.Max(1f, jumpBoostMultiplier);
    }

    private bool IsBoostActive(string key)
    {
        string raw = PlayerPrefs.GetString(key, string.Empty);
        if (string.IsNullOrWhiteSpace(raw))
            return false;

        if (!System.DateTime.TryParse(raw, null, System.Globalization.DateTimeStyles.RoundtripKind, out System.DateTime untilUtc))
            return false;

        return System.DateTime.UtcNow < untilUtc.ToUniversalTime();
    }

    private void CacheAnimatorParameters()
    {
        if (anim == null)
            return;

        AnimatorControllerParameter[] parameters = anim.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type != AnimatorControllerParameterType.Bool)
                continue;

            if (parameters[i].name == "isRunning")
                animatorHasIsRunning = true;
            else if (parameters[i].name == "isGrounded")
                animatorHasIsGrounded = true;
        }
    }

    private void SetAnimatorBool(string parameterName, bool value)
    {
        if (anim == null)
            return;

        if (parameterName == "isRunning" && animatorHasIsRunning)
            anim.SetBool(parameterName, value);
        else if (parameterName == "isGrounded" && animatorHasIsGrounded)
            anim.SetBool(parameterName, value);
    }
}
