using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Represents a single cell (node) in the pathfinding grid.
/// </summary>
public class Node : System.IComparable<Node>
{
    public Vector3 worldPosition;
    public bool walkable;
    public float movementCost;

    public int gCost;
    public int hCost;
    public Node parent;

    public int gridX;
    public int gridY;

    public int fCost => gCost + hCost;

    public Node(Vector3 worldPos, bool isWalkable, float moveCost, int x, int y)
    {
        worldPosition = worldPos;
        walkable = isWalkable;
        movementCost = moveCost;
        gridX = x;
        gridY = y;
    }

    /// <summary>
    /// Used by the priority queue to sort nodes by fCost and hCost.
    /// </summary>
    public int CompareTo(Node other)
    {
        int compare = fCost.CompareTo(other.fCost);
        if (compare == 0)
            compare = hCost.CompareTo(other.hCost);
        return -compare; // Reverse sort: lower fCost has higher priority
    }
}
