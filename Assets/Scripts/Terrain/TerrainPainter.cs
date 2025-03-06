using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class TerrainPainter : MonoBehaviour
{
    public Terrain terrain; // Assign your terrain in the Inspector
    public NavMeshSurface navMeshSurface; // Assign your NavMeshSurface in the Inspector
    public GameObject navMeshObstaclePrefab; // Prefab with a NavMeshObstacle component
    public int redLayerIndex = 3; // Index of the red layer in the Terrain Layers
    public float brushSize = 5f; // Size of the brush
    public float paintStrength = 0.5f; // Strength of the painting (0-1)
    public LayerMask terrainLayerMask; // Layer mask for the terrain

    private float[,,] originalSplatmapData; // Backup of the original splatmap data
    private TerrainData terrainData; // Terrain data reference
    private bool isPainting = false; // Tracks whether the player is actively painting

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain is not assigned!");
            return;
        }
        /*
                if (navMeshSurface == null)
                {
                    Debug.LogError("NavMeshSurface is not assigned!");
                    return;
                }
        */
        terrainData = terrain.terrainData;

        // Backup the original splatmap data
        originalSplatmapData = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        if (originalSplatmapData != null)
        {
            Debug.Log("Original splatmap data successfully backed up.");
        }
        else
        {
            Debug.LogError("Failed to back up original splatmap data.");
        }
    }

    void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButton(0)) // Left mouse button
        {
            isPainting = true; // Start painting
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, terrainLayerMask))
            {
                if (hit.collider.gameObject == terrain.gameObject) // Ensure the raycast hits the terrain
                {
                    PaintTerrain(hit.point);
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            if (isPainting)
            {
                isPainting = false; // Stop painting
                UpdateNavMesh(); // Rebuild the NavMesh once painting is done
            }
        }
    }

    private void PaintTerrain(Vector3 worldPosition)
    {
        Vector3 terrainPosition = worldPosition - terrain.transform.position;

        float normalizedX = terrainPosition.x / terrainData.size.x;
        float normalizedZ = terrainPosition.z / terrainData.size.z;

        int mapX = Mathf.FloorToInt(normalizedX * terrainData.alphamapWidth);
        int mapZ = Mathf.FloorToInt(normalizedZ * terrainData.alphamapHeight);

        int brushSizeInMap = Mathf.FloorToInt(brushSize * terrainData.alphamapWidth / terrainData.size.x);

        float[,,] alphaMap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);

        for (int z = -brushSizeInMap; z <= brushSizeInMap; z++)
        {
            for (int x = -brushSizeInMap; x <= brushSizeInMap; x++)
            {
                int newX = Mathf.Clamp(mapX + x, 0, terrainData.alphamapWidth - 1);
                int newZ = Mathf.Clamp(mapZ + z, 0, terrainData.alphamapHeight - 1);

                float distance = Vector2.Distance(new Vector2(x, z), Vector2.zero) / brushSizeInMap;
                float strength = Mathf.Clamp01(1f - distance) * paintStrength;

                float totalWeight = 0f;

                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    if (i == redLayerIndex)
                    {
                        alphaMap[newZ, newX, i] += strength;
                    }
                    else
                    {
                        alphaMap[newZ, newX, i] *= (1f - strength);
                    }

                    totalWeight += alphaMap[newZ, newX, i];
                }

                for (int i = 0; i < terrainData.alphamapLayers; i++)
                {
                    alphaMap[newZ, newX, i] /= totalWeight;
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, alphaMap);

        // Spawn NavMeshObstacle for the painted area
        SpawnNavMeshObstacle(worldPosition);
    }

    private void SpawnNavMeshObstacle(Vector3 worldPosition)
    {
        if (navMeshObstaclePrefab == null)
        {
            Debug.LogError("NavMeshObstaclePrefab is not assigned!");
            return;
        }

        GameObject obstacle = Instantiate(navMeshObstaclePrefab, worldPosition, Quaternion.identity);
        NavMeshObstacle navObstacle = obstacle.GetComponent<NavMeshObstacle>();
        if (navObstacle != null)
        {
            navObstacle.size = new Vector3(brushSize * 2, 20f, brushSize * 2);
            navObstacle.center = Vector3.zero;
            navObstacle.carving = true; // Enable carving to dynamically affect the NavMesh
            //Debug.Log($"Spawned NavMeshObstacle at {worldPosition} with size {navObstacle.size}");
        }
        else
        {
            Debug.LogError("Spawned obstacle is missing NavMeshObstacle component.");
        }
    }

    public void UpdateNavMesh()
    {
        Debug.Log("Updating NavMesh...");
        navMeshSurface.BuildNavMesh();
        Debug.Log("NavMesh updated.");
    }

    public void RevertTerrain()
    {
        if (terrain != null && originalSplatmapData != null)
        {
            terrainData.SetAlphamaps(0, 0, originalSplatmapData);
            Debug.Log("Terrain changes reverted to the original state.");
        }
        else
        {
            Debug.LogWarning("Original splatmap data is not available to revert. Ensure it is backed up properly.");
        }

        // Destroy all NavMeshObstacles created by painting
        foreach (var obstacle in FindObjectsOfType<NavMeshObstacle>())
        {
            Destroy(obstacle.gameObject);
        }
        Debug.Log("All NavMeshObstacles removed.");
    }
}
