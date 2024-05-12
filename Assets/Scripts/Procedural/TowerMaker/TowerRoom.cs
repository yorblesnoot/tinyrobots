using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRoom : MonoBehaviour
{
    [SerializeField] Transform[] doors;
    public List<Vector2Int> GetDoorPositions()
    {
        List<Vector2Int> doorPositions = new();
        foreach (Transform t in doors)
        {
            Vector2 flatPos = new(t.localPosition.x, t.localPosition.z);
            flatPos.Normalize();
            Vector2Int outPos = Vector2Int.RoundToInt(flatPos);
            doorPositions.Add(outPos);
        }
        return doorPositions;
    }
}
