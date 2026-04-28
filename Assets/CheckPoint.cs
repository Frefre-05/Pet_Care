using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CheckPoint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0f, 0.3f);

    private bool activated = false;

    private void Reset()
    {
        sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;

        var ph = other.GetComponent<PlayerHealth>();
        Vector3 checkpointPos = transform.position + (Vector3)spawnOffset;

        if (ph != null)
        {
            ph.SetCheckpoint(checkpointPos);
            ph.HealToFull();
        }

        var rof = other.GetComponent<RespawnOnFall>();
        if (rof != null)
            rof.SetRespawnPosition(checkpointPos);

        activated = true;
        if (sr != null && activeSprite != null) sr.sprite = activeSprite;
    }
}
