using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class SecretResetHotkey : MonoBehaviour
{
    private static bool bootstrapped;
    private static bool isResetting;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        if (bootstrapped) return;
        bootstrapped = true;

        if (FindFirstObjectByType<SecretResetHotkey>() != null) return;

        GameObject go = new GameObject("__SecretResetHotkey");
        DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideInHierarchy;
        go.AddComponent<SecretResetHotkey>();
    }

    private void Update()
    {
        if (!WasSecretComboPressed()) return;
        if (isResetting) return;
        StartCoroutine(ExecuteHardResetRoutine());
    }

    private static bool WasSecretComboPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k != null)
        {
            bool ctrlHeld = k.leftCtrlKey.isPressed || k.rightCtrlKey.isPressed;
            bool shiftHeld = k.leftShiftKey.isPressed || k.rightShiftKey.isPressed;
            if (ctrlHeld && shiftHeld && (k.sKey.wasPressedThisFrame || k.f6Key.wasPressedThisFrame))
                return true;
        }
#endif

        bool legacyHeld = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                          (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        return legacyHeld && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.F6));
    }

    private System.Collections.IEnumerator ExecuteHardResetRoutine()
    {
        isResetting = true;
        SceneManager.sceneLoaded += OnSceneLoadedAfterReset;

        // 1) Clear all persisted progress.
        SaveData.HardResetAllProgress();

        // 2) Reset static runtime progress.
        LevelProgress.ResetAll();
        PetNeeds.ResetSessionInitialization();
        CharacterSelection2.selectedCharacter = -1;
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // 3) Destroy persistent managers so runtime state matches a fresh launch.
        DestroyPersistentObjectsExceptSelf();

        yield return null;

        // 4) Reload first build scene.
        int firstScene = SceneManager.sceneCountInBuildSettings > 0 ? 0 : SceneManager.GetActiveScene().buildIndex;
        SceneTransitionLoader.LoadScene(firstScene);
    }

    private static void OnSceneLoadedAfterReset(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoadedAfterReset;
        SaveData.CompleteHardReset();
        isResetting = false;
        Debug.Log("[SecretResetHotkey] Full reset complete. Restarted from first scene.");
    }

    private void DestroyPersistentObjectsExceptSelf()
    {
        Scene ddolScene = gameObject.scene;
        GameObject[] roots = ddolScene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            GameObject root = roots[i];
            if (root == null || root == gameObject) continue;
            Destroy(root);
        }
    }
}
