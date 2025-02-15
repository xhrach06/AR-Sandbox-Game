using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Calibration : MonoBehaviour
{
    private Camera KinectTerrainCamera;
    [SerializeField] private KinectDepthTerrain kinectDepthTerrain;

    private List<Vector3> castlePositions = new List<Vector3>();
    private List<Vector3> towerPositions = new List<Vector3>();
    private List<Vector3> enemyPositions = new List<Vector3>();

    private string selectedPreset = "Preset1"; // Default preset

    public float moveSpeed = 10f;
    public float verticalMoveSpeed = 5f;

    private void Start()
    {
        KinectTerrainCamera = Camera.main;

        // 🔹 Enable Kinect and dynamically generate terrain during calibration
        kinectDepthTerrain.enabled = true;
        kinectDepthTerrain.SyncTerrainColliderWithTerrain();

        Debug.Log("📌 Calibration mode: Kinect terrain generation is active.");

        LoadPreset(); // Load the selected preset
        Debug.Log("🔹 Select preset, adjust camera, place entities, then save.");
    }

    private void Update()
    {
        HandleCameraMovement();

        if (Input.GetKeyDown(KeyCode.C)) // C for Castle
        {
            RegisterPosition(ref castlePositions, "Castle Position");
        }
        if (Input.GetKeyDown(KeyCode.T)) // T for Tower
        {
            RegisterPosition(ref towerPositions, "Tower Position");
        }
        if (Input.GetKeyDown(KeyCode.E)) // E for Enemy Spawn
        {
            RegisterPosition(ref enemyPositions, "Enemy Spawn Point");
        }
        if (Input.GetKeyDown(KeyCode.S)) // S to Save Preset
        {
            SavePreset();
        }
    }

    private void RegisterPosition(ref List<Vector3> positionList, string label)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            positionList.Add(hit.point);
            Debug.Log($"{label} registered at: {hit.point}");
        }
        else
        {
            Debug.LogWarning("Click did not hit the terrain.");
        }
    }

    public void SelectPreset(string presetName)
    {
        selectedPreset = presetName;
        PlayerPrefs.SetString("SelectedPreset", selectedPreset);
        PlayerPrefs.Save();
        LoadPreset(); // Load the preset when it's selected
        Debug.Log($"🔹 Selected preset: {selectedPreset}");
    }

    private void SavePreset()
    {
        PlayerPrefs.SetInt($"{selectedPreset}_CastleCount", castlePositions.Count);
        PlayerPrefs.SetInt($"{selectedPreset}_TowerCount", towerPositions.Count);
        PlayerPrefs.SetInt($"{selectedPreset}_EnemyCount", enemyPositions.Count);

        for (int i = 0; i < castlePositions.Count; i++)
            PlayerPrefs.SetString($"{selectedPreset}_Castle_{i}", JsonUtility.ToJson(castlePositions[i]));

        for (int i = 0; i < towerPositions.Count; i++)
            PlayerPrefs.SetString($"{selectedPreset}_Tower_{i}", JsonUtility.ToJson(towerPositions[i]));

        for (int i = 0; i < enemyPositions.Count; i++)
            PlayerPrefs.SetString($"{selectedPreset}_Enemy_{i}", JsonUtility.ToJson(enemyPositions[i]));

        PlayerPrefs.Save();
        Debug.Log($"✅ Preset {selectedPreset} Saved!");
    }

    private void LoadPreset()
    {
        int castleCount = PlayerPrefs.GetInt($"{selectedPreset}_CastleCount", 0);
        int towerCount = PlayerPrefs.GetInt($"{selectedPreset}_TowerCount", 0);
        int enemyCount = PlayerPrefs.GetInt($"{selectedPreset}_EnemyCount", 0);

        castlePositions.Clear();
        towerPositions.Clear();
        enemyPositions.Clear();

        for (int i = 0; i < castleCount; i++)
            castlePositions.Add(JsonUtility.FromJson<Vector3>(PlayerPrefs.GetString($"{selectedPreset}_Castle_{i}")));

        for (int i = 0; i < towerCount; i++)
            towerPositions.Add(JsonUtility.FromJson<Vector3>(PlayerPrefs.GetString($"{selectedPreset}_Tower_{i}")));

        for (int i = 0; i < enemyCount; i++)
            enemyPositions.Add(JsonUtility.FromJson<Vector3>(PlayerPrefs.GetString($"{selectedPreset}_Enemy_{i}")));

        Debug.Log($"📌 Preset {selectedPreset} Loaded: {castleCount} castles, {towerCount} towers, {enemyCount} enemy spawn points.");
    }

    public void SaveSettings()
    {
        SavePreset(); // Ensure preset is saved before leaving calibration

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
        float moveX = -Input.GetAxis("Horizontal");
        float moveZ = -Input.GetAxis("Vertical");

        float moveY = 0f;
        if (Input.GetKey(KeyCode.Q)) moveY = 1f;
        else if (Input.GetKey(KeyCode.E)) moveY = -1f;

        Vector3 move = new Vector3(moveX, moveY * verticalMoveSpeed, moveZ) * moveSpeed * Time.deltaTime;
        KinectTerrainCamera.transform.Translate(move, Space.World);
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
