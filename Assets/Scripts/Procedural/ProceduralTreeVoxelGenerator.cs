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
        Node.mapSize = mapSize;

        Djikstra djikGraph = new(mapSize);
        List<Node> origins = GetInitialBranchPoints(djikGraph, canopyCenterHeight, canopyRadius, initialBranches);

        foreach(var origin in origins)
        {
            AddTreeLines(origin);
        }

        void AddTreeLines(Node point)
        {
            output[point.position.x, point.position.y, point.position.z] = 1;
            //this line is causing an infinite loop; parenting must be messed up
            if(point.parent != null) AddTreeLines(point.parent);
        }

        return output;
    }

    private static List<Node> GetInitialBranchPoints(Djikstra djikGraph, int canopyCenterHeight, int canopyRadius, int numberOfBranches)
    {
        Vector3Int canopyCenter = djikGraph.Origin;
        List<Node> potentialOrigins = new();
        canopyCenter.z += canopyCenterHeight;
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
            Node origin = potentialOrigins[Random.Range(0, potentialOrigins.Count)];
            origins.Add(origin);
            numberOfBranches--;
        }

        return origins;
        } 

    class Node
    {
        public static int mapSize;
        public class Edge
        {
            public Vector3Int neighbor;
            public float weight;
            public void CalculateEdgeWeight(Vector3 guiding, Node source)
            {
                Vector3 edge = source.position - neighbor;
                Vector3 direction = edge.normalized;
                
                //check this if things go wrong
                weight = edge.magnitude * (1 - Vector3.Dot(direction, guiding));
            }
        }

        public HashSet<Edge> neighborLinks = new();
        public Vector3Int position;
        public Vector3 guidingVector;

        public float distanceFromRoot = float.PositiveInfinity;

        public Node parent;


        readonly static int checkMin = -1;
        readonly static int checkMax = 2;
        public void FindNeighbors()
        {
            for (int x = checkMin; x < checkMax; x++)
            {
                for (int y = checkMin; y < checkMax; y++)
                {
                    for (int z = checkMin; z < checkMax; z++)
                    {
                        Vector3Int checkedPos = new(x, y, z);
                        if (checkedPos == Vector3Int.zero) continue;
                        checkedPos += position;
                        if (checkedPos.x < 0 || checkedPos.y < 0 || checkedPos.z < 0) continue;
                        if(checkedPos.x >= mapSize || checkedPos.y >= mapSize || checkedPos.z >= mapSize) continue;
                        neighborLinks.Add(new() { neighbor = checkedPos });
                    }
                }
            }
        }
        public void CalculateGuidingVector()
        {
            if (parent == null) guidingVector = Vector3.up;
            else guidingVector = trueRotation * parent.guidingVector;
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

        public Node[,,] Map;
        public Vector3Int Origin;

        public Djikstra(int size)
        {
            
            //create the root node for the tree
            Node currentNode = GenerateMap(size);
            Origin = currentNode.position;

            //add the root to the frontier
            frontier.Enqueue(currentNode, 0);

            while(unvisited.Count > 0)
            {
                Debug.Log("visited");
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
                        Vector3Int pos = new(x, y, z);
                        Node outgoing = new() { position = pos };
                        Map[x,y,z] = outgoing;
                        unvisited.Add(outgoing);
                    }
                }
            }
            Node origin = Map[size / 2, size / 2, 0];
            origin.distanceFromRoot = 0;
            return origin;
        }
        void VisitCurrent()
        {
            //pick the lowest distance from the current frontier
            Node currentlyVisiting = frontier.Dequeue();
            currentlyVisiting.CalculateGuidingVector();
            currentlyVisiting.FindNeighbors();
            currentlyVisiting.CalculateEdgeWeights();
            

            //check all the neighbors and assign them tentative distances
            foreach (var edge in currentlyVisiting.neighborLinks)
            {
                Node neighbor = Map[edge.neighbor.x, edge.neighbor.y, edge.neighbor.z];
                if (!visited.Contains(neighbor))
                {
                    float tentativeDistance = (neighbor.position - currentlyVisiting.position).magnitude;
                    float pathDistance = currentlyVisiting.distanceFromRoot + tentativeDistance;
                    if(pathDistance < neighbor.distanceFromRoot)
                    {
                        neighbor.distanceFromRoot = pathDistance;
                        neighbor.parent = currentlyVisiting;
                        Debug.Log("added parent");
                    }
                    frontier.Enqueue(neighbor, neighbor.distanceFromRoot);
                }
            }

            unvisited.Remove(currentlyVisiting);
            visited.Add(currentlyVisiting);
        }
    }
}




