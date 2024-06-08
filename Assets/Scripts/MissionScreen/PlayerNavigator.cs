using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

public class PlayerNavigator : MonoBehaviour
{
    [SerializeField] float moveTime = 1f;
    [SerializeField] PlayerData playerData;
    [SerializeField] EventProvider eventProvider;
    public static PlayerNavigator Instance { get; private set; }
    [HideInInspector] public TowerNavZone occupiedZone;

    bool moveAvailable;

    private void Awake()
    {
        Instance = this;
        moveAvailable = true;
    }

    public void TryMoveToZone(TowerNavZone zone)
    {
        if (!moveAvailable) return;
        if (occupiedZone != null && !occupiedZone.neighbors.Contains(zone)) return;

        moveAvailable = false;
        Tween.Position(transform, endValue: zone.unitPosition, duration: moveTime).OnComplete(() => FinishMove(zone));
    }

    public void FinishMove(TowerNavZone zone)
    {
        
        occupiedZone = zone;
        zone.RevealNeighbors();
        playerData.mapData[zone.zoneIndex].revealed = true;
        playerData.zoneLocation = zone.zoneIndex;

        if(zone.zoneEvent != null) zone.zoneEvent.Activate(zone, ReallowMove);
        else moveAvailable = true;
    }

    void ReallowMove()
    {
        moveAvailable = true;
        occupiedZone.zoneEvent.Clear(occupiedZone);
        occupiedZone.zoneEvent = null;
        occupiedZone.zoneEventType = 0;
        playerData.mapData[playerData.zoneLocation].eventType = 0;
    }
}
