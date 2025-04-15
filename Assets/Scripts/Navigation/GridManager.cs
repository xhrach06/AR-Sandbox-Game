using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public Vector2Int gridSize = new Vector2Int(64, 53); // Adjusted size
    public float nodeSize = 8f;                          // Adjusted node resolution
    public Terrain terrain;
    public Node[,] grid;

    // Reference to tower positions (assigned after placement)
    private HashSet<Vector2Int> occupiedNodes = new HashSet<Vector2Int>();

    void Start()
    {
        GenerateGrid();
    }

    public void SetObstaclePositions(List<Vector3> worldPositions)
    {
        occupiedNodes.Clear();

        foreach (Vector3 pos in worldPositions)
        {
            Vector2Int gridPos = new Vector2Int(
                Mathf.FloorToInt(pos.x / nodeSize),
                Mathf.FloorToInt(pos.z / nodeSize)
            );

            if (!occupiedNodes.Contains(gridPos))
                occupiedNodes.Add(gridPos);
        }
    }

    public void GenerateGrid()
    {
        grid = new Node[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPoint = new Vector3(x * nodeSize, 0, y * nodeSize);
                worldPoint.y = terrain.SampleHeight(worldPoint);

                Vector2Int currentGridPos = new Vector2Int(x, y);
                bool walkable = !occupiedNodes.Contains(currentGridPos);

                float heightCost = Mathf.Abs(worldPoint.y - terrain.SampleHeight(worldPoint));

                grid[x, y] = new Node(worldPoint, walkable, heightCost, x, y);
            }
        }
    }

    public Node GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / nodeSize);
        int y = Mathf.FloorToInt(worldPosition.z / nodeSize);

        if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
            return null;

        return grid[x, y];
    }

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
