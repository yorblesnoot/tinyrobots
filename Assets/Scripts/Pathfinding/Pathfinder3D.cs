using System;
using System.Collections.Generic;
using System.Linq;
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
    public static int XSize, YSize, ZSize;
    static byte[,,] byteMap;
    public static UnityEvent MapInitialized = new();
    #region Initialization
    public static void Initialize(byte[,,] map)
    {
        byteMap = map;
        XSize = map.GetLength(0); YSize = map.GetLength(1); ZSize = map.GetLength(2);
        nodeMap = new();
        for (int x = 0; x < XSize; x++)
        {
            for (int y = 0; y < YSize; y++)
            {
                for (int z = 0; z < ZSize; z++)
                {
                   ClassifyNode(x, y, z);
                }
            }
        }
        directionMagnitudes = Directions.Select(d => d.magnitude).ToArray();
        foreach (Node node in nodeMap.Values) SetNeighbors(node);
        MapInitialized.Invoke();
    }
    static void SetNeighbors(Node current)
    {
        current.Edges = new();
        for (int i = 0; i < Directions.Length; i++)
        {
            Vector3Int direction = Directions[i];
            Vector3Int locationCheck = direction + current.Location;
            if (nodeMap.TryGetValue(locationCheck, out Node val))
            {
                current.Edges.Add(new() { Neighbor = val, Magnitude = directionMagnitudes[i] });
            }
        }
    }
    static void ClassifyNode(int x, int y, int z)
    {
        Vector3Int location = new(x, y, z);
        byte value = byteMap[x, y, z];
        Node node = new()
        {
            Location = location,
            StyleAccess = new bool[Enum.GetNames(typeof(MoveStyle)).Length]
        };
        //foreach (MoveStyle style in Enum.GetValues(typeof(MoveStyle))) node.StyleAccess[(int)style] = false;
        if (value == 1) node.Terrain = true;
        else
        {
            if (NodeIsWalkable(x, y, z)) { node.StyleAccess[(int)MoveStyle.WALK] = true; node.StyleAccess[(int)MoveStyle.CRAWL] = true; }

            else if (NeighborIsTerrain(x, y - 1, z)
                || NeighborIsTerrain(x, y + 1, z) || NeighborIsTerrain(x - 1, y, z)
                || NeighborIsTerrain(x + 1, y, z) || NeighborIsTerrain(x, y, z + 1)
                || NeighborIsTerrain(x, y, z - 1)) node.StyleAccess[(int)MoveStyle.CRAWL] = true;

            else if (!NeighborIsTerrain(x, y + 2, z)) node.StyleAccess[(int)MoveStyle.FLY] = true;
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

        foreach (Node node in nodeMap.Values)
        {
            node.G = float.PositiveInfinity;
            node.Parent = null;
            node.Visited = false;
        }
        Queue<Node> frontier = new();
        //PriorityQueue<Node, float> frontier = new();
        //frontier.EnsureCapacity(xSize * ySize * zSize / 4);

        start.G = 0;

        frontier.Enqueue(start);
        //frontier.Enqueue(start, start.G);

        while (frontier.Count > 0)
        {
            Node current = frontier.Dequeue();
            current.Visited = true;
            if (current.G == float.PositiveInfinity) return;

            foreach (Node.Edge edge in current.Edges)
            {
                Node neighbor = edge.Neighbor;
                if (neighbor.Terrain || neighbor.Occupied || neighbor.Visited || neighbor.StyleAccess[(int)style] == false) continue;
                float possibleG = current.G + edge.Magnitude;
                if (possibleG < neighbor.G)
                {
                    neighbor.G = possibleG;
                    neighbor.Parent = current;
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
            worldPath.Add(node.Location.ToWorldVector());
            gValues.Add(node.G);
        }
        return worldPath;
    }
    public static List<Vector3Int> GetPathableLocations(int moveBudget)
    {
        return nodeMap.Values.Where(node => node.G <= moveBudget).OrderBy(node => node.G).Select(node => node.Location).ToList();
    }
    public static List<Vector3Int> GetPathableLocations()
    {
        return nodeMap.Values.Where(node => node.G < float.PositiveInfinity).Select(node => node.Location).ToList();
    }
    public static List<Vector3Int> GetCompatibleLocations(Vector3 position, float range, MoveStyle style)
    {
        return nodeMap.Values.Where(x => x.StyleAccess[(int)style] 
        && Vector3.Distance(position, x.Location) < range).Select(n => n.Location).ToList();
    } 
    #endregion

    #region Map Services

    public static List<MoveStyle> GetNodeStyles(Vector3Int position)
    {
        if (PointIsOffMap(position.x, position.y, position.z)) return new(); //problem here
        Node node = nodeMap[position];
        List<MoveStyle> styles = new();
        for(int i = 0; i < node.StyleAccess.Count(); i++)
        {
            if (node.StyleAccess[i]) styles.Add((MoveStyle)i);
        }
        return styles;
    }
    
    public static bool GetLandingPointBy(Vector3 target, MoveStyle style, out Vector3Int coords)
    {
        coords = Vector3Int.RoundToInt(target);
        if (!nodeMap.ContainsKey(coords)) return false;
        if (nodeMap[coords].StyleAccess[(int)style]) return true;
        else
        {
            foreach (var edge in nodeMap[coords].Edges)
            {
                if (!nodeMap[edge.Neighbor.Location].StyleAccess[(int)style]) continue;
                coords = edge.Neighbor.Location;
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
        foreach(var edge in source.Edges)
        {
            if (!edge.Neighbor.Terrain) continue;

            total += edge.Neighbor.Location - source.Location;
            number++;
        }
        return -total/number;
    }
    public static bool PointIsOffMap(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0) return true;
        if (x >= XSize || y >= YSize || z >= ZSize) return true;
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
        node.Occupied = status;
        foreach(var edge in node.Edges)
        {
            edge.Neighbor.Occupied = status;
        }
    }
    #endregion

    static List<Node> FindPath(Vector3Int endCoords)
    {
        List<Node> finishedList = new();
        if (!nodeMap.TryGetValue(endCoords, out var currentNode)) return null;


        while (currentNode.Parent != null)
        {
            finishedList.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        finishedList.Reverse();
        return finishedList;
    }
    class Node
    {
        public float G = float.PositiveInfinity;

        public Vector3Int Location;

        public bool Visited;
        public bool Terrain;
        public bool Occupied;
        public bool[] StyleAccess;

        public Node Parent;

        public class Edge
        {
            public Node Neighbor;
            public float Magnitude;
        }
        public List<Edge> Edges;
    }
}



