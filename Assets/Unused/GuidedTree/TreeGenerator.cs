using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeGenerator 
{
    
    [SerializeField] TreeParams tp;
    [SerializeField] GameObject ground;
    [SerializeField] TreeRenderer treeRenderer;
    
    HashSet<TreeGeneratorNode> treeNodes;

    [SerializeField] GameObject debugger;

    public static TreeParams.FirstIteration firstIteration;
    public static TreeParams.Iteration currentIteration;
    public static Vector3 baseGuidance = Vector3.up;

    PriorityQueue<TreeGeneratorNode, float> frontier = new();
    HashSet<TreeGeneratorNode> unvisited = new();
    static TreeGeneratorNode[,,] Map;

    TreeGeneratorNode origin;
    static bool leaf = false;

    private void Awake()
    {
        leaf = false;
        currentIteration = null;
        firstIteration = tp.initialGeneration;
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

    void DebugGuiding()
    {
        for(int x = 0; x < TreeGeneratorNode.mapSize; x++)
        {
            for(int y = 0;y < TreeGeneratorNode.mapSize; y++)
            {
                TreeGeneratorNode node = Map[x, y, TreeGeneratorNode.mapSize / 2];
                GameObject.Instantiate(debugger, node.worldPosition, Quaternion.LookRotation(node.guidingVector));
                
            }
        }
    }

    public void GenerateCoreMap()
    {
        TreeGeneratorNode.mapSize = tp.mapSize;
        GenerateInitialMap(tp.mapSize);
        
        IterateGrowthField();

        HashSet<TreeGeneratorNode> canpoyInitialGrowthPoints = GetInitialBranchPoints(tp.initialGeneration.canopyCenterHeight, tp.initialGeneration.canopyRadius, tp.initialGeneration.branches);
        BuildTreeToBranchPoints(canpoyInitialGrowthPoints, true);

        foreach(var iteration in tp.iterations) PerformIteration(iteration);
        leaf = true;
        foreach (var iteration in tp.leafIterations) PerformIteration(iteration);
        //DebugGuiding();

        float longestBranch = treeNodes.Select(node => node.hopsFromRoot).Max();
        SimplifiedTreeNode saveableOrigin = SimplifyTree(origin);
        treeRenderer.RenderTree(saveableOrigin, longestBranch);

        void PerformIteration(TreeParams.Iteration iteration)
        {
            currentIteration = iteration;
            ReinitializeMap();
            IterateGrowthField();

            HashSet<TreeGeneratorNode> newOrigins = GetSecondaryBranchPoints();
            BuildTreeToBranchPoints(newOrigins);
        }

        void BuildTreeToBranchPoints(HashSet<TreeGeneratorNode> origins, bool thicken = false)
        {
            foreach (var origin in origins)
            {
                TreePointToOutput(origin);
            }

            
            void TreePointToOutput(TreeGeneratorNode origin)
            {
                if(thicken) origin.isTrunk = true;
                treeNodes.Add(origin);
                if (origin.Parent == null || treeNodes.Contains(origin.Parent)) return;
                TreePointToOutput(origin.Parent);
            }
        }

        
    }

    private SimplifiedTreeNode SimplifyTree(TreeGeneratorNode origin)
    {
        foreach(var node in treeNodes)
        {
            node.Parent?.children.Add(node);
            node.Parent?.outgoingVectors.Add(node.incomingVector);
        }
        return ConvertNode(origin);

        static SimplifiedTreeNode ConvertNode(TreeGeneratorNode node)
        {
            Vector3 growthDirection = node.Parent == null ? Vector3.up : (node.incomingVector
            + (node.outgoingVectors.Count > 0 ? node.outgoingVectors[0] : node.incomingVector));
            growthDirection.Normalize();
            SimplifiedTreeNode output = new() { growthDirection = growthDirection,
                hopsFromRoot = node.hopsFromRoot, isLeaf = node.isLeaf, worldPosition = node.worldPosition, isTrunk = node.isTrunk };
            if(node.children != null && node.children.Count > 0)
            {
                output.children = new();
                foreach (var child in node.children)
                {
                    output.children.Add(ConvertNode(child));
                }
            }
            return output;
        }
    }

    public byte[,,] GetByteMap()
    {
        return InflateTree(treeNodes);
    }

    private byte[,,] InflateTree(HashSet<TreeGeneratorNode> treeNodes)
    {
        int inflatedSize = tp.inflationFactor * TreeGeneratorNode.mapSize;
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

    private HashSet<TreeGeneratorNode> GetSecondaryBranchPoints()
    {
        HashSet<TreeGeneratorNode> output = new();
        List<TreeGeneratorNode> branchPoints = new();
        for (int x = 0; x < TreeGeneratorNode.mapSize; x++)
        {
            for (int y = 0; y < TreeGeneratorNode.mapSize; y++)
            {
                for (int z = 0; z < TreeGeneratorNode.mapSize; z++)
                {
                    TreeGeneratorNode node = Map[x, y, z];
                    if (node.hopsFromOrigin == currentIteration.surfaceRadius) branchPoints.Add(node);
                }
            }
        }

        branchPoints = branchPoints.OrderByDescending(node => node.hopsFromRoot).ToList();
        branchPoints = branchPoints.Take(Mathf.RoundToInt(branchPoints.Count * currentIteration.inclusionZone)).ToList();
        //DebugSecondaryBranches(branchPoints);
        int branchNum = Mathf.Clamp(currentIteration.branches, 0, branchPoints.Count);
        for (int i = 0; i < currentIteration.branches; i++)
        {
            output.Add(branchPoints.GrabRandomly());
        }
        return output;

        void DebugSecondaryBranches(List<TreeGeneratorNode> orderedPoints)
        {
            foreach (TreeGeneratorNode node in orderedPoints)
            {
                GameObject.Instantiate(debugger, node.worldPosition, Quaternion.identity);
            }
        }
    }

    HashSet<TreeGeneratorNode> GetInitialBranchPoints(int canopyCenterHeight, int canopyRadius, int numberOfBranches)
    {
        Vector3Int canopyCenter = new(tp.mapSize / 2, 0, tp.mapSize / 2);
        List<TreeGeneratorNode> potentialOrigins = new();
        canopyCenter.y += canopyCenterHeight;
        for (int x = 0; x < TreeGeneratorNode.mapSize; x++)
        {
            for (int y = 0; y < TreeGeneratorNode.mapSize; y++)
            {
                for (int z = 0; z < TreeGeneratorNode.mapSize; z++)
                {
                    Vector3Int check = new(x, y, z);
                    if (Vector3.Distance(check, canopyCenter) < canopyRadius)
                    {
                        potentialOrigins.Add(Map[x,y,z]);
                    }
                }
            }
        }

        HashSet<TreeGeneratorNode> origins = new();
        while (numberOfBranches > 0)
        {
            TreeGeneratorNode origin = potentialOrigins[UnityEngine.Random.Range(0, potentialOrigins.Count)];
            origins.Add(origin);
            numberOfBranches--;
        }

        return origins;
    }

    void IterateGrowthField()
    {
        while (unvisited.Count > 0)
        {
            VisitCurrent();
        }
    }

    void GenerateInitialMap(int size)
    {
        Map = new TreeGeneratorNode[size, size, size];
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    TreeGeneratorNode outgoing = new() 
                    { positionX = x, positionY = y, positionZ = z, 
                      worldPosition = new( Jitter(x), Jitter(y), Jitter(z)) };
                    Map[x, y, z] = outgoing;
                    unvisited.Add(outgoing);
                }
            }
        }
        TreeGeneratorNode origin = Map[size / 2, 0, size / 2];
        this.origin = origin;
        treeNodes = new() { origin };
        AddOriginNodeToFrontier(origin);
    }

    float Jitter(int input)
    {
        float factor = Random.Range(-tp.jitterSize, tp.jitterSize);
        return factor + input;
    }

    void AddOriginNodeToFrontier(TreeGeneratorNode outgoing)
    {
        frontier.Enqueue(outgoing, 0);
        outgoing.shortestPath = 0;
        outgoing.hopsFromOrigin = 0;
    }

    void ReinitializeMap()
    {
        int size = TreeGeneratorNode.mapSize;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int z = 0; z < size; z++)
                {
                    TreeGeneratorNode outgoing = Map[x, y, z];
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

    void VisitCurrent()
    {
        //pick the lowest distance from the current frontier
        TreeGeneratorNode currentlyVisiting = frontier.Dequeue();
        unvisited.Remove(currentlyVisiting);

        currentlyVisiting.CalculateGuidingVector();
        currentlyVisiting.CalculateEdgeWeights();

        //check all the neighbors and assign them tentative distances
        for (int i = 0; i < EdgePrecalculator.DirectionCount; i++)
        {
            int x = currentlyVisiting.positionX + EdgePrecalculator.GetDirectionComponent(i, 0);
            int y = currentlyVisiting.positionY + EdgePrecalculator.GetDirectionComponent(i, 1);
            int z = currentlyVisiting.positionZ + EdgePrecalculator.GetDirectionComponent(i, 2);

            if (PointIsOffMap(x, y, z, TreeGeneratorNode.mapSize)) continue;
            TreeGeneratorNode neighbor = Map[x, y, z];
            if (!unvisited.Contains(neighbor)) continue;
            float travelCost = currentlyVisiting.shortestPath + currentlyVisiting.edges[i].weight;
            if (travelCost < neighbor.shortestPath)
            {
                neighbor.Parent = currentlyVisiting;
                neighbor.shortestPath = travelCost;
                neighbor.hopsFromRoot = currentlyVisiting.hopsFromRoot + 1;
                neighbor.hopsFromOrigin = currentlyVisiting.hopsFromOrigin + 1;
               
                neighbor.incomingVector = currentlyVisiting.edges[i].jitteredDirection;
                frontier.Enqueue(neighbor, neighbor.shortestPath);
            }

        }

    }

    public static bool PointIsOffMap(int x, int y, int z, int size)
    {
        if (x < 0 || y < 0 || z < 0) return true;
        if (x >= size || y >= size || z >= size) return true;
        return false;
    }

    public void PlaceSecondaries()
    {
        ground.transform.position = origin.worldPosition;
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

        public static int DirectionCount { get; private set; }

        static float[] magnitudes;
        
        static Vector3[] vectors;

        public static Vector3 GetDirectionVector(int directionIndex)
        {
            return vectors[directionIndex];
        }
        public static void Initialize()
        {
            DirectionCount = directions.GetLength(0);
            magnitudes = new float[directions.GetLength(0)];
            vectors = new Vector3[directions.GetLength(0)];
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
    }

    class TreeGeneratorNode
    {
        public static int mapSize;
        public struct Edge
        {
            public Vector3 jitteredDirection;
            public int directionIndex;
            public float weight;
        }

        public Edge[] edges;

        public TreeGeneratorNode()
        {
            edges = new Edge[EdgePrecalculator.DirectionCount];
            for (int i = 0; i < EdgePrecalculator.DirectionCount; i++)
            {
                edges[i] = new Edge() { directionIndex = i };
            }
        }

        public void Reset()
        {
            isLeaf = leaf;
            shortestPath = float.PositiveInfinity;
            Parent = null;
        }

        public int positionX, positionY, positionZ;

        public Vector3 worldPosition;
        public Vector3 guidingVector;

        public float shortestPath = float.PositiveInfinity;
        public int hopsFromRoot = 0, hopsFromOrigin;

        public TreeGeneratorNode Parent;
        public List<TreeGeneratorNode> children = new();

        public Vector3 incomingVector;
        public List<Vector3> outgoingVectors = new();

        public bool isLeaf;
        public bool isTrunk;


        public void CalculateGuidingVector()
        {
            if (Parent == null) guidingVector = baseGuidance;
            else
            {
                Vector3 rotationAxis = Vector3.Cross(incomingVector, baseGuidance);
                Quaternion rotator = Quaternion.AngleAxis(GetRotationFactor(), rotationAxis);
                guidingVector = rotator * Parent.guidingVector;
                guidingVector.Normalize();
                //Debug.Log(Parent.guidingVector + " pgv " + rotationAxis + ": axis " + rotator.eulerAngles + ": rotator " + guidingVector + " gv");
            }
        }

        float GetRotationFactor()
        {
            if(currentIteration == null)
            {
                return firstIteration.rotationFactor;
            }
            else if(hopsFromOrigin >= currentIteration.rotationChangeThreshold)
            {
                return currentIteration.secondaryRotationFactor;
            }
            else { return currentIteration.rotationFactor; }
        }

        public void CalculateEdgeWeights()
        {
            for (int i = 0; i < EdgePrecalculator.DirectionCount; i++)
            {
                //weighting is applied based on direction vectors between jittered points
                int x = positionX + EdgePrecalculator.GetDirectionComponent(i, 0);
                int y = positionY + EdgePrecalculator.GetDirectionComponent(i, 1);
                int z = positionZ + EdgePrecalculator.GetDirectionComponent(i, 2);
                if (PointIsOffMap(x, y, z, mapSize)) { edges[i].weight = float.PositiveInfinity; continue; }
                TreeGeneratorNode edgeTo = Map[x, y, z];
                Vector3 edge = edgeTo.worldPosition - worldPosition;
                Vector3 normalEdge = edge.FastNormalize();
                edges[i].jitteredDirection = normalEdge;
                edges[i].weight = edge.magnitude * (1 - Vector3.Dot(normalEdge, guidingVector));
            }
        }

        
    }
}






