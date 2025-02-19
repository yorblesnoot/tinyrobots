using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ImpactTarget : TargetPoint
{
    readonly float checkRadius = 1;
    int layerMask;
    private void Awake()
    {
        layerMask = LayerMask.GetMask("Default");   
    }
    public override void Draw(List<Vector3> trajectory)
    {
        
    }

    public override void Hide()
    {
        
    }

    public override List<Targetable> FindTargets(List<Vector3> trajectory)
    {
        List<Targetable> targets = new();
        Vector3 castPoint = trajectory[^1];
        Collider[] possibleTargets = Physics.OverlapSphere(castPoint, checkRadius, layerMask);
        if(possibleTargets.Length > 0)
        {
            possibleTargets = possibleTargets.OrderBy(x => Vector3.Distance(castPoint, x.transform.position)).ToArray();
            targets.Add(possibleTargets[0].GetComponent<Targetable>());
        } 
        return targets;
    }
}
