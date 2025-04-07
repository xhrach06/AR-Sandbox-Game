using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class Calibration : MonoBehaviour
{
    private Camera KinectTerrainCamera;
    [SerializeField] private KinectDepthTerrain kinectDepthTerrain;
    [SerializeField] private GameObject castlePrefab;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private GameObject enemySpawnPrefab;

    private List<GameObject> spawnedEntities = new List<GameObject>();

    private List<Vector3> castlePositions = new List<Vector3>();
    private List<Vector3> towerPositions = new List<Vector3>();
    private List<Vector3> enemyPositions = new List<Vector3>();

    private string selectedPreset = "Preset1"; // Default preset

    public float moveSpeed = 10f;
    public float verticalMoveSpeed = 5f;

    private PresetManager presetManager;
    private float terrainUpdateTimer = 0f;
    private const float terrainUpdateInterval = 0.5f;


    private void Start()
    {
        KinectTerrainCamera = Camera.main;
        presetManager = FindObjectOfType<PresetManager>();
        if (presetManager == null)
        {
            presetManager.calibrationRunning = true;
            Debug.LogError("❌ No PresetManager found in the scene!");
            return;
        }
        // 🔹 Enable Kinect and dynamically generate terrain during calibration
        kinectDepthTerrain.enabled = true;
        kinectDepthTerrain.EnableLiveKinectTerrain();
        kinectDepthTerrain.SyncTerrainColliderWithTerrain();

        Debug.Log("📌 Calibration mode: Kinect terrain generation is active.");

        //kinectDepthTerrain.DebugKinect();

        presetManager.LoadPreset(); // Load the selected preset
        VisualizePreset();
        Debug.Log("🔹 Select preset, adjust camera, place entities, then save.");
    }

    private void Update()
    {
        HandleCameraMovement();

        if (Input.GetKeyDown(KeyCode.C)) // C for Castle
        {
            RegisterPosition("castle");
        }
        if (Input.GetKeyDown(KeyCode.T)) // T for Tower
        {
            RegisterPosition("tower");
        }
        if (Input.GetKeyDown(KeyCode.E)) // E for Enemy Spawn
        {
            RegisterPosition("enemy");
        }

        // 🔄 Update terrain every 0.5 seconds during calibration
        terrainUpdateTimer += Time.deltaTime;
        if (terrainUpdateTimer >= terrainUpdateInterval)
        {
            kinectDepthTerrain.CheckAndUpdateTerrain();
            terrainUpdateTimer = 0f;
        }
    }


    public void VisualizePreset()
    {
        ClearVisualization(); // Remove old visual objects

        // 🔹 Spawn castle
        foreach (Vector3 position in presetManager.GetCastlePositions())
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(position);
            Vector3 adjustedPosition = new Vector3(position.x, terrainHeight + 2f, position.z);

            GameObject castle = Instantiate(castlePrefab, adjustedPosition, Quaternion.identity);
            spawnedEntities.Add(castle);
        }

        // 🔹 Spawn towers
        foreach (Vector3 position in presetManager.GetTowerPositions())
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(position);
            Vector3 adjustedPosition = new Vector3(position.x, terrainHeight + 1f, position.z);

            GameObject tower = Instantiate(towerPrefab, adjustedPosition, Quaternion.identity);
            spawnedEntities.Add(tower);
        }

        // 🔹 Spawn enemy spawn points (Spheres)
        foreach (Vector3 position in presetManager.GetEnemySpawnPositions())
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(position);
            Vector3 adjustedPosition = new Vector3(position.x, terrainHeight + 1f, position.z);

            GameObject enemySpawn = Instantiate(enemySpawnPrefab, adjustedPosition, Quaternion.identity);
            spawnedEntities.Add(enemySpawn);
        }

        Debug.Log("✅ Preset visualized with terrain height adjustment.");
    }

    private void ClearVisualization()
    {
        foreach (GameObject obj in spawnedEntities)
        {
            Destroy(obj);
        }
        spawnedEntities.Clear();
    }


    private void RegisterPosition(string type)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 position = hit.point;
            //Debug.Log($"{type} registered at: {position}");

            // 🔹 Save based on type
            if (type == "castle") presetManager.AddCastle(position);
            if (type == "tower") presetManager.AddTower(position);
            if (type == "enemy") presetManager.AddEnemySpawn(position);
            VisualizePreset();
        }
        else
        {
            Debug.LogWarning("Click did not hit the terrain.");
        }
    }

    public void SaveSettings()
    {
        presetManager.SavePreset();
        PlayerPrefs.SetString("SelectedPreset", presetManager.GetSelectedPreset());
        PlayerPrefs.Save();

        PlayerPrefs.SetFloat("CameraPositionX", KinectTerrainCamera.transform.position.x);
        PlayerPrefs.SetFloat("CameraPositionY", KinectTerrainCamera.transform.position.y);
        PlayerPrefs.SetFloat("CameraPositionZ", KinectTerrainCamera.transform.position.z);

        PlayerPrefs.SetFloat("xRotation", kinectDepthTerrain.terrainRotation.x);
        PlayerPrefs.SetFloat("yRotation", kinectDepthTerrain.terrainRotation.y);

        // 🔹 Save terrain heightmap for gameplay use
        SaveTerrainHeightmap();

        PlayerPrefs.Save();
        Debug.Log("✅ Calibration settings saved.");
        GoToMainMenu();
    }

    private void SaveTerrainHeightmap()
    {
        TerrainData terrainData = kinectDepthTerrain.terrain.terrainData;
        float[,] heightmap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        kinectDepthTerrain.isCalibrationRunning = false;

        string heightmapJson = JsonUtility.ToJson(new HeightmapData(heightmap));
        PlayerPrefs.SetString("SavedHeightmap", heightmapJson);

        // 🔹 Stop Kinect updates AFTER saving terrain
        kinectDepthTerrain.SaveTerrain();
        kinectDepthTerrain.enabled = false;
    }

    private void HandleCameraMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        float moveY = 0f;
        if (Input.GetKey(KeyCode.O)) moveY = 1f;
        else if (Input.GetKey(KeyCode.I)) moveY = -1f;

        Vector3 move = new Vector3(moveX, moveY * verticalMoveSpeed, moveZ) * moveSpeed * Time.deltaTime;
        KinectTerrainCamera.transform.Translate(move, Space.World);
    }

    private void GoToMainMenu()
    {
        ClearVisualization();
        presetManager.calibrationRunning = false;
        SceneManager.LoadScene("MainMenu");
    }
}
