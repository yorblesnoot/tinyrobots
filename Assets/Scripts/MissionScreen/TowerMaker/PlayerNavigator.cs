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
    [HideInInspector] public TowerNavZone OccupiedZone;
    MapData mapData;
    bool moveAvailable;

    public static UnityEvent<TowerNavZone> EnteredZone = new();

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
        if (OccupiedZone != null && !OccupiedZone.Neighbors.Contains(zone)) return;

        moveAvailable = false;
        Tween.Position(transform, endValue: zone.UnitPosition, duration: moveTime).OnComplete(() => FinishMove(zone));
    }

    public void FinishMove(TowerNavZone zone)
    {
        OccupiedZone = zone;
        EnteredZone.Invoke(zone);
        zone.RevealNeighbors();
        mapData.Zones[zone.ZoneIndex].revealed = true;
        mapData.ZoneLocation = zone.ZoneIndex;

        if(zone.ZoneEvent != null) zone.ZoneEvent.Activate(zone, CompleteEvent);
        else moveAvailable = true;
    }

    void CompleteEvent()
    {
        moveAvailable = true;
        MoveComplete?.Invoke();

        if (OccupiedZone.ZoneEvent == null) return;
        OccupiedZone.ZoneEvent.Clear(OccupiedZone);
        OccupiedZone.ZoneEvent = null;
        OccupiedZone.ZoneEventType = 0;
        mapData.Zones[mapData.ZoneLocation].eventType = 0;
    }
}
