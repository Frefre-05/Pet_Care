using UnityEngine;

public class SpikeFaceTrapHitbox : MonoBehaviour
{
    private SpikeFaceTrap owner;

    void Awake()
    {
        if (owner == null)
            owner = GetComponentInParent<SpikeFaceTrap>();
    }

    public void SetOwner(SpikeFaceTrap trap)
    {
        owner = trap;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null)
            owner.HandleRelayTrigger(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (owner != null)
            owner.HandleRelayTrigger(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (owner != null)
            owner.HandleRelayCollision(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (owner != null)
            owner.HandleRelayCollision(collision);
    }
}
