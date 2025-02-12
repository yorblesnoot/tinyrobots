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
        lastOccupied = new();
        directionMagnitudes = Directions.Select(d => d.magnitude).ToArray();
        foreach (Node node in nodeMap.Values) SetNeighbors(node);
        FloodWalkable();
        MapInitialized.Invoke();
        MapInitialized.RemoveAllListeners();
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
            /*
            for (int ix = -1; ix < 2; ix++)
            {
                if (NeighborIsTerrain(x + ix, y + 1, z)) return false;
            }
            for (int iz = -1; iz < 2; iz++)
            {
                if (NeighborIsTerrain(x, y + 1, z + iz)) return false;
            }
            */
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
        //Queue<Node> frontier = new();
        PriorityQueue<Node, float> frontier = new();
        //frontier.EnsureCapacity(xSize * ySize * zSize / 4);

        start.G = 0;

        //frontier.Enqueue(start);
        frontier.Enqueue(start, start.G);

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
                    //frontier.Enqueue(neighbor);
                    frontier.Enqueue(neighbor, neighbor.G);
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

    public static List<Vector3Int> GetDashTargets(Vector3 position, float range, MoveStyle style, bool approach = true)
    {
        List<Node> compatible = SearchSphere(Vector3Int.RoundToInt(position), Mathf.FloorToInt(range)).Where(x => x.StyleAccess[(int)style]).ToList();
        return compatible.Select(n => n.Location).ToList();
    }

    public static List<Vector3Int> FilterByWalkAccessible(Vector3Int nearestTarget, float abilityRange, List<Vector3Int> nearPositions)
    {
        //find nodes within ability range of nearest enemy
        List<Node> enemyRange = SearchSphere(nearestTarget, Mathf.FloorToInt(abilityRange));

        //get flood zones of those nodes
        HashSet<int> targetZones = enemyRange.Select(n => n.FloodZone).ToHashSet();

        //filter nearby nodes by the found flood zones
        nearPositions = nearPositions.Where(p => targetZones.Contains(nodeMap[p].FloodZone)).ToList();

        //order those nodes by distance from the user
        return nearPositions;
    }

    static List<Node> SearchSphere(Vector3Int source, int radius)
    {
        List<Node> output = new();
        for (int x = source.x - radius; x <= source.x + radius; x++)
        {
            for (int y = source.y - radius; y <= source.y + radius; y++)
            {
                for (int z = source.z - radius; z <= source.z + radius; z++)
                {
                    Vector3Int checkedPosition = new(x, y, z);
                    if (Vector3.Distance(checkedPosition, source) > radius) continue;
                    if (nodeMap.TryGetValue(checkedPosition, out Node node)) output.Add(node);
                }
            }
        }
        return output;
    }

    #endregion

    #region Map Services

    public static HashSet<MoveStyle> GetNodeStyles(Vector3Int position)
    {
        if (PointIsOffMap(position.x, position.y, position.z)) return new(); //problem here
        Node node = nodeMap[position];
        HashSet<MoveStyle> styles = new();
        for(int i = 0; i < node.StyleAccess.Count(); i++)
        {
            if (node.StyleAccess[i]) styles.Add((MoveStyle)i);
        }
        return styles;
    }

    public static bool GetBestApproachPath(Vector3Int target, int radius, MoveStyle style, out Vector3Int point)
    {
        List<Node> zone = SearchSphere(target, radius);
        var positions = zone.Where(n => n.StyleAccess[(int)style]).OrderBy(n => n.G/2 + Vector3.Distance(target, n.Location)).Select(n => n.Location);
        point = positions.FirstOrDefault();
        if(positions.Count() == 0) return false;
        return true;
    }
    
    public static bool GetLandingPointBy(Vector3 target, MoveStyle style, out Vector3Int coords)
    {
        coords = Vector3Int.RoundToInt(target);
        if (!nodeMap.TryGetValue(coords, out Node origin)) return false;
        List<Node> nodes = origin.Edges.Select(edge => edge.Neighbor).ToList();
        nodes.Add(origin);
        List<Vector3Int> candidates = new();
        foreach (var node in nodes)
        {
            if(node.Occupied || node.Terrain || !node.IsAccessible(style)) continue;
            candidates.Add(node.Location);
        }
        if(candidates.Count == 0) return false;
        coords = candidates.OrderBy(place => Vector3.Distance(place, target)).First();
        return true;
    }

    static readonly int crawlRadius = 3;

    public static Vector3 GetCrawlOrientation(Vector3 source)
    {
        List<Vector3> directions = new();
        int layerMask = LayerMask.GetMask("Terrain");
        GetLandingPointBy(source, MoveStyle.CRAWL, out Vector3Int sourceNode);
        List<Node> sphere = SearchSphere(sourceNode, crawlRadius);
        foreach (var node in sphere)
        {
            Vector3 direction = (node.Location - source).normalized;
            if (!Physics.Raycast(source, direction, out RaycastHit hit, crawlRadius, layerMask)) continue;
            Vector3 maximum = source + direction * crawlRadius;
            Vector3 priority = hit.point - maximum;
            Debug.DrawLine(source, maximum, Color.blue, 1f);

            directions.Add(priority);
        }
        Vector3 final = directions.Average();
        final.Normalize();
        Debug.DrawLine(source, source + final, Color.red, 1f);
        return final;
    }

    /*public static Vector3 GetCrawlOrientation(Vector3 source)
    {
        Vector3 total = Vector3.zero;
        int number = 0;
        Vector3Int node = Vector3Int.RoundToInt(source);
        List<Node> sphere = SearchSphere(node, crawlRadius);
        foreach(var neighbor in sphere)
        {
            if (!neighbor.Terrain) continue;

            Vector3 offset = neighbor.Location - source;
            total += offset.normalized;
            number++;
        }
        return -total/number;
    }*/
    public static bool PointIsOffMap(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0) return true;
        if (x >= XSize || y >= YSize || z >= ZSize) return true;
        return false;
    }
    #endregion

    #region Occupancy
    static List<Vector3Int> lastOccupied = new();
    public static UnityEvent<Vector3[]> GetOccupancy = new();
    /// <summary>
    /// Causes all active units to evaluate their current occupancy except for the unit at position, blocking pathfinding around those units.
    /// </summary>
    /// <param name="position"></param>
    public static void EvaluateNodeOccupancy(params Vector3[] position)
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

    static void FloodWalkable()
    {
        int activeZone = 0;

        foreach (Node node in nodeMap.Values)
        {
            StartFlood(node);
        }

        void StartFlood(Node node)
        {
            if (!NodeIsFloodable(node)) return;
            activeZone++;
            Flood(node);
        }

        void Flood(Node node)
        {
            if (!NodeIsFloodable(node)) return;
            node.Visited = true;
            node.FloodZone = activeZone;

            foreach (var edge in node.Edges)
            {
                Flood(edge.Neighbor);
            }
        }

        bool NodeIsFloodable(Node node)
        {
            return !node.Visited && node.StyleAccess[(int)MoveStyle.WALK];
        }
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
        public int FloodZone;

        public bool IsAccessible(MoveStyle style)
        {
            return StyleAccess[(int)style] && !Occupied;
        }

        public class Edge
        {
            public Node Neighbor;
            public float Magnitude;
        }
        public List<Edge> Edges;
    }
}



