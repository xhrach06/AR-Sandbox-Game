using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform castle;
    public float spawnInterval = 2f;

    private List<Vector3> spawnPoints = new List<Vector3>();
    private bool spawningEnemies = false;

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

            // Check if it's on the NavMesh
            if (NavMesh.SamplePosition(adjustedSpawn, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                validSpawnPoints.Add(hit.position);
                Debug.Log($"✅ Enemy spawn point adjusted to: {hit.position}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Enemy spawn point {adjustedSpawn} is not on the NavMesh! Skipping...");
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

            // Ensure spawn point is valid on the NavMesh
            if (!NavMesh.SamplePosition(spawnPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                Debug.LogWarning($"⚠️ Enemy spawn point {spawnPoint} is not on the NavMesh! Skipping...");
                yield return null;
                continue;
            }

            GameObject enemy = Instantiate(enemyPrefab, hit.position, Quaternion.identity);
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

            if (agent != null && castle != null)
            {
                agent.Warp(hit.position);
                agent.SetDestination(castle.position);
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

/*
private IEnumerator InitializeSpawnPoints()
{
    const int numberOfSpawnPoints = 3;
    const int maxAttempts = 20;
    Bounds terrainBounds = GameManager.Instance.GetTerrainBounds();

    for (int i = 0; i < numberOfSpawnPoints; i++)
    {
        bool pointInitialized = false;

        for (int attempts = 0; attempts < maxAttempts; attempts++)
        {
            float x = Random.Range(terrainBounds.min.x, terrainBounds.max.x);
            float z = Random.Range(terrainBounds.min.z, terrainBounds.max.z);
            float y = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z)) + 1f;

            Vector3 randomPosition = new Vector3(x, y, z);
            Debug.Log($"Enemy Spawn Attempt {attempts + 1}: Trying position {randomPosition}");

            if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 25f, NavMesh.AllAreas))
            {
                spawnPoints.Add(hit.position); // Add valid spawn point
                Debug.Log($"Enemy spawn point {i} initialized at: {hit.position}");
                pointInitialized = true;
                break;
            }
            else
            {
                Debug.LogWarning($"Enemy spawn validation failed at: {randomPosition}");
            }
        }

        if (!pointInitialized)
        {
            Debug.LogError($"Failed to initialize spawn point {i}! Adding fallback point.");
            spawnPoints.Add(Vector3.zero); // Add a fallback invalid point
        }

        yield return null; // Allow other processes to run while initializing
    }

    spawnPointsInitialized = true; // Mark spawn points as ready
    Debug.Log("EnemyManager: All spawn points initialized.");
}

public void StartSpawningEnemiesContinuously()
{
    if (!spawnPointsInitialized)
    {
        Debug.LogWarning("EnemyManager: Spawn points are not initialized yet. Delaying spawning.");
        StartCoroutine(WaitForSpawnPointsThenSpawn());
        return;
    }

    spawningEnemies = true;
    StartCoroutine(SpawnEnemies());
}
*/
