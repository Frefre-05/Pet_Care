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

    [SerializeField] private HealthManager hud; // Optional, assign if you have heart UI

    private Vector3 respawnPoint;

    private void Awake()
    {
        if (hud == null)
            hud = FindObjectOfType<HealthManager>();
        currentHearts = maxHearts;
        if (hud != null)
            hud.SetHearts(currentHearts, maxHearts);
        respawnPoint = transform.position;
    }

    public void SetCheckpoint(Vector3 pos)
    {
        respawnPoint = pos;
    }

    public void TakeDamageAndRespawn()
    {
        currentHearts = Mathf.Max(0, currentHearts - 1);

        if (hud != null) hud.SetHearts(currentHearts, maxHearts);

        if (currentHearts <= 0)
        {
            // Restart the scene
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        StartCoroutine(RespawnRoutine());
    }

    // 🔥🔥 ADD THIS BELOW (this is what traps call)
    public void TakeDamage(int amount)
    {
        TakeDamageAndRespawn(); // Use your existing system
    }
    // 🔥🔥 END OF FIX

    public void HealToFull()
    {
        currentHearts = maxHearts;
        if (hud != null) hud.SetHearts(currentHearts, maxHearts);
    }

    private IEnumerator RespawnRoutine()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (freezeVelocityOnRespawn && rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(respawnDelay);
        transform.position = respawnPoint;
    }
}
