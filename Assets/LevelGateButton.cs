using UnityEngine;
using UnityEngine.UI;

public class LevelGateButton : MonoBehaviour
{
    [Tooltip("0=Tutorial, 1=Level1, 2=Level2, 3=Level3, 4=Level4")]
    public int requiredUnlockedIndex = 1; // e.g., Level1 needs 1 (Tutorial done)

    [Header("Optional visuals")]
    public GameObject lockIcon; // e.g., a small padlock image
    public CanvasGroup greyOut; // optional: to fade/grey the whole button

    Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        LevelProgress.OnProgressChanged += Refresh;
    }

    void OnEnable() => Refresh();

    void OnDestroy() => LevelProgress.OnProgressChanged -= Refresh;

    void Refresh()
    {
        bool unlocked = LevelProgress.CanPlay(requiredUnlockedIndex);
        if (btn) btn.interactable = unlocked;

        if (lockIcon) lockIcon.SetActive(!unlocked);
        if (greyOut) greyOut.alpha = unlocked ? 1f : 0.5f;
    }
}
