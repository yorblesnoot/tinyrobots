using System.Collections.Generic;
using UnityEngine;

public class SimplifiedTreeNode
{
    public List<SimplifiedTreeNode> children;

    public Vector3 incomingVector;
    public List<Vector3> outgoingVectors;
    public int hopsFromRoot;
    public bool isLeaf;
    public Vector3 worldPosition;
}
