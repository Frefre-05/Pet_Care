using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonClickSfxHook : MonoBehaviour, IPointerDownHandler, ISubmitHandler
{
    private Button button;
    private bool initialized;

    public void Initialize(Button targetButton = null)
    {
        button = targetButton != null ? targetButton : GetComponent<Button>();
        initialized = button != null;
    }

    private void Awake()
    {
        if (!initialized)
            Initialize();
    }

    private void OnDestroy()
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PlaySharedButtonSound();
    }

    public void OnSubmit(BaseEventData eventData)
    {
        PlaySharedButtonSound();
    }

    private void PlaySharedButtonSound()
    {
        if (!initialized)
            return;

        if (button == null || !button.IsActive() || !button.interactable)
            return;

        if (button.GetComponent<ShopButtonSfxOverride>() != null)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayButtonClickSfx();
    }
}
