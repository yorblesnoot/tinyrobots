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
    [SerializeField][Range(0, 1)] float maxGlow = .8f;
    [SerializeField] float highlightDuration = .3f;

    public bool AvailableMove { get; private set; }
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

    bool glowPaused = false;
    
    
    public void Initialize()
    {
        Neighbors = new();
        TowerPiece = GetComponent<TowerPiece>();
        foreach (var room in TowerPiece.rooms)
        {
            room.associatedZone = this;
        }
        PrepBlackout();
        PlayerNavigator.EnteredZone.AddListener(ToggleAvailability);
    }

    void ToggleAvailability(TowerNavZone zone)
    {
        if(Neighbors.Contains(zone))
        {
            if (!AvailableMove)
            {
                AvailableMove = true;
                StartCoroutine(GlowPulse());
            }
        }
        else AvailableMove = false;
    }

    IEnumerator GlowPulse()
    {
        while (AvailableMove)
        {
            if(glowPaused) yield return RampHighlight();
            float level = Mathf.Sin(Time.time * pulseFrequency);
            level = level.Remap(-1, 1, -maxGlow, -1);
            foreach (var renderer in renderers) renderer.material.SetFloat(glowThreshold, level);
            yield return null;
        }
        StartCoroutine(RampHighlight(false));
    }

    IEnumerator RampHighlight(bool up = true)
    {
        Sequence sequence = Sequence.Create();
        foreach (var renderer in renderers)
        {
            sequence.Group(Tween.MaterialProperty(renderer.material, glowThreshold, up ? -maxGlow/2 : -1, highlightDuration));
        }
        yield return sequence.ToYieldInstruction();
        yield return new WaitUntil(() => glowPaused == false);
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
        if (!AvailableMove) return;
        PlayerNavigator.Instance.TryMoveToZone(this);
    }

    public void MouseHighlight(bool on = true)
    {
        glowPaused = on;
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
            if (instant) renderer.material.SetFloat(marginDistance, maxDistance);
            else Tween.MaterialProperty(renderer.material, marginDistance, duration: instant ? 0 : revealDuration, endValue: maxDistance);
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
