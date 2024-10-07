using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpatialTarget : TargetPoint
{
    [SerializeField] protected int maxTargets = 100;
    [Range(0, 360)][SerializeField] int spatialDegree = 45;

    public override void Draw(List<Vector3> trajectory)
    {
        SliceTargeter.ToggleVisual(true);
    }

    public override List<Targetable> FindTargets(List<Vector3> trajectory)
    {
        SliceTargeter.SetShape(spatialDegree, TargetRadius);
        Vector3 point = trajectory[^1];
        Vector3 direction = point - trajectory[0];
        SliceTargeter.Transform.SetPositionAndRotation(point, Quaternion.LookRotation(direction));
        SliceTargeter.Transform.gameObject.SetActive(true);
        return SliceTargeter.Sensor.GetIntersectingTargets().Take(maxTargets)
            .OrderBy(target => Vector3.Distance(target.transform.position, point)).ToList();
    }

    public override List<Targetable> FindTargetsAI(List<Vector3> trajectory)
    {
        Vector3 point = trajectory[^1];
        Collider[] hits = Physics.OverlapSphere(point, TargetRadius);
        List<Targetable> targets = new();
        foreach (Collider hit in hits)
        {
            if(hit.TryGetComponent(out Targetable target)) targets.Add(target);
        }
        return targets;
    }

    public override void EndTargeting()
    {
        SliceTargeter.ToggleVisual(false);
        SliceTargeter.Hide();
        SliceTargeter.Sensor.ResetIntersecting();
    }
}
