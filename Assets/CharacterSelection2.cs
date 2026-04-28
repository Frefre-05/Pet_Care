using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.IO;

public class CharacterSelection2 : MonoBehaviour
{
    [System.Serializable]
    private class PlayerProfileData
    {
        public int selectedCharacter = -1;
        public string playerName = "";
    }

    [Header("Assign your character SELECT buttons (order 0..N)")]
    public Button[] characterButtons;

    [Header("Play / Name UI")]
    public Button playButton; // Disabled until name + selection
    public TMP_InputField nameInput; // TMP input for player name
    [Tooltip("Minimum characters required for a valid player name")]
    public int minNameLength = 1;

    [Header("Highlight (no extra images needed)")]
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(0.75f, 1f, 0.75f, 1f);
    [Range(0.5f, 2f)] public float normalScale = 1f;
    [Range(0.5f, 2f)] public float selectedScale = 1.2f;
    [Range(0f, 0.5f)] public float tweenTime = 0.15f;

    [Header("Scene to Load (optional)")]
    public string playSceneName = ""; // leave empty to load next build index
    [SerializeField] private bool skipTutorialAfterFirstCompletion = true;
    [SerializeField] private string returnSceneAfterTutorial = "House";
    public AudioSource clickSfx;

    public static int selectedCharacter = -1;

    private const string PREF_SELECTED = "SelectedCharacter";
    private const string PREF_PLAYERNAME = "PlayerName";
    private const string PREF_TUTORIAL_COMPLETED = "TutorialCompleted";

    void Awake()
    {
        LoadProfileFromDiskIfNeeded();

        // Restore previous selection and name
        selectedCharacter = PlayerPrefs.GetInt(PREF_SELECTED, -1);
        if (nameInput != null)
            nameInput.text = PlayerPrefs.GetString(PREF_PLAYERNAME, "");

        // Hook buttons
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int idx = i;
            if (characterButtons[i] != null)
                characterButtons[i].onClick.AddListener(() => Select(idx));
        }

        // Hook name input validation
        if (nameInput)
            nameInput.onValueChanged.AddListener(OnNameChanged);

