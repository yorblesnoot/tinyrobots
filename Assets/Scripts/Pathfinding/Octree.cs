using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Octree : MonoBehaviour
{
    //1: how to structure octree data?
    //can look at node tree for guidance

    //2: how to get an array map into an octree
    public void ConvertArrayToOctree(byte[,,] map)
    {
        int length = map.GetLength(0);
        int height = map.GetLength(1);
        int depth = map.GetLength(2);
        OctreeNode[,,] nodeArray = new OctreeNode[length, height, depth];
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    nodeArray[x, y, z] = new OctreeNode { Value = map[x,y,z] };
                }
            }
        }
    }

    OctreeNode[,,] AscendOctreeTier(OctreeNode[,,] nodeArray)
    {
        int length = nodeArray.GetLength(0) / 2;
        int height = nodeArray.GetLength(1) / 2;
        int depth = nodeArray.GetLength(2) / 2;
        OctreeNode[,,] parentArray = new OctreeNode[length, height, depth];
        for (int x = 0; x < length; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < depth; z++)
                {
                    parentArray[x, y, z] = CompactCube(x, y, z, nodeArray);
                }
            }
        }
        return parentArray;
    }

    private const int regionSize = 2;
    OctreeNode CompactCube(int ox, int oy, int oz, OctreeNode[,,] nodeArray)
    {
        bool pruned = false;
        OctreeNode parent = new();
        OctreeNode[] children = new OctreeNode[8];
        byte comparisonValue = nodeArray[2*ox, 2*oy, 2*oz].Value;
        for (int x = 0; x < regionSize; x++)
        {
            for (int y = 0; y < regionSize; y++)
            {
                for (int z = 0; z < regionSize; z++)
                {
                    if(nodeArray[2 * ox + x, 2 * oy + y, 2 * oz + z].Value != comparisonValue)
                    {
                        pruned = true;
                        break;
                    }
                    children[x+y+z] = new OctreeNode();
                }
            }
        }
        if (!pruned)
        {
            parent.Children = children;
        }
        else
        {
            parent.Value = comparisonValue;
        }
        return parent;
    }

    //3: how to pathfind on an octree
}

class OctreeNode
{
    public OctreeNode Parent;
    public OctreeNode[] Children;
    public Bounds Area;
    public byte Value;
}

