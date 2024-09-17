using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeNode 
{
    public OctreeNode Parent;
    public OctreeNode[] Children;
    public OctreeNode[] Neighbors;
    public byte Value;
    public List<Vector3Int> Positions;

    public void Traverse(Action<OctreeNode> action)
    {
        action(this);
        foreach (var child in Children)
            child.Traverse(action);
    }
}
