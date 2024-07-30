using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveAbility : Ability
{
    public AbilityType Type;
    
    public bool useAirCursor = true;
    
    public GameObject emissionPoint;
    
    GameObject trackedTarget;
    protected bool playerTargeting;
    protected List<Vector3> currentTrajectory;
    protected List<Targetable> currentTargets = new();    
    
    [SerializeField] ToggleAnimation trackingToggle;
    [SerializeField] AnimationController[] preAnimations;
    [SerializeField] AnimationController[] postAnimations;
    [SerializeField] AnimationController[] endAnimations;
    protected TargetPoint targetType;
    
    Trajectory trajectoryDefinition;
    TrackingAnimation trackingAnimation;

    readonly float skillDelay = .5f;
    private void Awake()
    {
        durationModule = GetComponent<DurationModule>();
        trajectoryDefinition = GetComponent<Trajectory>();
        targetType = TryGetComponent(out TargetPoint point) ? point : gameObject.AddComponent<ImpactTarget>();
        trackingAnimation = GetComponent<TrackingAnimation>();
        if (emissionPoint == null) emissionPoint = transform.gameObject;
    }

    public IEnumerator Execute()
    {
        Vector3 rawPosition = Owner.transform.position;
        Vector3Int startPosition = Vector3Int.RoundToInt(rawPosition);
        MainCameraControl.ActionPanTo(GetCameraAimPoint());
        currentCooldown = cooldown;
        PrimaryCursor.actionInProgress = true;
        yield return new WaitForSeconds(skillDelay);
        ReleaseLockOn();
        yield return ToggleAnimations(preAnimations);
        yield return StartCoroutine(PerformEffects());
        yield return ToggleAnimations(postAnimations);
        currentTargets = new();
        ScheduleAbilityEnd();
        PrimaryCursor.actionInProgress = false;
        if (Vector3Int.RoundToInt(Owner.transform.position) != startPosition) Pathfinder3D.GeneratePathingTree(Owner.MoveStyle, Owner.transform.position);
    }

    public virtual void EndAbility()
    {
        LineMaker.HideLine();
        targetType.EndTargeting();
        if(durationModule != null) durationModule.ClearCallback();
        if(trackingAnimation != null) trackingAnimation.ResetTracking();
        StartCoroutine(ToggleAnimations(endAnimations));
    }

    
    protected abstract IEnumerator PerformEffects();

    IEnumerator TrackTarget()
    {
        while (trackedTarget != null)
        {
            AimAt(trackedTarget, emissionPoint.transform.position, false);
            yield return null;
        }
    }

    public List<Targetable> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        Vector3 rangeTarget = GetRangeLimitedTarget(sourcePosition, target);
        currentTrajectory = trajectoryDefinition == null ? new() { sourcePosition, rangeTarget }
            : trajectoryDefinition.GetTrajectory(rangeTarget, sourcePosition, range);
        List<Targetable> newTargets = range == 0 ? new() { Owner } 
        : aiMode ? targetType.FindTargetsAI(currentTrajectory) : targetType.FindTargets(currentTrajectory);

        if (!aiMode)
        {
            if(trackingAnimation != null) trackingAnimation.Aim(currentTrajectory);
            Owner.PrimaryMovement.RotateToTrackEntity(trackedTarget);
        }
        
        if (playerTargeting)
        {
            if(trajectoryDefinition != null) LineMaker.DrawLine(currentTrajectory.ToArray());
            targetType.Draw(currentTrajectory);
            SetHighlightedTargets(newTargets);
        }
        currentTargets = new(newTargets);
        return newTargets;
    }

    Vector3 GetRangeLimitedTarget(Vector3 sourcePosition, GameObject target)
    {
        Vector3 targetPosition = target.transform.position;
        float distance = Vector3.Distance(sourcePosition, targetPosition);
        Vector3 direction = (targetPosition - sourcePosition).normalized;
        return sourcePosition + direction * Mathf.Min(distance, range);
    }
    void ScheduleAbilityEnd()
    {
        if (durationModule == null) EndAbility();
        else durationModule.SetDuration(Owner, EndAbility);
    }
    IEnumerator ToggleAnimations(AnimationController[] animations)
    {
        if (animations == null || animations.Length == 0) yield break;
        foreach (var ani in animations)
        {
            yield return StartCoroutine(ani.Play(Owner, currentTrajectory, currentTargets));
        }
            
    }

    public virtual bool IsUsable(Vector3 targetPosition)
    {
        return true;
    }

    public virtual bool IsAvailable()
    {
        if (currentCooldown > 0 || locked) return false;
        return true;
    }

    public virtual void LockOnTo(GameObject target, bool draw)
    {
        if(trackingToggle != null) StartCoroutine(trackingToggle.Play(Owner, currentTrajectory, currentTargets));
        trackedTarget = target;
        playerTargeting = draw;
        StartCoroutine(TrackTarget());
    }
    public virtual void ReleaseLockOn()
    {
        if (trackingToggle != null) trackingToggle.Stop();
        trackedTarget = null;
        StartCoroutine(Owner.PrimaryMovement.NeutralStance());
        LineMaker.HideLine();
        SetHighlightedTargets(null);
    }

    private void SetHighlightedTargets(List<Targetable> newTargets)
    {
        newTargets ??= new();
        for (int i = 0; i < newTargets.Count; i++)
        {
            Targetable bot = newTargets[i];
            if (bot == null) newTargets.Remove(bot);
            else if (!currentTargets.Contains(bot)) bot.SetOutlineColor(Color.red);
        }
        foreach(Targetable target in currentTargets)
        {
            if(!newTargets.Contains(target)) target.SetOutlineColor(Color.white);
        }
    }
    Vector3 GetCameraAimPoint()
    {
        if (currentTargets != null && currentTargets.Count > 0)
        {
            Vector3 average = Vector3.zero;
            foreach (var target in currentTargets)
            {
                average += target.transform.position;
            }
            average /= currentTargets.Count;
            return average;
        }
        else
        {
            Vector3 offset = PrimaryCursor.Transform.position - Owner.transform.position;
            offset = offset.normalized;
            offset *= Mathf.Min(range, offset.magnitude);
            return offset + Owner.transform.position;
        }
    }
}

public enum AbilityType
{
    ATTACK,
    BUFF,
    SHIELD,
    DASH,
    SUMMON
}
