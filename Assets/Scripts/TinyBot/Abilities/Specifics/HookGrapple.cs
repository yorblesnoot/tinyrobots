using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HookGrapple : LinearAbility
{
    [SerializeField] float backDistance = 1;
    [SerializeField] float travelTime;
    [SerializeField] TurretTracker turretTracker;
    [SerializeField] LineRenderer line;
    [SerializeField] GameObject hook;

    Vector3 baseHookPosition;
    private void Start()
    {
        baseHookPosition = hook.transform.localPosition;
        line.useWorldSpace = true;
    }
    protected override IEnumerator PerformEffects()
    {
        
        List<Vector3> trajectory = CastAlongPoints(targetTrajectory.ToArray(), blockingLayerMask, out var hit);
        
        line.positionCount = 2;
        Transform parent = hook.transform.parent;
        hook.transform.SetParent(null, true);
        
        float intervalTime = travelTime / trajectory.Count;
        yield return StartCoroutine(LaunchWithLine(hook, trajectory, intervalTime));
        if (hit.collider != null)
        {
            if (hit.collider.TryGetComponent(out TinyBot bot)) bot.ReceiveDamage(damage, Owner.transform.position, hit.point);
            Vector3 backDirection = trajectory[1] - trajectory[0];
            backDirection.Normalize();
            backDirection *= backDistance;
            Vector3 secondTarget = trajectory[1] - backDirection;
            List<Vector3> secondaryTrajectory = new() { trajectory[0], secondTarget };
            yield return StartCoroutine(LaunchWithLine(Owner.gameObject, secondaryTrajectory, intervalTime));
        }
        else
        {
            List<Vector3> reverseTrajectory = new() { hook.transform.position, parent.TransformPoint(baseHookPosition) };
            yield return StartCoroutine(LaunchWithLine(hook, reverseTrajectory, intervalTime));
        }
        line.positionCount = 0;
        hook.transform.SetParent(parent);
        hook.transform.localPosition = baseHookPosition;
        NeutralAim();
        if (hit.collider != null)
        {
            yield return StartCoroutine(Owner.Fall());
            Pathfinder3D.GeneratePathingTree(Owner);
        }
    }

    private IEnumerator LaunchWithLine(GameObject launched, List<Vector3> trajectory, float intervalTime)
    {
        float timeElapsed;
        launched.transform.rotation = Quaternion.LookRotation(trajectory[1] - trajectory[0]);
        for (int i = 0; i < trajectory.Count - 1; i++)
        {
            timeElapsed = 0;
            while (timeElapsed < intervalTime)
            {
                timeElapsed += Time.deltaTime;
                float interpolator = timeElapsed / intervalTime;
                launched.transform.position = Vector3.Lerp(trajectory[i], trajectory[i + 1], interpolator);
                Vector3[] linePoints = new Vector3[2] { emissionPoint.transform.position, hook.transform.position };
                line.SetPositions(linePoints);
                yield return null;
            }
        }
    }

    public override List<TinyBot> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        List<TinyBot> hits = base.AimAt(target, sourcePosition, aiMode);
        if (!aiMode) turretTracker.TrackTrajectory(targetTrajectory);
        return hits;
    }

    public override void NeutralAim()
    {
        turretTracker.ResetTracking();
    }
}
