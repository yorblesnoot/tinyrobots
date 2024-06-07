using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public abstract class ZoneEvent : MonoBehaviour
{
    public int weight;
    public abstract void Activate(TowerNavZone zone, UnityAction eventComplete);
    public abstract void Visualize(TowerNavZone zone);
}
