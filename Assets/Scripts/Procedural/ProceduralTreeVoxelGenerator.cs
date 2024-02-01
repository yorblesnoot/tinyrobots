using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ProceduralTreeVoxelGenerator : MapGenerator
{
    
    [SerializeField] TreeParams tp;
    [SerializeField] GameObject ground;
    [SerializeField] GameObject debugger;
    [SerializeField] GameObject hopDebugger;
    HashSet<Node> treeNodes;

    static TreeParams.Iteration currentIteration;
    static Vector3 baseGuidance = Vector3.up;

    PriorityQueue<Node, float> frontier = new();
    HashSet<Node> unvisited = new();
    Node[,,] Map;

    Vector3 originLocation;

    private void Awake()
    {
        EdgePrecalculator.Initialize();
        GenerateSpheres();
    }
    List<Vector3Int>[] spheres;
    private void GenerateSpheres()
    {

        int maxSphere = tp.sphereLevels + 1;
        spheres = new List<Vector3Int>[maxSphere];
        for (int i = 0; i < maxSphere; i++)
        {
            GenerateSphere(i);
        }
    }
    private void GenerateSphere(int i)
    {
        if(i == 0)
        {
            spheres[i] = new List<Vector3Int> { Vector3Int.zero };
            return;
        }
        spheres[i] = new();
        int b = i + tp.sphereBoost;
        int diameter = 2 * b + 1;
        Vector3 centerPoint = new(b, b, b);
        for (int x = 0; x < diameter; x++)
        {
            for (int y = 0; y < diameter; y++)
            {
                for (int z = 0; z < diameter; z++)
                {
                    Vector3 candidate = new(x, y, z);
                    if (Vector3.Distance(candidate, centerPoint) < b) spheres[i].Add(Vector3Int.CeilToInt(candidate));
                }
            }
        }
    }

    public override byte[,,] GenerateCoreMap()
    {
        byte[,,] output = new byte[tp.mapSize, tp.mapSize, tp.mapSize];
        Node.mapSize = tp.mapSize;
        GenerateInitialMap(tp.mapSize);
        
        IterateGrowthField();

        HashSet<Node> canpoyInitialGrowthPoints = GetInitialBranchPoints(tp.initialGeneration.canopyCenterHeight, tp.initialGeneration.canopyRadius, tp.initialGeneration.branches);
        OutputFromOrigins(canpoyInitialGrowthPoints);

        foreach(var iteration in tp.iterations)
        {
            currentIteration = iteration;
            ReinitializeMap();
            IterateGrowthField();
            HashSet<Node> newOrigins = GetSecondaryBranchPoints();
            OutputFromOrigins(newOrigins);
        }

        
        
        //DebugDirections();

        return InflateTree(treeNodes);

        void OutputFromOrigins(HashSet<Node> origins)
        {
            foreach (var origin in origins)
            {
                TreePointToOutput(origin);
            }

            
            void TreePointToOutput(Node origin)
            {
                treeNodes.Add(origin);
                if (origin.Parent == null || treeNodes.Contains(origin.Parent)) return;
                TreePointToOutput(origin.Parent);
            }
        }
    }

    private byte[,,] InflateTree(HashSet<Node> treeNodes)
    {
        int inflatedSize = tp.inflationFactor * Node.mapSize;
        byte[,,] output = new byte[inflatedSize, inflatedSize, inflatedSize];

        int longestBranch = treeNodes.Select(x => x.hopsFromRoot).Max();
        float sphereTierSize = (float)longestBranch / tp.sphereLevels;

        foreach(var node in treeNodes)
        {
            int inflationLevel = tp.sphereLevels - Mathf.FloorToInt(node.hopsFromRoot / sphereTierSize);

            Vector3Int center = new(node.positionX, node.positionY, node.positionZ);
            center *= tp.inflationFactor;
            Vector3Int corner = center;
            corner -= new Vector3Int(inflationLevel, inflationLevel, inflationLevel);
            //Debug.Log(inflationLevel + "ilvl " + node.hopsFromRoot + " hops");
            foreach(var point in spheres[inflationLevel])
            {
                Vector3Int position = corner + point;
                if (PointIsOffMap(position.x, position.y, position.z, inflatedSize)) continue;
                output[position.x, position.y, position.z] = 1;
            }
        }
        return output;
    }
    
    private void DebugDirections()
    {
        for (int x = 0; x < Node.mapSize; x++)
        {
            for (int y = 0; y < Node.mapSize; y++)
            {
                Node outgoing = Map[x, y, Node.mapSize/2];
                GameObject spawned = Instantiate(debugger, new Vector3(x, y, Node.mapSize / 2), Quaternion.LookRotation(outgoing.guidingVector));
                //spawned.name = outgoing.hopsFromOrigin.ToString();
            }
        }
    }

    private HashSet<Node> GetSecondaryBranchPoints()
    {
        HashSet<Node> output = new();
        HashSet<Node> branchPoints = GenerateBranchSurface();
        List<Node> orderedPoints = branchPoints.OrderByDescending(x => x.distanceFromRoot)
            .Take(Mathf.RoundToInt(branchPoints.Count * currentIteration.inclusionZone))
            .ToList();
        /*foreach (Node node in orderedPoints)
        {
            Instantiate(hopDebugger, new Vector3(node.positionX, node.positionY, node.positionZ), Quaternion.identity).GetComponent<TMP_Text>().text = node.distanceFromRoot.ToString();
            
        }*/
        for(int i = 0; i < currentIteration.branches; i++)
        {
            output.Add(orderedPoints.GrabRandomly());
        }
        return output;
    }

    static readonly Vector3[] crossDirections = {
        new Vector3(-1, 0, 0), new Vector3(1, 0, 0),    // Faces along x-axis
        new Vector3(0, -1, 0), new Vector3(0, 1, 0),    // Faces along y-axis
        new Vector3(-1, -1, 0), new Vector3(1, -1, 0),  // Edges along xy-plane
        new Vector3(-1, 1, 0), new Vector3(1, 1, 0)    // Edges along xy-plane
    };

    HashSet<Node> GenerateBranchSurface()
    {
        HashSet<Node> output = new();
        
        foreach(var node in treeNodes)
        {
            Quaternion rot = Quaternion.LookRotation(EdgePrecalculator.GetDirectionVector(node.parentEdgeIndex));
            
            for(int i = 0; i < 8; i++)
            {
                Vector3 tiltedDirection = rot * crossDirections[i];
                tiltedDirection *= currentIteration.branchLength;
                Vector3Int finalPosition = new(node.positionX, node.positionY, node.positionZ);
                finalPosition += Vector3Int.RoundToInt(tiltedDirection);
                int x = finalPosition.x;
                int y = finalPosition.y;
                int z = finalPosition.z;
                if (PointIsOffMap(x, y, z, Node.mapSize)) continue;
                output.Add(Map[x, y, z]);
            }
        }
        return output;
    }
    HashSet<Node> GetInitialBranchPoints(int canopyCenterHeight, int canopyRadius, int numberOfBranches)
    {
        Vector3Int canopyCenter = new(tp.mapSize / 2, 0, tp.mapSize / 2);
        List<Node> potentialOrigins = new();
        canopyCenter.y += canopyCenterHeight;
        for (int x = 0; x < Node.mapSize; x++)
        {
            for (int y = 0; y < Node.mapSize; y++)
            {
                for (int z = 0; z < Node.mapSize; z++)
                {
                    Vector3Int check = new(x, y, z);
                    if (Vector3.Distance(check, canopyCenter) < canopyRadius)
                    {
                        potentialOrigins.Add(Map[x,y,z]);
                    }
                }
            }
        }

        HashSet<Node> origins = new();
        while (numberOfBranches > 0)
        {
            Node origin = potentialOrigins[UnityEngine.Random.Range(0, potentialOrigins.Count)];
            origins.Add(origin);
            numberOfBranches--;
        }

        return origins;
    }

    void IterateGrowthField()
    {
        int factor = currentIteration == null ? tp.initialGeneration.rotationFactor : currentIteration.rotationFactor;
        while (unvisited.Count > 0)
        {
            VisitCurrent(factor);
        }
    }

    void GenerateInitialMap(int size)
    {
        Map = new Node[size, size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Node outgoing = new() { positionX = x, positionY = y, positionZ = z };
                    Map[x, y, z] = outgoing;
                    unvisited.Add(outgoing);
                }
            }
        }
        Node origin = Map[size / 2, 0, size / 2];
        originLocation = new(origin.positionX, origin.positionY, origin.positionZ);
        treeNodes = new() { origin };
        AddOriginNodeToFrontier(origin);
    }

    void AddOriginNodeToFrontier(Node outgoing)
    {
        frontier.Enqueue(outgoing, 0);
        outgoing.distanceFromRoot = 0;
    }

    void ReinitializeMap()
    {
        int size = Node.mapSize;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    Node outgoing = Map[x, y, z];
                    unvisited.Add(outgoing);
                    
                    if (treeNodes.Contains(outgoing))
                    {
                        AddOriginNodeToFrontier(outgoing);
                    }
                    else
                    {
                        outgoing.Reset();
                    }
                }
            }
        }
        
    }

    void VisitCurrent(int rotationFactor)
    {
        //pick the lowest distance from the current frontier
        Node currentlyVisiting = frontier.Dequeue();
        unvisited.Remove(currentlyVisiting);

        currentlyVisiting.CalculateGuidingVector(rotationFactor);
        currentlyVisiting.CalculateEdgeWeights();


        //check all the neighbors and assign them tentative distances
        for (int i = 0; i < EdgePrecalculator.DirectionCount; i++)
        {
            int x = currentlyVisiting.positionX + EdgePrecalculator.GetDirectionComponent(i, 0);
            int y = currentlyVisiting.positionY + EdgePrecalculator.GetDirectionComponent(i, 1);
            int z = currentlyVisiting.positionZ + EdgePrecalculator.GetDirectionComponent(i, 2);

            if (PointIsOffMap(x, y, z, Node.mapSize)) continue;
            Node neighbor = Map[x, y, z];
            if (!unvisited.Contains(neighbor)) continue;

            float finalWeight = currentlyVisiting.hopsFromRoot + currentlyVisiting.edges[i].weight;
            if (finalWeight < neighbor.distanceFromRoot)
            {
                neighbor.hopsFromRoot = currentlyVisiting.hopsFromRoot + 1;
                neighbor.distanceFromRoot = finalWeight;
                neighbor.Parent = currentlyVisiting;
                neighbor.parentEdgeIndex = i;
                frontier.Enqueue(neighbor, neighbor.distanceFromRoot);
            }

        }

    }

    bool PointIsOffMap(int x, int y, int z, int size)
    {
        if (x < 0 || y < 0 || z < 0) return true;
        if (x >= size || y >= size || z >= size) return true;
        return false;
    }

    public override void PlaceSecondaries()
    {
        ground.transform.position = originLocation;
    }

    public static class EdgePrecalculator
    {
        static readonly int[,] directions = {
            {-1, 0, 0}, {1, 0, 0},    // Faces along x-axis
            {0, -1, 0}, {0, 1, 0},    // Faces along y-axis
            {0, 0, -1}, {0, 0, 1},    // Faces along z-axis
            {-1, -1, 0}, {1, -1, 0},  // Edges along xy-plane
            {-1, 1, 0}, {1, 1, 0},    // Edges along xy-plane
            {-1, 0, -1}, {1, 0, -1},  // Edges along xz-plane
            {0, -1, -1}, {0, 1, -1},  // Edges along yz-plane
            {-1, -1, -1}, {1, -1, -1},  // Corners
            {-1, 1, -1}, {1, 1, -1},    // Corners
            {-1, -1, 1}, {1, -1, 1},    // Corners
            {-1, 1, 1}, {1, 1, 1}        // Corners
        };

        public static int DirectionCount { get { return directions.GetLength(0); } }

        static float[] magnitudes;
        
        static Vector3[] vectors;
        static Vector3[] crosses;

        public static Vector3 GetDirectionVector(int directionIndex)
        {
            return vectors[directionIndex];
        }
        public static void Initialize()
        {
            magnitudes = new float[directions.GetLength(0)];
            vectors = new Vector3[directions.GetLength(0)];
            crosses = new Vector3[directions.GetLength(0)];
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                magnitudes[i] = GetMagnitude(directions[i, 0], directions[i, 1], directions[i, 2]);
                //magnitudes[i] = Mathf.Sqrt(Mathf.Pow(directions[i, 0], 2) + Mathf.Pow(directions[i, 1], 2) + Mathf.Pow(directions[i, 2], 2));
                vectors[i] = new(directions[i, 0], directions[i,1], directions[i,2]);
                crosses[i] = Vector3.Cross(baseGuidance, vectors[i]);
            }
        }

        static float GetMagnitude(int x, int y, int z)
        {
            return Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2) + Mathf.Pow(z, 2));
        }

        public static Vector3 GetCross(int index)
        {
            return crosses[index];
        }

        public static int GetDirectionComponent(int directionIndex, int componentIndex)
        {
            return directions[directionIndex, componentIndex];
        }

        public static float GetMagnitude(int directionIndex)
        {
            return magnitudes[directionIndex];
        }
    }

    
    class Node
    {
        public static int mapSize;
        public struct Edge
        {
            public int directionIndex;
            public float weight;
        }

        public Edge[] edges;

        public Node()
        {
            edges = new Edge[EdgePrecalculator.DirectionCount];
            for (int i = 0; i < EdgePrecalculator.DirectionCount; i++)
            {
                edges[i] = new Edge() { directionIndex = i };
            }
        }

        public void Reset()
        {
            hopsFromRoot = 0;
            distanceFromRoot = float.PositiveInfinity;
            Parent = null;
        }

        public int positionX, positionY, positionZ;
        public Vector3 guidingVector;

        public float distanceFromRoot = float.PositiveInfinity;

        public Node Parent { get; set; }
        Node parent;
        HashSet<Node> children;
        public int parentEdgeIndex, hopsFromRoot;

        
        public void CalculateGuidingVector(float rotationFactor)
        {
            if (Parent == null) guidingVector = baseGuidance;
            else
            {
                Quaternion rotator = Quaternion.AngleAxis(rotationFactor, EdgePrecalculator.GetCross(parentEdgeIndex));
                guidingVector = rotator * Parent.guidingVector;
                //Debug.Log("parent guidance " + parent.guidingVector + ", incoming vector" + EdgePrecalculator.GetDirectionVector(parentEdgeIndex) + " guiding vector " + guidingVector);
            }
        }

        public void CalculateEdgeWeights()
        {
            for (int i = 0; i < edges.Count(); i++)
            {
                Vector3 direction = EdgePrecalculator.GetDirectionVector(i);
                edges[i].weight = EdgePrecalculator.GetMagnitude(i) * (1 - Vector3.Dot(direction, guidingVector));
            }
        }
    }
}




