using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MoveStyle
{
    FLY,
    CRAWL,
    WALK
}
public static class Pathfinder3D
{
    static Dictionary<Vector3Int, PathfindingNode> nodeMap = new();
    public static LineRenderer lineRenderer;
    static int xSize, ySize, zSize;

    static byte[,,] coreMap;

    public static void Initialize(byte[,,] map)
    {
        coreMap = map;
        xSize = map.GetLength(0); ySize = map.GetLength(1); zSize = map.GetLength(2);
        nodeMap = new();
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                   EvaluateNode(x, y, z);
                }
            }
        }
        foreach(PathfindingNode node in nodeMap.Values)
        {
            node.neighbors = GetNeighbors(node);
        }
    }

    static void EvaluateNode(int x, int y, int z)
    {
        Vector3Int location = new(x, y, z);
        byte value = coreMap[x, y, z];
        PathfindingNode node = new()
        {
            location = location,
            modeAccess = new()
        };
        foreach(MoveStyle style in Enum.GetValues(typeof(MoveStyle)))
        {
            node.modeAccess.Add(style, false);
        }
        if (value == 1) { node.blocked = true;}
        else if (NeighborIsTerrain(x, y - 1, z)) {node.modeAccess[MoveStyle.WALK] = true; node.modeAccess[MoveStyle.CRAWL] = true; }
        else if (NeighborIsTerrain(x, y + 1, z) || NeighborIsTerrain(x-1, y, z) 
            || NeighborIsTerrain(x + 1, y, z) || NeighborIsTerrain(x, y, z + 1) 
            || NeighborIsTerrain(x, y, z-1)) node.modeAccess[MoveStyle.CRAWL] = true;
        else node.modeAccess[MoveStyle.FLY] = true;
        nodeMap.Add(location, node);

        static bool NeighborIsTerrain(int x, int y, int z)
        {
            return !PointIsOffMap(x, y, z) && coreMap[x, y, z] == 1;
        }
    }

    static bool PointIsOffMap(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0) return true;
        if (x >= xSize || y >= ySize || z >= zSize) return true;
        return false;
    }

    public static List<Vector3> FindVectorPath(MoveStyle style, Vector3Int start, Vector3Int end)
    {
        List<PathfindingNode> path = FindPath(style, start, end);
        if (path == null || path.Count == 0) return null;
        List<Vector3> worldPath = path.Select(x => x.location.ToWorldVector()).ToList();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = worldPath.Count;
            lineRenderer.SetPositions(worldPath.ToArray());
        }
        return worldPath;
    }

    static List<PathfindingNode> FindPath(MoveStyle style, Vector3Int startCoords, Vector3Int endCoords)
    {
        if (!nodeMap.TryGetValue(startCoords, out PathfindingNode start) || !nodeMap.TryGetValue(endCoords, out PathfindingNode end)) return null;
        if(end.blocked) return null;
        PriorityQueue<PathfindingNode, float> openList = new();
        HashSet<PathfindingNode> openHash = new();
        HashSet<PathfindingNode> closeList = new();

        start.G = 0;

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
                if (neighbor.blocked || neighbor.modeAccess[style] == false || closeList.Contains(neighbor))
                {
                    continue;
                }
                neighbor.H = Vector3.Distance(neighbor.location, end.location);
                float possibleG = current.G + 1;
                if (possibleG < neighbor.G)


                neighbor.G = possibleG;
                neighbor.previous = current;


                if (!openHash.Contains(neighbor))
                {
                    openHash.Add(neighbor);
                    openList.Enqueue(neighbor, neighbor.F);
                }
            }
        }

        return new List<PathfindingNode>();
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
    static Vector3Int[] directions = {
            new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0),    // Faces along x-axis
            new Vector3Int(0, -1, 0), new Vector3Int(0, 1, 0),    // Faces along y-axis
            new Vector3Int(0, 0, -1), new Vector3Int(0, 0, 1),    // Faces along z-axis
            new Vector3Int(-1, -1, 0), new Vector3Int(1, -1, 0),  // Edges along xy-plane
            new Vector3Int(-1, 1, 0), new Vector3Int(1, 1, 0),    // Edges along xy-plane
            new Vector3Int(-1, 0, -1), new Vector3Int(1, 0, -1),  // Edges along xz-plane
            new Vector3Int(0, -1, -1), new Vector3Int(0, 1, -1),  // Edges along yz-plane
            new Vector3Int(-1, -1, -1), new Vector3Int(1, -1, -1),  // Corners
            new Vector3Int(-1, 1, -1), new Vector3Int(1, 1, -1),    // Corners
            new Vector3Int(-1, -1, 1), new Vector3Int(1, -1, 1),    // Corners
            new Vector3Int(-1, 1, 1), new Vector3Int(1, 1, 1)        // Corners
        };
}



class PathfindingNode
{
    public float G = float.PositiveInfinity;
    public float H;
    public float F { get { return G+H; } }

    public Vector3Int location;

    public bool blocked;
    public Dictionary<MoveStyle, bool> modeAccess;

    public PathfindingNode previous;

    public List<PathfindingNode> neighbors;
}
