using System.Collections.Generic;
using UnityEngine;

public class ParabolicTrajectory : Trajectory
{
    [SerializeField] protected int parabolaPoints = 10;
    [SerializeField] float gravityAccel = -.3f;
    [SerializeField] bool gravityRange = true;

    protected override Vector3[] CalculateTrajectory(Vector3 source, Vector3 target)
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

    public override Vector3 RestrictRange(Vector3 point, Vector3 source, float range)
    {
        if(!gravityRange) return base.RestrictRange(point, source, range);
        point.y = Mathf.Min(point.y, source.y + range);
        Vector3 offset = point - source;
        float parabolaHeight = offset.y - range;
        offset.y = 0;
        float distance = offset.magnitude;
        offset.Normalize();
        float width = gravityAccel;
        float x = Mathf.Sqrt(parabolaHeight/width);
        distance = Mathf.Min(distance, x);
        Vector3 finalPoint = source + offset * distance;
        finalPoint.y = point.y;
        return finalPoint;
    }
}
