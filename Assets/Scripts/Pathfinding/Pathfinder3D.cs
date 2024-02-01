using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Pathfinder3D
{
    static Dictionary<Vector3Int, PathfindingNode> nodeMap = new();
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
                        new PathfindingNode { blocked = map[x,y,z] == 1,
                        location = location });
                }
            }
        }
        foreach(PathfindingNode node in nodeMap.Values)
        {
            node.neighbors = GetNeighbors(node);
        }
    }

    public static List<Vector3> FindVectorPath(Vector3Int start, Vector3Int end)
    {
        List<PathfindingNode> path = FindPath(start, end);
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
        List<PathfindingNode> path = FindPath(start, end);
        if (path != null) return path.Count;
        else return -1;
    }
    static List<PathfindingNode> FindPath(Vector3Int startCoords, Vector3Int endCoords)
    {
        if (!nodeMap.TryGetValue(startCoords, out PathfindingNode start) || !nodeMap.TryGetValue(endCoords, out PathfindingNode end)) return null;
        if(end.blocked) return null;
        PriorityQueue<PathfindingNode, int> openList = new();
        HashSet<PathfindingNode> openHash = new();
        HashSet<PathfindingNode> closeList = new();

        openList.Enqueue(start, start.G);
        openHash.Add(start);

        while (openList.Count > 0)
        {
            PathfindingNode current = openList.Dequeue();
            closeList.Add(current);

            if (current == end)
            {
                //finalize our path.
                return GetFinishedRoute(start, end);
            }

            foreach (PathfindingNode neighbor in current.neighbors)
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

        return new List<PathfindingNode>();
    }

    static PathfindingNode GetMinimum(HashSet<PathfindingNode> set)
    {
        var ordered = set.OrderBy(x => x.G);
        /*var en = set.GetEnumerator();
        en.MoveNext();
        return en.Current;*/
        return ordered.FirstOrDefault();
    }

    static List<PathfindingNode> GetFinishedRoute(PathfindingNode start, PathfindingNode end)
    {
        List<PathfindingNode> finishedList = new();

        PathfindingNode current = end;

        while (current != start)
        {
            finishedList.Add(current);
            current = current.previous;
        }

        finishedList.Reverse();
        return finishedList;
    }

    static int GetTaxiDistance(PathfindingNode start, PathfindingNode neighbor)
    {
        return System.Math.Abs(start.location.x - neighbor.location.x) + System.Math.Abs(start.location.y - neighbor.location.y) + System.Math.Abs(start.location.z - neighbor.location.z);
    }

    static List<PathfindingNode> GetNeighbors(PathfindingNode current)
    {
        List<PathfindingNode> neighbors = new();
        for (int i = 0; i < directions.Length; i++)
        {
            Vector3Int direction = directions[i];
            Vector3Int locationCheck = direction + current.location;
            if (nodeMap.TryGetValue(locationCheck, out PathfindingNode val)) neighbors.Add(val);
        }
        return neighbors;
    }
    static readonly Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.back, Vector3Int.forward, Vector3Int.left, Vector3Int.right };
}



class PathfindingNode
{
    public int G;

    public Vector3Int location;

    public bool blocked;

    public PathfindingNode previous;

    public List<PathfindingNode> neighbors;
}
