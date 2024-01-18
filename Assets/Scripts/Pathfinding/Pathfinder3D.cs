using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinder3D
{
    static Dictionary<Vector3Int, Node> nodeMap = new();
    public static LineRenderer lineRenderer;

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
                        new Node { blocked = map[x,y,z] == 1,
                        location = location });
                }
            }
        }
        foreach(Node node in nodeMap.Values)
        {
            node.neighbors = GetNeighbors(node);
        }
    }

    public static List<Vector3> FindVectorPath(Vector3Int start, Vector3Int end)
    {
        List<Node> path = FindPath(start, end);
        if (path == null || path.Count == 0) return null;
        List<Vector3> worldPath = path.Select(x => x.location.ToWorldVector()).ToList();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = worldPath.Count;
            lineRenderer.SetPositions(worldPath.ToArray());
        }
        return worldPath;
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
        if(end.blocked) return null;
        PriorityQueue<Node, int> openList = new();
        HashSet<Node> openHash = new();
        HashSet<Node> closeList = new();

        openList.Enqueue(start, start.G);
        openHash.Add(start);

        while (openList.Count > 0)
        {
            Node current = openList.Dequeue();
            closeList.Add(current);

            if (current == end)
            {
                //finalize our path.
                return GetFinishedRoute(start, end);
            }

            foreach (Node neighbor in current.neighbors)
            {
                if (neighbor.blocked || closeList.Contains(neighbor))
                {
                    continue;
                }

                neighbor.G = GetTaxiDistance(start, neighbor);

                neighbor.previous = current;

                if (!openHash.Contains(neighbor))
                {
                    openHash.Add(neighbor);
                    openList.Enqueue(neighbor, neighbor.G);
                }
            }
        }

        return new List<Node>();
    }

    static Node GetMinimum(HashSet<Node> set)
    {
        var ordered = set.OrderBy(x => x.G);
        /*var en = set.GetEnumerator();
        en.MoveNext();
        return en.Current;*/
        return ordered.FirstOrDefault();
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
        return System.Math.Abs(start.location.x - neighbor.location.x) + System.Math.Abs(start.location.y - neighbor.location.y) + System.Math.Abs(start.location.z - neighbor.location.z);
    }

    static List<Node> GetNeighbors(Node current)
    {
        List<Node> neighbors = new();
        for (int i = 0; i < directions.Length; i++)
        {
            Vector3Int direction = directions[i];
            Vector3Int locationCheck = direction + current.location;
            if (nodeMap.TryGetValue(locationCheck, out Node val)) neighbors.Add(val);
        }
        return neighbors;
    }
    static readonly Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.back, Vector3Int.forward, Vector3Int.left, Vector3Int.right };
}



class Node
{
    public int G;

    public Vector3Int location;

    public bool blocked;

    public Node previous;

    public List<Node> neighbors;
}
