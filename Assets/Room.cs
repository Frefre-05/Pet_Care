using UnityEngine;
using TMPro;
using System.IO;

public class Room : MonoBehaviour
{
    [System.Serializable]
    private class PlayerProfileData
    {
        public int selectedCharacter = -1;
        public string playerName = "";
    }

    private const string PrefSelected = "SelectedCharacter";
    private const string PrefPlayerName = "PlayerName";
    private const string TutorialCompletedKey = "TutorialCompleted";
    [SerializeField] private string firstRunScene = "Tutorial";
    [SerializeField] private string returningScene = "House";

    public void PlayGame()
    {
        CharacterSelection2 selectionScreen = FindAnyObjectByType<CharacterSelection2>();
        if (selectionScreen != null && !selectionScreen.CanStartGameFromSelection())
            return;

        int selected = PlayerPrefs.GetInt(PrefSelected, -1);
        string playerName = PlayerPrefs.GetString(PrefPlayerName, "").Trim();

        if (selected < 0 && CharacterSelection2.selectedCharacter >= 0)
            selected = CharacterSelection2.selectedCharacter;

        if (string.IsNullOrEmpty(playerName))
        {
            TMP_InputField input = FindAnyObjectByType<TMP_InputField>();
            if (input != null)
                playerName = (input.text ?? string.Empty).Trim();
        }

        if (selected < 0 || string.IsNullOrEmpty(playerName))
        {
            PlayerProfileData backup = TryLoadProfileBackup();
            if (backup != null)
            {
                if (selected < 0 && backup.selectedCharacter >= 0)
                    selected = backup.selectedCharacter;
                if (string.IsNullOrEmpty(playerName) && !string.IsNullOrWhiteSpace(backup.playerName))
                    playerName = backup.playerName.Trim();
            }
        }

        if (selected < 0 || string.IsNullOrEmpty(playerName))
            return;

        PlayerPrefs.SetInt(PrefSelected, selected);
        PlayerPrefs.SetString(PrefPlayerName, playerName);
        PlayerPrefs.Save();
        SaveProfileBackup(selected, playerName);

        bool tutorialCompleted = PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1;
        string targetScene = tutorialCompleted ? returningScene : firstRunScene;
        if (string.IsNullOrWhiteSpace(targetScene))
            targetScene = tutorialCompleted ? "House" : "Tutorial";

        SceneTransitionLoader.LoadScene(targetScene);
    }

    private void SaveProfileBackup(int selected, string playerName)
    {
        try
        {
            PlayerProfileData data = new PlayerProfileData
            {
                selectedCharacter = selected,
                playerName = playerName ?? ""
            };
            string path = Path.Combine(Application.persistentDataPath, "player_profile_v1.json");
            File.WriteAllText(path, JsonUtility.ToJson(data));
        }
        catch
        {
            // ignore backup failures, prefs already saved
        }
    }

    private PlayerProfileData TryLoadProfileBackup()
    {
        try
        {
            string path = Path.Combine(Application.persistentDataPath, "player_profile_v1.json");
            if (!File.Exists(path))
                return null;

            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonUtility.FromJson<PlayerProfileData>(json);
        }
        catch
        {
            return null;
        }
    }
}
