using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UnityEngine;

public class ProceduralTreeVoxelGenerator : MonoBehaviour
{
    [SerializeField] Vector3 eulerRotation;
    [SerializeField] TreeParams treeParams;
    static Quaternion trueRotation;
    HashSet<Node> treeNodes;
    private void Awake()
    {
        trueRotation = Quaternion.Euler(eulerRotation);
        EdgePrecalculator.Initialize();
    }

    [System.Serializable]
    public struct TreeParams
    {
        public int mapSize, canopyCenterHeight, canopyRadius, initialBranches, iterations, iterationRange;

        public float iterationBranchMultiplier;
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
        Node firstNode = InitialMapGeneration(mapSize);
        treeNodes = new() { firstNode };
        IterateGrowthField();

        HashSet<Node> canpoyInitialGrowthPoints = GetInitialBranchPoints(canopyCenterHeight, canopyRadius, initialBranches);
        OutputFromOrigins(canpoyInitialGrowthPoints);

        for(int i = 0; i < iterations; i++)
        {
            IterateGrowthField();
            HashSet<Node> newOrigins = GetSecondaryBranchPoints();
            OutputFromOrigins(newOrigins);
        }

        return output;

        void OutputFromOrigins(HashSet<Node> origins)
        {
            foreach (var origin in origins)
            {
                TreePointToOutput(origin);
            }

            void TreePointToOutput(Node point)
            {
                output[point.positionX, point.positionY, point.positionZ] = 1;
                treeNodes.Add(point);
                if (point.parent == null || treeNodes.Contains(point.parent)) return;
                TreePointToOutput(point.parent);
            }
        }
    }

    private HashSet<Node> GetSecondaryBranchPoints()
    {
        HashSet<Node> output = new();
        treeParams.initialBranches = Mathf.RoundToInt(treeParams.initialBranches * treeParams.iterationBranchMultiplier);
        List<Node> branchPoints = treeNodes.Where(node => node.hopsFromOrigin == treeParams.iterationRange).ToList();
        for(int i = 0; i < branchPoints.Count; i++)
        {
            output.Add(branchPoints.GrabRandomly());
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
    public int[] Origin;

    void IterateGrowthField()
    {
        ReinitializeMap();
        Debug.Log(treeNodes.Count + " in oset");
        //add the root to the frontier
        foreach (Node node in treeNodes)
        {
            frontier.Enqueue(node, 0);
        }
        

        while (unvisited.Count > 0)
        {
            VisitCurrent();
        }
    }

    Node InitialMapGeneration(int size)
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
        origin.distanceFromRoot = 0;
        origin.hopsFromOrigin = 0;
        return origin;
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
                    if (treeNodes.Contains(outgoing))
                    {
                        outgoing.distanceFromRoot = 0;
                    }
                    else
                    {
                        unvisited.Add(outgoing);
                        outgoing.Reset();
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

        currentlyVisiting.CalculateGuidingVector();
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
                neighbor.distanceFromRoot = finalWeight;
                neighbor.parent = currentlyVisiting;
                neighbor.hopsFromOrigin = currentlyVisiting.hopsFromOrigin + 1;
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
        static Vector3Int[] vectors;

        public static Vector3Int GetDirectionVector(int directionIndex)
        {
            return vectors[directionIndex];
        }
        public static void Initialize()
        {
            magnitudes = new float[directions.GetLength(0)];
            vectors = new Vector3Int[directions.GetLength(0)];
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                magnitudes[i] = GetMagnitude(directions[i, 0], directions[i, 1], directions[i, 2]);
                //magnitudes[i] = Mathf.Sqrt(Mathf.Pow(directions[i, 0], 2) + Mathf.Pow(directions[i, 1], 2) + Mathf.Pow(directions[i, 2], 2));
                vectors[i] = new(directions[i, 0], directions[i,1], directions[i,2]);
            }
        }

        static float GetMagnitude(int x, int y, int z)
        {
            return Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2) + Mathf.Pow(z, 2));
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
            distanceFromRoot = float.PositiveInfinity;
            parent = null;
        }

        public int positionX, positionY, positionZ;
        public Vector3 guidingVector;

        public float distanceFromRoot = float.PositiveInfinity;

        public Node parent;
        public int hopsFromOrigin;

        Vector3 baseGuidance = Vector3.right;
        public void CalculateGuidingVector()
        {
            if (parent == null) guidingVector = baseGuidance;
            else guidingVector = trueRotation * parent.guidingVector;
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




