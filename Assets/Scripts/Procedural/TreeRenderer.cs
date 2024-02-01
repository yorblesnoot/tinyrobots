using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TreeRenderer : MonoBehaviour
{
    [SerializeField] int numberOfPanelsPerRing;
    [SerializeField] GameObject meshSource;
    Vector3[] directions;
    private void Awake()
    {
        GenerateDirections();

        RenderBranch(BuildDummyTree(5));
    }

    TreeRenderNode BuildDummyTree(int levels)
    {
        TreeRenderNode origin = new() { position = Vector3.zero, growthDirection = Vector3.up, thickness = 1f};
        TreeRenderNode current = origin;
        for(int i = 0; i < levels; i++)
        {
            current.children = new TreeRenderNode[1];
            current.children[0] = new() { position = current.position + current.growthDirection, growthDirection = Vector3.left + Vector3.up, thickness = 2f };
            current = current.children[0];
        }
        return origin;
    }

    void RenderBranch(TreeRenderNode origin, int childIndex = 0)
    {
        GameObject spawned = Instantiate(meshSource);
        var mesh = new Mesh();
        spawned.GetComponent<MeshFilter>().mesh = mesh;
        List<Vector3> vertices = new();
        List<int> triangles = new();
        int startIndex = 0;
        vertices.AddRange(origin.SetVertexRing(directions));
        BuildAndAttachRing(origin, childIndex);
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        spawned.GetComponent<MeshCollider>().sharedMesh = mesh;


        void BuildAndAttachRing(TreeRenderNode source, int childIndex)
        {
            if (source.children == null) return;

            vertices.AddRange(source.children[childIndex].SetVertexRing(directions));
            for(int i = 0; i < numberOfPanelsPerRing; i++)
            {
                int vertex = startIndex + i;
                int nextVertex = i == numberOfPanelsPerRing - 1 ? startIndex : vertex + 1;
                int[] upperTri = { vertex, nextVertex, nextVertex + numberOfPanelsPerRing};
                int[] lowerTri = { vertex, nextVertex + numberOfPanelsPerRing, vertex + numberOfPanelsPerRing };
                triangles.AddRange(upperTri);
                triangles.AddRange(lowerTri);
            }
            startIndex += numberOfPanelsPerRing;
            BuildAndAttachRing(source.children[childIndex], 0);
            if (source.children.Count() == 1) return;
            for(int i = 1; i < source.children.Count(); i++)
            {
                RenderBranch(source, i);
            }
        }
        
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

public class TreeRenderNode
{
    public Vector3 position, growthDirection;
    public Vector3[] vertices;
  
    public float thickness;

    public TreeRenderNode[] children;

    public Vector3[] SetVertexRing(Vector3[] directions)
    {
        vertices = new Vector3[directions.Length];
        Quaternion mod = Quaternion.LookRotation(growthDirection);
        for (int i = 0; i < directions.Length; i++)
        {
            Vector3 finalDirection = mod * directions[i];
            finalDirection *= thickness;
            vertices[i] = finalDirection + position;
        }
        return vertices;
    }
}