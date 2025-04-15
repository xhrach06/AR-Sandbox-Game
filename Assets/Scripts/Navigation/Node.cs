using UnityEngine;
using System.Collections.Generic;
public class SimplePriorityQueue<T> where T : System.IComparable<T>
{
    private readonly List<T> items = new List<T>();

    public int Count => items.Count;

    public void Enqueue(T item)
    {
        items.Add(item);
        items.Sort();
    }

    public T Dequeue()
    {
        T first = items[items.Count - 1];
        items.RemoveAt(items.Count - 1);
        return first;
    }

    public void UpdateItem(T item)
    {
        items.Sort();
    }

    public bool Contains(T item) => items.Contains(item);
}


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

    public int CompareTo(Node other)
    {
        int compare = fCost.CompareTo(other.fCost);
        if (compare == 0)
            compare = hCost.CompareTo(other.hCost);
        return -compare; // lower fCost first
    }
}
