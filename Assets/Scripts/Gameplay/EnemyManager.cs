using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform castle;
    public float spawnInterval = 2f;

    private List<Vector3> spawnPoints = new List<Vector3>();
    private bool spawningEnemies = false;
    public GridManager grid;
    public Pathfinding pathfinding;

    void Start()
    {
        //grid = FindObjectOfType<GridManager>();
        //pathfinding = FindObjectOfType<Pathfinding>();
        if (grid == null)
        {
            Debug.LogError("❌ EnemyManager: GridManager is missing! Pathfinding won't work.");
        }
        if (pathfinding == null)
        {
            Debug.LogError("❌ EnemyManager: PathFinding is missing! Pathfinding won't work.");
        }
    }

    public void InitializeSpawnPoints()
    {
        PresetManager presetManager = FindObjectOfType<PresetManager>();
        spawnPoints = presetManager.GetEnemySpawnPositions();

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("EnemyManager: No valid enemy spawn points found in preset!");
            return;
        }

        List<Vector3> validSpawnPoints = new List<Vector3>();

        foreach (Vector3 spawnPoint in spawnPoints)
        {
            // Adjust height to match terrain
            float y = Terrain.activeTerrain.SampleHeight(spawnPoint) + 1f;
            Vector3 adjustedSpawn = new Vector3(spawnPoint.x, y, spawnPoint.z);

            // Ensure the spawn point is within the grid bounds
            Node node = grid.GetNodeFromWorldPosition(adjustedSpawn);
            if (node != null && node.walkable)
            {
                validSpawnPoints.Add(node.worldPosition);
                Debug.Log($"✅ Enemy spawn point adjusted to: {node.worldPosition}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Enemy spawn point {adjustedSpawn} is not walkable! Skipping...");
            }
        }

        // Replace old spawn points with valid ones
        spawnPoints = validSpawnPoints;
    }

    public void StartSpawningEnemiesContinuously()
    {
        if (spawnPoints.Count == 0)
        {
            Debug.LogError("EnemyManager: Cannot start spawning. No spawn points available.");
            return;
        }

        spawningEnemies = true;
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        while (spawningEnemies)
        {
            Vector3 spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            // Ensure spawn point is valid
            Node node = grid.GetNodeFromWorldPosition(spawnPoint);
            if (node == null || !node.walkable)
            {
                Debug.LogWarning($"⚠️ Enemy spawn point {spawnPoint} is not walkable! Skipping...");
                yield return null;
                continue;
            }

            GameObject enemy = Instantiate(enemyPrefab, node.worldPosition, Quaternion.identity);
            Enemy enemyScript = enemy.GetComponent<Enemy>();

            if (enemyScript != null && castle != null)
            {
                enemyScript.SetTarget(castle);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public void SetCastleTarget(Transform castleTransform)
    {
        castle = castleTransform;
    }

    /// <summary>
    /// / clear active enemies
    /// </summary>
    public void StopSpawning()
    {
        spawningEnemies = false;
    }
}
