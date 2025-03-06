using UnityEngine;
using Unity.AI.Navigation; // Required for NavMesh
using System.Linq;
using System.Collections;
public class KinectDepthTerrain : MonoBehaviour
{
    [SerializeField] public MultiSourceManager multiSourceManager;
    private ushort[] rawDepthData;
    public Vector2 terrainRotation;
    private readonly Vector2Int depthResolution = new Vector2Int(512, 424);
    private const ushort minDepth = 900;
    private const ushort maxDepth = 1250;
    bool mirrorDepth = true;
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
    private float updateTimer = 0f;
    private float updateInterval = 0.2f; // Update terrain every 0.2 seconds


    private void Start()
    {
        string dllPath = System.IO.Path.Combine(Application.dataPath, "Plugins/KinectUnityAddin.dll");
        if (System.IO.File.Exists(dllPath))
        {
            Debug.Log("✅ Kinect DLL Found in Build: " + dllPath);
        }
        else
        {
            Debug.LogWarning("❌ Kinect DLL MISSING! Ensure it is in the Plugins folder.");
        }

        if (terrain == null)
        {
            Debug.LogError("❌ Terrain component not assigned.");
            return;
        }

        SyncTerrainColliderWithTerrain();

        // 🔹 Ensure terrain layers are assigned
        if (terrain.terrainData.terrainLayers == null || terrain.terrainData.terrainLayers.Length == 0)
        {
            Debug.LogWarning("⚠ No terrain layers found. Assigning default layers.");
            terrain.terrainData.terrainLayers = new TerrainLayer[] { lowLandsLayer, plainsLayer, rocksLayer };
        }

        // 🔹 Adjust only required terrain properties (instead of resetting the whole TerrainData)
        terrain.terrainData.heightmapResolution = depthResolution.x;
        terrain.terrainData.size = new Vector3(depthResolution.x, 50, depthResolution.y);

        // 🔹 Load saved terrain if available, otherwise start Kinect updates
        if (PlayerPrefs.HasKey("SavedHeightmap"))
        {
            Debug.Log("📌 Saved heightmap found. Loading...");
            LoadSavedTerrain();
            isCalibrationRunning = false; // Stop Kinect updates
        }
        else
        {
            Debug.Log("📌 No saved heightmap. Starting Kinect updates.");
            isCalibrationRunning = true;
        }
    }


    public void EnableLiveKinectTerrain()
    {
        isCalibrationRunning = true; // Enable real-time Kinect updates
        Debug.Log("🔹 Kinect depth terrain updates enabled.");
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
        //if (!isCalibrationRunning) return;
        updateTimer += Time.deltaTime;
        if (updateTimer < updateInterval) return; // Prevent excessive updates
        updateTimer = 0f;
        if (multiSourceManager == null)
        {
            Debug.LogError("❌ KinectDepthTerrain: MultiSourceManager is NULL! Cannot get Kinect data.");
            return;
        }

        rawDepthData = multiSourceManager.GetDepthData();
        if (rawDepthData == null || rawDepthData.Length == 0)
        {
            Debug.LogError("❌ Kinect depth data is NULL! Skipping terrain update.");
            return;
        }
        // ✅ DEBUG: Check if depth data changes
        Debug.Log($"🔄 KinectDepthTerrain: Updating terrain at {Time.time} with {rawDepthData.Length} depth points.");

        GenerateTerrainFromDepthData();
        ApplyTexturesBasedOnHeight();
    }


    private IEnumerator RetryKinectConnection()
    {
        int attempts = 0;
        while (attempts < 5) // Try 5 times
        {
            yield return new WaitForSeconds(10f); // Wait before retrying

            rawDepthData = multiSourceManager.GetDepthData();
            if (rawDepthData != null && rawDepthData.Length > 0)
            {
                Debug.Log("✅ Kinect reconnected successfully.");
                yield break;
            }

            attempts++;
            Debug.LogWarning($"⚠️ Kinect reconnect attempt {attempts}/5 failed.");
        }

        Debug.LogError("❌ Kinect failed to initialize after multiple attempts.");
    }

    public void GenerateTerrainFromDepthData()
    {
        int width = depthResolution.x;
        int height = depthResolution.y;
        float[,] heightMap = new float[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                ushort depth = rawDepthData[index];

                // If depth is 0 (missing data), get an estimated value from neighbors
                if (depth == 0)
                {
                    depth = GetValidDepth(x, y, rawDepthData);
                }

                // Convert depth to height (normalize & scale)
                float normalizedDepth = Mathf.InverseLerp(minDepth, maxDepth, depth);
                heightMap[y, x] = (1f - normalizedDepth) * terrainDepthMultiplier; // Invert depth to height mapping
            }
        }

        terrain.terrainData.SetHeights(0, 0, heightMap);
        SyncTerrainColliderWithTerrain();
    }




    private ushort GetValidDepth(int x, int y, ushort[] depthData)
    {
        int index = y * depthResolution.x + x;
        if (depthData[index] != 0) return depthData[index];

        // Try getting a valid depth from neighboring pixels
        if (x > 0 && depthData[index - 1] != 0) return depthData[index - 1];
        if (y > 0 && depthData[index - depthResolution.x] != 0) return depthData[index - depthResolution.x];

        return minDepth; // Default fallback
    }



    public void ApplyTexturesBasedOnHeight()
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
                // Normalize coordinates
                float normalizedX = (float)x / alphamapWidth;
                float normalizedY = (float)y / alphamapHeight;

                // Get height from terrain heightmap
                float height = terrain.terrainData.GetHeight(
                    Mathf.RoundToInt(normalizedY * terrain.terrainData.heightmapResolution),
                    Mathf.RoundToInt(normalizedX * terrain.terrainData.heightmapResolution)
                );

                height /= terrain.terrainData.size.y; // Normalize height

                // Reset texture blending
                splatMap[x, y, 0] = 0f;
                splatMap[x, y, 1] = 0f;
                splatMap[x, y, 2] = 0f;

                // Assign textures based on height thresholds
                if (height < lowlandsThreshold)
                {
                    splatMap[x, y, 0] = 1f; // Lowlands (Sand, Grass, etc.)
                }
                else if (height < plainsThreshold)
                {
                    splatMap[x, y, 1] = 1f; // Plains (Dirt, Grass)
                }
                else
                {
                    splatMap[x, y, 2] = 1f; // Rocks (Mountains, Cliffs)
                }
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, splatMap);
        Debug.Log("✅ Terrain textures applied successfully.");
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
            //navMeshSurface.BuildNavMesh();
        }
        else
        {
            Debug.LogError("No saved heightmap found!");
        }
    }

    public void DebugKinect()
    {
        Debug.Log("🔍 Checking Kinect connection...");

        if (multiSourceManager == null)
        {
            Debug.LogError("❌ MultiSourceManager not assigned!");
            return;
        }

        if (multiSourceManager.GetDepthData() == null)
        {
            Debug.LogError("❌ Kinect depth data is NULL! Kinect might not be detected.");
        }
        else
        {
            Debug.Log("✅ Kinect is detected and providing depth data.");
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
