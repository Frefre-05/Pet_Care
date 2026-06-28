using UnityEngine;

public class RespawnOnFall : MonoBehaviour
{
    [Header("Respawn")]
    public Transform respawnPoint; // drag SpawnPoint here
    public float killY = -20f; // also die if you fall below this Y

    Rigidbody2D rb;
    PlayerHealth playerHealth;
    CameraFollowing2 cameraFollow;
    Vector3 respawnPosition;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
        cameraFollow = Camera.main != null ? Camera.main.GetComponent<CameraFollowing2>() : null;
        if (respawnPoint == null)
        {
            // auto-find an object named "SpawnPoint" in the scene
            var sp = GameObject.Find("SpawnPoint");
            if (sp != null) respawnPoint = sp.transform;
        }

        respawnPosition = respawnPoint != null ? respawnPoint.position : transform.position;
    }

    void Update()
    {
        if (transform.position.y < killY)
            DamageOrRespawn();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Death"))
            DamageOrRespawn();
    }

    void DamageOrRespawn()
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamageAndRespawn();
            return;
        }

        Respawn(true);
    }

    void Respawn(bool applyDeathPenalty)
    {
        FallWhenTouched.ResetAllPlatforms();

        // reset velocity and position
        if (rb != null) rb.linearVelocity = Vector2.zero;
        transform.position = respawnPosition;
        if (cameraFollow != null) cameraFollow.SnapToTarget();
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.RestoreGameplayIfPanelsClosed();

        if (applyDeathPenalty)
            LevelDeathPenalty.TryApply();

        // (optional) reset animations/state here
        // GetComponent<Animator>()?.Play("Idle", 0, 0f);
    }

    public void RespawnNow()
    {
        Respawn(false);
    }

    public void SetRespawnPosition(Vector3 newPos)
    {
        respawnPosition = newPos;
    }
}

public static class LevelDeathPenalty
{
    const int ApplesLostPerDeath = 3;
    static int lastAppliedFrame = -1;

    public static void TryApply()
    {
        if (!IsPenaltyScene())
            return;

        if (lastAppliedFrame == Time.frameCount)
            return;

        lastAppliedFrame = Time.frameCount;
        AppleCurrency.Spend(ApplesLostPerDeath);
    }

    static bool IsPenaltyScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (string.IsNullOrWhiteSpace(sceneName))
            return false;

        string s = sceneName.Trim().ToLowerInvariant().Replace(" ", string.Empty);
        return s == "level1" || s == "level2" || s == "level3" || s == "level4";
    }
}
