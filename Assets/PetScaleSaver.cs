using UnityEngine;

public class PetScaleSaver : MonoBehaviour
{
    private const string ScaleKey = "PET_SCALE";
    [SerializeField] private float defaultScale = 7f; // your current size

    private void Awake()
    {
        // Load saved size (or start at default 7)
        float saved = PlayerPrefs.GetFloat(ScaleKey, defaultScale);
        var t = transform.localScale;
        t.x = saved;
        t.y = saved;
        transform.localScale = t;
    }

    public void SaveCurrentScale()
    {
        float s = transform.localScale.x;
        PlayerPrefs.SetFloat(ScaleKey, s);
        PlayerPrefs.Save();
    }
}
