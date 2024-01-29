using System.Collections.Generic;
using UnityEngine;

public class ProceduralTreeVoxelGenerator : MonoBehaviour
{
    [SerializeField] Vector3 eulerRotation;
    [SerializeField] TreeParams treeParams;
    static Quaternion trueRotation;
    private void Awake()
    {
        trueRotation = Quaternion.Euler(eulerRotation);
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

        Djikstra djikGraph = new(mapSize);
        List<Node> origins = GetInitialBranchPoints(djikGraph, canopyCenterHeight, canopyRadius, initialBranches);

        foreach(var origin in origins)
        {
            AddTreeLines(origin);
        }

        void AddTreeLines(Node point)
        {
            output[point.position.x, point.position.y, point.position.z] = 1;
            if(point.parent != null) AddTreeLines(point.parent);
        }

        return output;
    }

    private static List<Node> GetInitialBranchPoints(Djikstra djikGraph, int canopyCenterHeight, int canopyRadius, int numberOfBranches)
    {
        Vector3Int canopyCenter = djikGraph.Origin;
        List<Node> potentialOrigins = new();
        canopyCenter.z += canopyCenterHeight;
        foreach (var nodePosition in djikGraph.Map.Keys)
        {
            if (Vector3.Distance(nodePosition, canopyCenter) < canopyRadius)
            {
                potentialOrigins.Add(djikGraph.Map[nodePosition]);
            }
        }

        List<Node> origins = new();
        while(numberOfBranches > 0)
        {
            Node origin = potentialOrigins[Random.Range(0, potentialOrigins.Count)];
            origins.Add(origin);
            numberOfBranches--;
        }

        return origins;
    }

    class Node
    {
        public class Edge
        {
            public Node neighbor;
            public float weight;
            public void CalculateEdgeWeight(Vector3 guiding, Node source)
            {
                Vector3 edge = source.position - neighbor.position;
                Vector3 direction = edge.normalized;
                
                //check this if things go wrong
                weight = edge.magnitude * (1 - Vector3.Dot(direction, guiding));
            }
        }
        public Vector3Int position;
        public Vector3 guidingVector;

        public float distanceFromRoot = float.PositiveInfinity;

        public Node parent;
        public HashSet<Edge> neighborLinks = new();

        readonly static int checkRange = 2;
        public void GenerateNeighbors(Dictionary<Vector3Int, Node> fullMap)
        {
            for (int x = 0; x < checkRange; x++)
            {
                for (int y = 0; y < checkRange; y++)
                {
                    for (int z = 0; z < checkRange; z++)
                    {
                        Vector3Int pos = new(x, y, z);
                        if (pos == Vector3Int.zero) continue;
                        if (!fullMap.TryGetValue(pos, out var value)) continue;
                        neighborLinks.Add(new() { neighbor = value });
                    }
                }
            }
        }
        public void CalculateGuidingVector()
        {
            if (parent == null) guidingVector = Vector3.up;
            else guidingVector =  trueRotation * parent.guidingVector;
        }

        public void CalculateEdgeWeights()
        {
            foreach (var edge in neighborLinks)
            {
                edge.CalculateEdgeWeight(guidingVector, this);
            }
        }
    }

    class Djikstra
    {
        PriorityQueue<Node, float> frontier = new();
        HashSet<Node> unvisited = new();
        HashSet<Node> visited = new();

        public Dictionary<Vector3Int, Node> Map = new();
        public Vector3Int Origin;

        public Djikstra(int size)
        {
            //create the root node for the tree
            Node currentNode = GenerateMap(size);
            Origin = currentNode.position;

            foreach(var val in Map.Values)
            {
                val.GenerateNeighbors(Map);
            }

            //add the root to the frontier
            frontier.Enqueue(currentNode, 0);

            while(unvisited.Count > 0)
            {
                VisitCurrent();
            }           
        }

        Node GenerateMap(int size)
        {
            for(int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    for(int z = 0; z < size; z++)
                    {
                        Vector3Int pos = new(x, y, z);
                        Map.Add(pos, new() { position = pos });
                    }
                }
            }
            return Map[new(size / 2, size / 2, 0)];
        }
        void VisitCurrent()
        {
            //pick the lowest distance from the current frontier
            Node currentlyVisiting = frontier.Dequeue();
            currentlyVisiting.CalculateGuidingVector();
            currentlyVisiting.CalculateEdgeWeights();
            

            //check all the neighbors and assign them tentative distances
            foreach (var edge in currentlyVisiting.neighborLinks)
            {
                Node neighbor = edge.neighbor;
                if (!visited.Contains(neighbor))
                {
                    float tentativeDistance = (neighbor.position - currentlyVisiting.position).magnitude;
                    float pathDistance = currentlyVisiting.distanceFromRoot + tentativeDistance;
                    if(pathDistance < neighbor.distanceFromRoot)
                    {
                        neighbor.distanceFromRoot = pathDistance;
                        neighbor.parent = currentlyVisiting;
                    }
                    frontier.Enqueue(neighbor, neighbor.distanceFromRoot);
                }
            }

            unvisited.Remove(currentlyVisiting);
            visited.Add(currentlyVisiting);
        }
    }
}




