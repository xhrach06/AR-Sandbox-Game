using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public CastleManager castleManager;
    public TowerManager towerManager;
    public EnemyManager enemyManager;
    public SpellManager spellManager;
    public Terrain terrain;
    public Canvas healthBarCanvas;
    public TerrainPainter terrainPainter;
    public KinectDepthTerrain kinectDepthTerrain; // ✅ Added Kinect terrain reference
    public GridManager gridManager;

    public int numberOfTowers = 3;
    public float gameDuration = 120f;

    public float timer;
    private bool gameRunning = false;
    private PresetManager presetManager;

    public float terrainUpdateInterval = 1f; // ✅ Update interval for live terrain and paths
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        terrain = FindObjectOfType<Terrain>();
        gridManager = FindObjectOfType<GridManager>();
        kinectDepthTerrain = FindObjectOfType<KinectDepthTerrain>();
        spellManager = FindObjectOfType<SpellManager>();

        if (terrain == null || kinectDepthTerrain == null)
        {
            Debug.LogError("❌ GameManager: Terrain or KinectDepthTerrain not found!");
            return;
        }

        timer = gameDuration;
        presetManager = FindObjectOfType<PresetManager>();

        // ✅ ENABLE LIVE KINECT TERRAIN UPDATES
        kinectDepthTerrain.EnableLiveKinectTerrain();

        // ✅ Start periodic updates of terrain & paths
        StartCoroutine(UpdateLiveTerrain());

        StartCoroutine(DelayedGameInitialization());
    }

    void Update()
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

    private IEnumerator DelayedGameInitialization()
    {
        Debug.Log("⏳ Waiting for terrain to fully load...");
        yield return new WaitForSeconds(1f);

        Debug.Log("🏰 Placing castle and towers...");
        castleManager.PlaceCastle();
        towerManager.PlaceTowers();

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

    private IEnumerator UpdateLiveTerrain()
    {
        while (true)
        {
            yield return new WaitForSeconds(terrainUpdateInterval);

            if (kinectDepthTerrain != null)
            {
                Debug.Log("🔄 Updating live terrain from Kinect...");
                if (kinectDepthTerrain.CheckAndUpdateTerrain())
                {
                    if (gridManager != null)
                    {
                        Debug.Log("🔄 Re-generating pathfinding grid...");
                        gridManager.GenerateGrid();
                    }
                }
            }
        }
    }

    public void EndGame(string message)
    {
        gameRunning = false;
        Debug.Log(message);
        HudManager hudManager = FindObjectOfType<HudManager>();
        hudManager.SetGameOverText(message);
        if (terrainPainter != null)
        {
            Debug.Log("Reverting terrain at the end of the game.");
            terrainPainter.RevertTerrain();
        }

        enemyManager.StopSpawning();
        Time.timeScale = 0;
    }

    void OnApplicationQuit()
    {
        if (terrainPainter != null)
        {
            Debug.Log("Reverting terrain on application quit.");
            terrainPainter.RevertTerrain();
        }
    }
    void OnDrawGizmos()
    {
        if (Camera.main != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(Camera.main.transform.position, -Camera.main.transform.up * 10f);
        }
    }

}
