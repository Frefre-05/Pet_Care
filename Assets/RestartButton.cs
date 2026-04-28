using UnityEngine;

public class RestartButton : MonoBehaviour
{
    // Called by the button OnClick()
    public void RestartLevel()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        RespawnOnFall respawn = player.GetComponent<RespawnOnFall>();
        if (respawn != null)
        {
            respawn.RespawnNow();
        }
    }
}
