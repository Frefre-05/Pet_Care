using UnityEngine;

public class CharacterSpawner : MonoBehaviour
{
    [Header("Assign your 3 character PREFABS here (not the UI buttons)")]
    public GameObject[] characters; // Size = number of pets

    [Header("Optional")]
    [SerializeField] private Transform spawnPoint; // Leave empty to spawn at 0,0,0
    [SerializeField] private CameraFollowing2 cameraFollowing; // Match your new script name

    private void Start()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("[CharacterSpawner] No character prefabs assigned!");
            return;
        }

        // --- Load previously selected character index (default = 0) ---
        int index = PlayerPrefs.GetInt("SelectedCharacter", 0);
        index = Mathf.Clamp(index, 0, characters.Length - 1);

        // --- Spawn the character ---
        Vector3 pos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        GameObject player = Instantiate(characters[index], pos, Quaternion.identity);

        // --- Find the camera if not assigned ---
        if (cameraFollowing == null)
            cameraFollowing = Camera.main.GetComponent<CameraFollowing2>();

        // --- Attach the camera to the spawned character ---
        if (cameraFollowing != null)
            cameraFollowing.SetTarget(player.transform);
        else
            Debug.LogWarning("[CharacterSpawner] No CameraFollowing2 component found on Main Camera!");
    }
}
