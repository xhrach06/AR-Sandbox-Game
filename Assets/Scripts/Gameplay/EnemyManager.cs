using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages enemy spawning, tracking, and death handling.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public GameObject enemyPrefab;
    public Transform castle;
    public float spawnInterval = 2f;

    public GridManager grid;
    public Pathfinding pathfinding;

    private List<Vector3> spawnPoints = new();
    private List<Enemy> activeEnemies = new();
    private bool spawningEnemies = false;
    private int enemiesDefeated = 0;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (grid == null)
            return;

        if (pathfinding == null)
            return;

        HudManager hud = FindObjectOfType<HudManager>();
        if (hud != null)
        {
            hud.SetKillCounter(0);
            hud.SetEnemyCounter(0);
        }
    }

    public void InitializeSpawnPoints()
    {
        PresetManager presetManager = FindObjectOfType<PresetManager>();
        spawnPoints = presetManager.GetEnemySpawnPositions();

        if (spawnPoints.Count == 0)
            return;

        List<Vector3> validSpawnPoints = new();

        foreach (Vector3 spawnPoint in spawnPoints)
        {
            float y = Terrain.activeTerrain.SampleHeight(spawnPoint) + 1f;
            Vector3 adjustedSpawn = new(spawnPoint.x, y, spawnPoint.z);

            Node node = grid.GetNodeFromWorldPosition(adjustedSpawn);
            if (node != null && node.walkable)
            {
                validSpawnPoints.Add(node.worldPosition);
                //Debug..Log($"✅ Enemy spawn point adjusted to: {node.worldPosition}");
            }
            else
            {
                //Debug..LogWarning($"⚠️ Enemy spawn point {adjustedSpawn} is not walkable! Skipping...");
            }
        }

        spawnPoints = validSpawnPoints;
    }

    public void StartSpawningEnemiesContinuously()
    {
        if (spawnPoints.Count == 0)
        {
            //Debug..LogError("❌ EnemyManager: Cannot spawn enemies. No spawn points available.");
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

            Node node = grid.GetNodeFromWorldPosition(spawnPoint);
            if (node == null || !node.walkable)
            {
                //Debug..LogWarning($"⚠️ Enemy spawn point {spawnPoint} is not walkable! Skipping...");
                yield return null;
                continue;
            }

            GameObject enemy = Instantiate(enemyPrefab, node.worldPosition, Quaternion.identity);
            Enemy enemyScript = enemy.GetComponent<Enemy>();

            if (enemyScript != null && castle != null)
            {
                enemyScript.SetTarget(castle);
                activeEnemies.Add(enemyScript);
                enemyScript.OnEnemyDeath += HandleEnemyDeath;

                HudManager hud = FindObjectOfType<HudManager>();
                hud?.SetKillCounter(activeEnemies.Count);
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    public List<Enemy> GetAllEnemies()
    {
        return activeEnemies;
    }

    public void SetCastleTarget(Transform castleTransform)
    {
        castle = castleTransform;
    }

    public void StopSpawning()
    {
        spawningEnemies = false;
        activeEnemies.Clear();
    }

    public void HandleEnemyDeath(Enemy deadEnemy)
    {
        activeEnemies.Remove(deadEnemy);
        enemiesDefeated++;

        HudManager hud = FindObjectOfType<HudManager>();
        if (hud != null)
        {
            hud.SetKillCounter(activeEnemies.Count);
            hud.SetEnemyCounter(enemiesDefeated);
        }
    }
}
