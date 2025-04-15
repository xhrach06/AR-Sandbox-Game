using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
    public GridManager grid;
    public int heightWeight = 10;

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Node startNode = GetNodeFromWorldPosition(startPos);
        Node targetNode = GetNodeFromWorldPosition(targetPos);

        if (startNode == null || targetNode == null || !startNode.walkable || !targetNode.walkable)
        {
            Debug.LogWarning("❌ Invalid start or target node for pathfinding.");
            return new List<Node>();
        }

        var openSet = new SimplePriorityQueue<Node>();
        var closedSet = new HashSet<Node>();

        openSet.Enqueue(startNode);
        startNode.gCost = 0;
        startNode.hCost = GetDistance(startNode, targetNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Dequeue();
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int moveCost = currentNode.gCost + GetDistance(currentNode, neighbor) + Mathf.RoundToInt(neighbor.movementCost);

                if (moveCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = moveCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor);
                    else
                        openSet.UpdateItem(neighbor); // Re-sort if fCost improved
                }
            }
        }

        Debug.LogWarning("❌ No valid path found!");
        return new List<Node>();
    }

    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node current = endNode;

        while (current != startNode)
        {
            path.Add(current);
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private int GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);

        float heightDiff = Mathf.Abs(a.worldPosition.y - b.worldPosition.y);
        int heightPenalty = Mathf.RoundToInt(heightDiff * heightWeight);

        return dx + dy + heightPenalty;
    }

    private Node GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x / grid.nodeSize);
        int y = Mathf.FloorToInt(worldPosition.z / grid.nodeSize);

        x = Mathf.Clamp(x, 0, grid.gridSize.x - 1);
        y = Mathf.Clamp(y, 0, grid.gridSize.y - 1);

        return grid.grid[x, y];
    }

    private List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();
        Vector2Int[] dirs = {
            new(0, 1), new(1, 0), new(0, -1), new(-1, 0),
            new(1, 1), new(1, -1), new(-1, 1), new(-1, -1)
        };

        foreach (var dir in dirs)
        {
            int checkX = node.gridX + dir.x;
            int checkY = node.gridY + dir.y;

            if (checkX >= 0 && checkX < grid.gridSize.x && checkY >= 0 && checkY < grid.gridSize.y)
            {
                Node neighbor = grid.grid[checkX, checkY];
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }
}
