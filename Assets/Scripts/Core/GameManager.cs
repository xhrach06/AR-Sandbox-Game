using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        timer = gameDuration;

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

            if (terrainPainter != null)
            {
                terrainPainter.UpdateNavMesh();
                Debug.Log("🔄 NavMesh updated after loading terrain.");
            }

            // Wait to ensure terrain changes are fully applied before baking the NavMesh
            yield return new WaitForSeconds(1f);

            yield return StartCoroutine(RebakeNavMesh());
        }
        else
        {
            Debug.LogWarning("❌ No saved heightmap found! Using default terrain.");
        }

        // 🔹 Load preset from Calibration
        string preset = PlayerPrefs.GetString("SelectedPreset", "Preset1");
        Debug.Log($"Loading {preset}...");
        FindObjectOfType<PresetManager>().SelectPreset(preset); // LoadPreset

        yield return new WaitForSeconds(1f); // Ensure preset loads before placing objects

        StartCoroutine(DelayedGameInitialization());
    }

    private IEnumerator RebakeNavMesh()
    {
        yield return new WaitForSeconds(0.5f); // Small delay to ensure terrain updates fully

        NavMeshSurface navMeshSurface = FindObjectOfType<NavMeshSurface>();
        if (navMeshSurface != null)
        {
            Debug.Log("🔄 Rebaking NavMesh dynamically...");
            navMeshSurface.BuildNavMesh();
            Debug.Log("✅ NavMesh rebaked successfully.");
        }
        else
        {
            Debug.LogError("❌ No NavMeshSurface found in the scene!");
        }
    }

    private IEnumerator DelayedGameInitialization()
    {
        Debug.Log("⏳ Waiting for terrain to fully load...");

        yield return new WaitForSeconds(1f); // Ensure terrain height and NavMesh are updated

        if (terrainPainter != null)
        {
            Debug.Log("🔄 Updating NavMesh after terrain is ready...");
            terrainPainter.UpdateNavMesh();
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("🏰 Placing castle and towers...");
        castleManager.PlaceCastle();
        towerManager.PlaceTowers();

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
