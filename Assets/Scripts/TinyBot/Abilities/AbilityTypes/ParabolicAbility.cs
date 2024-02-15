using System.Collections.Generic;
using UnityEngine;

public abstract class ParabolicAbility : Ability
{
    [SerializeField] protected int parabolaPoints = 10;
    [SerializeField] float gravityAccel = -.98f;
    protected int terrainMask;
    private void Awake()
    {
        terrainMask = LayerMask.GetMask("Terrain");
    }
    protected override void AimAt(GameObject target)
    {
        Vector3[] points = GenerateParabola(emissionPoint.transform.position, target.transform.position, parabolaPoints);
        points = CastAlongParabola(points).ToArray();
        LineMaker.DrawLine(points);
    }

    readonly float overlapLength = .1f;
    protected virtual List<Vector3> CastAlongParabola(Vector3[] castTargets)
    {
        List<Vector3> modifiedTargets = new();
        for (int i = 0; i < castTargets.Length - 1; i++)
        {
            modifiedTargets.Add(castTargets[i]);
            Vector3 direction = castTargets[i + 1] - castTargets[i];
            Ray ray = new(castTargets[i], direction);
            if (Physics.Raycast(ray, out var hitInfo, direction.magnitude + overlapLength, terrainMask))
            {
                modifiedTargets.Add(hitInfo.point);
                break;
            }
        }
        return modifiedTargets;
    }

    protected Vector3[] GenerateParabola(Vector3 source, Vector3 target, int numberOfPoints)
    {
        Vector3 localCoords = target - source;
        Vector2 flatLocal = new(localCoords.x, localCoords.z);
        float horizontalDistance = flatLocal.magnitude;

        
        float b = (localCoords.y - .5f * gravityAccel * numberOfPoints * numberOfPoints) / numberOfPoints;


        Vector3 xzDirection = new(localCoords.x, 0, localCoords.z);
        xzDirection.Normalize();
        float horizontalVelocity = horizontalDistance / numberOfPoints;
        List<Vector3> points = new();
        for(int x = 0; x <= numberOfPoints;x++)
        {
            Vector3 finalPosition = horizontalVelocity * x * xzDirection;
            finalPosition.y = (gravityAccel * x * x/ 2) + (b * x);
            finalPosition += source;
            points.Add(finalPosition);
        }
        return points.ToArray();
    }
}
