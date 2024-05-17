using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerRoom : MonoBehaviour
{
    [SerializeField] Transform[] doors;
    [SerializeField] Transform[] anchors;

    [HideInInspector] public TowerNavigableZone associatedZone;
    public List<Vector2Int> GetDoorPositions()
    {
        return GetGridPositions(doors);
    }

    public List<Vector2Int> GetAnchorPositions()
    {
        return GetGridPositions(anchors);
    }

    public List<Vector2Int> GetGridPositions(Transform[] targets)
    {
        List<Vector2Int> doorPositions = new();
        foreach (Transform t in targets)
        {
            Vector2 flatPos = new(t.localPosition.x, t.localPosition.z);
            flatPos.Normalize();
            Vector2Int outPos = Vector2Int.RoundToInt(flatPos);
            doorPositions.Add(outPos);
        }
        return doorPositions;
    }

    private void OnMouseDown()
    {
        associatedZone.ZoneClicked();
    }
}
