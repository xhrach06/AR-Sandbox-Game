using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsible for placing the castle and notifying GameManager when it's destroyed.
/// </summary>
public class CastleManager : MonoBehaviour
{
    public GameObject castlePrefab;
    public GameObject projectilePrefab;
    public EnemyManager enemyManager;

    private GameObject placedCastle;

    void Start()
    {
        if (enemyManager == null)
            enemyManager = FindObjectOfType<EnemyManager>();
    }
    // Places castle based on preset
    public void PlaceCastle()
    {
        PresetManager presetManager = FindObjectOfType<PresetManager>();
        List<Vector3> positions = presetManager.GetCastlePositions();

        if (positions.Count == 0)
        {
            Debug.LogError("CastleManager: No valid castle position found in preset!");
            return;
        }

        float terrainHeight = Terrain.activeTerrain.SampleHeight(positions[0]);
        Vector3 adjustedPosition = new Vector3(positions[0].x, terrainHeight + 2f, positions[0].z);

        placedCastle = Instantiate(castlePrefab, adjustedPosition, Quaternion.identity);
        Debug.Log($"Castle placed at: {adjustedPosition}");
    }
    // Ends game when castle is destroyed
    public void HandleCastleDestroyed()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.EndGame(false);
    }
    // Return castle position
    public Transform GetCastleTransform()
    {
        return placedCastle?.transform;
    }
}
