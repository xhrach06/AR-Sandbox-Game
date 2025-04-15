using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// A simple priority queue for A* using a sorted list.
/// </summary>
public class SimplePriorityQueue<T> where T : System.IComparable<T>
{
    private readonly List<T> items = new();

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

