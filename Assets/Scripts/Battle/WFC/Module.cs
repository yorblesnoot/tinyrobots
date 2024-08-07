using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Module
{
    public int PieceIndex;
    public ModulePrototype Prototype;
    public HashSet<Connection>[] FaceConnections;
    public Vector3Int GridPosition;

    public Module()
    {
        FaceConnections = new HashSet<Connection>[6];
        for (int i = 0; i < FaceConnections.Length; i++)
        {
            FaceConnections[i] = new();
        }
    }
}


