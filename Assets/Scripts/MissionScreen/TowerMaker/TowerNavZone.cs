using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerNavZone : MonoBehaviour
{
    public GameObject battleMap;
    [SerializeField] float unitHeight = 1;
    [SerializeField] float revealDuration = 1;
    [SerializeField] Transform unitPoint;
    [SerializeField] float pulseFrequency = 1;

    [HideInInspector] public HashSet<TowerNavZone> Neighbors;
    [HideInInspector] public Vector3 UnitPosition { get { return (unitPoint != null ? unitPoint : transform).position + Vector3.up * unitHeight; } }
    [HideInInspector] public TowerPiece TowerPiece;
    [HideInInspector] public int ZoneIndex;
    [HideInInspector] public int ZoneEventType;
    [HideInInspector] public ZoneEvent ZoneEvent;

    Renderer[] renderers;
    int marginDistance;
    int evaporationSource;
    int glowThreshold;

    
    public void Initialize()
    {
        Neighbors = new();
        TowerPiece = GetComponent<TowerPiece>();
        foreach (var room in TowerPiece.rooms)
        {
            room.associatedZone = this;
        }
        PrepBlackout();
        PlayerNavigator.EnteredZone.AddListener(ToggleHighlight);
    }

    void ToggleHighlight(TowerNavZone zone)
    {
        if (Neighbors.Contains(zone)) StartCoroutine(GlowPulse());
        else StopCoroutine(GlowPulse());
    }

    IEnumerator GlowPulse()
    {
        while (true)
        {
            float level = Mathf.Sin(Time.time * pulseFrequency);
            level = Mathf.Clamp(level - 1, -1, 0);
            foreach (var renderer in renderers)
            {
                renderer.material.SetFloat(glowThreshold, level);
            }
            yield return null;
        }
        
    }

    void PrepBlackout()
    {
        
        marginDistance = Shader.PropertyToID("_MarginDistance");
        evaporationSource = Shader.PropertyToID("_EvaporationSource");
        glowThreshold = Shader.PropertyToID("_GlowThreshold");
        renderers = GetComponentsInChildren<Renderer>().Where(ren => ren.material.HasProperty(marginDistance)).ToArray();
        HideRooms();
    }

    public void ZoneClicked()
    {
        PlayerNavigator.Instance.TryMoveToZone(this);
    }

    public void RevealNeighbors(bool instant = false)
    {
        foreach(var room in Neighbors)
        {
            room.Reveal(UnitPosition, instant);
        }
    }

    bool revealed = false;
    public void Reveal(Vector3 source, bool instant = false)
    {
        if (revealed) return;
        revealed = true;
        ZoneEvent?.Visualize(this);
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
