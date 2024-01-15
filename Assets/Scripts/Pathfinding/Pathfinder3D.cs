using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinder3D
{
    static Dictionary<Vector3Int, Node> nodeMap = new();

    public static void Initialize(byte[,,] map)
    {
        nodeMap = new();
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                for (int z = 0; z < map.GetLength(2); z++)
                {
                    Vector3Int location = new(x, y, z);
                    nodeMap.Add(location,
                        new Node { blocked = map[x,y,z] == 0,
                        location = location });
                }
            }
        }
    }

    public static List<Vector3Int> FindVectorPath(Vector3Int start, Vector3Int end)
    {
        List<Node> path = FindPath(start, end);
        if (path == null || path.Count == 0) return null;
        return path.Select(x => x.location).ToList();
    }

    public static int GetPathLength(Vector3Int start, Vector3Int end)
    {
        List<Node> path = FindPath(start, end);
        if (path != null) return path.Count;
        else return -1;
    }
    static List<Node> FindPath(Vector3Int startCoords, Vector3Int endCoords)
    {
        if (!nodeMap.TryGetValue(startCoords, out Node start) || !nodeMap.TryGetValue(endCoords, out Node end)) return null;
        List<Node> openList = new();
        List<Node> closeList = new();

        openList.Add(start);

        while (openList.Count > 0)
        {
            Node current = openList.OrderBy(x => x.F).First();

            openList.Remove(current);
            closeList.Add(current);

            if (current == end)
            {
                //finalize our path.
                return GetFinishedRoute(start, end);
            }

            var neighbors = GetNeighbors(current);

            foreach (Node neighbor in neighbors)
            {
                if (neighbor.blocked || closeList.Contains(neighbor))
                {
                    continue;
                }

                neighbor.G = GetTaxiDistance(start, neighbor);
                neighbor.H = GetTaxiDistance(end, neighbor);

                neighbor.previous = current;

                if (!openList.Contains(neighbor))
                {
                    openList.Add(neighbor);
                }
            }
        }

        return new List<Node>();
    }

    static List<Node> GetFinishedRoute(Node start, Node end)
    {
        List<Node> finishedList = new();

        Node current = end;

        while (current != start)
        {
            finishedList.Add(current);
            current = current.previous;
        }

        finishedList.Reverse();
        return finishedList;
    }

    static int GetTaxiDistance(Node start, Node neighbor)
    {
        return Mathf.Abs(start.location.x - neighbor.location.x) + Mathf.Abs(start.location.y - neighbor.location.y);
    }

    static List<Node> GetNeighbors(Node current)
    {
        List<Node> neighbors = new();
        foreach (var direction in directions)
        {
            Vector3Int locationCheck = direction + current.location;
            if (nodeMap.ContainsKey(locationCheck)) neighbors.Add(nodeMap[locationCheck]);
        }
        return neighbors;
    }
    static readonly Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.back, Vector3Int.forward, Vector3Int.left, Vector3Int.right };
}



class Node
{
    public int G;
    public int H;

    public int P;
    public int F { get { return G + H + P; } }

    public Vector3Int location;

    public bool blocked;

    public Node previous;
}
