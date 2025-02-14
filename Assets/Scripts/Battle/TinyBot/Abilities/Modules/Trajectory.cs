using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Trajectory : MonoBehaviour
{
    public string[] blockingLayers = { "Default", "Terrain", "Shield" };
    protected int BlockingLayerMask;

    readonly float overlapLength = .1f;

    private void Awake()
    {
        BlockingLayerMask = LayerMask.GetMask(blockingLayers);
    }
    public virtual List<Vector3> GetTrajectory(Vector3 sourcePosition, Vector3 target, out RaycastHit hit, bool wide = false)
    {
        Vector3[] targets = CalculateTrajectory(sourcePosition, target);
        List < Vector3 > trajectory = CastAlongPoints(targets, BlockingLayerMask, out hit, wide ? BotAI.terrainCheckSize : 0);
        return trajectory;
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
                //add penetration here
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
