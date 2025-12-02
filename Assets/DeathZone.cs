using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var ph = other.GetComponent<PlayerHealth>();
        if (ph != null) ph.TakeDamageAndRespawn();
    }
}