        ApplyVisualsImmediate();
        RefreshPlayInteractivity();
        SaveProfileToDisk(selectedCharacter, GetEffectivePlayerName());
    }

    public void Select(int index)
    {
        if (characterButtons == null || characterButtons.Length == 0) return;

        index = Mathf.Clamp(index, 0, characterButtons.Length - 1);
        if (selectedCharacter == index) return;

        selectedCharacter = index;
        PlayerPrefs.SetInt(PREF_SELECTED, selectedCharacter);
        PlayerPrefs.Save();
        SaveProfileToDisk(selectedCharacter, GetEffectivePlayerName());

        if (clickSfx) clickSfx.Play();

        // Animate buttons
        for (int i = 0; i < characterButtons.Length; i++)
            StartCoroutine(TweenButton(characterButtons[i], i == selectedCharacter));

        RefreshPlayInteractivity();
    }

    public void Play()
    {
        int effectiveSelected = selectedCharacter >= 0 ? selectedCharacter : PlayerPrefs.GetInt(PREF_SELECTED, -1);
        string effectiveName = GetEffectivePlayerName();
        if (string.IsNullOrEmpty(effectiveName) || effectiveSelected < 0) return;

        PlayerPrefs.SetString(PREF_PLAYERNAME, effectiveName);
        PlayerPrefs.SetInt(PREF_SELECTED, effectiveSelected);
        PlayerPrefs.Save();
        SaveProfileToDisk(effectiveSelected, effectiveName);

        bool tutorialCompleted = PlayerPrefs.GetInt(PREF_TUTORIAL_COMPLETED, 0) == 1;
        if (skipTutorialAfterFirstCompletion && tutorialCompleted && !string.IsNullOrWhiteSpace(returnSceneAfterTutorial))
        {
            SceneTransitionLoader.LoadScene(returnSceneAfterTutorial);
        }
        else if (!string.IsNullOrEmpty(playSceneName))
            SceneTransitionLoader.LoadScene(playSceneName);
        else
            SceneTransitionLoader.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // ---------- Helpers ----------
    bool IsNameValid()
    {
        string n = GetEffectivePlayerName();
        return !string.IsNullOrEmpty(n) && n.Length >= minNameLength;
    }

    string GetEffectivePlayerName()
    {
        string typed = nameInput ? nameInput.text.Trim() : string.Empty;
        if (!string.IsNullOrEmpty(typed)) return typed;
        return PlayerPrefs.GetString(PREF_PLAYERNAME, "").Trim();
    }

    string GetProfilePath()
    {
        return Path.Combine(Application.persistentDataPath, "player_profile_v1.json");
    }

    void LoadProfileFromDiskIfNeeded()
    {
        int savedSelected = PlayerPrefs.GetInt(PREF_SELECTED, -1);
        string savedName = PlayerPrefs.GetString(PREF_PLAYERNAME, "").Trim();
        if (savedSelected >= 0 && !string.IsNullOrEmpty(savedName))
            return;

        string path = GetProfilePath();
        if (!File.Exists(path))
            return;

        try
        {
            string json = File.ReadAllText(path);
            if (string.IsNullOrWhiteSpace(json))
                return;

            PlayerProfileData data = JsonUtility.FromJson<PlayerProfileData>(json);
            if (data == null)
                return;

            if (savedSelected < 0 && data.selectedCharacter >= 0)
                PlayerPrefs.SetInt(PREF_SELECTED, data.selectedCharacter);
            if (string.IsNullOrEmpty(savedName) && !string.IsNullOrWhiteSpace(data.playerName))
                PlayerPrefs.SetString(PREF_PLAYERNAME, data.playerName.Trim());

            PlayerPrefs.Save();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[CharacterSelection2] Could not load profile backup: " + ex.Message);
        }
    }

    void SaveProfileToDisk(int selected, string playerName)
    {
        try
        {
            PlayerProfileData data = new PlayerProfileData
            {
                selectedCharacter = selected,
                playerName = playerName ?? ""
            };
            File.WriteAllText(GetProfilePath(), JsonUtility.ToJson(data));
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("[CharacterSelection2] Could not save profile backup: " + ex.Message);
        }
    }

    void RefreshPlayInteractivity()
    {
        int effectiveSelected = selectedCharacter >= 0 ? selectedCharacter : PlayerPrefs.GetInt(PREF_SELECTED, -1);
        bool canPlay = (effectiveSelected >= 0) && IsNameValid();
        if (playButton) playButton.interactable = canPlay;
    }

    void OnNameChanged(string _)
    {
        string typed = GetEffectivePlayerName();
        if (!string.IsNullOrEmpty(typed))
        {
            PlayerPrefs.SetString(PREF_PLAYERNAME, typed);
            PlayerPrefs.Save();
            SaveProfileToDisk(selectedCharacter, typed);
        }

        RefreshPlayInteractivity();
    }

    void ApplyVisualsImmediate()
    {
        for (int i = 0; i < characterButtons.Length; i++)
            SetButtonVisual(characterButtons[i], i == selectedCharacter, true);
    }

    IEnumerator TweenButton(Button btn, bool isSelected)
    {
        if (!btn) yield break;

        var rt = btn.transform as RectTransform;
        var img = btn.targetGraphic as Graphic;

        float t = 0f;
        float fromScale = rt ? rt.localScale.x : 1f;
        float toScale = isSelected ? selectedScale : normalScale;

        Color fromColor = img ? img.color : Color.white;
        Color toColor = isSelected ? selectedColor : normalColor;

        while (t < tweenTime)
        {
            t += Time.unscaledDeltaTime;
            float k = tweenTime <= 0f ? 1f : Mathf.Clamp01(t / tweenTime);
            float s = Mathf.Lerp(fromScale, toScale, k);
            if (rt) rt.localScale = new Vector3(s, s, 1f);
            if (img) img.color = Color.Lerp(fromColor, toColor, k);
            yield return null;
        }

        SetButtonVisual(btn, isSelected, true);
    }

    void SetButtonVisual(Button btn, bool isSelected, bool immediate)
    {
        if (!btn) return;
        var rt = btn.transform as RectTransform;
        if (rt) rt.localScale = new Vector3(isSelected ? selectedScale : normalScale, isSelected ? selectedScale : normalScale, 1f);

        var img = btn.targetGraphic as Graphic;
        if (img) img.color = isSelected ? selectedColor : normalColor;
    }
}
