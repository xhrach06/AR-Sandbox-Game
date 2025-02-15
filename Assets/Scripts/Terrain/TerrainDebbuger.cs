using UnityEngine;

public class TerrainClickDebugger : MonoBehaviour
{
    public Terrain terrain; // Assign your terrain in the Inspector
    public GameObject debugMarkerPrefab; // Assign a marker (small sphere/cube) if needed
    public LayerMask terrainLayerMask; // Set this to "Terrain" layer in the Inspector

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left Mouse Click
        {
            Debug.Log("Click detected");
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                Debug.Log($"Raycast hit: {hit.collider.name}");

                if (hit.collider != null && hit.collider.gameObject == terrain.gameObject)
                {
                    Vector3 terrainPosition = hit.point;
                    Debug.Log($"Clicked Terrain at: {terrainPosition}");

                    // Place a debug marker if a prefab is assigned
                    if (debugMarkerPrefab != null)
                    {
                        Instantiate(debugMarkerPrefab, terrainPosition, Quaternion.identity);
                    }
                }
                else
                {
                    Debug.Log("Raycast hit something, but it is not the terrain.");
                }
            }
            else
            {
                Debug.Log("Raycast did not hit anything.");
            }
        }
    }

}
