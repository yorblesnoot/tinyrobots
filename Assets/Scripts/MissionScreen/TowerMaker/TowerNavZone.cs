using PrimeTween;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerNavZone : MonoBehaviour
{
    public GameObject battleMap;
    [SerializeField] float unitHeight = 1;
    [SerializeField] float revealDuration = 1;
    [SerializeField] Transform unitPoint;

    [HideInInspector] public HashSet<TowerNavZone> neighbors;
    [HideInInspector] public Vector3 UnitPosition { get { return (unitPoint != null ? unitPoint : transform).position + Vector3.up * unitHeight; } }
    [HideInInspector] public TowerPiece towerPiece;
    [HideInInspector] public int zoneIndex;
    [HideInInspector] public int zoneEventType;
    [HideInInspector] public ZoneEvent zoneEvent;

    Renderer[] renderers;
    int marginDistance;
    int evaporationSource;
    public void Initialize()
    {
        neighbors = new();
        towerPiece = GetComponent<TowerPiece>();
        foreach (var room in towerPiece.rooms)
        {
            room.associatedZone = this;
        }
        PrepBlackout();
    }

    void PrepBlackout()
    {
        
        marginDistance = Shader.PropertyToID("_MarginDistance");
        evaporationSource = Shader.PropertyToID("_EvaporationSource");
        renderers = GetComponentsInChildren<Renderer>().Where(ren => ren.material.HasProperty(marginDistance)).ToArray();
        HideRooms();
    }

    public void ZoneClicked()
    {
        PlayerNavigator.Instance.TryMoveToZone(this);
    }

    public void RevealNeighbors(bool instant = false)
    {
        foreach(var room in neighbors)
        {
            room.Reveal(UnitPosition, instant);
        }
    }

    bool revealed = false;
    public void Reveal(Vector3 source, bool instant = false)
    {
        if (revealed) return;
        revealed = true;
        zoneEvent?.Visualize(this);
        float maxDistance = source == UnitPosition ? 10 : Vector3.Distance(UnitPosition, source) * 2;
        foreach (var renderer in renderers)
        {
            renderer.material.SetVector(evaporationSource, source);
            Tween.MaterialProperty(renderer.material, marginDistance, duration: instant ? 0 : revealDuration, endValue: maxDistance);
        }
    }

    internal void HideRooms()
    {
        foreach (var renderer in renderers)
        {
            renderer.material.SetFloat(marginDistance, -1f);
        }
    }
}
