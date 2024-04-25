using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LegMovement : PrimaryMovement
{
    
    [SerializeField] protected float legStepDuration = .1f;
    [SerializeField] protected float lookHeightModifier = 1f;

    [SerializeField] protected Anchor[] anchors;
    [SerializeField] protected float anchorZoneRadius = 1f;
    [SerializeField] protected Transform legModel;
    [SerializeField] protected float anchorUpwardLimit = 2f;
    [SerializeField] protected float anchorDownwardLength = 3f;
    [SerializeField] AnimationCurve legRaise;
    [SerializeField] SphereCollider detector;
    [SerializeField] protected float forwardBias = .5f;

    protected bool stepping;

    private void Awake()
    {
        InitializeParameters();
        foreach (var anchor in anchors)
        {
            anchor.Initialize();
        }
    }

    public override void RotateToTrackEntity(GameObject trackingTarget)
    {
        base.RotateToTrackEntity(trackingTarget);
        AnimateToOrientation();
    }

    protected override void HandleImpulse()
    {
        foreach(var anchor in anchors)
        {
            GluePosition(anchor);
        }
    }

    protected abstract void InitializeParameters();
    protected override void AnimateToOrientation()
    {
        Anchor farthestFromBase = null;
        foreach (var anchor in anchors)
        {
            farthestFromBase ??= anchor;
            GluePosition(anchor);
            if (anchor.distanceFromDeadZone > farthestFromBase.distanceFromDeadZone) farthestFromBase = anchor;
        }

        if (farthestFromBase.distanceFromDeadZone > anchorZoneRadius && !stepping)
        {
            StartCoroutine(StepToBase(farthestFromBase));
        }
    }

    protected void GluePosition(Anchor anchor)
    {
        if (anchor.stepping) return;
        anchor.ikTarget.position = anchor.gluedWorldPosition;
        anchor.distanceFromDeadZone = LegDistanceFromDeadZone(anchor);
    }

    protected virtual float LegDistanceFromDeadZone(Anchor anchor)
    {
        Vector3 localForward = anchor.ikTarget.parent.InverseTransformPoint(Owner.transform.forward);
        localForward.Normalize();
        return Vector3.Distance(anchor.ikTarget.localPosition, anchor.localBasePosition + localForward * forwardBias);
    }
    
    protected IEnumerator StepToBase(Anchor anchor, bool goToNeutral = false)
    {
        stepping = true;
        anchor.stepping = true;

        Vector3 localStartPosition = anchor.ikTarget.localPosition;
        Vector3 finalPosition = GetLimbTarget(anchor, goToNeutral, localStartPosition);

        float timeElapsed = 0;
        while (timeElapsed < legStepDuration)
        {
            float interpolator = timeElapsed / legStepDuration;
            Vector3 targetPosition = Vector3.Lerp(localStartPosition, finalPosition, interpolator);
            //targetPosition.y += legRaise.Evaluate(interpolator);
            anchor.ikTarget.localPosition = targetPosition;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        anchor.UpdateGluedPosition();
        stepping = false;
        anchor.stepping = false;
    }

    protected abstract Vector3 GetLimbTarget(Anchor anchor, bool goToNeutral, Vector3 localStartPosition);

    public override IEnumerator NeutralStance()
    {
        foreach (var anchor in anchors)
        {
            yield return StartCoroutine(StepToBase(anchor, true));
        }
    }

    protected Vector3 GetMeshNormalAt(Vector3 target)
    {
        Collider[] colliders = Physics.OverlapSphere(target, 1f, LayerMask.GetMask("Terrain"));
        detector.transform.SetParent(null);
        detector.transform.position = target;
        CheckSphereExtra(colliders[0], detector, out Vector3 closestPoint, out Vector3 surfaceNormal);
        return surfaceNormal;
    }
    protected static bool CheckSphereExtra(Collider target_collider, SphereCollider sphere_collider, out Vector3 closestPoint, out Vector3 surfaceNormal)
    {
        closestPoint = Vector3.zero;
        Vector3 sphere_pos = sphere_collider.transform.position;
        if (Physics.ComputePenetration(target_collider, target_collider.transform.position, target_collider.transform.rotation, sphere_collider, sphere_pos, Quaternion.identity, out surfaceNormal, out float surfacePenetrationDepth))
        {
            closestPoint = sphere_pos + (surfaceNormal * (sphere_collider.radius - surfacePenetrationDepth));

            surfaceNormal = -surfaceNormal;

            return true;
        }
        return false;
    }
    [Serializable]
    protected class Anchor
    {
        [HideInInspector] public bool stepping;
        public Transform ikTarget;
        [HideInInspector] public Vector3 localBasePosition;
        [HideInInspector] public float distanceFromDeadZone;
        [HideInInspector] public Vector3 gluedWorldPosition;
        public void Initialize()
        {
            localBasePosition = ikTarget.localPosition;
        }

        public void UpdateGluedPosition()
        {
            gluedWorldPosition = ikTarget.position;
        }
    }
}
