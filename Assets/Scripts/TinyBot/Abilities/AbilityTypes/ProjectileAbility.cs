using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;

public abstract class ProjectileAbility : Ability
{
    protected List<Vector3> targetTrajectory;
    public override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        List<TinyBot> hitBots = new();
        Vector3 targetPosition = target.transform.position;
        float distance = Vector3.Distance(sourcePosition, targetPosition);
        Vector3 direction = (targetPosition - sourcePosition).normalized;
        Vector3 modifiedTarget = sourcePosition + direction * Mathf.Min(distance, range);
        Vector3[] targets = GetTrajectory(sourcePosition, modifiedTarget);
        targetTrajectory = CastAlongPoints(targets, blockingLayerMask, out RaycastHit hit, aiMode ? BotAI.terrainCheckSize : 0);
        if (hit.collider != null && hit.collider.TryGetComponent(out TinyBot bot)) hitBots.Add(bot);
        if(playerTargeting) LineMaker.DrawLine(targetTrajectory.ToArray());
        return hitBots;
    }

    protected IEnumerator LaunchAlongLine(GameObject launched, List<Vector3> trajectory, float travelTime, RaycastHit hit)
    {
        GameObject spawned = Instantiate(launched);
        float intervalTime = travelTime / trajectory.Count;
        float timeElapsed;
        spawned.transform.rotation = Quaternion.LookRotation(trajectory[1] - trajectory[0]);
        for (int i = 0; i < trajectory.Count - 1; i++)
        {
            Quaternion startRotation = spawned.transform.rotation;
            Quaternion targetRotation = Quaternion.LookRotation(trajectory[i + 1] - trajectory[i]);
            timeElapsed = 0;
            while (timeElapsed < intervalTime)
            {
                yield return null;
                float interpolator = timeElapsed / intervalTime;
                spawned.transform.SetPositionAndRotation(Vector3.Lerp(trajectory[i], trajectory[i + 1], interpolator),
                    Quaternion.Slerp(startRotation, targetRotation, interpolator));
                timeElapsed += Time.deltaTime;

            }
        }
        CompleteTrajectory(trajectory.Last(), spawned, hit);
    }
    protected abstract Vector3[] GetTrajectory(Vector3 source, Vector3 target);

    readonly float overlapLength = .1f;
    protected List<Vector3> CastAlongPoints(Vector3[] castTargets, int mask, out RaycastHit hit, float radius = 0)
    {
        hit = default;
        List<Vector3> modifiedTargets = new()
        {
            castTargets[0]
        };
        for (int i = 0; i < castTargets.Length - 1; i++)
        {
            Vector3 direction = castTargets[i + 1] - castTargets[i];
            bool castHit;
            castHit = radius == 0 ? Physics.Raycast(castTargets[i], direction, out RaycastHit hitInfo, direction.magnitude + overlapLength, mask)
                : Physics.SphereCast(castTargets[i], radius, direction, out hitInfo, direction.magnitude + overlapLength, mask);
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

    protected virtual void CompleteTrajectory(Vector3 position, GameObject launched, RaycastHit hit) { }
}
