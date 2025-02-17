using System.Collections.Generic;
using UnityEngine;

public class SpatialTarget : TargetPoint
{
    [SerializeField] protected int maxTargets = 100;
    [Range(0, 360)][SerializeField] int spatialDegree = 45;
    [SerializeField] bool containWithinTrajectory = false;

    public override float AddedRange => containWithinTrajectory ? 0 : TargetRadius;
    int layerMask;

    private void Awake()
    {
        layerMask = LayerMask.GetMask("Default");
    }

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
        List<Collider> coneTargets = PhysicsHelper.OverlapCone(point, TargetRadius, direction, spatialDegree, layerMask);

        List<Targetable> targets = new();
        foreach (Collider hit in coneTargets) if (hit.TryGetComponent(out Targetable target)) targets.Add(target);
        return targets;
    }

    public override void Hide()
    {
        SliceTargeter.ToggleVisual(false);
        SliceTargeter.Hide();
    }
}
