using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactTarget : TargetPoint
{
    readonly float overlap = .1f;
    int layerMask;
    private void Awake()
    {
        layerMask = LayerMask.GetMask("Default");   
    }
    public override void Draw(List<Vector3> trajectory)
    {
        
    }

    public override void EndTargeting()
    {
        
    }

    public override List<Targetable> FindTargets(List<Vector3> trajectory)
    {
        bool singlePoint = trajectory.Count == 1;
        Vector3 origin = singlePoint ? trajectory[0] + Vector3.up : trajectory[^2];
        Vector3 direction = singlePoint ? Vector3.down : trajectory[^1] - origin;
        Physics.Raycast(origin, direction, out var hitInfo, direction.magnitude + overlap, layerMask);
        if (hitInfo.collider.TryGetComponent(out Targetable target)) return new() { target };
        return null;
    }

    public override List<Targetable> FindTargetsAI(List<Vector3> trajectory)
    {
        return FindTargets(trajectory);
    }
}
