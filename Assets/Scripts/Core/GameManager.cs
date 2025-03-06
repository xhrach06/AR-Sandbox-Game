using System.Collections;
using System.Collections.Generic;
// using Unity.AI.Navigation; // NAVMESH REMOVED
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public CastleManager castleManager; // Reference to the CastleManager
    public TowerManager towerManager;   // Reference to the TowerManager
    public EnemyManager enemyManager;   // Reference to the EnemyManager
    public Terrain terrain;
    public TerrainPainter terrainPainter;

    public int numberOfTowers = 3; // Number of towers to place
    public float gameDuration = 120f; // Duration of the game in seconds

    private Bounds cameraBounds;
    public static GameManager Instance { get; private set; }

    private float timer; // Timer to track game duration
    private bool gameRunning = false; // Whether the game is running
    private PresetManager presetManager;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        terrain = FindObjectOfType<Terrain>(); // Auto-assign terrain if not set
        if (terrain == null)
        {
            Debug.LogError("❌ GameManager: No Terrain found in the scene! Assign it in the Inspector.");
            return;
        }
        timer = gameDuration;
        presetManager = FindObjectOfType<PresetManager>();

        // 🔹 Load terrain saved from Calibration
        StartCoroutine(LoadSavedTerrainAndInitializeGame());
    }

    void Update()
    {
        if (!gameRunning) return;

        // Decrease the timer
        timer -= Time.deltaTime;

        // Check if time has run out
        if (timer <= 0)
        {
            EndGame("You survived! The castle held out for 2 minutes.");
        }

        // Check if the castle is destroyed
        if (castleManager.GetCastleTransform() == null)
        {
            EndGame("Game Over! The castle was destroyed.");
        }
    }

    private IEnumerator LoadSavedTerrainAndInitializeGame()
    {
        if (PlayerPrefs.HasKey("SavedHeightmap"))
        {
            string heightmapJson = PlayerPrefs.GetString("SavedHeightmap");
            HeightmapData savedData = JsonUtility.FromJson<HeightmapData>(heightmapJson);

            TerrainData terrainData = terrain.terrainData;
            terrainData.SetHeights(0, 0, savedData.To2DArray());

            Debug.Log("✅ Loaded saved terrain heightmap.");
        }
        else
        {
            Debug.LogWarning("❌ No saved heightmap found! Using default terrain.");
        }

        // 🔹 Load preset from Calibration
        string preset = PlayerPrefs.GetString("SelectedPreset", "Preset1");
        Debug.Log($"Loading {preset}...");
        presetManager.SelectPreset(preset); // LoadPreset

        yield return new WaitForSeconds(1f); // Ensure preset loads before placing objects

        StartCoroutine(DelayedGameInitialization());
    }

    private IEnumerator DelayedGameInitialization()
    {
        Debug.Log("⏳ Waiting for terrain to fully load...");

        yield return new WaitForSeconds(1f); // Ensure terrain height is updated

        Debug.Log("🏰 Placing castle and towers...");
        castleManager.PlaceCastle();
        towerManager.PlaceTowers();
        GridManager gridManager = FindObjectOfType<GridManager>();
        if (gridManager != null)
        {
            Debug.Log("🔄 Re-generating grid after placing towers...");
            gridManager.GenerateGrid();
        }
        else
        {
            Debug.LogError("❌ GridManager not found! Pathfinding may not work correctly.");
        }

        // ✅ FIX: Assign castle to enemy manager!
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

        yield return new WaitForSeconds(1f); // Ensure all objects are placed before spawning

        Debug.Log("🚀 Starting enemy spawning...");
        enemyManager.StartSpawningEnemiesContinuously();

        gameRunning = true;
    }

    void EndGame(string message)
    {
        gameRunning = false;
        Debug.Log(message);

        // 🔹 Revert terrain if applicable
        if (terrainPainter != null)
        {
            Debug.Log("Reverting terrain at the end of the game.");
            terrainPainter.RevertTerrain();
        }

        // Stop enemy spawning
        enemyManager.StopSpawning();

        Time.timeScale = 0; // Pause the game
    }

    void OnApplicationQuit()
    {
        if (terrainPainter != null)
        {
            Debug.Log("Reverting terrain on application quit.");
            terrainPainter.RevertTerrain();
        }
    }
}
