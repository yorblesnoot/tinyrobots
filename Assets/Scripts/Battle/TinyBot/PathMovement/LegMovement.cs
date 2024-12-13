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

    private void Awake()
    {
        InitializeParameters();
        TerrainMask = LayerMask.GetMask("Terrain");
        foreach (var anchor in anchors)
        {
            anchor.Initialize();
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach(var anchor in anchors)
        {
            Gizmos.DrawWireSphere(anchor.ikTarget.position, anchorZoneRadius);
        }
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
            anchor.ikTarget.position = GetRaisedLimbTarget(anchor, Vector3.zero);
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
    
    protected void TryStepToBase(Anchor anchor, Vector3 legDirection)
    {
        Vector3 localStartPosition = anchor.ikTarget.localPosition;
        Vector3 finalPosition = GetRaisedLimbTarget(anchor, legDirection);
        if (finalPosition == default) return;
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

    Vector3 GetRaisedLimbTarget(Anchor anchor, Vector3 legDirection)
    {
        Vector3 target = GetLimbTarget(anchor, legDirection);
        target.y += footHeight;
        return target;
    }

    protected abstract Vector3 GetLimbTarget(Anchor anchor, Vector3 legDirection);

    protected override bool LeapedPathIsValid(Vector3 testSource, Vector3 direction, float distance, int sanitizeMask)
    {
        if (!base.LeapedPathIsValid(testSource, direction, distance, sanitizeMask)) return false;
        for (int i = 0; i < distance; i++)
        {
            Vector3 testPoint = testSource + direction * i;
            if (!Physics.CheckSphere(testPoint, locomotionHeight, TerrainMask)) return false;
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


