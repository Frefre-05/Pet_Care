using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TrapDamage : MonoBehaviour
{
    [SerializeField] private int damageHearts = 1; // how many hearts to remove

    private void Reset()
    {
        // Make sure the collider is a trigger so the player can pass through
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Look for PlayerHealth on whatever we hit
        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(damageHearts); // this will remove 1 heart
        }
    }
}