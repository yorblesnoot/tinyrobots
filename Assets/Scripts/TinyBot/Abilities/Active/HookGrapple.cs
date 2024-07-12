using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HookGrapple : ProjectileAbility
{
    [SerializeField] float backDistance = 2;
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
        line.positionCount = 2;
        Transform parent = hook.transform.parent;
        hook.transform.SetParent(null, true);
        
        float intervalTime = travelTime / currentTrajectory.Count;
        yield return StartCoroutine(LaunchWithLine(hook, currentTrajectory, intervalTime));
        bool hitSomething = Vector3.Distance(currentTrajectory[0], currentTrajectory[^1]) < range;
        if (hitSomething)
        {
            if (currentTargets.Count > 0) currentTargets[0].ReceiveHit(damage, Owner.transform.position, currentTrajectory[^1]);
            Vector3 backDirection = currentTrajectory[1] - currentTrajectory[0];
            backDirection.Normalize();
            backDirection *= backDistance;
            Vector3 secondTarget = currentTrajectory[1] - backDirection;
            List<Vector3> secondaryTrajectory = new() { currentTrajectory[0], secondTarget };
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
        EndAbility();
        if (hitSomething)
        {
            yield return StartCoroutine(Owner.Fall());
            Pathfinder3D.GeneratePathingTree(Owner.MoveStyle, Owner.transform.position);
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
}
