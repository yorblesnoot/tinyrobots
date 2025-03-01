using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public abstract class Trajectory : MonoBehaviour
{
    public string[] blockingLayers = { "Default", "Terrain", "Shield" };
    [SerializeField] float castWidth = 0;
    [HideInInspector] public int BlockingLayerMask;

    readonly float overlapLength = .1f;

    private void Awake()
    {
        BlockingLayerMask = LayerMask.GetMask(blockingLayers);
    }
    public virtual List<Vector3> GetTrajectory(Vector3 sourcePosition, Vector3 target, out Collider collider)
    {
        Vector3[] targets = CalculateTrajectory(sourcePosition, target);
        Vector3 finalDirection = (targets[^1] - targets[^2]).normalized;
        targets[^1] += finalDirection * overlapLength;
        List < Vector3 > trajectory = CastAlongPoints(targets, BlockingLayerMask, out RaycastHit hit, castWidth);
        collider = hit.collider;
        return trajectory;
    }

    public virtual Vector3 RestrictRange(Vector3 point, Vector3 source, float range)
    {
        float distance = Vector3.Distance(source, point);
        Vector3 direction = (point - source).normalized;
        return source + direction * Mathf.Min(distance, range);
    }

    protected abstract Vector3[] CalculateTrajectory(Vector3 source, Vector3 target);

    protected List<Vector3> CastAlongPoints(Vector3[] castTargets, int mask, out RaycastHit hit, float radius = 0)
    {
        hit = default;
        if (castTargets.Length == 1) return new() { castTargets[0] };
        List<Vector3> modifiedTargets = new()
        {
            castTargets[0]
        };
        for (int i = 0; i < castTargets.Length - 1; i++)
        {
            Vector3 direction = castTargets[i + 1] - castTargets[i];
            bool castHit = radius > 0 ? Physics.SphereCast(castTargets[i], radius, direction, out RaycastHit hitInfo, direction.magnitude + overlapLength, mask) 
                :  Physics.Raycast(castTargets[i], direction, out hitInfo, direction.magnitude + overlapLength, mask);
            if (castHit)
            {
                //add penetration here
                
                hit = hitInfo;
                Vector3 finalPoint = hit.point + radius * hit.normal;
                modifiedTargets.Add(finalPoint);
                break;
            }
            else
            {
                modifiedTargets.Add(castTargets[i + 1]);
            }
        }
        return modifiedTargets;
    }
}
