using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public abstract class ZoneEvent : MonoBehaviour
{
    [SerializeField] float clearDuration = .5f;
    [SerializeField] protected GameObject marker;
    Dictionary<TowerNavZone, GameObject> zoneMarkers = new();
    public int weight;
    public abstract void Activate(TowerNavZone zone, UnityAction eventComplete);
    public virtual void Visualize(TowerNavZone zone)
    {
        GameObject spawnedMarker = Instantiate(marker, zone.UnitPosition, Quaternion.identity);
        spawnedMarker.transform.SetParent(zone.transform, true);
        CanvasGroup spawnedGroup = spawnedMarker.GetComponentInChildren<CanvasGroup>();
        spawnedGroup.alpha = 0;
        Tween.Alpha(spawnedGroup, endValue: 1, duration: clearDuration);
        zoneMarkers.Add(zone, spawnedMarker);
    }

    public virtual void Clear(TowerNavZone zone)
    {
        GameObject spawnedMarker = zoneMarkers[zone];
        CanvasGroup spawnedGroup = spawnedMarker.GetComponentInChildren<CanvasGroup>();
        Tween.Alpha(spawnedGroup, endValue: 0, duration: clearDuration).OnComplete(() => Destroy(spawnedMarker));
        zoneMarkers.Remove(zone);
    }
}
