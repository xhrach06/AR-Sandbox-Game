using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Stores and manages preset positions for castle, towers, and enemy spawns.
/// </summary>
[System.Serializable]
public class PresetData
{
    public List<Vector3> castlePositions = new List<Vector3>();        // Single castle position
    public List<Vector3> towerPositions = new List<Vector3>();         // 3 tower positions
    public List<Vector3> enemySpawnPositions = new List<Vector3>();    // 3 enemy spawn positions
}

public class PresetManager : MonoBehaviour
{
    private string selectedPreset = "Preset1";              // Currently selected preset
    private PresetData presetData = new PresetData();       // Stores loaded preset data
    private Calibration calibration;                        // Reference to calibration component

    public bool calibrationRunning = false;                 // Whether calibration is active

    private readonly string[] presetNames = { "Preset1", "Preset2", "Preset3" }; // Available presets
    private int currentPresetIndex;                         // Index of current preset


    // Gets the full path to the preset JSON file
    private string GetPresetFilePath() =>
        Path.Combine(Application.persistentDataPath, $"{selectedPreset}.json");

    private void Awake()
    {
        // Load saved preset index and corresponding preset
        currentPresetIndex = PlayerPrefs.GetInt("CurrentPresetIndex", 0);
        selectedPreset = presetNames[currentPresetIndex];

        calibration = FindObjectOfType<Calibration>(); // Get calibration component
        LoadPreset(); // Load preset data from file
    }
    // Switches to the next preset in the list
    public void SelectNextPreset()
    {
        currentPresetIndex = (currentPresetIndex + 1) % presetNames.Length;
        PlayerPrefs.SetInt("CurrentPresetIndex", currentPresetIndex);

        selectedPreset = presetNames[currentPresetIndex];
        LoadPreset();

        if (calibrationRunning)
            calibration.VisualizePreset();

        Debug.Log($"🔁 Switched to {selectedPreset}");
    }
    // Selects a preset by name
    public void SelectPreset(string presetName)
    {
        selectedPreset = presetName;
        LoadPreset();

        if (calibrationRunning)
            calibration.VisualizePreset();

        Debug.Log($"Selected preset: {selectedPreset}");
    }
    // Saves current preset data to file
    public void SavePreset()
    {
        string json = JsonUtility.ToJson(presetData, true);
        File.WriteAllText(GetPresetFilePath(), json);
        Debug.Log($"Preset '{selectedPreset}' saved.");
    }
    // Loads preset data from file, or creates new if file not found
    public void LoadPreset()
    {
        string path = GetPresetFilePath();

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            presetData = JsonUtility.FromJson<PresetData>(json);
            Debug.Log($"Preset '{selectedPreset}' loaded.");
        }
        else
        {
            presetData = new PresetData();
            Debug.LogWarning($"Preset '{selectedPreset}' not found. Created new.");
        }
    }
    // Getters for external access
    public List<Vector3> GetCastlePositions() => presetData.castlePositions;
    public List<Vector3> GetTowerPositions() => presetData.towerPositions;
    public List<Vector3> GetEnemySpawnPositions() => presetData.enemySpawnPositions;
    public string GetSelectedPreset() => selectedPreset;
    // Adds or replaces the castle position
    public void AddCastle(Vector3 position)
    {
        if (presetData.castlePositions.Count > 0)
            presetData.castlePositions[0] = position;
        else
            presetData.castlePositions.Add(position);

        SavePreset();
    }
    // Adds a tower position, keeping up to maxTowers
    public void AddTower(Vector3 position, int maxTowers = 3)
    {
        if (presetData.towerPositions.Count >= maxTowers)
            presetData.towerPositions.RemoveAt(0);

        presetData.towerPositions.Add(position);
        SavePreset();
    }
    // Adds an enemy spawn position, keeping up to maxEnemySpawns
    public void AddEnemySpawn(Vector3 position, int maxEnemySpawns = 3)
    {
        if (presetData.enemySpawnPositions.Count >= maxEnemySpawns)
            presetData.enemySpawnPositions.RemoveAt(0);

        presetData.enemySpawnPositions.Add(position);
        SavePreset();
    }
}
