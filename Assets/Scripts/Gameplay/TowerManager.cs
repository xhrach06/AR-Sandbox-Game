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


/*
public void PlaceTowersAutomatically(int numberOfTowers)
{
    Transform castleTransform = GameManager.Instance.CastleManager.GetCastleTransform();

    for (int i = 0; i < numberOfTowers; i++)
    {
        Vector3 randomPosition = GetValidRandomPosition(castleTransform);
        if (randomPosition != Vector3.zero)
        {
            GameObject tower = Instantiate(towerPrefab, randomPosition, Quaternion.identity);
            placedTowerPositions.Add(randomPosition);

            Tower towerScript = tower.GetComponent<Tower>();
            if (towerScript != null)
            {
                towerScript.projectilePrefab = projectilePrefab;
            }

            Debug.Log($"Tower {i} placed at: {randomPosition}");
        }
        else
        {
            Debug.LogWarning($"Tower {i} failed to find a valid spawn position.");
        }
    }
}

private Vector3 GetValidRandomPosition(Transform castleTransform)
{
    const int maxAttempts = 3;
    Bounds terrainBounds = GameManager.Instance.GetTerrainBounds();

    for (int i = 0; i < maxAttempts; i++)
    {
        float x = Random.Range(terrainBounds.min.x, terrainBounds.max.x);
        float z = Random.Range(terrainBounds.min.z, terrainBounds.max.z);
        float y = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z)) + 2f;

        Vector3 randomPosition = new Vector3(x, y, z);
        Debug.Log($"Tower Attempt {i + 1}: Trying position {randomPosition}");

        if (castleTransform != null && Vector3.Distance(randomPosition, castleTransform.position) < minDistanceFromCastle)
        {
            Debug.LogWarning($"Tower too close to castle. Position: {randomPosition}");
            continue;
        }

        bool tooCloseToOtherTowers = false;
        foreach (Vector3 towerPosition in placedTowerPositions)
        {
            if (Vector3.Distance(randomPosition, towerPosition) < minDistanceBetweenTowers)
            {
                tooCloseToOtherTowers = true;
                Debug.LogWarning($"Tower too close to another tower. Position: {randomPosition}");
                break;
            }
        }

        if (tooCloseToOtherTowers) continue;

        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 25f, NavMesh.AllAreas))
        {
            Debug.Log($"Tower valid position found at: {hit.position}");
            return hit.position;
        }
    }

    Debug.LogWarning("TowerManager: Failed to find a valid position for a tower.");
    return Vector3.zero;
}
*/

