using System.Collections.Generic;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    public GameObject meteorPrefab; // Assign in Inspector
    public GameObject barrierPrefab; // Assign in Inspector

    private KinectDepthTerrain kinectDepthTerrain;
    private Terrain terrain;
    private readonly float detectionThreshold = 0.1f; // Minimum depth difference for detection

    void Start()
    {
        kinectDepthTerrain = FindObjectOfType<KinectDepthTerrain>();
        terrain = kinectDepthTerrain.terrain;

        if (kinectDepthTerrain == null || terrain == null)
        {
            Debug.LogError("❌ SpellManager: KinectDepthTerrain or Terrain is missing!");
            return;
        }
    }

    void Update()
    {
        DetectMeteorStrike();
        DetectBarrier();
    }

    public void DetectMeteorStrike()
    {
        int width = kinectDepthTerrain.depthResolution.x;
        int height = kinectDepthTerrain.depthResolution.y;
        float[,] heights = terrain.terrainData.GetHeights(0, 0, width, height);
        List<Vector2Int> circlePoints = new List<Vector2Int>();

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                float centerHeight = heights[y, x];
                float averageHeight = (
                    heights[y - 1, x] + heights[y + 1, x] +
                    heights[y, x - 1] + heights[y, x + 1]
                ) / 4f;

                if (centerHeight < averageHeight - detectionThreshold)
                {
                    circlePoints.Add(new Vector2Int(x, y));
                }
            }
        }

        if (circlePoints.Count > 20) // Ensure it's large enough
        {
            Vector2Int center = CalculateCenter(circlePoints);
            TriggerMeteor(center);
        }
    }

    private Vector2Int CalculateCenter(List<Vector2Int> points)
    {
        int sumX = 0, sumY = 0;
        foreach (var point in points)
        {
            sumX += point.x;
            sumY += point.y;
        }
        return new Vector2Int(sumX / points.Count, sumY / points.Count);
    }

    private void TriggerMeteor(Vector2Int position)
    {
        Vector3 worldPosition = new Vector3(position.x,
            terrain.SampleHeight(new Vector3(position.x, 0, position.y)),
            position.y);

        Instantiate(meteorPrefab, worldPosition + Vector3.up * 30f, Quaternion.identity);
        Debug.Log("🔥 Meteor strike activated!");
    }

    public void DetectBarrier()
    {
        int width = kinectDepthTerrain.depthResolution.x;
        int height = kinectDepthTerrain.depthResolution.y;
        float[,] heights = terrain.terrainData.GetHeights(0, 0, width, height);
        List<Vector2Int> linePoints = new List<Vector2Int>();

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                float centerHeight = heights[y, x];
                float leftHeight = heights[y, Mathf.Max(x - 1, 0)];
                float rightHeight = heights[y, Mathf.Min(x + 1, width - 1)];

                if (Mathf.Abs(centerHeight - leftHeight) > detectionThreshold &&
                    Mathf.Abs(centerHeight - rightHeight) > detectionThreshold)
                {
                    linePoints.Add(new Vector2Int(x, y));
                }
            }
        }

        if (linePoints.Count > 10) // Ensure it's long enough
        {
            SpawnBarrier(linePoints);
        }
    }

    private void SpawnBarrier(List<Vector2Int> linePoints)
    {
        foreach (Vector2Int point in linePoints)
        {
            Vector3 worldPosition = new Vector3(point.x,
                terrain.SampleHeight(new Vector3(point.x, 0, point.y)),
                point.y);

            Instantiate(barrierPrefab, worldPosition, Quaternion.identity);
        }
        Debug.Log("🛑 Barrier created!");
    }
}
