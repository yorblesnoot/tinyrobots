using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EventProvider : MonoBehaviour 
{
    List<ZoneEvent> zoneEvents;
    [SerializeField] ZoneEvent bossEvent;
    [SerializeField] ZoneEvent healEvent;

    private void Awake()
    {
        zoneEvents = GetComponentsInChildren<ZoneEvent>().ToList();
    }
    public ZoneEvent this[int i]
    {
        get { return zoneEvents[i]; }
    }
    public int GetRandomWeightedEvent()
    {
        int totalWeight = zoneEvents.Sum(x => x.weight);
        int random = UnityEngine.Random.Range(0, totalWeight);
        float current = 0;
        for(int i = 0; i < zoneEvents.Count(); i++)
        {
            current += zoneEvents[i].weight;
            if(random < current) return i;
        }
        return 0;
    }

    public void PlaceBossEvent(TowerNavZone zone)
    {
        zone.ZoneEventType = zoneEvents.IndexOf(bossEvent);
        zone.ZoneEvent = bossEvent;
    }

    public void PlaceHealEvent(TowerNavZone zone)
    {
        zone.ZoneEventType = zoneEvents.IndexOf(healEvent);
        zone.ZoneEvent = healEvent;
    }
}

