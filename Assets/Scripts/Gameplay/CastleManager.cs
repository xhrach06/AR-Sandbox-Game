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

    public void HandleCastleDestroyed()
    {
        GameManager gameManager = FindObjectOfType<GameManager>();
        gameManager.EndGame("Game Over! The castle was destroyed.");
    }


    public Transform GetCastleTransform()
    {
        return placedCastle?.transform;
    }
}