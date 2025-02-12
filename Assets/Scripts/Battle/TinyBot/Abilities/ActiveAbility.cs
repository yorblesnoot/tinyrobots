using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.Events;

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
    [HideInInspector] public static UnityEvent ResetHighlights = new();

    public float TotalRange { get { return range + TargetType.TargetRadius; } }
    public override bool IsActive => true;

    protected override AbilityEffect[] Effects => abilityEffects;

    private void Awake()
    {
        foreach (var effect in abilityEffects) effect.Initialize(this);
        foreach (var effect in endEffects) effect.Initialize(this);

        DurationModule = GetComponent<DurationModule>();
        TrajectoryDefinition = TryGetComponent(out Trajectory trajectory) ? trajectory : gameObject.AddComponent<NoTrajectory>();
        TargetType = TryGetComponent(out TargetPoint point) ? point : gameObject.AddComponent<ImpactTarget>();
        trackingAnimation = GetComponent<TrackingAnimation>();
    }

    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
        if (emissionPoint == null) emissionPoint = Owner.transform;
    }

    public IEnumerator Execute()
    {
        GameObject dummy = new();
        dummy.transform.position = CurrentTrajectory[^1];
        yield return Execute(dummy);
    }

    public IEnumerator Execute(GameObject target)
    {
        LockOnTo(target, false);
        yield return new WaitForSeconds(1f);
        Owner.SpendResource(cost, StatType.ACTION);
        Vector3 rawPosition = Owner.transform.position;
        Vector3Int startPosition = Vector3Int.RoundToInt(rawPosition);
        MainCameraControl.PanToPosition(CurrentTrajectory[^1]);
        CurrentCooldown = SceneGlobals.PlayerData.DevMode && Owner.Allegiance == Allegiance.PLAYER ? 0 : cooldown;
        yield return new WaitForSeconds(skillDelay);
        ReleaseLockOn();
        
        foreach (var effect in abilityEffects) yield return effect.PerformEffect(Owner, CurrentTrajectory, CurrentTargets);

        CurrentTargets = new();
        ScheduleAbilityEnd();
        //MainCameraControl.FindViewOfPosition(Owner.TargetPoint.position, false, false);
        if (Vector3Int.RoundToInt(Owner.transform.position) != startPosition) Pathfinder3D.GeneratePathingTree(Owner.MoveStyle, Owner.transform.position);
        
    }

    public virtual void EndAbility()
    {
        LineMaker.HideLine();
        TargetType.EndTargeting();
        if(DurationModule != null) DurationModule.ClearCallback();
        if(trackingAnimation != null) trackingAnimation.ResetTracking();
        foreach(var effect in endEffects)
            StartCoroutine(effect.PerformEffect(Owner, null, null));
    }


    protected virtual IEnumerator PerformEffects() { yield break; }

    readonly float targetOffsetTolerance = .1f;
    Vector3Int lastValidCastSource;
    IEnumerator TrackTarget(bool draw)
    {
        while (trackedTarget != null)
        {
            Vector3 targetPosition = trackedTarget.transform.position;
            Vector3Int cleanTarget = Vector3Int.RoundToInt(targetPosition);
            EvaluateTrajectory(targetPosition, emissionPoint.position, false);
            ActiveAbility aimer = this;
            if (draw && TargetType.GetTargetQuality(targetPosition, CurrentTrajectory) > targetOffsetTolerance)
            {
                if (cleanTarget != lastValidCastSource)
                {
                    Vector3Int alternateCastSource = EvaluateAlternateCastPositions(targetPosition);
                    if (alternateCastSource == default) alternateCastSource = lastValidCastSource;
                    else lastValidCastSource = alternateCastSource;
                    Vector3 facing = targetPosition - alternateCastSource;
                    PrimaryCursor.Instance.GenerateMovePreview(alternateCastSource, facing);
                }
                aimer = Owner.EchoMap[this];
                aimer.EvaluateTrajectory(targetPosition, aimer.emissionPoint.position, false);
                CurrentTrajectory = aimer.CurrentTrajectory;
            }
            else PrimaryCursor.InvalidatePath();
            PhysicalAimAlongTrajectory();
            if (draw) aimer.DrawPlayerTargeting();
            yield return null;
        }
    }

    Vector3Int EvaluateAlternateCastPositions(Vector3 targetPosition)
    {
        List<Vector3Int> pathableLocations = Pathfinder3D.GetPathableLocations(Owner.Stats.Current[StatType.MOVEMENT])
            .Where(position => Vector3.Distance(position, targetPosition) <= TotalRange)
            .OrderBy(position => Vector3.Distance(position, Owner.transform.position))
            .ToList();
        PriorityQueue<Vector3Int, float> priority = new();
        Vector3Int position = default;
        foreach (Vector3Int location in pathableLocations)
        {
            EvaluateTrajectory(targetPosition, location, true);
            float quality = TargetType.GetTargetQuality(targetPosition, CurrentTrajectory);
            if (quality == 0)
            {
                position = location;
                break;
            }
            priority.Enqueue(location, quality);
        }
        if(position == default && pathableLocations.Count > 0) position = priority.Dequeue();
        return position;
    }

    public List<Targetable> EvaluateTrajectory(Vector3 targetPosition, Vector3 sourcePosition, bool imaginary)
    {
        Vector3 rangeTarget = GetRangeLimitedTarget(sourcePosition, targetPosition);
        CurrentTrajectory = TrajectoryDefinition.GetTrajectory(sourcePosition, rangeTarget, out RaycastHit hit, imaginary);
        TrajectoryCollided = hit.collider != null;
        List<Targetable> newTargets = range == 0 ? new() { Owner } : (imaginary ? TargetType.FindTargetsAI(CurrentTrajectory) : TargetType.FindTargets(CurrentTrajectory));
        CurrentTargets = new(newTargets);
        return newTargets;
    }

    public void PhysicalAimAlongTrajectory()
    {
        if (trackingAnimation != null) trackingAnimation.Aim(CurrentTrajectory);
        Owner.PrimaryMovement.PivotToFacePosition(trackedTarget.transform.position);
    }

    public void DrawPlayerTargeting()
    {
        TrajectoryDefinition.Draw(CurrentTrajectory);
        TargetType.Draw(CurrentTrajectory);
        SetHighlightedTargets(CurrentTargets);
    }



    Vector3 GetRangeLimitedTarget(Vector3 sourcePosition, Vector3 targetPosition)
    {
        float distance = Vector3.Distance(sourcePosition, targetPosition);
        Vector3 direction = (targetPosition - sourcePosition).normalized;
        return sourcePosition + direction * Mathf.Min(distance, range);
    }
    void ScheduleAbilityEnd()
    {
        if (DurationModule == null) EndAbility();
        else DurationModule.SetDuration(Owner, EndAbility);
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

    public void LockOnTo(GameObject target, bool draw)
    {
        if(trackingToggle != null) StartCoroutine(trackingToggle.PerformEffect(Owner, CurrentTrajectory, CurrentTargets));
        trackedTarget = target;
        PlayerTargeting = draw;
        StartCoroutine(TrackTarget(draw));
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
        ResetHighlights?.Invoke();
        if (newTargets == null) return;
        for (int i = 0; i < newTargets.Count; i++)
        {
            Targetable bot = newTargets[i];
            if (bot == null) newTargets.Remove(bot);
            bot.SetOutlineColor(Color.red);
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
