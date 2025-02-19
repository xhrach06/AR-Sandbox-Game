using UnityEngine;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class PresetData
{
    public List<Vector3> castlePositions = new List<Vector3>();
    public List<Vector3> towerPositions = new List<Vector3>();
    public List<Vector3> enemySpawnPositions = new List<Vector3>();
}

public class PresetManager : MonoBehaviour
{
    private string selectedPreset = "Preset1"; // Default preset
    private PresetData presetData = new PresetData();

    private Calibration calibration;
    public bool calibrationRunning = false;
    private string GetPresetFilePath() => Path.Combine(Application.persistentDataPath, $"{selectedPreset}.json");

    private void Awake()
    {
        selectedPreset = PlayerPrefs.GetString("SelectedPreset", "Preset1");
        calibration = FindObjectOfType<Calibration>();
        LoadPreset();
    }

    public void SelectPreset(string presetName)
    {
        selectedPreset = presetName;
        LoadPreset();
        if (calibrationRunning)
            calibration.VisualizePreset();
        Debug.Log($"🔹 Selected preset: {selectedPreset}");
    }

    public void SavePreset()
    {
        string filePath = GetPresetFilePath();
        string json = JsonUtility.ToJson(presetData, true);
        File.WriteAllText(filePath, json);
        Debug.Log($"✅ Preset '{selectedPreset}' saved to: {filePath}");
    }

    public void LoadPreset()
    {
        string filePath = GetPresetFilePath();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            presetData = JsonUtility.FromJson<PresetData>(json);
            Debug.Log($"📌 Preset '{selectedPreset}' loaded.");
        }
        else
        {
            presetData = new PresetData();
            Debug.LogWarning($"⚠ Preset '{selectedPreset}' not found. Creating new preset.");
        }
    }

    public void DisplayPresetCounts()
    {
        Debug.Log($"📌 Preset: {selectedPreset}");
        Debug.Log($"🏰 Castle: {(presetData.castlePositions.Count > 0 ? "✔ Present" : "❌ Not Set")}");
        Debug.Log($"🗼 Towers: {presetData.towerPositions.Count}");
        Debug.Log($"👹 Enemy Spawns: {presetData.enemySpawnPositions.Count}");
    }

    public void ClearCastle()
    {
        if (presetData.castlePositions.Count > 0)
        {
            presetData.castlePositions.Clear();
            SavePreset();
            Debug.Log($"✅ All castles removed from {selectedPreset}.");
        }
        else
        {
            Debug.Log("⚠ No castle data found. Nothing to delete.");
        }
    }

    public List<Vector3> GetCastlePositions() => presetData.castlePositions;
    public List<Vector3> GetTowerPositions() => presetData.towerPositions;
    public List<Vector3> GetEnemySpawnPositions() => presetData.enemySpawnPositions;
    public string GetSelectedPreset() => selectedPreset;

    public void AddCastle(Vector3 position)
    {
        if (presetData.castlePositions.Count > 0)
        {
            presetData.castlePositions[0] = position;
        }
        else
        {
            presetData.castlePositions.Add(position);
        }
        SavePreset();
    }

    public void AddTower(Vector3 position, int maxTowers = 3)
    {
        if (presetData.towerPositions.Count >= maxTowers)
        {
            presetData.towerPositions.RemoveAt(0);
        }
        presetData.towerPositions.Add(position);
        SavePreset();
    }

    public void AddEnemySpawn(Vector3 position, int maxEnemySpawns = 3)
    {
        if (presetData.enemySpawnPositions.Count >= maxEnemySpawns)
        {
            presetData.enemySpawnPositions.RemoveAt(0);
        }
        presetData.enemySpawnPositions.Add(position);
        SavePreset();
    }
}
