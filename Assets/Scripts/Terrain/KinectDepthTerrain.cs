using UnityEngine;
using Unity.AI.Navigation; // Required for NavMesh
using System.Linq;

public class KinectDepthTerrain : MonoBehaviour
{
    [SerializeField] public MultiSourceManager multiSourceManager;
    private ushort[] rawDepthData;
    public Vector2 terrainRotation;
    private readonly Vector2Int depthResolution = new Vector2Int(512, 424);
    private const ushort minDepth = 900;
    private const ushort maxDepth = 1250;

    public float lowlandsThreshold = 0.33f; // Default value
    public float plainsThreshold = 0.66f;  // Default value

    public Terrain terrain;
    public NavMeshSurface navMeshSurface; // Reference to NavMesh Surface
    public float terrainDepthMultiplier = 0.1f;

    // Terrain layers for texture mapping
    public TerrainLayer lowLandsLayer;
    public TerrainLayer plainsLayer;
    public TerrainLayer rocksLayer;

    public bool isCalibrationRunning = true; // Continually update terrain during calibration

    private void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Terrain component not assigned.");
            return;
        }

        SyncTerrainColliderWithTerrain();

        // Ensure terrain layers are assigned
        if (terrain.terrainData.alphamapLayers == 0)
        {
            Debug.LogError("No terrain layers assigned. Assigning default layers.");
            terrain.terrainData.terrainLayers = new TerrainLayer[] { lowLandsLayer, plainsLayer, rocksLayer };
        }

        // Initialize terrain size and resolution
        terrain.terrainData = new TerrainData
        {
            size = new Vector3(depthResolution.x, 50, depthResolution.y),
            heightmapResolution = depthResolution.x
        };
        terrain.terrainData.size = new Vector3(depthResolution.x, 50, depthResolution.y);

        // Reassign layers explicitly
        terrain.terrainData.terrainLayers = new TerrainLayer[] { lowLandsLayer, plainsLayer, rocksLayer };

        // Check for saved terrain
        if (PlayerPrefs.HasKey("SavedHeightmap"))
        {
            Debug.Log("Saved heightmap found. Loading...");
            LoadSavedTerrain();
            isCalibrationRunning = false; // Stop Kinect updates
        }
        else
        {
            Debug.Log("No saved heightmap. Starting Kinect updates.");
            isCalibrationRunning = true;
        }
    }

    public void SyncTerrainColliderWithTerrain()
    {
        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
        if (terrainCollider != null && terrainCollider.terrainData != terrain.terrainData)
        {
            terrainCollider.terrainData = terrain.terrainData;
            Debug.Log("TerrainCollider synced with TerrainData.");
        }
        else
        {
            Debug.Log("TerrainCollider already synced with TerrainData.");
        }
    }

    private void Update()
    {
        if (!isCalibrationRunning) return;

        // If calibration is running, fetch Kinect data
        rawDepthData = multiSourceManager.GetDepthData();
        if (rawDepthData != null && rawDepthData.Length > 0)
        {
            GenerateTerrainFromDepthData();
            ApplyTexturesBasedOnHeight();
        }
    }

    private void GenerateTerrainFromDepthData()
    {
        float[,] heightMap = new float[depthResolution.y, depthResolution.x];

        for (int y = 0; y < depthResolution.y; y++)
        {
            for (int x = 0; x < depthResolution.x; x++)
            {
                int index = y * depthResolution.x + x;
                ushort depth = rawDepthData[index];

                if (depth == 0)
                {
                    depth = GetValidDepth(x, y, rawDepthData);
                }

                if (depth >= minDepth && depth <= maxDepth)
                {
                    float normalizedDepth = (float)(depth - minDepth) / (maxDepth - minDepth);
                    heightMap[y, x] = normalizedDepth * terrainDepthMultiplier;
                }
                else
                {
                    heightMap[y, x] = 0f;
                }
            }
        }
        terrain.terrainData.SetHeights(0, 0, heightMap);
    }

    private ushort GetValidDepth(int x, int y, ushort[] depthData)
    {
        if (x > 0) return depthData[y * depthResolution.x + x - 1];
        if (y > 0) return depthData[(y - 1) * depthResolution.x + x];
        return minDepth;
    }

    private void ApplyTexturesBasedOnHeight()
    {
        if (terrain.terrainData.alphamapLayers == 0)
        {
            Debug.LogError("No terrain layers are assigned in the TerrainData! Please assign layers to the terrain.");
            return;
        }

        int alphamapWidth = terrain.terrainData.alphamapWidth;
        int alphamapHeight = terrain.terrainData.alphamapHeight;
        float[,,] splatMap = new float[alphamapWidth, alphamapHeight, 3];

        for (int y = 0; y < alphamapHeight; y++)
        {
            for (int x = 0; x < alphamapWidth; x++)
            {
                float normalizedX = (float)x / alphamapWidth;
                float normalizedY = (float)y / alphamapHeight;

                float height = terrain.terrainData.GetHeight(
                    Mathf.RoundToInt((1 - normalizedY) * terrain.terrainData.heightmapResolution),
                    Mathf.RoundToInt(normalizedX * terrain.terrainData.heightmapResolution)
                );

                height /= terrain.terrainData.size.y;

                if (height > plainsThreshold)
                {
                    splatMap[x, y, 0] = 1f; // Low-lands
                    splatMap[x, y, 1] = 0f;
                    splatMap[x, y, 2] = 0f;
                }
                else if (height > lowlandsThreshold)
                {
                    splatMap[x, y, 0] = 0f;
                    splatMap[x, y, 1] = 1f; // Plains
                    splatMap[x, y, 2] = 0f;
                }
                else
                {
                    splatMap[x, y, 0] = 0f;
                    splatMap[x, y, 1] = 0f;
                    splatMap[x, y, 2] = 1f; // Rocks
                }
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, splatMap);
    }

    public void SaveTerrain()
    {
        // Save terrain to PlayerPrefs
        HeightmapData heightmapData = new HeightmapData(terrain.terrainData.GetHeights(0, 0, depthResolution.x, depthResolution.y));
        string heightmapJson = JsonUtility.ToJson(heightmapData);
        PlayerPrefs.SetString("SavedHeightmap", heightmapJson);
        PlayerPrefs.Save();

        isCalibrationRunning = false;

        SmoothTerrain(3, 0.6f);
        navMeshSurface.BuildNavMesh();
        Debug.Log("Terrain saved and NavMesh updated.");
    }

    private void LoadSavedTerrain()
    {
        Debug.Log("Attempting to load saved terrain...");
        string heightmapJson = PlayerPrefs.GetString("SavedHeightmap", "");
        if (!string.IsNullOrEmpty(heightmapJson))
        {
            HeightmapData savedHeightmap = JsonUtility.FromJson<HeightmapData>(heightmapJson);
            terrain.terrainData.SetHeights(0, 0, savedHeightmap.To2DArray());
            SyncTerrainColliderWithTerrain();
            ApplyTexturesBasedOnHeight();
            SmoothTerrain(3, 0.6f);
            navMeshSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogError("No saved heightmap found!");
        }
    }

    private void SmoothTerrain(int iterations = 2, float strength = 0.5f)
    {
        float[,] heightmap = terrain.terrainData.GetHeights(0, 0, depthResolution.x, depthResolution.y);

        for (int iter = 0; iter < iterations; iter++)
        {
            for (int y = 1; y < depthResolution.y - 1; y++)
            {
                for (int x = 1; x < depthResolution.x - 1; x++)
                {
                    float averageHeight = (
                        heightmap[y - 1, x] +
                        heightmap[y + 1, x] +
                        heightmap[y, x - 1] +
                        heightmap[y, x + 1]
                    ) / 4f;

                    heightmap[y, x] = Mathf.Lerp(heightmap[y, x], averageHeight, strength);
                }
            }
        }

        terrain.terrainData.SetHeights(0, 0, heightmap);
        Debug.Log("Terrain smoothing applied.");
    }
}
