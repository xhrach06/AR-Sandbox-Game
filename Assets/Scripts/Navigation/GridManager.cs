using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the grid used for pathfinding based on terrain height and obstacles.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector2Int gridSize = new Vector2Int(64, 53); // Grid dimensions (width, height)
    public float nodeSize = 8f;                          // World size of each node

    [Header("References")]
    public Terrain terrain;

    // The 2D grid storing node data
    public Node[,] grid;

    // Grid positions occupied by towers (used for marking non-walkable nodes)
    private HashSet<Vector2Int> occupiedNodes = new();

    void Start()
    {
        GenerateGrid();
    }

    /// Marks grid cells as occupied (e.g., by towers) to make them unwalkable.
    public void SetObstaclePositions(List<Vector3> worldPositions)
    {
        occupiedNodes.Clear();

        foreach (Vector3 pos in worldPositions)
        {
            Vector2Int gridPos = new Vector2Int(
                Mathf.FloorToInt(pos.x / nodeSize),
                Mathf.FloorToInt(pos.z / nodeSize)
            );

            occupiedNodes.Add(gridPos);
        }
    }

    /// Generates the grid and calculates walkability based on terrain and tower positions.
    public void GenerateGrid()
    {
        grid = new Node[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPoint = new Vector3(x * nodeSize, 0, y * nodeSize);
                worldPoint.y = terrain.SampleHeight(worldPoint);

                Vector2Int gridPos = new Vector2Int(x, y);
                bool walkable = !occupiedNodes.Contains(gridPos);

                float heightCost = Mathf.Abs(worldPoint.y - terrain.SampleHeight(worldPoint));

                grid[x, y] = new Node(worldPoint, walkable, heightCost, x, y);
            }
        }
    }

    /// Returns the node corresponding to a world position.
    public Node GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / nodeSize);
        int y = Mathf.FloorToInt(worldPosition.z / nodeSize);

        if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
            return null;

        return grid[x, y];
    }

    /// Visualizes the grid in the scene view using Gizmos.
    void OnDrawGizmos()
    {
        if (grid == null) return;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Node node = grid[x, y];
                Gizmos.color = node.walkable ? Color.green : Color.red;
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeSize * 0.5f));
            }
        }
    }
}
