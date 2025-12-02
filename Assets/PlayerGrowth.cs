using UnityEngine;

public class PlayerGrowth : MonoBehaviour
{
    [Header("Growth Settings")]
    public float growthStep = 0.2f;
    public float maxScale = 2f;

    private static Vector3 savedScale = Vector3.zero;

    void Start()
    {
        if (savedScale != Vector3.zero)
            transform.localScale = savedScale;
        else
            savedScale = transform.localScale;
    }

    public void Grow()
    {
        Vector3 newScale = transform.localScale + Vector3.one * growthStep;
        newScale.x = Mathf.Min(newScale.x, maxScale);
        newScale.y = Mathf.Min(newScale.y, maxScale);
        newScale.z = Mathf.Min(newScale.z, maxScale);

        transform.localScale = newScale;
        savedScale = newScale;
    }
}
