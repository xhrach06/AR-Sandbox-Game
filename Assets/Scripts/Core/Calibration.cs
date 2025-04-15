using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Calibration scene logic for visualizing Kinect terrain and placing castle/towers/enemy spawns.
/// </summary>
public class Calibration : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private KinectDepthTerrain kinectDepthTerrain;
    [SerializeField] private GameObject castlePrefab;
    [SerializeField] private GameObject towerPrefab;
    [SerializeField] private GameObject enemySpawnPrefab;

    private Camera KinectTerrainCamera;
    private PresetManager presetManager;

    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float verticalMoveSpeed = 5f;

    [Header("Terrain Update")]
    private float terrainUpdateTimer = 0f;
    private const float terrainUpdateInterval = 0.5f;

    private List<GameObject> spawnedEntities = new List<GameObject>();

    private void Start()
    {
        KinectTerrainCamera = Camera.main;

        presetManager = FindObjectOfType<PresetManager>();
        if (presetManager == null)
        {
            Debug.LogError("❌ No PresetManager found in the scene!");
            return;
        }

        presetManager.calibrationRunning = true;

        // Enable Kinect and terrain updates
        kinectDepthTerrain.enabled = true;
        kinectDepthTerrain.SyncTerrainColliderWithTerrain();

        Debug.Log("📌 Calibration mode: Kinect terrain generation is active.");
        StartCoroutine(DelayedVisualization());
    }

    private void Update()
    {
        HandleCameraMovement();

        // Key press handlers
        if (Input.GetKeyDown(KeyCode.C)) RegisterPosition("castle");
        if (Input.GetKeyDown(KeyCode.T)) RegisterPosition("tower");
        if (Input.GetKeyDown(KeyCode.E)) RegisterPosition("enemy");

        // Periodic terrain updates
        terrainUpdateTimer += Time.deltaTime;
        if (terrainUpdateTimer >= terrainUpdateInterval)
        {
            if (kinectDepthTerrain.CheckAndUpdateTerrain())
                terrainUpdateTimer = 0f;
        }
    }

    private IEnumerator DelayedVisualization()
    {
        yield return new WaitForSeconds(0.5f);
        presetManager.LoadPreset();
        VisualizePreset();
    }

    /// <summary>
    /// Loads and instantiates entities from the preset onto the terrain.
    /// </summary>
    public void VisualizePreset()
    {
        ClearVisualization();

        foreach (Vector3 position in presetManager.GetCastlePositions())
        {
            Vector3 adjustedPosition = GetHeightAdjustedPosition(position, 2f);
            GameObject castle = Instantiate(castlePrefab, adjustedPosition, Quaternion.identity);
            spawnedEntities.Add(castle);
        }

        foreach (Vector3 position in presetManager.GetTowerPositions())
        {
            Vector3 adjustedPosition = GetHeightAdjustedPosition(position, 1f);
            GameObject tower = Instantiate(towerPrefab, adjustedPosition, Quaternion.identity);
            spawnedEntities.Add(tower);
        }

        foreach (Vector3 position in presetManager.GetEnemySpawnPositions())
        {
            Vector3 adjustedPosition = GetHeightAdjustedPosition(position, 1f);
            GameObject enemySpawn = Instantiate(enemySpawnPrefab, adjustedPosition, Quaternion.identity);
            spawnedEntities.Add(enemySpawn);
        }

        Debug.Log("✅ Preset visualized with terrain height adjustment.");
    }

    private Vector3 GetHeightAdjustedPosition(Vector3 position, float yOffset)
    {
        float terrainHeight = Terrain.activeTerrain.SampleHeight(position);
        return new Vector3(position.x, terrainHeight + yOffset, position.z);
    }

    private void ClearVisualization()
    {
        foreach (GameObject obj in spawnedEntities)
        {
            Destroy(obj);
        }
        spawnedEntities.Clear();
    }

    /// <summary>
    /// Registers a new position into the preset file via mouse raycast.
    /// </summary>
    private void RegisterPosition(string type)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 position = hit.point;

            switch (type)
            {
                case "castle": presetManager.AddCastle(position); break;
                case "tower": presetManager.AddTower(position); break;
                case "enemy": presetManager.AddEnemySpawn(position); break;
            }

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

        // Save camera position
        PlayerPrefs.SetFloat("CameraPositionX", KinectTerrainCamera.transform.position.x);
        PlayerPrefs.SetFloat("CameraPositionY", KinectTerrainCamera.transform.position.y);
        PlayerPrefs.SetFloat("CameraPositionZ", KinectTerrainCamera.transform.position.z);

        // Save terrain rotation
        PlayerPrefs.SetFloat("xRotation", kinectDepthTerrain.terrainRotation.x);
        PlayerPrefs.SetFloat("yRotation", kinectDepthTerrain.terrainRotation.y);

        // Save terrain height data
        SaveTerrainHeightmap();

        PlayerPrefs.Save();
        Debug.Log("✅ Calibration settings saved.");
        GoToMainMenu();
    }

    private void SaveTerrainHeightmap()
    {
        TerrainData terrainData = kinectDepthTerrain.terrain.terrainData;
        float[,] heightmap = terrainData.GetHeights(0, 0, terrainData.heightmapResolution, terrainData.heightmapResolution);
        string heightmapJson = JsonUtility.ToJson(new HeightmapData(heightmap));
        PlayerPrefs.SetString("SavedHeightmap", heightmapJson);

        // Disable terrain updates once saved
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
