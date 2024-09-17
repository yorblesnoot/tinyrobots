using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Events;

public enum MoveStyle
{
    FLY,
    CRAWL,
    WALK
}


public static class Pathfinder3D
{
    static Dictionary<Vector3Int, Node> nodeMap = new();
    public static int xSize, ySize, zSize;
    static byte[,,] byteMap;
    static Dictionary<MoveStyle, List<Vector3Int>> styleSpots;
    #region Initialization
    public static void Initialize(byte[,,] map)
    {
        byteMap = map;
        xSize = map.GetLength(0); ySize = map.GetLength(1); zSize = map.GetLength(2);
        nodeMap = new();
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                for (int z = 0; z < zSize; z++)
                {
                   ClassifyNode(x, y, z);
                }
            }
        }
        directionMagnitudes = new float[Directions.Length];
        for (int i = 0; i < Directions.Length; i++)
        {
            directionMagnitudes[i] = Directions[i].magnitude;
        }

        foreach (Node node in nodeMap.Values)
        {
            SetNeighbors(node);
        }
    }
    static void SetNeighbors(Node current)
    {
        current.edges = new();
        for (int i = 0; i < Directions.Length; i++)
        {
            Vector3Int direction = Directions[i];
            Vector3Int locationCheck = direction + current.location;
            if (nodeMap.TryGetValue(locationCheck, out Node val))
            {
                current.edges.Add(new() { neighbor = val, magnitude = directionMagnitudes[i] });
            }
        }
    }
    static void ClassifyNode(int x, int y, int z)
    {
        Vector3Int location = new(x, y, z);
        byte value = byteMap[x, y, z];
        Node node = new()
        {
            location = location,
            modeAccess = new bool[Enum.GetNames(typeof(MoveStyle)).Length]
        };
        foreach (MoveStyle style in Enum.GetValues(typeof(MoveStyle)))
        {
            node.modeAccess[(int)style] = false;
        }
        if (value == 1) { node.blocked = true; }
        else
        {
            if (NodeIsWalkable(x, y, z)) { node.modeAccess[(int)MoveStyle.WALK] = true; node.modeAccess[(int)MoveStyle.CRAWL] = true; }

            else if (NeighborIsTerrain(x, y - 1, z)
                || NeighborIsTerrain(x, y + 1, z) || NeighborIsTerrain(x - 1, y, z)
                || NeighborIsTerrain(x + 1, y, z) || NeighborIsTerrain(x, y, z + 1)
                || NeighborIsTerrain(x, y, z - 1)) node.modeAccess[(int)MoveStyle.CRAWL] = true;

            else if (!NeighborIsTerrain(x, y + 2, z)) node.modeAccess[(int)MoveStyle.FLY] = true;
        }

        nodeMap.Add(location, node);

        static bool NodeIsWalkable(int x, int y, int z)
        {
            if (!NeighborIsTerrain(x, y - 1, z)) return false;
            for (int ix = -1; ix < 2; ix++)
            {
                if (NeighborIsTerrain(x + ix, y + 1, z)) return false;
            }
            for (int iz = -1; iz < 2; iz++)
            {
                if (NeighborIsTerrain(x, y + 1, z + iz)) return false;
            }
            return true;
        }

        static bool NeighborIsTerrain(int x, int y, int z)
        {
            return !PointIsOffMap(x, y, z) && byteMap[x, y, z] == 1;
        }
    }
    public static Vector3Int[] Directions = {
            new(-1, 0, 0), new(1, 0, 0),    // Faces along x-axis
            new(0, -1, 0), new(0, 1, 0),    // Faces along y-axis
            new(0, 0, -1), new(0, 0, 1),    // Faces along z-axis
            new(-1, -1, 0), new(1, -1, 0),  // Edges along xy-plane
            new(-1, 1, 0), new(1, 1, 0),    // Edges along xy-plane
            new(-1, 0, -1), new(1, 0, -1),  // Edges along xz-plane
            new(0, -1, -1), new(0, 1, -1),  // Edges along yz-plane
            new(-1, -1, -1), new(1, -1, -1),  // Corners
            new(-1, 1, -1), new(1, 1, -1),    // Corners
            new(-1, -1, 1), new(1, -1, 1),    // Corners
            new(-1, 1, 1), new(1, 1, 1)        // Corners
    };
    static float[] directionMagnitudes;
    #endregion
    

    #region Path Services
    public static void GeneratePathingTree(MoveStyle style, Vector3 position)
    {
        Vector3Int startCoords = Vector3Int.RoundToInt(position);
        if (!nodeMap.TryGetValue(startCoords, out Node start)) return;
        EvaluateNodeOccupancy(position);

        HashSet<Node> visited = new();
        foreach (Node node in nodeMap.Values)
        {
            node.G = float.PositiveInfinity;
            node.parent = null;
        }
        Queue<Node> frontier = new();
        //PriorityQueue<Node, float> frontier = new();
        //frontier.EnsureCapacity(xSize * ySize * zSize / 4);

        start.G = 0;

        frontier.Enqueue(start);
        //frontier.Enqueue(start, start.G);

        int nodeCount = nodeMap.Values.Count;
        while (visited.Count < nodeCount)
        {
            if (frontier.Count == 0) return;
            Node current = frontier.Dequeue();
            visited.Add(current);
            if (current.G == float.PositiveInfinity) return;

            foreach (Node.Edge edge in current.edges)
            {
                Node neighbor = edge.neighbor;
                if (neighbor.blocked || neighbor.occupied || neighbor.modeAccess[(int)style] == false || visited.Contains(neighbor)) continue;
                float possibleG = current.G + edge.magnitude;
                if (possibleG < neighbor.G)
                {
                    neighbor.G = possibleG;
                    neighbor.parent = current;
                    frontier.Enqueue(neighbor);
                    //frontier.Enqueue(neighbor, neighbor.G);
                }
            }
        }
    }
    public static List<Vector3> FindVectorPath(Vector3Int end, out List<float> gValues)
    {
        gValues = new();
        List<Vector3> worldPath = new();
        List<Node> path = FindPath(end);
        if (path == null || path.Count == 0) return null;
        foreach (var node in path)
        {
            worldPath.Add(node.location.ToWorldVector());
            gValues.Add(node.G);
        }
        return worldPath;
    }
    public static List<Vector3Int> GetPathableLocations(int moveBudget)
    {
        return nodeMap.Values.Where(node => node.G <= moveBudget).OrderBy(node => node.G).Select(node => node.location).ToList();
    }
    public static List<Vector3Int> GetPathableLocations()
    {
        return nodeMap.Values.Where(node => node.G < float.PositiveInfinity).Select(node => node.location).ToList();
    }
    public static List<Vector3Int> GetCompatibleLocations(Vector3 position, float range, MoveStyle style)
    {
        return styleSpots[style].Where(x => Vector3.Distance(position, x) < range).ToList();
    } 
    #endregion

    #region Map Services
    public static Dictionary<MoveStyle, List<Vector3Int>> GetStyleNodes()
    {
        if(styleSpots != null) return styleSpots;
        styleSpots = new();
        foreach (MoveStyle style in Enum.GetValues(typeof(MoveStyle))) styleSpots.Add(style, new());
        foreach (var node in nodeMap.Values)
        {
            foreach (MoveStyle style in Enum.GetValues(typeof(MoveStyle))) if (node.modeAccess[(int)style]) styleSpots[style].Add(node.location);
        }
        return styleSpots;
    }
    
    public static bool GetLandingPointBy(Vector3 target, MoveStyle style, out Vector3Int coords)
    {
        coords = Vector3Int.RoundToInt(target);
        if (!nodeMap.ContainsKey(coords)) return false;
        if (nodeMap[coords].modeAccess[(int)style]) return true;
        else
        {
            foreach (var edge in nodeMap[coords].edges)
            {
                if (!nodeMap[edge.neighbor.location].modeAccess[(int)style]) continue;
                coords = edge.neighbor.location;
                return true;
            }
            return false;
        }
    }
    public static Vector3 GetCrawlOrientation(Vector3Int node)
    {
        Vector3 total = Vector3.zero;
        int number = 0;
        Node source = nodeMap[node];
        foreach(var edge in source.edges)
        {
            if (!edge.neighbor.blocked) continue;

            total += edge.neighbor.location - source.location;
            number++;
        }
        return -total/number;
    }
    public static bool PointIsOffMap(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0) return true;
        if (x >= xSize || y >= ySize || z >= zSize) return true;
        return false;
    }
    #endregion

    #region Occupancy
    static List<Vector3Int> lastOccupied = new();
    public static UnityEvent<Vector3> GetOccupancy = new();
    public static void EvaluateNodeOccupancy(Vector3 position)
    {
        ClearOccupancy();
        GetOccupancy.Invoke(position);
    }

    private static void ClearOccupancy()
    {
        foreach (var node in lastOccupied) SetNodeOccupancy(node, false);
        lastOccupied = new();
    }

    public static void SetNodeOccupancy(Vector3Int position, bool status)
    {
        if(status) lastOccupied.Add(position);
        Node node = nodeMap[position];
        node.occupied = status;
        foreach(var edge in node.edges)
        {
            edge.neighbor.occupied = status;
        }
    }
    #endregion

    #region Jobs
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
    #endregion
    static List<Node> FindPath(Vector3Int endCoords)
    {
        List<Node> finishedList = new();
        if (!nodeMap.TryGetValue(endCoords, out var currentNode)) return null;


        while (currentNode.parent != null)
        {
            finishedList.Add(currentNode);
            currentNode = currentNode.parent;
        }

        finishedList.Reverse();
        return finishedList;
    }
    class Node
    {
        public float G = float.PositiveInfinity;

        public Vector3Int location;

        public bool blocked;
        public bool occupied;
        public bool[] modeAccess;

        public Node parent;

        public class Edge
        {
            public Node neighbor;
            public float magnitude;
        }
        public List<Edge> edges;
    }
}



