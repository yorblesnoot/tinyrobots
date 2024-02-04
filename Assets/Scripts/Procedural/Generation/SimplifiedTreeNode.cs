using System.Collections.Generic;
using UnityEngine;

public class SimplifiedTreeNode
{
    public List<SimplifiedTreeNode> children;

    public Vector3 growthDirection;
    public int hopsFromRoot;
    public bool isLeaf;
    public bool isTrunk;
    public Vector3 worldPosition;
}
