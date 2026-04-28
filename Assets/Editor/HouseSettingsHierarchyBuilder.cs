using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class HouseSettingsHierarchyBuilder
{
    private const string HouseSceneName = "House";
    private const string CanvasName = "Canvas_NeedsUI";
    private const string SettingsButtonName = "settings";
    private const string SettingsPanelName = "Settings Panel";
    private const string ToggleSfxStatusName = "ToggleSfxStatusText";

    static HouseSettingsHierarchyBuilder()
    {
        EditorApplication.delayCall += TryBuildForActiveScene;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        TryBuild(scene);
    }

    private static void TryBuildForActiveScene()
    {
        TryBuild(SceneManager.GetActiveScene());
    }

    private static void TryBuild(Scene scene)
    {
        if (!scene.IsValid() || scene.name != HouseSceneName)
            return;

        Canvas canvas = FindCanvas(scene);
        if (canvas == null)
            return;

        bool changed = false;

        OptionsMenu optionsMenu = canvas.GetComponent<OptionsMenu>();
        if (optionsMenu == null)
        {
            optionsMenu = Undo.AddComponent<OptionsMenu>(canvas.gameObject);
            changed = true;
        }

        AudioSource musicSource = FindAudioSource(scene, "Music");
        AudioSource sfxSource = FindAudioSource(scene, "SFX");

        if (optionsMenu.musicSource != musicSource)
        {
            optionsMenu.musicSource = musicSource;
            changed = true;
        }

        if (optionsMenu.sfxSource != sfxSource)
        {
            optionsMenu.sfxSource = sfxSource;
            changed = true;
        }

        GameObject panel = FindChildRecursive(canvas.transform, SettingsPanelName)?.gameObject;
        if (panel == null)
        {
            panel = CreateSettingsPanel(canvas.transform);
            changed = true;
        }

        if (optionsMenu.settingsPanel != panel)
        {
            optionsMenu.settingsPanel = panel;
            changed = true;
        }

        Button settingsButton = FindButton(canvas.transform, SettingsButtonName, "settings");
        if (settingsButton == null)
        {
            settingsButton = CreateButton(canvas.transform, SettingsButtonName, "Settings", new Vector2(-120f, -70f));
            changed = true;
        }

        changed |= EnsureSettingsPanelChildren(panel.transform, optionsMenu);
        changed |= EnsurePersistentListener(settingsButton, optionsMenu, nameof(OptionsMenu.OpenSettings), optionsMenu.OpenSettings);

        if (changed)
        {
            EditorUtility.SetDirty(canvas.gameObject);
            EditorSceneManager.MarkSceneDirty(scene);
        }
    }

    private static Canvas FindCanvas(Scene scene)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindChildRecursive(root.transform, CanvasName);
            if (found != null)
                return found.GetComponent<Canvas>();
        }

        return null;
    }

    private static AudioSource FindAudioSource(Scene scene, string objectName)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform found = FindChildRecursive(root.transform, objectName);
            if (found != null)
                return found.GetComponent<AudioSource>();
        }

        return null;
    }

    private static GameObject CreateSettingsPanel(Transform parent)
    {
        GameObject panel = new GameObject(SettingsPanelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        Undo.RegisterCreatedObjectUndo(panel, "Create Settings Panel");
        panel.transform.SetParent(parent, false);
        panel.transform.SetAsLastSibling();

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(820f, 520f);
        rect.anchoredPosition = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = new Color32(30, 77, 108, 242);
        image.raycastTarget = true;

        panel.SetActive(false);
        return panel;
    }

    private static bool EnsureSettingsPanelChildren(Transform panel, OptionsMenu optionsMenu)
    {
        bool changed = false;

        if (FindChildRecursive(panel, "Title") == null)
        {
            CreateLabel(panel, "Title", "Settings", new Vector2(0f, -70f), new Vector2(400f, 50f), 34f, Color.white, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            changed = true;
        }

        Transform status = FindChildRecursive(panel, ToggleSfxStatusName);
        if (status == null)
        {
            GameObject statusObject = CreateLabel(panel, ToggleSfxStatusName, string.Empty, new Vector2(0f, -24f), new Vector2(580f, 44f), 24f, new Color32(156, 255, 168, 255), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f));
            statusObject.SetActive(false);
            changed = true;
        }

        Button toggleMusic = FindButton(panel, "ToggleMusicButton", "toggle music");
        if (toggleMusic == null)
        {
            toggleMusic = CreateButton(panel, "ToggleMusicButton", "Toggle Music", new Vector2(0f, 70f));
            changed = true;
        }
        changed |= EnsurePersistentListener(toggleMusic, optionsMenu, nameof(OptionsMenu.ToggleMusic), optionsMenu.ToggleMusic);

        Button toggleSfx = FindButton(panel, "ToggleSFXButton", "toggle sfx");
        if (toggleSfx == null)
        {
            toggleSfx = CreateButton(panel, "ToggleSFXButton", "Toggle SFX", new Vector2(0f, -10f));
            changed = true;
        }
        changed |= EnsurePersistentListener(toggleSfx, optionsMenu, nameof(OptionsMenu.ToggleSFX), optionsMenu.ToggleSFX);

        Button back = FindButton(panel, "BackButton", "back");
        if (back == null)
        {
            back = CreateButton(panel, "BackButton", "Back", new Vector2(0f, -120f));
            changed = true;
        }
        changed |= EnsurePersistentListener(back, optionsMenu, nameof(OptionsMenu.BackToMenu), optionsMenu.BackToMenu);

        return changed;
    }

    private static Button CreateButton(Transform parent, string objectName, string labelText, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(buttonObject, "Create Settings Button");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(300f, 64f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color32(223, 154, 106, 255);
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;

        GameObject textObject = CreateLabel(buttonObject.transform, "Text (TMP)", labelText, Vector2.zero, Vector2.zero, 28f, new Color32(50, 50, 50, 255), Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private static GameObject CreateLabel(Transform parent, string objectName, string textValue, Vector2 anchoredPosition, Vector2 sizeDelta, float fontSize, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        Undo.RegisterCreatedObjectUndo(textObject, "Create Settings Label");
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = textValue;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        text.outlineWidth = 0.18f;
        text.outlineColor = new Color32(0, 0, 0, 255);

        return textObject;
    }

    private static Button FindButton(Transform parent, string objectName, string labelText)
    {
        Transform named = FindChildRecursive(parent, objectName);
        if (named != null)
        {
            Button namedButton = named.GetComponent<Button>();
            if (namedButton != null)
                return namedButton;
        }

        Button[] buttons = parent.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            TextMeshProUGUI label = buttons[i].GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null && string.Equals((label.text ?? string.Empty).Trim(), labelText, System.StringComparison.OrdinalIgnoreCase))
                return buttons[i];
        }

        return null;
    }

    private static bool EnsurePersistentListener(Button button, OptionsMenu target, string methodName, UnityEngine.Events.UnityAction action)
    {
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == target && button.onClick.GetPersistentMethodName(i) == methodName)
                return false;
        }

        UnityEventTools.AddPersistentListener(button.onClick, action);
        return true;
    }

    private static Transform FindChildRecursive(Transform parent, string targetName)
    {
        if (parent == null)
            return null;

        if (string.Equals(parent.name, targetName, System.StringComparison.OrdinalIgnoreCase))
            return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindChildRecursive(parent.GetChild(i), targetName);
            if (found != null)
                return found;
        }

        return null;
    }
}
