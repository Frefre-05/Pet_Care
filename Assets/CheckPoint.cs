using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CheckPoint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite inactiveSprite;
    [SerializeField] private Sprite activeSprite;
    [SerializeField] private Vector2 spawnOffset = new Vector2(0f, 0.3f);
    [Header("Checkpoint SFX")]
    [SerializeField] private AudioClip checkpointSfx;
    [SerializeField] private AudioSource checkpointSfxSource;

    private bool activated = false;

    private void Reset()
    {
        sr = GetComponent<SpriteRenderer>();
        checkpointSfxSource = GetComponent<AudioSource>();
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        var ph = other.GetComponent<PlayerHealth>() ?? other.GetComponentInParent<PlayerHealth>();
        if (ph == null) return;

        Vector3 checkpointPos = transform.position + (Vector3)spawnOffset;

        ph.SetCheckpoint(checkpointPos);
        ph.HealToFull();

        var rof = other.GetComponent<RespawnOnFall>() ?? other.GetComponentInParent<RespawnOnFall>();
        if (rof != null)
            rof.SetRespawnPosition(checkpointPos);

        activated = true;
        if (sr != null && activeSprite != null) sr.sprite = activeSprite;
        PlayCheckpointSfx();
    }

    private void PlayCheckpointSfx()
    {
        if (checkpointSfx == null)
            return;

        if (checkpointSfxSource != null)
        {
            checkpointSfxSource.PlayOneShot(checkpointSfx);
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(checkpointSfx);
            return;
        }

        AudioSource.PlayClipAtPoint(checkpointSfx, transform.position);
    }
}
