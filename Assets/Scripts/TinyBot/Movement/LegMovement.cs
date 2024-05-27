using PrimeTween;
using System;
using System.Collections;
using System.Linq;
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
        foreach (var anchor in anchors)
        {
            GluePosition(anchor);
        }
        if (stepping) return;

        anchors = anchors.OrderByDescending(anchor => anchor.distanceFromDeadZone).ToArray();

        for(int i = 0; i < anchors.Length; i++)
        {
            if (anchors[i].distanceFromDeadZone > anchorZoneRadius)
            {
                TryStepToBase(anchors[i]);
            }
            if(stepping) return;
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
    
    protected void TryStepToBase(Anchor anchor, bool goToNeutral = false)
    {
        Vector3 localStartPosition = anchor.ikTarget.localPosition;
        Vector3 finalPosition = GetLimbTarget(anchor, goToNeutral, localStartPosition);
        if (finalPosition == default) return;
        stepping = true;
        anchor.stepping = true;
        Tween.LocalPosition(anchor.ikTarget, finalPosition, legStepDuration).OnComplete(() => CompleteStep(anchor));
    }

    private void CompleteStep(Anchor anchor)
    {
        anchor.UpdateGluedPosition();
        stepping = false;
        anchor.stepping = false;
    }

    protected abstract Vector3 GetLimbTarget(Anchor anchor, bool goToNeutral, Vector3 localStartPosition);

    public override IEnumerator NeutralStance()
    {
        foreach (var anchor in anchors)
        {
            TryStepToBase(anchor, true);
            yield return new WaitForSeconds(legStepDuration);
        }
    }

    protected Vector3 GetMeshNormalAt(Vector3 target)
    {
        return Pathfinder3D.GetCrawlOrientation(Vector3Int.RoundToInt(target));
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


