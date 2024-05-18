using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerNavigableZone : MonoBehaviour
{
    [SerializeField] float unitHeight = 1;
    [SerializeField] float revealDuration = 1;

    [HideInInspector] public HashSet<TowerNavigableZone> neighbors;
    [HideInInspector] public Vector3 unitPosition;

    TowerPiece towerPiece;
    Renderer[] renderers;
    int marginDistance;
    int evaporationSource;
    public void Initialize()
    {
        neighbors = new();
        unitPosition = transform.position;
        unitPosition.y += unitHeight;
        towerPiece = GetComponent<TowerPiece>();
        foreach (var room in towerPiece.rooms)
        {
            room.associatedZone = this;
        }

        PrepBlackout();
    }

    private void PrepBlackout()
    {
        renderers = GetComponentsInChildren<Renderer>();
        marginDistance = Shader.PropertyToID("_MarginDistance");
        evaporationSource = Shader.PropertyToID("_EvaporationSource");
        HideRooms();
    }

    public void ZoneClicked()
    {
        PlayerNavigator.Instance.TryMoveToZone(this);
    }

    public void RevealNeighbors()
    {
        foreach(var room in neighbors)
        {
            room.Reveal(unitPosition);
        }
    }

    bool revealed = false;
    public void Reveal(Vector3 source)
    {
        if (revealed) return;
        revealed = true;
        float maxDistance = source == unitPosition ? 10 : Vector3.Distance(unitPosition, source) * 2;
        foreach (var renderer in renderers)
        {
            renderer.material.SetVector(evaporationSource, source);
            Tween.MaterialProperty(renderer.material, marginDistance, duration: revealDuration, endValue: maxDistance);
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
