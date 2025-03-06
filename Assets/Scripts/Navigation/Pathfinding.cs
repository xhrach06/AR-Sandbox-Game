using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public GridManager grid; // Reference to GridManager
    int heightWeight = 10;
    // 🔹 Find the shortest path between startPos and targetPos
    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        // 🔹 Get the start and target nodes from the grid
        Node startNode = GetNodeFromWorldPosition(startPos);
        Node targetNode = GetNodeFromWorldPosition(targetPos);

        // Debugging logs to check node positions
        //Debug.Log($"🔎 Start Node: {startNode.worldPosition}, Walkable: {startNode.walkable}");
        //Debug.Log($"🔎 Target Node: {targetNode.worldPosition}, Walkable: {targetNode.walkable}");

        // 🔹 If the start or target node is unwalkable, return an empty path
        if (!startNode.walkable || !targetNode.walkable)
        {
            Debug.LogError("❌ Start or Target node is NOT walkable! Pathfinding cannot continue.");
            return new List<Node>();
        }

        // 🔹 Open and closed lists for A* search
        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();

        //Debug.Log("🚀 Starting A* pathfinding...");

        while (openSet.Count > 0)
        {
            // Find the node with the lowest fCost (total cost)
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                   (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // Debugging: Track nodes being checked
            // Debug.Log($"🔄 Checking node: {currentNode.worldPosition} (fCost: {currentNode.fCost})");

            // 🔹 If we reached the target, retrace and return the path
            if (currentNode == targetNode)
            {
                //Debug.Log("✅ Path found! Retracing path...");
                return RetracePath(startNode, targetNode);
            }

            // 🔹 Check all neighboring nodes
            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                {
                    //Debug.Log($"⛔ Skipping non-walkable or already checked node: {neighbor.worldPosition}");
                    continue;
                }

                int newMovementCost = currentNode.gCost + (int)(GetDistance(currentNode, neighbor) + neighbor.movementCost);
                if (newMovementCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                        //Debug.Log($"🟢 Adding node to open set: {neighbor.worldPosition} (gCost: {neighbor.gCost}, hCost: {neighbor.hCost})");
                    }
                }
            }
        }

        //Debug.LogError("❌ No valid path found!");
        return new List<Node>(); // No path found
    }

    // 🔹 Retrace the path from the target to the start
    private List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new List<Node>();
        Node currentNode = end;

        while (currentNode != start)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse(); // Reverse to get correct order
        //Debug.Log($"🔄 Path retraced! Total nodes in path: {path.Count}");
        return path;
    }

    // 🔹 Calculate distance between two nodes (Manhattan Distance)
    private int GetDistance(Node a, Node b)
    {
        int dstX = Mathf.Abs((int)a.worldPosition.x - (int)b.worldPosition.x);
        int dstY = Mathf.Abs((int)a.worldPosition.z - (int)b.worldPosition.z);

        // 🔹 Factor in Height Difference (More Costly for Steep Slopes)
        float heightDiff = Mathf.Abs(a.worldPosition.y - b.worldPosition.y);
        float heightPenalty = heightDiff * heightWeight; // Adjust multiplier based on difficulty

        return dstX + dstY + (int)heightPenalty;
    }

    // 🔹 Convert world position to a grid node
    private Node GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / grid.nodeSize);
        int y = Mathf.FloorToInt(worldPosition.z / grid.nodeSize);

        // Ensure the index is within bounds
        x = Mathf.Clamp(x, 0, grid.gridSize.x - 1);
        y = Mathf.Clamp(y, 0, grid.gridSize.y - 1);

        //Debug.Log($"📍 Converted World Position {worldPosition} to Grid Coordinates: ({x}, {y})");
        return grid.grid[x, y];
    }

    // 🔹 Get the neighboring nodes of a given node
    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        int gridX = Mathf.FloorToInt(node.worldPosition.x / grid.nodeSize);
        int gridY = Mathf.FloorToInt(node.worldPosition.z / grid.nodeSize);

        Vector2Int[] directions = {
        new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(0,-1), new Vector2Int(-1,0),
        new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1)
    };

        foreach (Vector2Int direction in directions)
        {
            int checkX = gridX + direction.x;
            int checkY = gridY + direction.y;

            if (checkX >= 0 && checkX < grid.gridSize.x && checkY >= 0 && checkY < grid.gridSize.y)
            {
                Node neighbor = grid.grid[checkX, checkY];

                if (neighbor.walkable)
                {
                    neighbors.Add(neighbor);
                }
            }
        }

        //Debug.Log($"📌 Node {node.worldPosition} has {neighbors.Count} neighbors.");
        return neighbors;
    }

}
