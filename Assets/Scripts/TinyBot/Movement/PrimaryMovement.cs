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
}
