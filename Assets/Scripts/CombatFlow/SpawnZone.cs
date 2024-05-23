using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnZone : MonoBehaviour
{
    public static UnityEvent<BotPlacer> GetSpawnZones = new();
    public Allegiance Allegiance;
    public float Radius = 1;
    public Vector3 Position { get { return transform.position; } }
    private void Awake()
    {
        GetSpawnZones.AddListener(SubmitSpawnZone);
    }

    void SubmitSpawnZone(BotPlacer placer)
    {
        placer.SubmitZone(this);
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(Position, Radius);
    }
}
