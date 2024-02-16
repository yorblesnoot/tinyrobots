using UnityEngine;

public abstract class LinearAbility : Ability
{
    protected override Vector3[] GetTrajectory(Vector3 source, Vector3 target)
    {
        Vector3 direction = target - source;
        direction.Normalize();
        Vector3 endPoint = source + direction * range;
        Vector3[] targets = new Vector3[] { source, endPoint };
        return targets;
    }

    
}
