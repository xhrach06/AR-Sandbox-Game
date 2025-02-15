using UnityEngine;
using System.Collections.Generic;

public class PresetCreator : MonoBehaviour
{
    private List<Vector3> castlePositions = new List<Vector3>();
    private List<Vector3> towerPositions = new List<Vector3>();
    private List<Vector3> enemySpawnPositions = new List<Vector3>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Left Click - Castle
        {
            Vector3 position = GetMouseWorldPosition();
            castlePositions.Add(position);
            Debug.Log($"Castle position saved: {position}");
        }
        else if (Input.GetMouseButtonDown(1)) // Right Click - Tower
        {
            Vector3 position = GetMouseWorldPosition();
            towerPositions.Add(position);
            Debug.Log($"Tower position saved: {position}");
        }
        else if (Input.GetKeyDown(KeyCode.E)) // 'E' Key - Enemy
        {
            Vector3 position = GetMouseWorldPosition();
            enemySpawnPositions.Add(position);
            Debug.Log($"Enemy spawn position saved: {position}");
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return new Vector3(hit.point.x, Terrain.activeTerrain.SampleHeight(hit.point) + 1f, hit.point.z);
        }
        return Vector3.zero;
    }

    public void SavePreset(string presetName)
    {
        string keyPrefix = "Preset_" + presetName + "_";

        for (int i = 0; i < castlePositions.Count; i++)
        {
            PlayerPrefs.SetFloat($"{keyPrefix}Castle_X", castlePositions[i].x);
            PlayerPrefs.SetFloat($"{keyPrefix}Castle_Z", castlePositions[i].z);
        }

        for (int i = 0; i < towerPositions.Count; i++)
        {
            PlayerPrefs.SetFloat($"{keyPrefix}Tower_{i}_X", towerPositions[i].x);
            PlayerPrefs.SetFloat($"{keyPrefix}Tower_{i}_Z", towerPositions[i].z);
        }

        for (int i = 0; i < enemySpawnPositions.Count; i++)
        {
            PlayerPrefs.SetFloat($"{keyPrefix}Enemy_{i}_X", enemySpawnPositions[i].x);
            PlayerPrefs.SetFloat($"{keyPrefix}Enemy_{i}_Z", enemySpawnPositions[i].z);
        }

        PlayerPrefs.SetInt($"{keyPrefix}TowerCount", towerPositions.Count);
        PlayerPrefs.SetInt($"{keyPrefix}EnemyCount", enemySpawnPositions.Count);
        PlayerPrefs.Save();

        Debug.Log($"Preset '{presetName}' saved successfully.");
    }
}
