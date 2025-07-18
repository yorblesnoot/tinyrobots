using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrimaryMovement : MonoBehaviour
{
    [field: SerializeField] public MoveStyle Style { get; protected set; }
    [HideInInspector] public TinyBot Owner;
    [HideInInspector] public float SpeedMultiplier = 1;


    public virtual float LocomotionHeight { get { return 0; } }
    public Transform sourceBone;

    [SerializeField] float lookSpeed = 1f;
    [SerializeField] protected float MoveSpeed = 3;
    [SerializeField] protected float PivotSpeed = 180f;
    

    int sanitizeMask;
    Animator animator;
    readonly float overlap = .01f;
    public readonly float PathHeight = .2f;
    public float FinalSpeed { get { return MoveSpeed * SpeedMultiplier; } }

    private void Awake()
    {
        AwakeInitialize();
    }

    protected virtual void AwakeInitialize()
    {
        sanitizeMask = LayerMask.GetMask("Terrain", "Default");
        animator = GetComponentInChildren<Animator>();
    }

    public void ToggleAnimations(bool on)
    {
        if(animator == null) return;
        animator.speed = on ? 1f : 0f;
    }

    public virtual IEnumerator TraversePath(List<Vector3> path)
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
            AnimateToOrientation();
            yield return null;
        }
        unit.rotation = targetRotation;
    }

    protected virtual void IncorporateBodyMotion(Transform unit)
    {

    }

    public virtual void AnimateToOrientation(Vector3 direction = default) { }
    public Quaternion GetRotationFromFacing(Vector3 position, Vector3 facing)
    {
        if(Style == MoveStyle.FLY && facing.normalized == Vector3.up) return Owner.transform.rotation;
        Vector3 targetNormal = GetUpVector(position);
        Vector3 lookTarget = Vector3.ProjectOnPlane(facing, targetNormal);
        if(lookTarget == Vector3.zero) return Owner.transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget, targetNormal);
        //Debug.DrawLine(Owner.transform.position, lookTarget, Color.blue, 2f);
        return targetRotation;
    }

    protected Vector3 GetUpVector(Vector3 position)
    {
        if(Style != MoveStyle.CRAWL) return Vector3.up;
        return Pathfinder3D.GetCrawlOrientation(position);
    }

    protected virtual void InstantNeutral() { }
    
    public virtual IEnumerator NeutralStance()
    {
        yield return null;
    }

    public void PivotToFacePosition(Vector3 worldTarget, bool instant = false)
    {
        Quaternion toRotation = GetRotationFromFacing(Owner.transform.position, worldTarget - Owner.transform.position);
        Owner.transform.rotation = instant ? toRotation : Quaternion.Slerp(Owner.transform.rotation, toRotation, lookSpeed * Time.deltaTime);
        if (instant) InstantNeutral(); 
        else AnimateToOrientation();
    }

    public IEnumerator ApplyImpulseToBody(Vector3 direction, float distance, float snapTime, float returnTime)
    {
        direction.Normalize();

        Transform target = Owner.transform;
        Vector3 neutralPosition = target.position;
        Vector3 snapPosition = neutralPosition + direction * distance;

        yield return ProcessImpulseDirection(neutralPosition, snapPosition, snapTime);
        yield return ProcessImpulseDirection(snapPosition, neutralPosition, returnTime);        
    }

    IEnumerator ProcessImpulseDirection(Vector3 neutralPosition, Vector3 snapPosition, float snapTime)
    {
        float timeElapsed = 0;
        while (timeElapsed < snapTime)
        {
            Owner.transform.position = Vector3.Lerp(neutralPosition, snapPosition, timeElapsed / snapTime);
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
