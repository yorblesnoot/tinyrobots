using System.Collections.Generic;
using UnityEngine;

public class TreeRenderer : MonoBehaviour
{
    [SerializeField] TreeRenderParams tp;
    
    [SerializeField] GameObject meshSource;
    [SerializeField] GameObject leaf;
    Vector3[] directions;
    float longestBranch;

    static List<Mesh> meshParts;
    private void Awake()
    {
        directions = GenerateDirections(tp.numberOfPanelsPerRing);
    }

    public void RenderTree(SimplifiedTreeNode origin, float longest)
    {
        longestBranch = longest;
        meshParts = new();
        RenderBranch(origin);
        FuseMeshes();
    }

    private void FuseMeshes()
    {
        Mesh finalMesh = new();
        GameObject spawned = Instantiate(meshSource);
        MeshFilter filter = spawned.GetComponent<MeshFilter>();
        filter.mesh = finalMesh;
        spawned.GetComponent<MeshCollider>().sharedMesh = finalMesh;
        CombineInstance[] combiner = new CombineInstance[meshParts.Count];
        for (int i = 0; i < meshParts.Count; i++)
        {
            combiner[i].mesh = meshParts[i];
            combiner[i].transform = filter.transform.localToWorldMatrix;
        }
        

        finalMesh.CombineMeshes(combiner);
        finalMesh.RecalculateNormals();
    }

    public void RenderBranch(SimplifiedTreeNode origin, int childIndex = 0)
    {
        var mesh = new Mesh();
        
        List<Vector3> vertices = new();
        List<int> triangles = new();
        int startIndex = 0;

        vertices.AddRange(GetVertexRing(origin));
        BuildAndAttachRing(origin, childIndex);
        FinalizeMesh(mesh, vertices, triangles);

        void BuildAndAttachRing(SimplifiedTreeNode parent, int childIndex)
        {
            if (parent.children == null || parent.children.Count == 0)
            {
                //place cap
                return;
            }

            SimplifiedTreeNode child = parent.children[childIndex];
            //create vertices for child node
            vertices.AddRange(GetVertexRing(child));
            for (int i = 0; i < tp.numberOfPanelsPerRing; i++)
            {
                //add vertices to master list and create triangles between source and child node
                int vertex = startIndex + i;
                int nextVertex = i == tp.numberOfPanelsPerRing - 1 ? startIndex : vertex + 1;
                int[] upperTri = { vertex, nextVertex, nextVertex + tp.numberOfPanelsPerRing };
                int[] lowerTri = { vertex, nextVertex + tp.numberOfPanelsPerRing, vertex + tp.numberOfPanelsPerRing };
                triangles.AddRange(upperTri);
                triangles.AddRange(lowerTri);
            }
            startIndex += tp.numberOfPanelsPerRing;

            //tell the child node to attach to its first child
            BuildAndAttachRing(child, 0);

            //if there is an additional child, create a new branch starting at the origin and connecting to the next child
            if (childIndex < parent.children.Count - 1)
            {
                RenderBranch(parent, childIndex + 1);
            }
        }

    }

    private static void FinalizeMesh(Mesh mesh, List<Vector3> vertices, List<int> triangles)
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        meshParts.Add(mesh);
    }

    public Vector3[] GetVertexRing(SimplifiedTreeNode node)
    {
        Vector3[] vertices = new Vector3[directions.Length];
        Vector3 growthDirection = node.incomingVector == Vector3.zero ? Vector3.up : node.incomingVector
            + (node.outgoingVectors.Count > 0 ? node.outgoingVectors[0] : Vector3.zero);
        growthDirection.Normalize();
        Quaternion mod = Quaternion.FromToRotation(Vector3.forward, growthDirection);
        float thickness;
        
        float thicknessLevel = node.hopsFromRoot / longestBranch;
        thickness = tp.thicknessCurve.Evaluate(thicknessLevel);
        thickness = Mathf.Clamp(thickness, .1f, thickness);
        if (node.isLeaf)
        {
            thickness /= 5;
            AttachLeaves(node, thickness, mod);
        }
        

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 finalDirection = mod * directions[i];
            finalDirection *= thickness;
            vertices[i] = finalDirection + node.worldPosition;
        }
        return vertices;
    }

    static readonly Vector3[] leafAngles = { Vector3.left, Vector3.right};
    private void AttachLeaves(SimplifiedTreeNode node, float thickness, Quaternion mod)
    {
        foreach(var angle in leafAngles)
        {
            Vector3 leafDirection = mod * angle;
            Vector3 leafOffset = leafDirection * thickness;
            Instantiate(leaf, node.worldPosition + leafOffset, Quaternion.LookRotation(leafDirection));
        }
    }

    public static Vector3[] GenerateDirections(int pointsPerSource)
    {
        Vector3[] directions = new Vector3[pointsPerSource];
        directions[0] = Vector3.left;
        Quaternion rotationFactor = Quaternion.AngleAxis(360 / pointsPerSource, Vector3.forward);
        for(int i = 1; i < pointsPerSource; i++)
        {
            directions[i] = rotationFactor * directions[i - 1];
        }
        return directions;
    }

}
