using System.Collections.Generic;
using UnityEngine;

public abstract class ParabolicAbility : ProjectileAbility
{
    [SerializeField] protected int parabolaPoints = 10;
    [SerializeField] float gravityAccel = -.98f;

    protected override Vector3[] GetTrajectory(Vector3 source, Vector3 target)
    {
        Vector3 localCoords = target - source;
        Vector2 flatLocal = new(localCoords.x, localCoords.z);
        float horizontalDistance = flatLocal.magnitude;
        float b = (localCoords.y - .5f * gravityAccel * parabolaPoints * parabolaPoints) / parabolaPoints;

        Vector3 xzDirection = new(localCoords.x, 0, localCoords.z);
        xzDirection.Normalize();
        float horizontalVelocity = horizontalDistance / parabolaPoints;
        List<Vector3> points = new();
        for(int x = 0; x <= parabolaPoints;x++)
        {
            Vector3 finalPosition = horizontalVelocity * x * xzDirection;
            finalPosition.y = (gravityAccel * x * x/ 2) + (b * x);
            finalPosition += source;
            points.Add(finalPosition);
        }
        return points.ToArray();
    }
}
