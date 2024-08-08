using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Module : IWeighted
{
    public int ModuleIndex;
    public int PieceIndex;
    public int OrientationIndex;
    public float Weight { get; set; } = 0;
    public ModulePrototype Prototype;
    public FaceConnections[] FaceConnections;
    public Vector3Int GridPosition;

    public Module()
    {
        FaceConnections = new FaceConnections[6];
        for (int i = 0; i < FaceConnections.Length; i++)
        {
            FaceConnections[i] = new();
        }
    }
}

[Serializable]
public class FaceConnections
{
    public List<int> ModuleLinks = new();
    public int this[int index]
    {
        get
        {
            return ModuleLinks[index];
        }

        set { ModuleLinks[index] = value; }
    }

}


