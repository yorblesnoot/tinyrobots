using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveAbility : Ability
{
    public AbilityType Type;
    public int cost;
    
    public bool useAirCursor = true;
    
    public GameObject emissionPoint;
    
    GameObject trackedTarget;
    protected bool playerTargeting;
    protected List<Vector3> currentTrajectory;
    protected List<Targetable> currentTargets = new();

    [SerializeField] Trajectory trajectoryDefinition;
    [SerializeField] TargetPoint endPoint;
    [SerializeField] TrackingAnimation trackingAnimation;
    [SerializeField] PartAnimation[] preAnimations;
    [SerializeField] PartAnimation[] postAnimations;
    [SerializeField] DurationModule durationModule;

    readonly float skillDelay = .5f;


    public IEnumerator Execute()
    {
        Vector3 rawPosition = Owner.transform.position;
        Vector3Int startPosition = Vector3Int.RoundToInt(rawPosition);
        MainCameraControl.ActionPanTo(GetFinalAimPoint());
        currentCooldown = cooldown;
        PrimaryCursor.actionInProgress = true;
        yield return new WaitForSeconds(skillDelay);
        yield return ToggleAnimations(preAnimations);
        yield return StartCoroutine(PerformEffects());
        yield return ToggleAnimations(postAnimations);
        ScheduleAbilityEnd();
        PrimaryCursor.actionInProgress = false;
        if (Vector3Int.RoundToInt(Owner.transform.position) != startPosition) Pathfinder3D.GeneratePathingTree(Owner.MoveStyle, Owner.transform.position);
    }

    void ScheduleAbilityEnd()
    {
        if (durationModule == null) EndAbility();
        else durationModule.SetDuration(Owner, EndAbility);
    }

    public virtual void EndAbility()
    {
        trackingAnimation.ResetTracking();
        StartCoroutine(ToggleAnimations(postAnimations, false));
        ReleaseLockOn();
    }

    Vector3 GetFinalAimPoint()
    {
        if(currentTargets != null && currentTargets.Count > 0)
        {
            Vector3 average = Vector3.zero;
            foreach(var target in currentTargets)
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

    
    public List<Targetable> AimAt(GameObject target, Vector3 sourcePosition, bool aiMode = false)
    {
        currentTrajectory = trajectoryDefinition.GetTrajectory(target, sourcePosition, range);
        List<Targetable> newTargets = aiMode ? endPoint.FindTargetsAI(currentTrajectory) : endPoint.FindTargets(currentTrajectory);

        if (!aiMode)
        {
            trackingAnimation.Aim(currentTrajectory);
            
            Owner.PrimaryMovement.RotateToTrackEntity(trackedTarget);
        }
        
        if (playerTargeting)
        {
            trajectoryDefinition.Draw(currentTrajectory);
            endPoint.Draw(currentTrajectory);
            SetHighlightedTargets(newTargets);
        }

        return newTargets;
    }

    IEnumerator ToggleAnimations(PartAnimation[] animations, bool play = true)
    {
        if (animations == null || animations.Length == 0) yield break;
        foreach (var ani in animations)
        {
            if(play) yield return StartCoroutine(ani.Play(Owner, currentTrajectory, currentTargets));
            else StartCoroutine(ani.Stop());
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
        trackedTarget = target;
        playerTargeting = draw;
        StartCoroutine(TrackTarget());
    }
    public virtual void ReleaseLockOn()
    {
        trackedTarget = null;
        StartCoroutine(Owner.PrimaryMovement.NeutralStance());
        LineMaker.HideLine();
        SetHighlightedTargets(null);
    }

    protected abstract IEnumerator PerformEffects();

    public void NeutralAim()
    {
        trackingAnimation.ResetTracking();
    }
    IEnumerator TrackTarget()
    {
        while(trackedTarget != null)
        {
            AimAt(trackedTarget, emissionPoint.transform.position, false);
            yield return null;
        }
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
        foreach(Targetable bot in currentTargets)
        {
            if(!newTargets.Contains(bot)) bot.SetOutlineColor(Color.white);
        }
        currentTargets = new(newTargets);
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
