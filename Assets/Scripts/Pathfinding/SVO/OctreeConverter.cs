using System.Collections.Generic;
using UnityEngine;

public static class OctreeConverter
{
    public static Dictionary<Vector3Int, OctreeNode> Convert(byte[,,] data, out OctreeNode root)
    {
        OctreeNode[,,] treeGrid = CreateInitialTier(data);
        while(treeGrid.GetLength(0) > 1) treeGrid = AscendTier(treeGrid);
        root = treeGrid[0, 0, 0];
        return BuildMap(root);
    }

    static OctreeNode[,,] CreateInitialTier(byte[,,] data)
    {
        int[] originalDimensions = new int[3];
        for(int i = 0; i < 3; i++) originalDimensions[i] = data.GetLength(i);
        int maxDimension = Mathf.Max(originalDimensions[0], originalDimensions[1], originalDimensions[2]);
        int newSize = 1;
        while (newSize < maxDimension) newSize *= 2;
        OctreeNode[,,] nodeMap = new OctreeNode[newSize, newSize, newSize];
        for(int x = 0; x < newSize; x++)
        {
            for(int y = 0; y < newSize; y++)
            {
                for (int z = 0; z < newSize; z++)
                {
                    OctreeNode node = new()
                    {
                        Positions = new() { new(x, y, z) }
                    };
                    if (x < originalDimensions[0] && y < originalDimensions[1] && z < originalDimensions[2]) node.Value = data[x, y, z];
                    nodeMap[x, y, z] = node;
                }
            }
        }
        return nodeMap;
    }

    static OctreeNode[,,] AscendTier(OctreeNode[,,] data)
    {
        int newSize = data.GetLength(0) / 2;
        OctreeNode[,,] newTier = new OctreeNode[newSize, newSize, newSize];
        for( int x = 0; x < newSize; x++)
        {
            for(int y = 0; y < newSize; y++)
            {
                for(int z = 0; z < newSize; z++)
                {
                    newTier[x,y,z] = GenerateParentNode(x, y, z, data);
                }
            }
        }
        return newTier;
    }

    private const int regionSize = 2;
    static OctreeNode GenerateParentNode(int ox, int oy, int oz, OctreeNode[,,] lowerTier)
    {
        bool pruned = true;
        OctreeNode parent = new();
        List<OctreeNode> children = new();
        int[] o = new int[] { 2 * ox, 2 * oy, 2 * oz };
        byte comparisonValue = lowerTier[o[0], o[1], o[2]].Value;
        for (int x = 0; x < regionSize; x++)
        {
            for (int y = 0; y < regionSize; y++)
            {
                for (int z = 0; z < regionSize; z++)
                {
                    OctreeNode child = lowerTier[o[0] + x, o[1] + y, o[2] + z];
                    if (child.Value != comparisonValue) pruned = false;
                    children.Add(child);
                }
            }
        }
        if (pruned) 
        {
            foreach(OctreeNode child in children) parent.Positions.AddRange(child.Positions);
            parent.Value = comparisonValue;
        }
        else parent.Children = children.ToArray();
        return parent;
    }

    static Dictionary<Vector3Int, OctreeNode> BuildMap(OctreeNode root)
    {
        Dictionary<Vector3Int, OctreeNode> map = new();
        root.Traverse(AddPositionsToMap);
        return map;

        void AddPositionsToMap(OctreeNode node)
        {
            foreach(Vector3Int postion in node.Positions) map.Add(postion, node);
        }
    }

}
