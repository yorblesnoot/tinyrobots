using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpatialTarget : TargetPoint
{
    [SerializeField] protected SpatialSensor indicator;
    [SerializeField] protected int maxTargets = 100;
    [SerializeField] protected float aiRadius;

    public override void Draw(List<Vector3> trajectory)
    {
        indicator.ToggleVisual(true);
    }

    public override List<Targetable> FindTargets(List<Vector3> trajectory)
    {
        Vector3 point = trajectory[^1];
        indicator.transform.position = point;
        indicator.transform.LookAt(point);
        indicator.gameObject.SetActive(true);
        return indicator.GetIntersectingTargets().Take(maxTargets)
            .OrderBy(target => Vector3.Distance(target.transform.position, point)).ToList();
    }

    public override List<Targetable> FindTargetsAI(List<Vector3> trajectory)
    {
        Vector3 point = trajectory[^1];
        Collider[] hits = Physics.OverlapSphere(point, aiRadius);
        List<Targetable> targets = new();
        foreach (Collider hit in hits)
        {
            if(hit.TryGetComponent(out Targetable target)) targets.Add(target);
        }
        return targets;
    }

    public override void EndTargeting()
    {
        indicator.ToggleVisual(false);
        indicator.gameObject.SetActive(false);
        indicator.ResetIntersecting();
    }
}