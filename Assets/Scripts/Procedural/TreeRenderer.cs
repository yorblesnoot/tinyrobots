using System.Collections.Generic;
using UnityEngine;
using static ProceduralTreeVoxelGenerator;

public class TreeRenderer : MonoBehaviour
{
    [SerializeField] int numberOfPanelsPerRing;
    [SerializeField] GameObject meshSource;
    [SerializeField] int thickFactor = 10;
    Vector3[] directions;
    float longestBranch;
    private void Awake()
    {
        GenerateDirections();
    }

    public void RenderTree(TreeGeneratorNode origin, float longest)
    {
        longestBranch = longest;
        RenderBranch(origin);
    }

    public void RenderBranch(TreeGeneratorNode origin, int childIndex = 0)
    {
        GameObject spawned = Instantiate(meshSource);
        var mesh = new Mesh();
        spawned.GetComponent<MeshFilter>().mesh = mesh;
        List<Vector3> vertices = new();
        List<int> triangles = new();
        int startIndex = 0;
        vertices.AddRange(GetVertexRing(origin));
        BuildAndAttachRing(origin, childIndex);
        FinalizeMesh(spawned, mesh, vertices, triangles);

        void BuildAndAttachRing(TreeGeneratorNode parent, int childIndex)
        {
            if (parent.children == null || parent.children.Count == 0) return;

            TreeGeneratorNode child = parent.children[childIndex];
            if (parent == child) {Debug.LogError("recursive tree structure"); return;
        }
                //create vertices for child node
                vertices.AddRange(GetVertexRing(child));
            for (int i = 0; i < numberOfPanelsPerRing; i++)
            {
                //add vertices to master list and create triangles between source and child node
                int vertex = startIndex + i;
                int nextVertex = i == numberOfPanelsPerRing - 1 ? startIndex : vertex + 1;
                int[] upperTri = { vertex, nextVertex, nextVertex + numberOfPanelsPerRing };
                int[] lowerTri = { vertex, nextVertex + numberOfPanelsPerRing, vertex + numberOfPanelsPerRing };
                triangles.AddRange(upperTri);
                triangles.AddRange(lowerTri);
            }
            startIndex += numberOfPanelsPerRing;

            //tell the child node to attach to its first child
            BuildAndAttachRing(child, 0);

            //if there is an additional child, create a new branch starting at the origin and connecting to the next child
            if (childIndex < parent.children.Count - 1)
            {
                RenderBranch(parent, childIndex + 1);
            }
        }

    }

    private static void FinalizeMesh(GameObject spawned, Mesh mesh, List<Vector3> vertices, List<int> triangles)
    {
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        spawned.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public Vector3[] GetVertexRing(TreeGeneratorNode node)
    {
        Vector3[] vertices = new Vector3[directions.Length];
        Vector3 parentGuiding = node.Parent == null ? Vector3.up : node.Parent.guidingVector;
        Vector3 growthDirection = parentGuiding + node.guidingVector;
        growthDirection.Normalize();
        float thickness = (longestBranch - node.hopsFromRoot) / thickFactor;
        Quaternion mod = Quaternion.FromToRotation(Vector3.forward, growthDirection);

        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 finalDirection = mod * directions[i];
            finalDirection *= thickness;
            vertices[i] = finalDirection + node.worldPosition;
        }
        return vertices;
    }



    void GenerateDirections()
    {
        directions = new Vector3[numberOfPanelsPerRing];
        directions[0] = Vector3.left;
        Quaternion rotationFactor = Quaternion.AngleAxis(360 / numberOfPanelsPerRing, Vector3.forward);
        for(int i = 1; i < numberOfPanelsPerRing; i++)
        {
            directions[i] = rotationFactor * directions[i - 1];
        }
    }

}
