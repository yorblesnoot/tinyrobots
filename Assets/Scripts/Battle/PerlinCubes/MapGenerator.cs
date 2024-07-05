using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapGenerator : MonoBehaviour
{
    public abstract void GenerateCoreMap();

    public abstract void PlaceSecondaries();

    public abstract byte[,,] GetByteMap();
}
