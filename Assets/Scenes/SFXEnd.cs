using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SFXEnd : MonoBehaviour
{
    [Header("EndPoint SFX")]
    [SerializeField] private AudioClip endpointSfx;
    [SerializeField] private AudioSource endpointSfxSource;
    [SerializeField] private string playerTag = "Player";

    private bool playedThisScene;

    private void Reset()
    {
        endpointSfxSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playedThisScene)
            return;

        if (!IsPlayerCollider(other))
            return;

        playedThisScene = true;
        PlayEndPointSfx();
    }

    private bool IsPlayerCollider(Collider2D other)
    {
        if (other == null)
            return false;

        if (!string.IsNullOrWhiteSpace(playerTag))
        {
            if (other.CompareTag(playerTag))
                return true;

            if (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag(playerTag))
                return true;

            Transform root = other.transform.root;
            if (root != null && root.CompareTag(playerTag))
                return true;
        }

        return other.GetComponent<PlayerHealth>() != null ||
               other.GetComponentInParent<PlayerHealth>() != null;
    }

    private void PlayEndPointSfx()
    {
        if (endpointSfx == null)
            return;

        if (endpointSfxSource != null)
        {
            endpointSfxSource.PlayOneShot(endpointSfx);
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(endpointSfx);
            return;
        }

        AudioSource.PlayClipAtPoint(endpointSfx, transform.position);
    }
}
