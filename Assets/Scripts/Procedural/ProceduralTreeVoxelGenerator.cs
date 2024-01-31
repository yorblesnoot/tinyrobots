using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ProceduralTreeVoxelGenerator : MonoBehaviour
{
    [SerializeField] [Range(0,5)] int rotationFactor;
    [SerializeField] TreeParams treeParams;
    [SerializeField] GameObject debugger;
    [SerializeField] GameObject hopDebugger;
    [SerializeField] int sphereLevels = 2;
    HashSet<Node> treeNodes;


    static Vector3 baseGuidance = Vector3.up;
    private void Awake()
    {
        EdgePrecalculator.Initialize();
        GenerateSpheres();
    }
    List<Vector3>[] spheres;
    private void GenerateSpheres()
    {
        spheres = new List<Vector3>[sphereLevels];
        for (int i = sphereLevels; i > 0; i++)
        {
            int diameter = 2 * i + 1;
            Vector3 centerPoint = new(i, i, i);
            for(int x = 0; x < diameter; x++)
            {
                for(int y = 0; y < diameter; y++)
                {
                    for(int z = 0; z < diameter; z++)
                    {
                        Vector3 candidate = new(x, y, z);
                        if (Vector3.Distance(candidate, centerPoint) <= i) spheres[i].Add(candidate);
                    }
                }
            }
        }
    }

    [System.Serializable]
    public struct TreeParams
    {
        public int mapSize, canopyCenterHeight, canopyRadius, initialBranches, iterations, iterationRange;

        public float iterationBranchMultiplier;
        [Range(0f, .95f)] public float iterationExclusionHeight;
        public void Deconstruct(out int size, out int height, out int radius, out int initial, out int iterations)
        {
            size = mapSize; height = canopyCenterHeight; radius = canopyRadius; initial = initialBranches; iterations = this.iterations;
        }
    }

    public byte[,,] Generate()
    {
        var (mapSize, canopyCenterHeight, canopyRadius, initialBranches, iterations) = treeParams;
        byte[,,] output = new byte[mapSize, mapSize, mapSize];
        Node.mapSize = mapSize;
        InitialMapGeneration(mapSize);
        
        IterateGrowthField();

        HashSet<Node> canpoyInitialGrowthPoints = GetInitialBranchPoints(canopyCenterHeight, canopyRadius, initialBranches);
        OutputFromOrigins(canpoyInitialGrowthPoints);

        for(int i = 1; i < iterations; i++)
        {
            ReinitializeMap(i);
            IterateGrowthField();
            HashSet<Node> newOrigins = GetSecondaryBranchPoints();
            OutputFromOrigins(newOrigins);
            Debug.Log(treeNodes.Count + " on iteration " + i);
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
                if (origin.parent == null || treeNodes.Contains(origin.parent)) return;
                TreePointToOutput(origin.parent);
            }
        }
    }

    private byte[,,] InflateTree(HashSet<Node> treeNodes)
    {
        int inflationFactor = 3;
        int inflatedSize = inflationFactor * Node.mapSize;
        byte[,,] output = new byte[inflatedSize, inflatedSize, inflatedSize];
        foreach(var node in treeNodes)
        {
            int ilevel = node.iterationLevel;
            Vector3 center = new(node.positionX, node.positionY, node.positionZ);
            center *= inflationFactor;
            Vector3 corner = center;
            corner -= new Vector3(ilevel, ilevel, ilevel);
            foreach(var point in spheres[ilevel])
            {
                Vector3 position = corner + point;
                Vector3Int cleanPosition = Vector3Int.RoundToInt(position);
                output[cleanPosition.x, cleanPosition.y, cleanPosition.z] = 1;
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
                spawned.name = outgoing.hopsFromOrigin.ToString();
            }
        }
    }

    private HashSet<Node> GetSecondaryBranchPoints()
    {
        HashSet<Node> output = new();
        treeParams.initialBranches = Mathf.RoundToInt(treeParams.initialBranches * treeParams.iterationBranchMultiplier);
        List<Node> branchPoints = GenerateBranchSurface();

        branchPoints = branchPoints.Where(node => node.positionY > Node.mapSize * treeParams.iterationExclusionHeight).ToList();
        for(int i = 0; i < treeParams.initialBranches; i++)
        {
            Node point = branchPoints.GrabRandomly();
            output.Add(point);
        }
        return output;
    }
    
    List<Node> GenerateBranchSurface()
    {
        List<Node> output = new();
        for (int x = 0; x < Node.mapSize; x++)
        {
            for (int y = 0; y < Node.mapSize; y++)
            {
                for (int z = 0; z < Node.mapSize; z++)
                {
                    Node outgoing = Map[x, y, z];
                    if (outgoing.hopsFromOrigin == treeParams.iterationRange)
                    {
                        output.Add(outgoing);
                    }

                }
            }
        }
        return output;
    }

    private HashSet<Node> GetInitialBranchPoints(int canopyCenterHeight, int canopyRadius, int numberOfBranches)
    {
        Vector3Int canopyCenter = new(treeParams.mapSize / 2, 0, treeParams.mapSize / 2);
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

    PriorityQueue<Node, float> frontier = new();
    HashSet<Node> unvisited = new();
    Node[,,] Map;

    void IterateGrowthField()
    {
        while (unvisited.Count > 0)
        {
            VisitCurrent();
        }
    }

    void InitialMapGeneration(int size)
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
        treeNodes = new() { origin };
        AddOriginNodeToFrontier(origin);
    }

    private void AddOriginNodeToFrontier(Node outgoing)
    {
        frontier.Enqueue(outgoing, 0);
        outgoing.distanceFromRoot = 0;
        outgoing.hopsFromOrigin = 0;
    }

    void ReinitializeMap(int iteration)
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
                        outgoing.Reset(iteration);
                    }
                }
            }
        }
        
    }

    

    void VisitCurrent()
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

            if (PointIsOffMap(x, y, z)) continue;
            Node neighbor = Map[x, y, z];
            if (!unvisited.Contains(neighbor)) continue;

            float finalWeight = currentlyVisiting.distanceFromRoot + currentlyVisiting.edges[i].weight;
            if (finalWeight < neighbor.distanceFromRoot)
            {
                neighbor.hopsFromOrigin = currentlyVisiting.hopsFromOrigin + 1;
                neighbor.distanceFromRoot = finalWeight;
                neighbor.parent = currentlyVisiting;
                neighbor.parentEdgeIndex = i;
                frontier.Enqueue(neighbor, neighbor.distanceFromRoot);
            }

        }

    }

    bool PointIsOffMap(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0) return true;
        if (x >= Node.mapSize || y >= Node.mapSize || z >= Node.mapSize) return true;
        return false;
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

        public void Reset(int iteration)
        {
            this.iterationLevel = iteration;
            rootRecord += distanceFromRoot;
            distanceFromRoot = float.PositiveInfinity;
            parent = null;
        }

        public int positionX, positionY, positionZ;
        public Vector3 guidingVector;

        public float distanceFromRoot = float.PositiveInfinity;
        float rootRecord;
        public float totalDistanceFromRoot { get { return distanceFromRoot + rootRecord; } }

        public Node parent;
        public int parentEdgeIndex, hopsFromOrigin;
        public int iterationLevel = 0;

        
        public void CalculateGuidingVector(float rotationFactor)
        {
            if (parent == null) guidingVector = baseGuidance;
            else
            {
                Quaternion rotator = Quaternion.AngleAxis(rotationFactor, EdgePrecalculator.GetCross(parentEdgeIndex));
                guidingVector = rotator * parent.guidingVector;
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




