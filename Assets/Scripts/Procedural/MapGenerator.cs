using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MapGenerator : MonoBehaviour
{
    public abstract byte[,,] GenerateCoreMap();

    public abstract void PlaceSecondaries();
}
