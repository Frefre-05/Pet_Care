using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlackoutFaders : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup; // assign the CanvasGroup on your Blackout object
    [SerializeField] private float defaultDuration = 0.35f;

    private void Reset()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
    }

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha);
    }

    public IEnumerator FadeOut(float duration = -1f) // 0 -> 1 (to black)
    {
        if (duration <= 0f) duration = defaultDuration;
        yield return FadeCanvasGroup(0f, 1f, duration);
    }

    public IEnumerator FadeIn(float duration = -1f) // 1 -> 0 (from black)
    {
        if (duration <= 0f) duration = defaultDuration;
        yield return FadeCanvasGroup(1f, 0f, duration);
    }

    private IEnumerator FadeCanvasGroup(float start, float end, float duration)
    {
        if (canvasGroup == null) yield break;

        float t = 0f;
        // optional: block clicks while black
        canvasGroup.blocksRaycasts = end > start;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        canvasGroup.alpha = end;
        canvasGroup.blocksRaycasts = end > 0.01f;
    }
}
