using UnityEngine;
using System.Collections;

/// <summary>
/// Dynamically generates and updates Unity Terrain based on Kinect depth data.
/// Includes heightmap transformation and texture mapping.
/// </summary>
public class KinectDepthTerrain : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public MultiSourceManager multiSourceManager;
    public Terrain terrain;

    [Header("Terrain Configuration")]
    public readonly Vector2Int depthResolution = new Vector2Int(512, 424); // Kinect depth map resolution
    public float terrainDepthMultiplier = 0.1f;                            // Height scale multiplier
    public Vector2 terrainRotation;                                       // Optional rotation (unused)

    [Header("Depth Thresholds")]
    public ushort minDepth = 900; // Minimum valid depth from Kinect
    public ushort maxDepth = 1250; // Maximum valid depth from Kinect

    [Header("Texture Thresholds")]
    public float lowlandsThreshold = 0.33f; // Height % for lowlands layer
    public float plainsThreshold = 0.66f;   // Height % for plains layer

    [Header("Terrain Layers")]
    public TerrainLayer lowLandsLayer;
    public TerrainLayer plainsLayer;
    public TerrainLayer rocksLayer;

    [Header("Runtime Caches / Buffers")]
    private ushort[] rawDepthData;                     // Current frame Kinect depth
    private ushort[] previousDepthSnapshot;            // Previous frame for comparison
    private float[,] heightMapCache;                   // Smoothed or processed heightmap
    private float[,,] splatMap;                        // Alpha blend map for texture painting

    [Header("Timing")]
    private float textureUpdateCooldown = 2f;          // Time between texture updates
    private float lastTextureUpdateTime = 0f;          // Last texture paint timestamp

    [Header("Change sensitivity")]
    public int depthChangedTolerance = 20;

    /// <summary>
    /// Initializes terrain, layers, and loads saved heightmap if present.
    /// </summary>
    private void Start()
    {
        heightMapCache = new float[depthResolution.y, depthResolution.x];

        if (terrain.terrainData.terrainLayers == null || terrain.terrainData.terrainLayers.Length == 0)
        {
            //Debug.LogWarning("⚠ No terrain layers found. Assigning default layers.");
            terrain.terrainData.terrainLayers = new TerrainLayer[] {
                lowLandsLayer, plainsLayer, rocksLayer
            };
        }

        terrain.terrainData.heightmapResolution = depthResolution.x;
        terrain.terrainData.size = new Vector3(depthResolution.x, 50, depthResolution.y);
    }

    /// <summary>
    /// Syncs TerrainCollider with the current TerrainData.
    /// </summary>
    public void SyncTerrainColliderWithTerrain()
    {
        TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
        if (terrainCollider != null && terrainCollider.terrainData != terrain.terrainData)
        {
            terrainCollider.terrainData = terrain.terrainData;
            //Debug.Log("TerrainCollider synced with TerrainData.");
        }
        else
        {
            //Debug.Log("TerrainCollider already synced with TerrainData.");
        }
    }

    /// <summary>
    /// Checks for changes in depth data and updates terrain and textures if needed.
    /// </summary>
    public bool CheckAndUpdateTerrain()
    {
        if (multiSourceManager == null)
        {
            //Debug.LogError("❌ KinectDepthTerrain: MultiSourceManager is NULL! Cannot get Kinect data.");
            return false;
        }

        rawDepthData = multiSourceManager.GetDepthData();
        if (rawDepthData == null || rawDepthData.Length == 0)
        {
            //Debug.LogError("❌ Kinect depth data is NULL! Skipping terrain update.");
            return false;
        }
        SmoothRawDepthData();
        if (!DepthChanged()) return false;

        //Debug.Log($"🔄 KinectDepthTerrain: Updating terrain at {Time.time} with {rawDepthData.Length} depth points.");

        GenerateTerrainFromDepthData();
        //SmoothTerrain();

        if (Time.time - lastTextureUpdateTime > textureUpdateCooldown)
        {
            ApplyTexturesBasedOnHeight();
            lastTextureUpdateTime = Time.time;
        }

        if (previousDepthSnapshot == null || previousDepthSnapshot.Length != rawDepthData.Length)
            previousDepthSnapshot = new ushort[rawDepthData.Length];

        System.Array.Copy(rawDepthData, previousDepthSnapshot, rawDepthData.Length);

        return true;
    }

    /// <summary>
    /// Tells all enemies to recalculate their path after terrain updates.
    /// </summary>
    public IEnumerator NotifyEnemiesToRecalculatePaths()
    {
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();

        for (int i = 0; i < allEnemies.Length; i++)
        {
            if (allEnemies[i] != null)
                allEnemies[i].FindNewPath();

            if (i % 5 == 0)
                yield return null;
        }

        GameManager.Instance.OnEnemyPathRecalculationComplete();
    }

    /// <summary>
    /// Checks if current Kinect depth data differs from the previous snapshot.
    /// </summary>
    private bool DepthChanged()
    {
        if (previousDepthSnapshot == null || previousDepthSnapshot.Length != rawDepthData.Length)
            return true;

        int significantChanges = 0;
        int checkEvery = 1000;
        int maxChecks = 30;

        for (int i = 0; i < rawDepthData.Length && significantChanges < 3 && maxChecks > 0; i += checkEvery, maxChecks--)
        {
            int oldDepth = previousDepthSnapshot[i];
            int newDepth = rawDepthData[i];

            if (Mathf.Abs(newDepth - oldDepth) > depthChangedTolerance)
            {
                significantChanges++;
            }
        }

        return significantChanges >= 3;
    }

    /// <summary>
    /// Converts Kinect depth data to a terrain heightmap.
    /// </summary>
    public void GenerateTerrainFromDepthData()
    {
        int width = depthResolution.x;
        int height = depthResolution.y;
        float[,] heightMap = heightMapCache;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                ushort depth = rawDepthData[index];

                if (depth == 0)
                {
                    depth = GetValidDepth(x, y, rawDepthData);
                }

                float normalizedDepth = Mathf.InverseLerp(minDepth, maxDepth, depth);
                heightMap[y, x] = (1f - normalizedDepth) * terrainDepthMultiplier;
            }
        }

        terrain.terrainData.SetHeights(0, 0, heightMap);
    }

    /// <summary>
    /// Attempts to find a non-zero depth value from neighboring pixels.
    /// </summary>
    private ushort GetValidDepth(int x, int y, ushort[] depthData)
    {
        int index = y * depthResolution.x + x;
        if (depthData[index] != 0) return depthData[index];

        if (x > 0 && depthData[index - 1] != 0) return depthData[index - 1];
        if (y > 0 && depthData[index - depthResolution.x] != 0) return depthData[index - depthResolution.x];

        return minDepth;
    }

    /// <summary>
    /// Applies texture layers to the terrain based on elevation thresholds.
    /// </summary>
    public void ApplyTexturesBasedOnHeight()
    {
        if (terrain.terrainData.alphamapLayers == 0)
        {
            //Debug.LogError("No terrain layers are assigned in the TerrainData!");
            return;
        }

        int alphamapWidth = terrain.terrainData.alphamapWidth;
        int alphamapHeight = terrain.terrainData.alphamapHeight;
        int layerCount = terrain.terrainData.alphamapLayers;

        if (splatMap == null ||
            splatMap.GetLength(0) != alphamapWidth ||
            splatMap.GetLength(1) != alphamapHeight ||
            splatMap.GetLength(2) != layerCount)
        {
            splatMap = new float[alphamapWidth, alphamapHeight, layerCount];
        }

        for (int y = 0; y < alphamapHeight; y++)
        {
            for (int x = 0; x < alphamapWidth; x++)
            {
                float normX = (float)x / alphamapWidth;
                float normY = (float)y / alphamapHeight;

                float height = terrain.terrainData.GetHeight(
                    Mathf.RoundToInt(normY * terrain.terrainData.heightmapResolution),
                    Mathf.RoundToInt(normX * terrain.terrainData.heightmapResolution)
                );

                height /= terrain.terrainData.size.y;

                splatMap[x, y, 0] = 0;
                splatMap[x, y, 1] = 0;
                splatMap[x, y, 2] = 0;

                if (height < lowlandsThreshold)
                    splatMap[x, y, 0] = 1f;
                else if (height < plainsThreshold)
                    splatMap[x, y, 1] = 1f;
                else
                    splatMap[x, y, 2] = 1f;
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, splatMap);
        //Debug.Log("✅ Terrain textures applied successfully.");
    }


    /// <summary>
    /// Smoothes the terrain using a blur pass over the heightmap.
    /// </summary>
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
        //Debug.Log("Terrain smoothing applied.");
    }
    /// <summary>
    /// Smoothes depth data using a blur pass over them.
    /// </summary>
    /// <summary>
    /// Smoothes raw depth data using a simple blur pass.
    /// </summary>
    private void SmoothRawDepthData(float strength = 0.5f)
    {
        ushort[] smoothedDepthData = new ushort[rawDepthData.Length];

        for (int y = 1; y < depthResolution.y - 1; y++)
        {
            for (int x = 1; x < depthResolution.x - 1; x++)
            {
                int index = y * depthResolution.x + x;

                ushort center = rawDepthData[index];
                if (center == 0)
                    continue;

                int upIndex = (y - 1) * depthResolution.x + x;
                int downIndex = (y + 1) * depthResolution.x + x;
                int leftIndex = y * depthResolution.x + (x - 1);
                int rightIndex = y * depthResolution.x + (x + 1);

                ushort up = rawDepthData[upIndex];
                ushort down = rawDepthData[downIndex];
                ushort left = rawDepthData[leftIndex];
                ushort right = rawDepthData[rightIndex];

                // Use center as fallback if neighbor is invalid
                float validUp = up != 0 ? up : center;
                float validDown = down != 0 ? down : center;
                float validLeft = left != 0 ? left : center;
                float validRight = right != 0 ? right : center;

                float average = (validUp + validDown + validLeft + validRight) / 4f;
                smoothedDepthData[index] = (ushort)Mathf.Lerp(center, average, strength);
            }
        }

        // ✅ Only copy once AFTER smoothing
        smoothedDepthData.CopyTo(rawDepthData, 0);
    }

}
