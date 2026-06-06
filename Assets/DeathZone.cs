using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth ph = FindPlayerHealth(other);
        if (ph == null)
            return;

        ph.TakeDamageAndRespawn();
    }

    private PlayerHealth FindPlayerHealth(Collider2D other)
    {
        if (other == null)
            return null;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != null)
            return ph;

        ph = other.GetComponentInParent<PlayerHealth>();
        if (ph != null)
            return ph;

        if (other.attachedRigidbody != null)
        {
            ph = other.attachedRigidbody.GetComponent<PlayerHealth>();
            if (ph != null)
                return ph;
        }

        Transform root = other.transform.root;
        if (root != null)
            return root.GetComponent<PlayerHealth>();

        return null;
    }
}
