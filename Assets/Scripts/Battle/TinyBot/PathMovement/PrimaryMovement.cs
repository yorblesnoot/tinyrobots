using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PrimaryMovement : MonoBehaviour
{
    [field: SerializeField] public MoveStyle Style { get; protected set; }
    [HideInInspector] public TinyBot Owner;
    [HideInInspector] public float SpeedMultiplier = 1;


    public abstract float LocomotionHeight { get; }
    public Transform sourceBone;

    [SerializeField] float lookSpeed = 1f;
    [SerializeField] protected float MoveSpeed = 3;
    [SerializeField] protected float PivotSpeed = 180f;
    

    int sanitizeMask;
    readonly float overlap = .01f;
    public readonly float PathHeight = .2f;
    public float FinalSpeed { get { return MoveSpeed * SpeedMultiplier; } }

    private void Start()
    {
        sanitizeMask = LayerMask.GetMask("Terrain", "Default");
    }

    public IEnumerator TraversePath(List<Vector3> path)
    {
        MainCameraControl.TrackTarget(Owner.transform);
        float timeElapsed = 0;
        while (timeElapsed > .3f)
        {
            timeElapsed += Time.deltaTime;
            PivotToFacePosition(path[0]);
            yield return null;
        }

        foreach (var target in path)
        {
            yield return StartCoroutine(InterpolatePositionAndRotation(Owner.transform, target));
        }
        StartCoroutine(NeutralStance());
        MainCameraControl.ReleaseTracking();
    }

    public virtual List<Vector3> SanitizePath(List<Vector3> path)
    {
        return path;
    }

    protected List<Vector3> ShortcutPath(List<Vector3> path)
    {
        int pathIndex = 0;
        List<Vector3> cleanPath = new() { path[0] };
        while (pathIndex < path.Count)
        {
            cleanPath.Add(path[pathIndex]);
            pathIndex = GetNextPoint(path, pathIndex);
        }

        return cleanPath;
    }

    int GetNextPoint(List<Vector3> path, int startIndex)
    {
        int leapSize = 1;
        while (LeapedPathOpen(path, startIndex, leapSize + startIndex + 1))
        {
            leapSize++;
        }
        //if(leapSize > 1) Debug.Log(leapSize);
        return startIndex + leapSize;
    }

    bool LeapedPathOpen(List<Vector3> path, int pathIndex, int leapIndex)
    {
        if (leapIndex >= path.Count) return false;
        Vector3 originPoint = path[pathIndex];
        Vector3 testPoint = path[leapIndex];
        Vector3 direction = testPoint - originPoint;
        float distance = Vector3.Distance(originPoint, testPoint) + overlap;

        return LeapedPathIsValid(path[pathIndex], direction, distance, sanitizeMask);
    }

    protected virtual bool LeapedPathIsValid(Vector3 testSource, Vector3 direction, float distance, int sanitizeMask)
    {
        return !Physics.Raycast(testSource, direction, distance, sanitizeMask);
    }

    public Vector3 SanitizePoint(Vector3 point)
    {
        return SanitizePath(new() { point })[0];
    }

    protected virtual IEnumerator InterpolatePositionAndRotation(Transform unit, Vector3 target)
    {
        yield return PivotToOrientationAt(target);
        
        Vector3 startPosition = unit.position;
        float timeElapsed = 0;
        float pathStepDuration = Vector3.Distance(unit.transform.position, target) / FinalSpeed;
        while (timeElapsed < pathStepDuration)
        {
            unit.position = Vector3.Lerp(startPosition, target, timeElapsed / pathStepDuration);
            IncorporateBodyMotion(unit);
            timeElapsed += Time.deltaTime;

            AnimateToOrientation(Owner.transform.forward);
            yield return null;
        }
        BattleEnder.IsMissionOver();
    }

    IEnumerator PivotToOrientationAt(Vector3 target)
    {
        Transform unit = Owner.transform;
        Quaternion targetRotation = GetRotationFromFacing(unit.position, target - unit.position);
        Quaternion startRotation = unit.rotation;
        float timeElapsed = 0;
        float pivotDuration = Quaternion.Angle(startRotation, targetRotation) / (PivotSpeed * SpeedMultiplier);
        while (timeElapsed < pivotDuration)
        {
            timeElapsed += Time.deltaTime;
            unit.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / pivotDuration);
            AnimateToOrientation(Vector3.zero);
            yield return null;
        }
        unit.rotation = targetRotation;
    }

    protected virtual void IncorporateBodyMotion(Transform unit)
    {

    }

    public virtual void AnimateToOrientation(Vector3 direction) { }
    public virtual Quaternion GetRotationFromFacing(Vector3 position, Vector3 facing)
    {
        Vector3 targetNormal = GetUpVector(position);
        Vector3 lookTarget = Vector3.ProjectOnPlane(facing, targetNormal);
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget, targetNormal);
        Debug.DrawLine(Owner.transform.position, lookTarget, Color.blue, 2f);
        return targetRotation;
    }

    protected virtual Vector3 GetUpVector(Vector3 position)
    {
        return Vector3.up;
    }

    protected abstract void InstantNeutral();
    
    public abstract IEnumerator NeutralStance();

    public void PivotToFacePosition(Vector3 worldTarget, bool instant = false)
    {
        Quaternion toRotation = GetRotationFromFacing(Owner.transform.position, worldTarget - Owner.transform.position);
        Owner.transform.rotation = instant ? toRotation : Quaternion.Slerp(Owner.transform.rotation, toRotation, lookSpeed * Time.deltaTime);
        if (instant) InstantNeutral(); 
        else AnimateToOrientation(Vector3.zero);
    }

    public IEnumerator ApplyImpulseToBody(Vector3 direction, float distance, float snapTime, float returnTime)
    {
        direction.Normalize();

        Transform target = Owner.transform;
        Vector3 neutralPosition = target.position;
        Vector3 snapPosition = neutralPosition + direction * distance;

        float timeElapsed = 0;
        while (timeElapsed < snapTime)
        {
            target.position = Vector3.Lerp(neutralPosition, snapPosition, timeElapsed / snapTime);
            timeElapsed += Time.deltaTime;
            HandleImpulse();
            yield return null;
        }
        timeElapsed = 0;
        while (timeElapsed < returnTime)
        {
            target.position = Vector3.Lerp(snapPosition, neutralPosition, timeElapsed / returnTime);
            timeElapsed += Time.deltaTime;
            HandleImpulse();
            yield return null;
        }
        
    }

    public virtual void LandingStance() { }

    protected virtual void HandleImpulse()
    {
    }
}
