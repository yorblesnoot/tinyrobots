using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralTreeVoxelGenerator : MonoBehaviour
{
    [SerializeField] Vector3 eulerRotation;
    [SerializeField] TreeParams treeParams;
    static Quaternion trueRotation;
    private void Awake()
    {
        trueRotation = Quaternion.Euler(eulerRotation);
        EdgePrecalculator.Initialize();
    }

    [System.Serializable]
    public struct TreeParams
    {
        public int mapSize, canopyCenterHeight, canopyRadius, initialBranches;
        public void Deconstruct(out int size, out int height, out int radius, out int initial)
        {
            size = mapSize; height = canopyCenterHeight; radius = canopyRadius; initial = initialBranches;
        }
    }

    public byte[,,] Generate()
    {
        var (mapSize, canopyCenterHeight, canopyRadius, initialBranches) = treeParams;
        byte[,,] output = new byte[mapSize, mapSize, mapSize];
        Node.mapSize = mapSize;

        Djikstra djikGraph = new(mapSize);
        List<Node> origins = GetInitialBranchPoints(djikGraph, canopyCenterHeight, canopyRadius, initialBranches);

        foreach(var origin in origins)
        {
            AddTreeLines(origin);
        }

        void AddTreeLines(Node point)
        {
            output[point.positionX, point.positionY, point.positionZ] = 1;
            //this line is causing an infinite loop; parenting must be messed up
            if(point.parent != null) AddTreeLines(point.parent);
        }

        return output;
    }

    private static List<Node> GetInitialBranchPoints(Djikstra djikGraph, int canopyCenterHeight, int canopyRadius, int numberOfBranches)
    {
        Vector3Int canopyCenter = new(djikGraph.Origin[0], djikGraph.Origin[1], djikGraph.Origin[2]);
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
                        potentialOrigins.Add(djikGraph.Map[x,y,z]);
                    }
                }
            }
        }

        List<Node> origins = new();
        while (numberOfBranches > 0)
        {
            Node origin = potentialOrigins[UnityEngine.Random.Range(0, potentialOrigins.Count)];
            origins.Add(origin);
            numberOfBranches--;
        }

        return origins;
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

        public static Vector3Int GetDirectionVector(int directionIndex)
        {
            return new Vector3Int(GetDirectionComponent(directionIndex, 0), GetDirectionComponent(directionIndex, 1),
            GetDirectionComponent(directionIndex, 2));
        }
        public static void Initialize()
        {
            magnitudes = new float[directions.GetLength(0)];
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                magnitudes[i] = Mathf.Sqrt(Mathf.Pow(directions[i, 0], 2) + Mathf.Pow(directions[i, 1], 2) + Mathf.Pow(directions[i, 2], 2));
            }
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
            Edge[] edges = new Edge[EdgePrecalculator.DirectionCount];
            for (int i = 0; i < EdgePrecalculator.DirectionCount; i++)
            {
                edges[i] = new Edge() { directionIndex = i };
            }
            Debug.Log(edges.Length);
        }

        public int positionX, positionY, positionZ;
        public Vector3 guidingVector;

        public float distanceFromRoot = float.PositiveInfinity;

        public Node parent;

        public void FindNeighbors()
        {
            for (int i = 0; i < EdgePrecalculator.DirectionCount; i++)
            {
                int x = EdgePrecalculator.GetDirectionComponent(i, 0);
                int y = EdgePrecalculator.GetDirectionComponent(i, 1);
                int z = EdgePrecalculator.GetDirectionComponent(i, 2);
                if (x == 0 && y==0 && z==0) continue;
                int checkX = x + positionX;
                int checkY = y + positionY;
                int checkZ = z + positionZ;
                if (checkX < 0 || checkY < 0 || checkZ < 0) continue;
                if(checkX >= mapSize || checkY >= mapSize || checkZ >= mapSize) continue;
            }
        }

        Vector3 baseGuidance = Vector3.right;
        public void CalculateGuidingVector()
        {
            if (parent == null) guidingVector = baseGuidance;
            else guidingVector = trueRotation * parent.guidingVector;
        }

        public void CalculateEdgeWeights()
        {
            Debug.Log(edges);
            for (int i = 0; i < edges.Count(); i++)
            {
                Vector3 direction = EdgePrecalculator.GetDirectionVector(i);
                edges[i].weight = EdgePrecalculator.GetMagnitude(i) * (1 - Vector3.Dot(direction, guidingVector));
            }
        }
    }

    class Djikstra
    {
        PriorityQueue<Node, float> frontier = new();
        HashSet<Node> unvisited = new();

        public Node[,,] Map;
        public int[] Origin;

        public Djikstra(int size)
        {
            
            //create the root node for the tree
            Node currentNode = GenerateMap(size);
            Origin = new int[] { currentNode.positionX, currentNode.positionY, currentNode.positionZ };

            //add the root to the frontier
            frontier.Enqueue(currentNode, 0);

            while(unvisited.Count > 0)
            {
                VisitCurrent();
            }   
            
        }

        Node GenerateMap(int size)
        {
            Map = new Node[size, size, size];
            for(int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    for(int z = 0; z < size; z++)
                    {
                        Node outgoing = new() { positionX = x, positionY = y, positionZ = z };
                        Map[x,y,z] = outgoing;
                        unvisited.Add(outgoing);
                    }
                }
            }
            Node origin = Map[size / 2, 0, size / 2];
            origin.distanceFromRoot = 0;
            return origin;
        }
        void VisitCurrent()
        {
            //pick the lowest distance from the current frontier
            Node currentlyVisiting = frontier.Dequeue();
            unvisited.Remove(currentlyVisiting);

            currentlyVisiting.CalculateGuidingVector();
            currentlyVisiting.FindNeighbors();
            currentlyVisiting.CalculateEdgeWeights();


            //check all the neighbors and assign them tentative distances
            for(int i = 0; i < EdgePrecalculator.DirectionCount; i++) 
            {
                int x = currentlyVisiting.positionX + EdgePrecalculator.GetDirectionComponent(i, 0);
                int y = currentlyVisiting.positionY + EdgePrecalculator.GetDirectionComponent(i, 1);
                int z = currentlyVisiting.positionZ + EdgePrecalculator.GetDirectionComponent(i, 2);

                Node neighbor = Map[x, y, z];
                if (!unvisited.Contains(neighbor)) continue;
                    
                float finalWeight = currentlyVisiting.distanceFromRoot + currentlyVisiting.edges[i].weight;
                if(finalWeight < neighbor.distanceFromRoot)
                {
                    neighbor.distanceFromRoot = finalWeight;
                    neighbor.parent = currentlyVisiting;
                    frontier.Enqueue(neighbor, neighbor.distanceFromRoot);
                }
                
            }

        }
    }
}




