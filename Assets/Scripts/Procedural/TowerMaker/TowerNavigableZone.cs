using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerNavigableZone : MonoBehaviour
{
    [HideInInspector] public HashSet<TowerNavigableZone> neighbors;
    [HideInInspector] public Vector3 unitPosition;

    [SerializeField] float unitHeight = 1;
    TowerPiece towerPiece;
    public void Initialize()
    {
        neighbors = new();
        unitPosition = transform.position;
        unitPosition.y += unitHeight;
        towerPiece = GetComponent<TowerPiece>();
        foreach(var room in towerPiece.rooms)
        {
            room.associatedZone = this;
        }
    }
    public void ZoneClicked()
    {
        PlayerNavigator.Instance.TryMoveToZone(this);
    }
}
