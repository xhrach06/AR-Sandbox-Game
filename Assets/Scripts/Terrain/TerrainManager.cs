using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class TerrainManager : MonoBehaviour
{
    public Terrain terrain; // Reference to the terrain
    public NavMeshSurface navMeshSurface; // Reference to the NavMeshSurface for runtime updates
    public float digDepth = -0.02f; // How deep the dent is
    public float digRadius = 2f; // The radius of the digging area
    public float navMeshUpdateDelay = 0.6f; // Delay for NavMesh updates after dragging stops

    private bool isDragging = false; // Whether the user is currently dragging
    private float lastNavMeshUpdateTime; // Tracks the last time the NavMesh was updated
    private bool pendingNavMeshUpdate = false; // Tracks if a NavMesh update is pending

    void Update()
    {
        // Start dragging when the left mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }

        // Stop dragging when the left mouse button is released
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            // Mark NavMesh update as pending, but delay the update
            pendingNavMeshUpdate = true;
            lastNavMeshUpdateTime = Time.time;
        }

        // If dragging, modify the terrain under the mouse
        if (isDragging)
        {
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //if (Physics.Raycast(ray, out RaycastHit hit))
            //{
            //    ModifyTerrain(hit.point);
            //}
        }

        // Delayed NavMesh update to avoid excessive calls
        if (pendingNavMeshUpdate && Time.time - lastNavMeshUpdateTime > navMeshUpdateDelay)
        {
            navMeshSurface.BuildNavMesh();
            pendingNavMeshUpdate = false; // Reset the pending flag
        }
    }

    void ModifyTerrain(Vector3 worldPosition)
    {
        // Get terrain data
        TerrainData terrainData = terrain.terrainData;
        Vector3 terrainPosition = terrain.transform.position;

        // Convert world position to terrain local position
        int x = Mathf.FloorToInt((worldPosition.x - terrainPosition.x) / terrainData.size.x * terrainData.heightmapResolution);
        int z = Mathf.FloorToInt((worldPosition.z - terrainPosition.z) / terrainData.size.z * terrainData.heightmapResolution);

        // Get the current heights in a radius
        int radius = Mathf.CeilToInt(digRadius / terrainData.size.x * terrainData.heightmapResolution);
        float[,] heights = terrainData.GetHeights(x - radius, z - radius, radius * 2, radius * 2);

        // Lower the terrain within the radius
        for (int i = 0; i < heights.GetLength(0); i++)
        {
            for (int j = 0; j < heights.GetLength(1); j++)
            {
                heights[i, j] += digDepth / terrainData.size.y;
                heights[i, j] = Mathf.Clamp01(heights[i, j]); // Clamp the height values between 0 and 1
            }
        }

        // Apply the modified heights back to the terrain
        terrainData.SetHeights(x - radius, z - radius, heights);
    }
}
