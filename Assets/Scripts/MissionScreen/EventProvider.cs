using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum ZoneEventType
{
    NONE,
    BATTLE
}
public class EventProvider : MonoDictionary<ZoneEventType, ZoneEvent>
{
    public ZoneEventType GetRandomWeightedEvent()
    {
        int totalWeight = Values.Sum(x => x.weight);
        int random = Random.Range(0, totalWeight);
        List<ZoneEventType> events = Keys.ToList();
        float current = 0;
        for(int i = 0; i < events.Count; i++)
        {
            current += this[events[i]].weight;
            if(random < current) return events[i];
        }
        return ZoneEventType.NONE;
    }
}

