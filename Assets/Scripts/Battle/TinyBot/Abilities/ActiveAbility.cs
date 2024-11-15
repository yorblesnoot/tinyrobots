using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveAbility : Ability
{
    enum TargetRequirement
    {
        NONE,
        OPEN,
        UNIT,
        TERRAIN,
        ALLY
    }

    
    public AbilityType Type;
    public bool EndTurn = false;
    [SerializeField] TargetRequirement targetRequirement;
      
    
    [SerializeField] ToggleAnimation trackingToggle;

    [SerializeField] AbilityEffect[] abilityEffects;
    [SerializeField] AbilityEffect[] endEffects;

    protected bool PlayerTargeting;
    protected List<Vector3> CurrentTrajectory;
    protected List<Targetable> CurrentTargets = new();
    protected TargetPoint TargetType;
    protected Trajectory TrajectoryDefinition;
    protected bool TrajectoryCollided;
    TrackingAnimation trackingAnimation;
    GameObject trackedTarget;

    readonly float skillDelay = .5f;
    HashSet<System.Object> prohibitionSources = new();

    [HideInInspector] public bool Locked { get { return prohibitionSources.Count > 0; } }

    public float TotalRange { get { return range + TargetType.TargetRadius; } }
    public override bool IsActive => true;
    private void Awake()
    {
        foreach (var effect in abilityEffects) effect.Initialize(this);
        foreach (var effect in endEffects) effect.Initialize(this);

        durationModule = GetComponent<DurationModule>();
        TrajectoryDefinition = TryGetComponent(out Trajectory trajectory) ? trajectory : gameObject.AddComponent<NoTrajectory>();
        TargetType = TryGetComponent(out TargetPoint point) ? point : gameObject.AddComponent<ImpactTarget>();
        trackingAnimation = GetComponent<TrackingAnimation>();
    }

    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
        if (emissionPoint == null) emissionPoint = Owner.gameObject;
    }

    public IEnumerator Execute()
    {
        Vector3 rawPosition = Owner.transform.position;
        Vector3Int startPosition = Vector3Int.RoundToInt(rawPosition);
        MainCameraControl.ActionPanTo(GetCameraAimPoint());
        CurrentCooldown = SceneGlobals.PlayerData.DevMode ? 0 : cooldown;
        PrimaryCursor.ActionInProgress = true;
        yield return new WaitForSeconds(skillDelay);
        ReleaseLockOn();
        
        foreach (var effect in abilityEffects) yield return effect.PerformEffect(Owner, CurrentTrajectory, CurrentTargets);

        CurrentTargets = new();
        ScheduleAbilityEnd();
        PrimaryCursor.ActionInProgress = false;
        if (Vector3Int.RoundToInt(Owner.transform.position) != startPosition) Pathfinder3D.GeneratePathingTree(Owner.MoveStyle, Owner.transform.position);
        
    }

    public virtual void EndAbility()
    {
        LineMaker.HideLine();
        TargetType.EndTargeting();
        if(durationModule != null) durationModule.ClearCallback();
        if(trackingAnimation != null) trackingAnimation.ResetTracking();
        foreach(var effect in endEffects)
            StartCoroutine(effect.PerformEffect(Owner, null, null));
    }


    protected virtual IEnumerator PerformEffects() { yield break; }

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
        CurrentTrajectory = TrajectoryDefinition.GetTrajectory(sourcePosition, rangeTarget, out RaycastHit hit, aiMode);
        TrajectoryCollided = hit.collider != null;
        List<Targetable> newTargets = range == 0 ? new() { Owner } 
        : aiMode ? TargetType.FindTargetsAI(CurrentTrajectory) : TargetType.FindTargets(CurrentTrajectory);

        if (!aiMode)
        {
            if(trackingAnimation != null) trackingAnimation.Aim(CurrentTrajectory);
            Owner.PrimaryMovement.RotateToTrackEntity(trackedTarget);
        }
        
        if (PlayerTargeting)
        {
            TrajectoryDefinition.Draw(CurrentTrajectory);
            TargetType.Draw(CurrentTrajectory);
            SetHighlightedTargets(newTargets);
        }
        CurrentTargets = new(newTargets);
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

    public virtual bool IsUsable()
    {
        if (targetRequirement == TargetRequirement.NONE) return true;
        else if (targetRequirement == TargetRequirement.OPEN && CurrentTrajectory != null
            && Pathfinder3D.GetLandingPointBy(CurrentTrajectory[^1], Owner.MoveStyle, out _)) return true;
        else if (targetRequirement == TargetRequirement.UNIT && CurrentTargets.Count > 0) return true;
        else if (targetRequirement == TargetRequirement.TERRAIN && TrajectoryCollided) return true;
        else if (targetRequirement == TargetRequirement.ALLY && CurrentTargets.Count > 0 
            && CurrentTargets[0].Allegiance == Owner.Allegiance) return true; 
        return false;
    }

    public virtual bool IsAvailable()
    {
        if (CurrentCooldown > 0 || Locked) return false;
        return true;
    }

    public virtual void LockOnTo(GameObject target, bool draw)
    {
        if(trackingToggle != null) StartCoroutine(trackingToggle.PerformEffect(Owner, CurrentTrajectory, CurrentTargets));
        trackedTarget = target;
        PlayerTargeting = draw;
        StartCoroutine(TrackTarget());
    }
    public virtual void ReleaseLockOn()
    {
        if (trackingToggle != null) trackingToggle.Stop();
        trackedTarget = null;
        TrajectoryDefinition.Hide();
        SetHighlightedTargets(null);
        TargetType.EndTargeting();
    }

    private void SetHighlightedTargets(List<Targetable> newTargets)
    {
        newTargets ??= new();
        for (int i = 0; i < newTargets.Count; i++)
        {
            Targetable bot = newTargets[i];
            if (bot == null) newTargets.Remove(bot);
            else if (!CurrentTargets.Contains(bot)) bot.SetOutlineColor(Color.red);
        }
        foreach(Targetable target in CurrentTargets)
        {
            if(target == null) continue;
            if(!newTargets.Contains(target)) target.SetOutlineColor(Color.white);
        }
    }
    Vector3 GetCameraAimPoint()
    {
        if (CurrentTargets != null && CurrentTargets.Count > 0)
        {
            Vector3 average = Vector3.zero;
            foreach (var target in CurrentTargets)
            {
                average += target.transform.position;
            }
            average /= CurrentTargets.Count;
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

    public void ProhibitAbility(System.Object source, bool prohibit = true)
    {
        if(prohibit) prohibitionSources.Add(source);
        else prohibitionSources.Remove(source);
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
