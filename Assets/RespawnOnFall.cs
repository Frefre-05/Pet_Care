using UnityEngine;

public class RespawnOnFall : MonoBehaviour
{
    [Header("Respawn")]
    public Transform respawnPoint; // drag SpawnPoint here
    public float killY = -20f; // also die if you fall below this Y

    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (respawnPoint == null)
        {
            // auto-find an object named "SpawnPoint" in the scene
            var sp = GameObject.Find("SpawnPoint");
            if (sp != null) respawnPoint = sp.transform;
        }
    }

    void Update()
    {
        if (transform.position.y < killY)
            Respawn();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Death"))
            Respawn();
    }

    void Respawn()
    {
        // reset velocity and position
        if (rb != null) rb.linearVelocity = Vector2.zero;
        transform.position = respawnPoint.position;

        // (optional) reset animations/state here
        // GetComponent<Animator>()?.Play("Idle", 0, 0f);
    }
}
