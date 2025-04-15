using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Generates and updates terrain based on Kinect depth input,
/// including heightmap and texture blending.
/// </summary>
public class KinectDepthTerrain : MonoBehaviour
{
    [SerializeField] public MultiSourceManager multiSourceManager;

    [Header("References")]
    public Terrain terrain;

    [Header("Depth Configuration")]
    public float terrainDepthMultiplier = 0.1f;
    public Vector2 terrainRotation;
    public readonly Vector2Int depthResolution = new Vector2Int(512, 424);
    public ushort minDepth = 900;
    public ushort maxDepth = 1250;


    [Header("Texture Thresholds")]
    public float lowlandsThreshold = 0.33f;
    public float plainsThreshold = 0.66f;

    [Header("Terrain Layers")]
    public TerrainLayer lowLandsLayer, plainsLayer, rocksLayer;

    private ushort[] rawDepthData;
    private ushort[] previousDepthSnapshot;
    private float[,] heightMapCache;
    private float[,,] splatMap;

    private float textureUpdateCooldown = 2f;
    private float lastTextureUpdateTime = 0f;

    /// <summary>
    /// Initializes the terrain and heightmap configuration.
    /// </summary>
    private void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("❌ Terrain not assigned.");
            return;
        }

        heightMapCache = new float[depthResolution.y, depthResolution.x];
        terrain.terrainData.heightmapResolution = depthResolution.x;
        terrain.terrainData.size = new Vector3(depthResolution.x, 50, depthResolution.y);

        // Ensure terrain layers are present
        if (terrain.terrainData.terrainLayers.Length == 0)
        {
            terrain.terrainData.terrainLayers = new TerrainLayer[]
            {
                lowLandsLayer, plainsLayer, rocksLayer
            };
        }
    }

    /// <summary>
    /// Syncs the TerrainCollider with the current TerrainData.
    /// </summary>
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

    /// <summary>
    /// Enables Kinect-driven terrain updates.
    /// </summary>
    public void EnableLiveKinectTerrain()
    {
        Debug.Log("🔹 Kinect terrain updates enabled.");
    }

    /// <summary>
    /// Checks if terrain needs updating and applies new heights and textures if so.
    /// </summary>
    /// <returns>True if terrain was updated, false otherwise.</returns>
    public bool CheckAndUpdateTerrain()
    {
        rawDepthData = multiSourceManager?.GetDepthData();
        if (rawDepthData == null || rawDepthData.Length == 0)
        {
            Debug.LogWarning("❌ No Kinect depth data.");
            return false;
        }

        if (!DepthChanged())
            return false;

        GenerateTerrainFromDepthData();

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
    /// Detects significant changes in depth data since the last frame.
    /// </summary>
    /// <returns>True if the terrain should be updated.</returns>
    private bool DepthChanged()
    {
        if (previousDepthSnapshot == null) return true;

        int significantChanges = 0;
        int checkEvery = 1000;
        int maxChecks = 30;
        int tolerance = 5;

        for (int i = 0; i < rawDepthData.Length && significantChanges < 3 && maxChecks > 0; i += checkEvery, maxChecks--)
        {
            if (Mathf.Abs(rawDepthData[i] - previousDepthSnapshot[i]) > tolerance)
                significantChanges++;
        }

        return significantChanges >= 3;
    }

    /// <summary>
    /// Converts the depth data into a Unity terrain heightmap.
    /// </summary>
    public void GenerateTerrainFromDepthData()
    {
        int width = depthResolution.x;
        int height = depthResolution.y;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                ushort depth = rawDepthData[index];

                if (depth == 0)
                    depth = GetValidDepth(x, y, rawDepthData);

                float normalizedDepth = Mathf.InverseLerp(minDepth, maxDepth, depth);
                heightMapCache[y, x] = (1f - normalizedDepth) * terrainDepthMultiplier;
            }
        }

        terrain.terrainData.SetHeights(0, 0, heightMapCache);
    }

    /// <summary>
    /// Tries to get a non-zero depth value from neighbors if the current is invalid.
    /// </summary>
    private ushort GetValidDepth(int x, int y, ushort[] data)
    {
        int i = y * depthResolution.x + x;
        if (data[i] != 0) return data[i];

        if (x > 0 && data[i - 1] != 0) return data[i - 1];
        if (y > 0 && data[i - depthResolution.x] != 0) return data[i - depthResolution.x];

        return minDepth;
    }

    /// <summary>
    /// Updates terrain textures based on elevation using thresholds.
    /// </summary>
    public void ApplyTexturesBasedOnHeight()
    {
        int width = terrain.terrainData.alphamapWidth;
        int height = terrain.terrainData.alphamapHeight;
        int layers = terrain.terrainData.alphamapLayers;

        if (splatMap == null ||
            splatMap.GetLength(0) != width ||
            splatMap.GetLength(1) != height ||
            splatMap.GetLength(2) != layers)
        {
            splatMap = new float[width, height, layers];
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normX = (float)x / width;
                float normY = (float)y / height;

                float heightVal = terrain.terrainData.GetInterpolatedHeight(normX, normY);
                heightVal /= terrain.terrainData.size.y;

                splatMap[x, y, 0] = 0;
                splatMap[x, y, 1] = 0;
                splatMap[x, y, 2] = 0;

                if (heightVal < lowlandsThreshold)
                    splatMap[x, y, 0] = 1f;
                else if (heightVal < plainsThreshold)
                    splatMap[x, y, 1] = 1f;
                else
                    splatMap[x, y, 2] = 1f;
            }
        }

        terrain.terrainData.SetAlphamaps(0, 0, splatMap);
        Debug.Log("✅ Textures applied.");
    }

    /// <summary>
    /// Notifies all enemies to recalculate their paths after a terrain change.
    /// </summary>
    public IEnumerator NotifyEnemiesToRecalculatePaths()
    {
        List<Enemy> enemies = EnemyManager.Instance.GetAllEnemies();

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].FindNewPath();

            if (i % 5 == 0)
                yield return null;
        }

        GameManager.Instance.OnEnemyPathRecalculationComplete();
    }
}
