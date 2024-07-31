using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PrimaryMovement : MonoBehaviour
{
    public MoveStyle Style { get; protected set; }
    [HideInInspector] public TinyBot Owner;

    [SerializeField] float lookSpeed = 1f;
    public float chassisHeight;
    public Transform sourceBone;

    [SerializeField] protected float moveSpeed;

    [HideInInspector] public float speedMultiplier = 1;

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

    protected IEnumerator InterpolatePositionAndRotation(Transform unit, Vector3 target)
    {
        Quaternion startRotation = unit.rotation;
        Quaternion targetRotation = GetRotationAtPosition(target);

        Vector3 startPosition = unit.position;
        float timeElapsed = 0;

        float pathStepDuration = Vector3.Distance(unit.transform.position, target) / (moveSpeed * speedMultiplier);
        while (timeElapsed < pathStepDuration)
        {
            unit.SetPositionAndRotation(Vector3.Lerp(startPosition, target, timeElapsed / pathStepDuration),
                Quaternion.Slerp(startRotation, targetRotation, timeElapsed / pathStepDuration));
            timeElapsed += Time.deltaTime;

            AnimateToOrientation();
            yield return null;
        }
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
        return new(Pathfinder3D.xSize / 2, transform.position.y, Pathfinder3D.zSize / 2);
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
    }

    public IEnumerator ApplyImpulseToBody(Vector3 source, float distance, float snapTime, float returnTime)
    {
        Vector3 direction = Owner.transform.position - source;
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
