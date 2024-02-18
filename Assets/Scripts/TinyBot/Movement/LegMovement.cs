using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LegMovement : PrimaryMovement
{
    [SerializeField] protected float pathStepDuration = .4f;
    [SerializeField] protected float legStepDuration = .1f;
    [SerializeField] protected float lookHeightModifier = 1f;

    [SerializeField] protected Anchor[] anchors;
    [SerializeField] protected float anchorZoneRadius = 1f;
    [SerializeField] protected Transform legModel;
    [SerializeField] protected float anchorUpwardLimit = 2f;
    [SerializeField] protected float anchorDownwardLength = 3f;
    [SerializeField] AnimationCurve legRaise;

    protected bool stepping;

    private void Awake()
    {
        InitializeParameters();
        InitializeAnchors();
    }
    void InitializeAnchors()
    {
        foreach (var anchor in anchors)
        {
            anchor.Initialize();
        }
    }

    public override void RotateToTrackEntity(GameObject trackingTarget)
    {
        base.RotateToTrackEntity(trackingTarget);
        CheckAnchorPositions();
    }

    protected abstract void InitializeParameters();
    protected void CheckAnchorPositions()
    {
        Anchor farthestFromBase = null;
        foreach (var anchor in anchors)
        {
            farthestFromBase ??= anchor;
            anchor.GluePosition();
            if (anchor.distanceFromBase > farthestFromBase.distanceFromBase) farthestFromBase = anchor;
        }

        if (farthestFromBase.distanceFromBase >= anchorZoneRadius && !stepping)
        {
            StartCoroutine(StepToBase(farthestFromBase));
        }
    }
    protected IEnumerator InterpolatePositionAndRotation(Transform unit, Vector3 target)
    {
        Quaternion startRotation = unit.rotation;
        Quaternion targetRotation = GetRotationAtPosition(target);

        Vector3 startPosition = unit.position;
        float timeElapsed = 0;
        while (timeElapsed < pathStepDuration)
        {
            unit.SetPositionAndRotation(Vector3.Lerp(startPosition, target, timeElapsed / pathStepDuration),
                Quaternion.Slerp(startRotation, targetRotation, timeElapsed / pathStepDuration));
            timeElapsed += Time.deltaTime;


            CheckAnchorPositions();
            yield return null;
        }
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
            targetPosition.y += legRaise.Evaluate(interpolator);
            anchor.ikTarget.localPosition = targetPosition;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        anchor.UpdateGluedPosition();
        stepping = false;
        anchor.stepping = false;
    }

    protected abstract Quaternion GetRotationAtPosition(Vector3 moveTarget);

    protected abstract Vector3 GetLimbTarget(Anchor anchor, bool goToNeutral, Vector3 localStartPosition);

    public override IEnumerator NeutralStance()
    {
        foreach (var anchor in anchors)
        {
            yield return StartCoroutine(StepToBase(anchor, true));
        }
    }
    [Serializable]
    protected class Anchor
    {
        [HideInInspector] public bool stepping;
        public Transform ikTarget;
        [HideInInspector] public Vector3 localBasePosition;
        [HideInInspector] public float distanceFromBase;

        Vector3 gluedWorldPosition;
        public void Initialize()
        {
            localBasePosition = ikTarget.localPosition;
        }

        public void UpdateGluedPosition()
        {
            gluedWorldPosition = ikTarget.position;
        }

        public void GluePosition()
        {
            if (stepping) return;
            ikTarget.position = gluedWorldPosition;
            distanceFromBase = Vector3.Distance(ikTarget.localPosition, localBasePosition);
        }
    }
}
