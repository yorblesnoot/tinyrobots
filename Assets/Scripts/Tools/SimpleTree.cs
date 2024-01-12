using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleTree<T>
{
    Dictionary<T, List<T>> tree = new();

    public void AddNode(T node)
    {
        tree.Add(node, new());
    }

    public void AddChildren(T node, List<T> children)
    {
        tree[node] = children;
    }

    public List<T> GetChildren(T node)
    {
        return tree[node];
    }

    public T GetOrigin()
    {
        List<T> sorted = tree.Keys.OrderByDescending(x => GetTotalChildren(x)).ToList();
        return sorted[0];
    }

    int GetTotalChildren(T node)
    {
        int count = 0;
        RecursiveCount(node);
        return count;

        void RecursiveCount(T node)
        {
            List<T> children = tree[node];
            if (children.Count == 0) return;
            foreach(T child in children)
            {
                count++;
                RecursiveCount(child);
            }
        }
    }
}
