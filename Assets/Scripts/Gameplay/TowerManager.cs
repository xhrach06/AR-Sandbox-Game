using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TowerManager : MonoBehaviour
{
    public GameObject towerPrefab;
    public GameObject projectilePrefab;

    private List<Vector3> placedTowerPositions = new List<Vector3>();

    public void PlaceTowers()
    {
        PresetManager presetManager = FindObjectOfType<PresetManager>();
        List<Vector3> positions = presetManager.GetTowerPositions();

        for (int i = 0; i < positions.Count; i++)
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(positions[i]);
            Vector3 adjustedPosition = new Vector3(positions[i].x, terrainHeight + 1f, positions[i].z);

            GameObject tower = Instantiate(towerPrefab, adjustedPosition, Quaternion.identity);
            Debug.Log($"🏗 Tower {i} placed at: {adjustedPosition}");
        }
    }

}