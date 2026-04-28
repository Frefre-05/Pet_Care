using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class SecretAppleHotkey : MonoBehaviour
{
    private static bool bootstrapped;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        bootstrapped = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoBootstrap()
    {
        if (bootstrapped) return;
        bootstrapped = true;

        if (FindFirstObjectByType<SecretAppleHotkey>() != null) return;

        GameObject go = new GameObject("__SecretAppleHotkey");
        DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideInHierarchy;
        go.AddComponent<SecretAppleHotkey>();
    }

    private void Update()
    {
        if (!WasSecretComboPressed())
            return;

        AppleCurrency.Add(100);
        Debug.Log("[SecretAppleHotkey] Added 100 Gold Coins.");
    }

    private static bool WasSecretComboPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k != null)
        {
            bool ctrlHeld = k.leftCtrlKey.isPressed || k.rightCtrlKey.isPressed;
            bool shiftHeld = k.leftShiftKey.isPressed || k.rightShiftKey.isPressed;
            bool altHeld = k.leftAltKey.isPressed || k.rightAltKey.isPressed;
            bool onePressed =
                k.digit1Key.wasPressedThisFrame ||
                k.numpad1Key.wasPressedThisFrame;
            if (ctrlHeld && shiftHeld && altHeld && onePressed)
                return true;
        }
#endif

        bool ctrlHeldLegacy = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool shiftHeldLegacy = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool altHeldLegacy = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        bool onePressedLegacy =
            Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1) ||
            Input.GetKeyDown(KeyCode.Exclaim);
        return ctrlHeldLegacy && shiftHeldLegacy && altHeldLegacy && onePressedLegacy;
    }
}
