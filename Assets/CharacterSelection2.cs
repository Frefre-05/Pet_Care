using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class CharacterSelection2 : MonoBehaviour
{
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
    public AudioSource clickSfx;

    public static int selectedCharacter = -1;

    private const string PREF_SELECTED = "SelectedCharacter";
    private const string PREF_PLAYERNAME = "PlayerName";

    void Awake()
    {
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
            nameInput.onValueChanged.AddListener(_ => RefreshPlayInteractivity());

        ApplyVisualsImmediate();
        RefreshPlayInteractivity();
    }

    public void Select(int index)
    {
        if (characterButtons == null || characterButtons.Length == 0) return;

        index = Mathf.Clamp(index, 0, characterButtons.Length - 1);
        if (selectedCharacter == index) return;

        selectedCharacter = index;
        PlayerPrefs.SetInt(PREF_SELECTED, selectedCharacter);
        PlayerPrefs.Save();

        if (clickSfx) clickSfx.Play();

        // Animate buttons
        for (int i = 0; i < characterButtons.Length; i++)
            StartCoroutine(TweenButton(characterButtons[i], i == selectedCharacter));

        RefreshPlayInteractivity();
    }

    public void Play()
    {
        if (!IsNameValid() || selectedCharacter < 0) return;

        string playerName = nameInput.text.Trim();
        PlayerPrefs.SetString(PREF_PLAYERNAME, playerName);
        PlayerPrefs.SetInt(PREF_SELECTED, selectedCharacter);
        PlayerPrefs.Save();

        if (!string.IsNullOrEmpty(playSceneName))
            SceneManager.LoadScene(playSceneName);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // ---------- Helpers ----------
    bool IsNameValid()
    {
        string n = nameInput ? nameInput.text.Trim() : "";
        return !string.IsNullOrEmpty(n) && n.Length >= minNameLength;
    }

    void RefreshPlayInteractivity()
    {
        bool canPlay = (selectedCharacter >= 0) && IsNameValid();
        if (playButton) playButton.interactable = canPlay;
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
