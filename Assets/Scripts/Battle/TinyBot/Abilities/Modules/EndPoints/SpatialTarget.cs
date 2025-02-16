using System.Collections.Generic;
using UnityEngine;

public class SpatialTarget : TargetPoint
{
    [SerializeField] protected int maxTargets = 100;
    [Range(0, 360)][SerializeField] int spatialDegree = 45;
    [SerializeField] bool containWithinTrajectory = false;

    public override float AddedRange => containWithinTrajectory ? 0 : TargetRadius;


    public override void Draw(List<Vector3> trajectory)
    {
        SliceTargeter.ToggleVisual(true);
        SliceTargeter.SetShape(spatialDegree, TargetRadius);
        CalculatePlacement(trajectory, out Vector3 point, out Vector3 direction);
        SliceTargeter.Transform.SetPositionAndRotation(point, Quaternion.LookRotation(direction));
        SliceTargeter.Transform.gameObject.SetActive(true);
    }

    private void CalculatePlacement(List<Vector3> trajectory, out Vector3 point, out Vector3 direction)
    {
        point = trajectory[^1];
        direction = point - trajectory[^2];
        if (containWithinTrajectory) point -= direction.normalized * Mathf.Min(TargetRadius, direction.magnitude);
    }

    public override List<Targetable> FindTargets(List<Vector3> trajectory)
    {
        CalculatePlacement(trajectory, out Vector3 point, out Vector3 direction);
        Collider[] hits = Physics.OverlapSphere(point, TargetRadius);

        List<Targetable> targets = new();
        foreach (Collider hit in hits)
        {
            Vector3 targetDirection = (hit.transform.position - point).normalized;
            float dot = Vector3.Dot(direction, targetDirection);
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
