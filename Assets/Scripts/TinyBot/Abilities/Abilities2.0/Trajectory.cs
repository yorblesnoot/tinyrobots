using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trajectory : MonoBehaviour
{
    public string[] blockingLayers;
    protected int blockingLayerMask;

    readonly float overlapLength = .1f;

    private void Awake()
    {
        blockingLayerMask = LayerMask.GetMask(blockingLayers);
    }
    public virtual List<Vector3> GetTrajectory(GameObject target, Vector3 sourcePosition, float range, bool aiMode = false)
    {
        Vector3 targetPosition = target.transform.position;
        float distance = Vector3.Distance(sourcePosition, targetPosition);
        Vector3 direction = (targetPosition - sourcePosition).normalized;
        Vector3 modifiedTarget = sourcePosition + direction * Mathf.Min(distance, range);
        Vector3[] targets = CalculateTrajectory(sourcePosition, modifiedTarget);
        return CastAlongPoints(targets, blockingLayerMask, out RaycastHit hit, aiMode ? BotAI.terrainCheckSize : 0);
    }

    public void Draw(List<Vector3> points)
    {
        LineMaker.DrawLine(points.ToArray());
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
            bool castHit = Physics.Raycast(castTargets[i], direction, out RaycastHit hitInfo, direction.magnitude + overlapLength, mask);
            if (castHit && radius > 0) castHit = Physics.SphereCast(castTargets[i], radius, direction, out hitInfo, direction.magnitude + overlapLength, mask);
            if (castHit)
            {
                modifiedTargets.Add(hitInfo.point);
                hit = hitInfo;
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
