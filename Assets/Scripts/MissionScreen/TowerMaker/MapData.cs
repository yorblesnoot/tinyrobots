using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MapData 
{
    public int ZoneLocation;
    public List<SavedNavZone> Zones;
}

[Serializable]
public class SavedNavZone
{
    public int PieceIndex;
    public Vector3 Position;
    public Quaternion Rotation;
    public int[] NeighborIndices;
    public bool Revealed;
    public int EventType;
}
