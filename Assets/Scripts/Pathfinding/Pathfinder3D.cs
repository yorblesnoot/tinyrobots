using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
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
        directionMagnitudes = new float[directions.Length];
        for (int i = 0; i < directions.Length; i++)
        {
            directionMagnitudes[i] = directions[i].magnitude;
        }

        foreach (PathfindingNode node in nodeMap.Values)
        {
            SetNeighbors(node);
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
        if (value == 1) { node.blocked = true; }
        else if (NeighborIsTerrain(x, y - 1, z)) { node.modeAccess[MoveStyle.WALK] = true; node.modeAccess[MoveStyle.CRAWL] = true; }
        else if (NeighborIsTerrain(x, y + 1, z) || NeighborIsTerrain(x - 1, y, z)
            || NeighborIsTerrain(x + 1, y, z) || NeighborIsTerrain(x, y, z + 1)
            || NeighborIsTerrain(x, y, z - 1)) node.modeAccess[MoveStyle.CRAWL] = true;
        else if (!NeighborIsTerrain(x, y + 2, z)) node.modeAccess[MoveStyle.FLY] = true;
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
    
    public static Dictionary<MoveStyle, List<Vector3>> GetStyleNodes()
    {
        Dictionary<MoveStyle, List<Vector3>> styleSpots = new();
        foreach (MoveStyle style in Enum.GetValues(typeof(MoveStyle))) styleSpots.Add(style, new());
        foreach(var node in nodeMap.Values)
        {
            foreach (MoveStyle style in Enum.GetValues(typeof(MoveStyle))) if (node.modeAccess[style]) styleSpots[style].Add(node.location);
        }
        return styleSpots;
    }

    public static List<Vector3Int> GetPathableLocations(int moveBudget)
    {
        return nodeMap.Values.Where(node => node.G <= moveBudget).OrderBy(node => node.G).Select(node => node.location).ToList();
    }

    public static List<Vector3> FindVectorPath(Vector3Int end, out float distance)
    {
        distance = 0;
        List<PathfindingNode> path = FindPath(end);
        if (path == null || path.Count == 0) return null;
        List<Vector3> worldPath = path.Select(x => x.location.ToWorldVector()).ToList();
        distance = nodeMap[end].G;
        return worldPath;
    }

    public static void GeneratePathingTree(MoveStyle style, Vector3Int startCoords)
    {
        if (!nodeMap.TryGetValue(startCoords, out PathfindingNode start)) return;

        HashSet<PathfindingNode> unvisited = new();
        foreach (PathfindingNode node in nodeMap.Values)
        {
            node.G = float.PositiveInfinity;
            node.parent = null;
            unvisited.Add(node);
        }
        PriorityQueue<PathfindingNode, float> frontier = new();
        frontier.EnsureCapacity(xSize * ySize * zSize/4);

        start.G = 0;

        frontier.Enqueue(start, start.G);

        while (unvisited.Count > 0)
        {
            if (frontier.Count == 0) return;
            PathfindingNode current = frontier.Dequeue();
            unvisited.Remove(current);
            if (current.G == float.PositiveInfinity) return;
            //if (current.G > maxDistance) continue;

            foreach (PathfindingNode.Edge edge in current.neighbors)
            {
                PathfindingNode neighbor = edge.neighbor;
                if (neighbor.blocked || neighbor.modeAccess[style] == false || !unvisited.Contains(neighbor)) continue;
                float possibleG = current.G + edge.magnitude;
                if (possibleG < neighbor.G)
                {
                    neighbor.G = possibleG;
                    neighbor.parent = current;
                    frontier.Enqueue(neighbor, neighbor.G);
                }
            }
        }
    }

    public static void GeneratePathingTreeWithJob()
    {
        var job = new PathmapJob();
        job.Schedule();
    }
    public struct PathmapJob : IJob
    {
        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
    static List<PathfindingNode> FindPath(Vector3Int endCoords)
    {
        List<PathfindingNode> finishedList = new();
        if (!nodeMap.TryGetValue(endCoords, out var currentNode)) return null;


        while (currentNode.parent != null)
        {
            finishedList.Add(currentNode);
            currentNode = currentNode.parent;
        }

        finishedList.Reverse();
        return finishedList;
    }
    static void SetNeighbors(PathfindingNode current)
    {
        current.neighbors = new();
        for (int i = 0; i < directions.Length; i++)
        {
            Vector3Int direction = directions[i];
            Vector3Int locationCheck = direction + current.location;
            if (nodeMap.TryGetValue(locationCheck, out PathfindingNode val))
            {
                current.neighbors.Add(new() { neighbor = val, magnitude = directionMagnitudes[i] });
            }
        }
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
    static float[] directionMagnitudes;
}


class PathfindingNode
{
    public float G = float.PositiveInfinity;

    public Vector3Int location;

    public bool blocked;
    public Dictionary<MoveStyle, bool> modeAccess;

    public PathfindingNode parent;

    public class Edge
    {
        public PathfindingNode neighbor;
        public float magnitude;
    }
    public List<Edge> neighbors;
}
