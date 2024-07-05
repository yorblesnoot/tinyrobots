using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EventProvider : MonoBehaviour 
{
    [SerializeField] ZoneEvent[] zoneEvents;
    public ZoneEvent this[int i]
    {
        get { return zoneEvents[i]; }
    }
    public int GetRandomWeightedEvent()
    {
        return 1;
        int totalWeight = zoneEvents.Sum(x => x.weight);
        int random = Random.Range(0, totalWeight);
        float current = 0;
        for(int i = 0; i < zoneEvents.Count(); i++)
        {
            current += zoneEvents[i].weight;
            if(random < current) return i;
        }
        return 0;
    }
}

