using UnityEngine;
using UnityEngine.UI;

public class CloseThisPanel : MonoBehaviour
{
    [Tooltip("Optional: if left empty, I'll auto-find the nearest parent panel.")]
    [SerializeField] private GameObject panelRoot;

    [Tooltip("Optional: your dim background object (Image).")]
    [SerializeField] private GameObject dimBackground;

    [Tooltip("Resume time when closing (if you paused on open).")]
    [SerializeField] private bool resumeTime = true;

    public void Close()
    {
        // Find a panel to close
        GameObject target = panelRoot;
        if (target == null)
        {
            var cg = GetComponentInParent<CanvasGroup>(includeInactive: true);
            if (cg != null) target = cg.gameObject;
            else target = transform.root.gameObject; // fallback
        }

        // Turn off interactability if there is a CanvasGroup
        var panelCg = target.GetComponent<CanvasGroup>();
        if (panelCg != null)
        {
            panelCg.alpha = 0f;
            panelCg.interactable = false;
            panelCg.blocksRaycasts = false;
        }

        // Hide the panel
        target.SetActive(false);

        // Hide dim background if assigned
        if (dimBackground != null) dimBackground.SetActive(false);

        if (resumeTime) Time.timeScale = 1f;
    }
}
