using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Vector2Int gridSize = new Vector2Int(20, 20); // Defines the grid's size (width and height)
    public float nodeSize = 1f; // The size of each grid cell
    public Terrain terrain; // Reference to the terrain
    public LayerMask obstacleMask; // The layer mask for obstacles (e.g., towers)

    public Node[,] grid; // 2D array of nodes representing the grid

    void Start()
    {
        GenerateGrid(); // Creates the grid at the start of the game
    }

    // 🔹 Generates the grid by creating nodes based on the terrain height and obstacles
    void GenerateGrid()
    {
        grid = new Node[gridSize.x, gridSize.y];

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                // Convert grid position to world coordinates
                Vector3 worldPoint = new Vector3(x * nodeSize, 0, y * nodeSize);
                worldPoint.y = terrain.SampleHeight(worldPoint); // Adjust y-coordinate based on terrain height

                // Check if this node is walkable (not inside an obstacle)
                bool walkable = !Physics.CheckSphere(worldPoint, nodeSize / 64, obstacleMask);

                // Calculate movement cost based on height difference (hills affect movement speed)
                float heightCost = Mathf.Abs(worldPoint.y - terrain.SampleHeight(worldPoint));

                // Create a new node and store it in the grid
                grid[x, y] = new Node(worldPoint, walkable, heightCost);
            }
        }
    }

    // 🔹 Draws the grid in the editor for visualization
    /*void OnDrawGizmos()
    {
        if (grid == null) return;

        // Loop through all nodes and draw their positions
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Node node = grid[x, y];

                Gizmos.color = node.walkable ? Color.green : Color.red; // Walkable nodes are green, obstacles are red
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeSize * 0.8f)); // Small cube for each node
            }
        }
    }
    */
}
