using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TeleportNPC : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Hospital";
    [SerializeField] private bool requireInteractKey = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";

    private bool playerInside;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Update()
    {
        if (!requireInteractKey || !playerInside) return;
        if (!Input.GetKeyDown(interactKey)) return;

        Teleport();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (requireInteractKey)
        {
            playerInside = true;
            return;
        }

        Teleport();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
    }

    private void Teleport()
    {
        if (string.IsNullOrWhiteSpace(targetSceneName))
        {
            Debug.LogWarning("TeleportNPC: target scene is empty.");
            return;
        }

        SceneTransitionLoader.LoadScene(targetSceneName);
    }
}

