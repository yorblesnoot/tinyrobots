using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public abstract class LegMovement : PrimaryMovement
{
    [Header("Settings")]
    [SerializeField] protected float legStepDuration = .1f;

    [SerializeField] protected float anchorZoneRadius = 1f;

    [SerializeField] protected float anchorUpwardLimit = 2f;
    [SerializeField] protected float anchorDownwardLength = 3f;
    [SerializeField] AnimationCurve legRaise;
    [SerializeField] AnimationCurve bodyArc;
    [SerializeField] protected float forwardBias = .5f;
    [SerializeField] float footHeight = .1f;

    [Header("Components")]
    [SerializeField] protected Anchor[] anchors;
    [SerializeField] protected Transform legModel;

    

    protected float StepProgress;
    Dictionary<Transform, (Vector3, Quaternion)> baseLimbPositions;
    protected int TerrainMask;
    [SerializeField][Range(0f, 1f)] float firstCastTilt;
    [SerializeField] float secondCastHeight = -1;
    [SerializeField] float secondCastLength;
    [SerializeField][Range(0f, 1f)] float secondCastTilt;

    public override float LocomotionHeight => locomotionHeight;
    float locomotionHeight;


    private void Awake()
    {
        InitializeParameters();
        TerrainMask = LayerMask.GetMask("Terrain");
        foreach (var anchor in anchors)
        {
            anchor.Initialize();
        }
        locomotionHeight = transform.position.y - anchors[0].ikTarget.position.y;
    }

    protected override void HandleImpulse()
    {
        foreach(var anchor in anchors)
        {
            GluePosition(anchor);
        }
    }

    protected override void InstantNeutral()
    {
        LoadLimbPositions();
        foreach (var anchor in anchors)
        {
            if (!GetRaisedLimbTarget(anchor, Vector3.zero, out Vector3 position)) continue;
            anchor.ikTarget.position = position;
            anchor.UpdateGluedPosition();
        }
    }

    void SaveLimbPosition(Transform transform)
    {
        baseLimbPositions.Add(transform, (transform.localPosition, transform.localRotation));
    }

    void LoadLimbPositions()
    {
        foreach (var entry in baseLimbPositions) entry.Key.SetLocalPositionAndRotation(entry.Value.Item1, entry.Value.Item2);
    }

    protected virtual void InitializeParameters()
    {
        baseLimbPositions = new();
        sourceBone.TraverseHierarchy(SaveLimbPosition);
        baseLimbPositions.Remove(sourceBone);
    }
    public override void AnimateToOrientation(Vector3 legDirection)
    {
        foreach (var anchor in anchors)
        {
            GluePosition(anchor);
            anchor.DistanceFromDeadZone = LegDistanceFromDeadZone(anchor, legDirection);
        }
        if (StepProgress > 0) return;

        anchors = anchors.OrderByDescending(anchor => anchor.DistanceFromDeadZone).ToArray();

        for(int i = 0; i < anchors.Length; i++)
        {
            if (anchors[i].DistanceFromDeadZone > anchorZoneRadius)
            {
                TryStepToBase(anchors[i], legDirection);
            }
            if(StepProgress > 0) break;
        }
    }

    protected void GluePosition(Anchor anchor)
    {
        if (anchor.Stepping) return;
        anchor.ikTarget.position = anchor.GluedWorldPosition;
    }

    protected virtual float LegDistanceFromDeadZone(Anchor anchor, Vector3 legDirection)
    {
        Vector3 localForward = anchor.ikTarget.InverseTransformDirection(legDirection).normalized;
        return Vector3.Distance(anchor.ikTarget.localPosition, anchor.LocalBasePosition + localForward * forwardBias);
    }
    
    protected void TryStepToBase(Anchor anchor, Vector3 movementDirection)
    {
        Vector3 localStartPosition = anchor.ikTarget.localPosition;
        if (!GetRaisedLimbTarget(anchor, movementDirection, out Vector3 finalPosition)) return;
        if(finalPosition == anchor.ikTarget.position) return;
        StepProgress = .001f;
        anchor.Stepping = true;
        Tween.Position(anchor.ikTarget, finalPosition, legStepDuration)
            .OnUpdate(this, (target, tween) => StepProgress = tween.progress)
            .OnComplete(() => CompleteStep(anchor));
    }

    private void CompleteStep(Anchor anchor)
    {
        anchor.UpdateGluedPosition();
        StepProgress = 0;
        anchor.Stepping = false;
    }

    bool GetRaisedLimbTarget(Anchor anchor, Vector3 movementDirection, out Vector3 raisedTarget)
    {
        raisedTarget = GetLimbTarget(anchor, movementDirection);
        if(raisedTarget == default) return false;
        raisedTarget.y += footHeight;
        return true;
    }

    protected override bool LeapedPathIsValid(Vector3 testSource, Vector3 direction, float distance, int sanitizeMask)
    {
        if (!base.LeapedPathIsValid(testSource, direction, distance, sanitizeMask)) return false;
        for (int i = 0; i < distance; i++)
        {
            Vector3 testPoint = testSource + direction * i;
            if (!Physics.CheckSphere(testPoint, LocomotionHeight, TerrainMask)) return false;
        }
        return true;
    }

    public override IEnumerator NeutralStance()
    {
        foreach (var anchor in anchors)
        {
            TryStepToBase(anchor, Vector3.zero);
            yield return new WaitForSeconds(legStepDuration);
        }
    }

    public override void LandingStance()
    {
        foreach (var anchor in anchors)
        {
            if (anchor.Stepping) continue;
            TryStepToBase(anchor, Vector3.zero);
        }
    }

    protected override void IncorporateBodyMotion(Transform unit)
    {
        float lift = bodyArc.Evaluate(StepProgress);
        Vector3 newPosition = unit.transform.position;
        Vector3 direction = Owner.transform.up;
        newPosition += direction * lift;
        unit.transform.position = newPosition;
    }

    private void CastFromAnchor(Anchor anchor, Vector3 movementDirection, out Ray firstRay, out Ray secondRay, float duration = 20f)
    {
        Vector3 worldForward = movementDirection * forwardBias;
        Vector3 localForward = anchor.ikTarget.InverseTransformDirection(worldForward);
        Vector3 firstRaySource = anchor.LocalBasePosition + localForward;
        firstRaySource.y += anchorUpwardLimit;
        Vector3 secondRaySource = firstRaySource;
        secondRaySource.y += secondCastHeight;

        firstRaySource = legModel.TransformPoint(firstRaySource) + worldForward;
        secondRaySource = legModel.TransformPoint(secondRaySource) + worldForward;
        Vector3 centerDirection = transform.position - firstRaySource;
        centerDirection.Normalize();
        Vector3 ownerUp = Owner == null ? Vector3.up : Owner.transform.up;
        Vector3 firstRayDirection = Vector3.Slerp(-ownerUp, centerDirection, firstCastTilt);
        Vector3 secondRayDirection = Vector3.Slerp(-ownerUp, centerDirection, secondCastTilt);

        firstRay = new(firstRaySource, firstRayDirection);
        secondRay = new(secondRaySource, secondRayDirection);
        Debug.DrawRay(firstRaySource, firstRayDirection * anchorDownwardLength, Color.green, duration);
        Debug.DrawRay(secondRaySource, secondRayDirection * secondCastLength, Color.red, duration);
    }

    Vector3 GetLimbTarget(Anchor anchor, Vector3 movementDirection)
    {
        Ray firstRay, secondRay;
        CastFromAnchor(anchor, movementDirection, out firstRay, out secondRay);

        if (Physics.Raycast(firstRay, out var hitInfo, anchorDownwardLength, LayerMask.GetMask("Terrain"))
            || Physics.Raycast(secondRay, out hitInfo, secondCastLength, LayerMask.GetMask("Terrain")))
        {
            return hitInfo.point;
        }
        else
        {
            return default;
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach(Anchor anchor in anchors)
        {
            anchor.Initialize();
            Ray[] rays = new Ray[2];
            CastFromAnchor(anchor, Vector3.zero, out rays[0], out rays[1], .1f);
            
            GizmoForRay(rays[0], anchorDownwardLength, Color.green);
            GizmoForRay(rays[1], secondCastLength, Color.red);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(anchor.ikTarget.position, anchorZoneRadius);
            GizmoForForwardBias(anchor);
            
        }

        void GizmoForForwardBias(Anchor anchor)
        {
            Gizmos.color = Color.yellow;
            Vector3 source = anchor.ikTarget.position;
            Vector3 target = source + transform.forward * forwardBias;
            Gizmos.DrawLine(source, target);
        }

        void GizmoForRay(Ray ray, float distance, Color color)
        {
            Gizmos.color = color;
            Vector3 destination = ray.origin + ray.direction * distance;
            Gizmos.DrawLine(ray.origin, destination);
        }
    }

    [Serializable]
    protected class Anchor
    {
        
        public Transform ikTarget;
        [HideInInspector] public bool Stepping;
        [HideInInspector] public Vector3 LocalBasePosition;
        [HideInInspector] public float DistanceFromDeadZone;
        [HideInInspector] public Vector3 GluedWorldPosition;
        public void Initialize()
        {
            LocalBasePosition = ikTarget.localPosition;
        }

        public void UpdateGluedPosition()
        {
            GluedWorldPosition = ikTarget.position;
        }
    }
}


