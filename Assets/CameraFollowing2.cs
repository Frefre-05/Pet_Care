using UnityEngine;

public class CameraFollowing2 : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f;
    public bool lockY = true;

    private Vector3 velocity;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position;
        targetPos.z = transform.position.z;

        if (lockY)
            targetPos.y = transform.position.y;

        transform.position = Vector3.SmoothDamp(
        transform.position,
        targetPos,
        ref velocity,
        smoothTime
        );
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
