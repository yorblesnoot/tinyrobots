using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PrimaryMovement : MonoBehaviour
{
    public MoveStyle Style { get; protected set; }
    [HideInInspector] public TinyBot Owner;

    
    public float locomotionHeight;
    public Transform sourceBone;

    [SerializeField] float lookSpeed = 1f;
    [SerializeField] protected float MoveSpeed = 3;
    [SerializeField] protected float PivotSpeed = 180f;

    [HideInInspector] public float SpeedMultiplier = 1;

    public IEnumerator TraversePath(List<Vector3> path)
    {
        MainCameraControl.TrackTarget(Owner.transform);
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

    public Vector3 SanitizePoint(Vector3 point)
    {
        return SanitizePath(new() { point })[0];
    }

    protected virtual IEnumerator InterpolatePositionAndRotation(Transform unit, Vector3 target)
    {
        Quaternion startRotation = unit.rotation;
        Quaternion targetRotation = GetRotationAtPosition(target);

        Vector3 startPosition = unit.position;
        float timeElapsed = 0;
        float pivotDuration = Quaternion.Angle(startRotation, targetRotation) / (PivotSpeed * SpeedMultiplier);
        while (timeElapsed < pivotDuration)
        {
            unit.rotation = Quaternion.Slerp(startRotation, targetRotation, timeElapsed / pivotDuration);
            timeElapsed += Time.deltaTime;
            AnimateToOrientation();
            yield return null;
        }
        unit.rotation = targetRotation;

        timeElapsed = 0;
        float pathStepDuration = Vector3.Distance(unit.transform.position, target) / (MoveSpeed * SpeedMultiplier);
        while (timeElapsed < pathStepDuration)
        {
            unit.position = Vector3.Lerp(startPosition, target, timeElapsed / pathStepDuration);
            timeElapsed += Time.deltaTime;

            AnimateToOrientation();
            yield return null;
        }
        BattleEnder.IsMissionOver();
    }
    protected virtual void AnimateToOrientation(bool inPlace = false) { }
    public virtual Quaternion GetRotationAtPosition(Vector3 moveTarget)
    {
        moveTarget.y = transform.position.y;
        Quaternion targetRotation = Quaternion.LookRotation(moveTarget - transform.position);
        return targetRotation;
    }
    public virtual void SpawnOrientation()
    {
        Owner.transform.LookAt(GetCenterColumn());
        StartCoroutine(NeutralStance());
    }
    protected Vector3 GetCenterColumn()
    {
        return new(Pathfinder3D.XSize / 2, transform.position.y, Pathfinder3D.ZSize / 2);
    }
    public abstract IEnumerator NeutralStance();

    public virtual void RotateToTrackEntity(GameObject trackingTarget)
    {
        Vector3 worldTarget = trackingTarget.transform.position;
        Vector3 localTarget = Owner.transform.InverseTransformPoint(worldTarget);
        localTarget.y = 0;
        worldTarget = Owner.transform.TransformPoint(localTarget);

        Vector3 direction = worldTarget - Owner.transform.position;

        Quaternion toRotation = Quaternion.LookRotation(direction, Owner.transform.up);
        Owner.transform.rotation = Quaternion.Slerp(Owner.transform.rotation, toRotation, lookSpeed * Time.deltaTime);
        AnimateToOrientation(true);
    }

    public IEnumerator ApplyImpulseToBody(Vector3 direction, float distance, float snapTime, float returnTime)
    {
        direction.Normalize();
        
        Vector3 neutralPosition = Owner.transform.position;
        Vector3 snapPosition = neutralPosition + direction * distance;

        float timeElapsed = 0;
        while (timeElapsed < snapTime)
        {
            Owner.transform.position = Vector3.Lerp(neutralPosition, snapPosition, timeElapsed / snapTime);
            timeElapsed += Time.deltaTime;
            HandleImpulse();
            yield return null;
        }
        timeElapsed = 0;
        while (timeElapsed < returnTime)
        {
            Owner.transform.position = Vector3.Lerp(snapPosition, neutralPosition, timeElapsed / returnTime);
            timeElapsed += Time.deltaTime;
            HandleImpulse();
            yield return null;
        }
        
    }

    protected virtual void HandleImpulse()
    {
    }
}
