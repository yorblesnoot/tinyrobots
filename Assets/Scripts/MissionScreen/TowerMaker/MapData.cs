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
    public int pieceIndex;
    public Vector3 position;
    public Quaternion rotation;
    public int[] neighborIndices;
    public bool revealed;
    public int eventType;
}
