using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Controls the overall game flow, including terrain updates, enemy spawning, and end game handling.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Managers & Components")]
    public CastleManager castleManager;
    public TowerManager towerManager;
    public EnemyManager enemyManager;
    public KinectDepthTerrain kinectDepthTerrain;
    public GridManager gridManager;
    public Terrain terrain;
    public Canvas healthBarCanvas;

    [Header("Gameplay Settings")]
    public int numberOfTowers = 3;
    public float gameDuration = 120f;
    public float terrainUpdateInterval = 1f;

    private Coroutine pathRecalcRoutine;
    private float timer;
    private bool gameRunning = false;
    private PresetManager presetManager;

    // -------------------- Unity Methods --------------------

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        terrain = FindObjectOfType<Terrain>();
        gridManager = FindObjectOfType<GridManager>();
        kinectDepthTerrain = FindObjectOfType<KinectDepthTerrain>();
        presetManager = FindObjectOfType<PresetManager>();

        if (terrain == null || kinectDepthTerrain == null)
        {
            Debug.LogError("❌ GameManager: Terrain or KinectDepthTerrain not found!");
            return;
        }

        timer = gameDuration;

        StartCoroutine(UpdateLiveTerrain());          // Begin terrain + path recalculation loop
        StartCoroutine(DelayedGameInitialization());  // Wait for terrain, then init gameplay
    }

    private void Update()
    {
        if (!gameRunning) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            EndGame("You survived! The castle held out for 2 minutes.");
        }

        if (castleManager.GetCastleTransform() == null)
        {
            EndGame("Game Over! The castle was destroyed.");
        }
    }

    private void OnDrawGizmos()
    {
        if (Camera.main != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Camera.main.transform.position, -Camera.main.transform.up * 10f);
        }
    }

    // -------------------- Game Initialization --------------------

    private IEnumerator DelayedGameInitialization()
    {
        Debug.Log("⏳ Waiting for terrain to fully load...");
        yield return new WaitForSeconds(1f);

        Debug.Log("🏰 Placing castle and towers...");
        castleManager.PlaceCastle();
        towerManager.PlaceTowers();

        // Pass tower positions as obstacles to grid
        List<Vector3> towerPositions = towerManager.GetTowerPositions();
        gridManager.SetObstaclePositions(towerPositions);

        if (gridManager != null)
        {
            Debug.Log("🔄 Re-generating grid after placing towers...");
            gridManager.GenerateGrid();
        }
        else
        {
            Debug.LogError("❌ GridManager not found! Pathfinding may not work correctly.");
        }

        Transform castleTransform = castleManager.GetCastleTransform();
        if (castleTransform != null)
        {
            Debug.Log($"🏰 Castle transform found: {castleTransform.position}");
            enemyManager.SetCastleTarget(castleTransform);
        }
        else
        {
            Debug.LogError("❌ Castle transform is NULL! Enemies won't have a target.");
        }

        Debug.Log("🛠 Initializing enemy spawn points...");
        enemyManager.InitializeSpawnPoints();

        yield return new WaitForSeconds(1f);

        Debug.Log("🚀 Starting enemy spawning...");
        enemyManager.StartSpawningEnemiesContinuously();

        gameRunning = true;
    }

    // -------------------- Terrain Updates --------------------

    private IEnumerator UpdateLiveTerrain()
    {
        while (true)
        {
            yield return new WaitForSeconds(terrainUpdateInterval);

            if (kinectDepthTerrain == null) continue;

            Debug.Log("🔄 Updating live terrain from Kinect...");
            if (kinectDepthTerrain.CheckAndUpdateTerrain())
            {
                if (gridManager != null)
                {
                    Debug.Log("🔄 Re-generating pathfinding grid...");
                    gridManager.GenerateGrid();
                }

                // Only run one instance of enemy path recalculation
                if (pathRecalcRoutine == null)
                {
                    pathRecalcRoutine = StartCoroutine(kinectDepthTerrain.NotifyEnemiesToRecalculatePaths());
                }
            }
        }
    }

    public void OnEnemyPathRecalculationComplete()
    {
        pathRecalcRoutine = null;
    }

    // -------------------- Game Ending --------------------

    public void EndGame(string message)
    {
        gameRunning = false;
        Debug.Log(message);

        HudManager hudManager = FindObjectOfType<HudManager>();
        if (hudManager != null)
        {
            hudManager.SetGameOverText(message);
        }

        enemyManager.StopSpawning();
        Time.timeScale = 0;
    }
}
