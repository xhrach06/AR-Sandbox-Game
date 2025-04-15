using UnityEngine;

/// <summary>
/// Follows a world target and positions UI elements (e.g., health bars) above it in screen space.
/// </summary>
public class FollowWorldTarget : MonoBehaviour
{
    private Transform target;
    private Vector3 offset = new Vector3(0, 1.5f, 0); // Offset above the target
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (target == null || mainCamera == null) return;

        Vector3 worldPosition = target.position + offset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        screenPosition.y += 5f; // Slight lift for visibility
        transform.position = screenPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
