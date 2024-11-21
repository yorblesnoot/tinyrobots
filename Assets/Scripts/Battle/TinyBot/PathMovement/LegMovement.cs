using PrimeTween;
using System;
using System.Collections;
using System.Collections.Generic;
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

    protected bool Stepping;
    Dictionary<Transform, (Vector3, Quaternion)> baseLimbPositions;

    private void Awake()
    {
        InitializeParameters();
        foreach (var anchor in anchors)
        {
            anchor.Initialize();
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
            anchor.ikTarget.position = GetLimbTarget(anchor, true);
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
    public override void AnimateToOrientation(bool inPlace = false)
    {
        foreach (var anchor in anchors)
        {
            GluePosition(anchor);
        }
        if (Stepping) return;

        anchors = anchors.OrderByDescending(anchor => anchor.DistanceFromDeadZone).ToArray();

        for(int i = 0; i < anchors.Length; i++)
        {
            if (anchors[i].DistanceFromDeadZone > anchorZoneRadius)
            {
                TryStepToBase(anchors[i], inPlace);
            }
            if(Stepping) return;
        }
        
    }

    protected void GluePosition(Anchor anchor)
    {
        if (anchor.Stepping) return;
        anchor.ikTarget.position = anchor.GluedWorldPosition;
        anchor.DistanceFromDeadZone = LegDistanceFromDeadZone(anchor);
    }

    protected virtual float LegDistanceFromDeadZone(Anchor anchor)
    {
        Vector3 localForward = anchor.ikTarget.parent.InverseTransformDirection(Owner.transform.forward);
        localForward.Normalize();
        return Vector3.Distance(anchor.ikTarget.localPosition, anchor.LocalBasePosition + localForward * forwardBias);
    }
    
    protected void TryStepToBase(Anchor anchor, bool goToNeutral = false)
    {
        Vector3 localStartPosition = anchor.ikTarget.localPosition;
        Vector3 finalPosition = GetLimbTarget(anchor, goToNeutral);
        if (finalPosition == default) return;
        Stepping = true;
        anchor.Stepping = true;
        Tween.Position(anchor.ikTarget, finalPosition, legStepDuration).OnComplete(() => CompleteStep(anchor));
    }

    private void CompleteStep(Anchor anchor)
    {
        anchor.UpdateGluedPosition();
        Stepping = false;
        anchor.Stepping = false;
    }

    protected abstract Vector3 GetLimbTarget(Anchor anchor, bool goToNeutral);

    public override IEnumerator NeutralStance()
    {
        foreach (var anchor in anchors)
        {
            TryStepToBase(anchor, true);
            yield return new WaitForSeconds(legStepDuration);
        }
    }

    public override void LandingStance()
    {
        foreach (var anchor in anchors)
        {
            if (anchor.Stepping) continue;
            TryStepToBase(anchor, true);
        }
    }

    protected Vector3 GetMeshNormalAt(Vector3 target)
    {
        return Pathfinder3D.GetCrawlOrientation(Vector3Int.RoundToInt(target));
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


