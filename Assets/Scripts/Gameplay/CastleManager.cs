using System.Collections.Generic;
using UnityEngine;

public class CastleManager : MonoBehaviour
{
    public GameObject castlePrefab;
    public GameObject projectilePrefab;
    public EnemyManager enemyManager;

    private GameObject placedCastle;

    void Start()
    {
        if (enemyManager == null)
        {
            enemyManager = FindObjectOfType<EnemyManager>();
        }
    }

    public void PlaceCastle()
    {
        PresetManager presetManager = FindObjectOfType<PresetManager>();
        List<Vector3> positions = presetManager.GetCastlePositions();

        if (positions.Count > 0)
        {
            float terrainHeight = Terrain.activeTerrain.SampleHeight(positions[0]);
            Vector3 adjustedPosition = new Vector3(positions[0].x, terrainHeight + 2f, positions[0].z);

            placedCastle = Instantiate(castlePrefab, adjustedPosition, Quaternion.identity);
            Debug.Log($"🏰 Castle placed at: {adjustedPosition}");
        }
        else
        {
            Debug.LogError("❌ CastleManager: No valid castle position found in preset!");
        }
    }



    public Transform GetCastleTransform()
    {
        return placedCastle?.transform;
    }
}



/*
public void PlaceCastleRandomly()
{
    Vector3 randomPosition = GetValidRandomPosition();
    placedCastle = Instantiate(castlePrefab, randomPosition, Quaternion.identity);
    Debug.Log("Castle placed at: " + randomPosition);

    Castle castleScript = placedCastle.GetComponent<Castle>();
    if (castleScript != null)
    {
        castleScript.projectilePrefab = projectilePrefab;
    }

    if (enemyManager != null)
    {
        enemyManager.SetCastleTarget(placedCastle.transform);
    }
    else
    {
        Debug.LogError("EnemyManager reference is missing!");
    }
}

private Vector3 GetValidRandomPosition()
{
    const int maxAttempts = 3;
    Bounds terrainBounds = GameManager.Instance.GetTerrainBounds();

    for (int i = 0; i < maxAttempts; i++)
    {
        float x = Random.Range(terrainBounds.min.x, terrainBounds.max.x);
        float z = Random.Range(terrainBounds.min.z, terrainBounds.max.z);
        float y = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z)) + 2f;
        Debug.Log($"Castle Manager x:{terrainBounds.min.x} to {terrainBounds.max.x} z:{terrainBounds.min.z} to {terrainBounds.max.z}");
        Vector3 randomPosition = new Vector3(x, y, z);

        if (UnityEngine.AI.NavMesh.SamplePosition(randomPosition, out UnityEngine.AI.NavMeshHit hit, 25f, UnityEngine.AI.NavMesh.AllAreas))
        {
            return hit.position;
        }
    }

    Debug.LogWarning("CastleManager: Failed to find valid position. Using fallback.");
    return new Vector3(250f, 10f, 250f);
}
*/
