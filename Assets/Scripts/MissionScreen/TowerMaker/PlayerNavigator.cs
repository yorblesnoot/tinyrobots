using System.Collections;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;
using UnityEngine.Events;

public class PlayerNavigator : MonoBehaviour
{
    [SerializeField] float moveTime = 1f;
    [SerializeField] EventProvider eventProvider;
    public static PlayerNavigator Instance { get; private set; }
    public static UnityEvent MoveComplete = new();
    [HideInInspector] public TowerNavZone occupiedZone;
    MapData mapData;
    bool moveAvailable;

    private void Awake()
    {
        Instance = this;
        moveAvailable = true;
    }

    public void Initialize(MapData data)
    {
        mapData = data;
    }

    public void TryMoveToZone(TowerNavZone zone)
    {
        if (!moveAvailable) return;
        if (occupiedZone != null && !occupiedZone.neighbors.Contains(zone)) return;

        moveAvailable = false;
        Tween.Position(transform, endValue: zone.UnitPosition, duration: moveTime).OnComplete(() => FinishMove(zone));
    }

    public void FinishMove(TowerNavZone zone)
    {
        occupiedZone = zone;
        zone.RevealNeighbors();
        mapData.Zones[zone.zoneIndex].revealed = true;
        mapData.ZoneLocation = zone.zoneIndex;

        if(zone.zoneEvent != null) zone.zoneEvent.Activate(zone, CompleteEvent);
        else moveAvailable = true;
    }

    void CompleteEvent()
    {
        moveAvailable = true;
        MoveComplete?.Invoke();

        if (occupiedZone.zoneEvent == null) return;
        occupiedZone.zoneEvent.Clear(occupiedZone);
        occupiedZone.zoneEvent = null;
        occupiedZone.zoneEventType = 0;
        mapData.Zones[mapData.ZoneLocation].eventType = 0;
    }
}
