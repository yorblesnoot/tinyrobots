using System.Collections.Generic;
using UnityEngine;

public class SpatialTarget : TargetPoint
{
    [SerializeField] protected int maxTargets = 100;
    [Range(0, 360)][SerializeField] int spatialDegree = 45;

    public override void Draw(List<Vector3> trajectory)
    {
        SliceTargeter.ToggleVisual(true);
        SliceTargeter.SetShape(spatialDegree, TargetRadius);
        Vector3 point = trajectory[^1];
        Vector3 direction = point - trajectory[0];
        SliceTargeter.Transform.SetPositionAndRotation(point, Quaternion.LookRotation(direction));
        SliceTargeter.Transform.gameObject.SetActive(true);
    }

    public override List<Targetable> FindTargets(List<Vector3> trajectory)
    {
        Vector3 point = trajectory[^1];
        Collider[] hits = Physics.OverlapSphere(point, TargetRadius);
        List<Targetable> targets = new();
        Vector3 forward = trajectory[^1] - trajectory[^2];
        foreach (Collider hit in hits)
        {
            Vector3 targetDirection = (hit.transform.position - point).normalized;
            float dot = Vector3.Dot(forward, targetDirection);
            float degree = (1 - dot) * 180;
            if (degree > spatialDegree) continue;
            if (hit.TryGetComponent(out Targetable target)) targets.Add(target);
        }
        return targets;
    }

    public override void Hide()
    {
        SliceTargeter.ToggleVisual(false);
        SliceTargeter.Hide();
    }
}
