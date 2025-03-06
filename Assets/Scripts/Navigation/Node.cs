using UnityEngine;
using System.Collections.Generic;

public class Node
{
    public Vector3 worldPosition;
    public bool walkable;
    public float movementCost;  // Higher cost for hills
    public Node parent; // For retracing path

    public int gCost; // Distance from start node
    public int hCost; // Heuristic (estimated distance to target)
    public int fCost => gCost + hCost; // Total cost

    public Node(Vector3 position, bool isWalkable, float cost)
    {
        worldPosition = position;
        walkable = isWalkable;
        movementCost = cost;
    }
}
