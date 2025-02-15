using UnityEngine;
using System.Collections.Generic;

public class PresetManager : MonoBehaviour
{
    private List<Vector3> castlePositions = new List<Vector3>();
    private List<Vector3> towerPositions = new List<Vector3>();
    private List<Vector3> enemySpawnPositions = new List<Vector3>();

    public void LoadPreset(string presetName)
    {
        string keyPrefix = presetName + "_";

        castlePositions.Clear();
        towerPositions.Clear();
        enemySpawnPositions.Clear();

        int castleCount = PlayerPrefs.GetInt($"{keyPrefix}CastleCount", 0);
        int towerCount = PlayerPrefs.GetInt($"{keyPrefix}TowerCount", 0);
        int enemyCount = PlayerPrefs.GetInt($"{keyPrefix}EnemyCount", 0);

        for (int i = 0; i < castleCount; i++)
            castlePositions.Add(JsonUtility.FromJson<Vector3>(PlayerPrefs.GetString($"{keyPrefix}Castle_{i}")));

        for (int i = 0; i < towerCount; i++)
            towerPositions.Add(JsonUtility.FromJson<Vector3>(PlayerPrefs.GetString($"{keyPrefix}Tower_{i}")));

        for (int i = 0; i < enemyCount; i++)
            enemySpawnPositions.Add(JsonUtility.FromJson<Vector3>(PlayerPrefs.GetString($"{keyPrefix}Enemy_{i}")));

        Debug.Log($"Preset {presetName} loaded successfully.");
    }

    public List<Vector3> GetCastlePositions() => castlePositions;
    public List<Vector3> GetTowerPositions() => towerPositions;
    public List<Vector3> GetEnemySpawnPositions() => enemySpawnPositions;
}
