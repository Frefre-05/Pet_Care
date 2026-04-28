using UnityEngine;
using System.IO;

public class MainMenu : MonoBehaviour
{
    [System.Serializable]
    private class PlayerProfileData
    {
        public int selectedCharacter = -1;
        public string playerName = "";
    }

    [SerializeField] private string characterSelectionScene = "Pet Selection";
    [SerializeField] private string returningPlayerScene = "House";

    private const string PrefSelected = "SelectedCharacter";
    private const string PrefPlayerName = "PlayerName";

    public void PlayGame()
    {
        if (HasReturningProfile())
            SceneTransitionLoader.LoadScene(returningPlayerScene);
        else if (!string.IsNullOrWhiteSpace(characterSelectionScene))
            SceneTransitionLoader.LoadScene(characterSelectionScene);
        else
            SceneTransitionLoader.LoadScene(1);
    }

    private bool HasReturningProfile()
    {
        int selected = PlayerPrefs.GetInt(PrefSelected, -1);
        string playerName = PlayerPrefs.GetString(PrefPlayerName, "").Trim();
        if (selected >= 0 && !string.IsNullOrEmpty(playerName))
            return true;

        string path = Path.Combine(Application.persistentDataPath, "player_profile_v1.json");
        if (!File.Exists(path))
            return false;

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
                return false;

            PlayerProfileData data = JsonUtility.FromJson<PlayerProfileData>(json);
            if (data == null || data.selectedCharacter < 0 || string.IsNullOrWhiteSpace(data.playerName))
                return false;

            PlayerPrefs.SetInt(PrefSelected, data.selectedCharacter);
            PlayerPrefs.SetString(PrefPlayerName, data.playerName.Trim());
            PlayerPrefs.Save();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
