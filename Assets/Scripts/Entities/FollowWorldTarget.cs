using UnityEngine;

public class FollowWorldTarget : MonoBehaviour
{
    private Transform target;
    private Vector3 offset = new Vector3(0, 1.5f, 0); // height offset above the target
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (target != null && mainCamera != null)
        {
            Vector3 worldPosition = target.position + offset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
            screenPosition.y += 5f;
            transform.position = screenPosition;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
