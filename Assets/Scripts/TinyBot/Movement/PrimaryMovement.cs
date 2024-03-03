using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PrimaryMovement : MonoBehaviour
{
    public MoveStyle Style { get; protected set; }
    public CursorType PreferredCursor { get; protected set; }
    [HideInInspector] public TinyBot Owner;

    [SerializeField] float lookSpeed = 1f;
    public float chassisHeight;
    public Transform sourceBone;

    public abstract IEnumerator PathToPoint(List<Vector3> path);
    public abstract void SpawnOrientation();
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
