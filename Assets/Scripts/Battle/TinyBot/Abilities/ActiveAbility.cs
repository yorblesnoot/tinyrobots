using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveAbility : Ability
{   
    public AbilityType Type;
    public bool EndTurn = false;
    [SerializeField] TargetRequirement targetRequirement;
    [SerializeField] ToggleAnimation trackingToggle;
    [SerializeField] AbilityEffect[] abilityEffects;
    [SerializeField] AbilityEffect[] endEffects;


    [HideInInspector] public TargetPoint TargetType;
    protected Trajectory TrajectoryDefinition;
    TrackingAnimation trackingAnimation;

    HashSet<System.Object> prohibitionSources = new();
    
    [HideInInspector] public bool Locked { get { return prohibitionSources.Count > 0; } }
    
    public float AddedRange { get { return TargetType != null ? TargetType.AddedRange : 0; } }
    public float TotalRange { get { return range + AddedRange; } }
    public override bool IsActive => true;

    protected override AbilityEffect[] Effects => abilityEffects;
    Vector3 baseEmissionPosition;

    private void Awake()
    {
        foreach (var effect in abilityEffects) effect.Initialize(this);
        foreach (var effect in endEffects) effect.Initialize(this);

        DurationModule = GetComponent<DurationModule>();
        TrajectoryDefinition = TryGetComponent(out Trajectory trajectory) ? trajectory : gameObject.AddComponent<NoTrajectory>();
        TargetType = gameObject.GetComponent<TargetPoint>();
        trackingAnimation = GetComponent<TrackingAnimation>();
    }

    public override void Initialize(TinyBot botUnit)
    {
        base.Initialize(botUnit);
        if (emissionPoint == null) emissionPoint = Owner.transform;
        baseEmissionPosition = Owner.transform.InverseTransformPoint(emissionPoint.position);
    }

    public IEnumerator Execute(List<Vector3> trajectory, List<Targetable> targets)
    {
        foreach (var effect in abilityEffects) yield return effect.PerformEffect(Owner, trajectory, targets);
        ScheduleAbilityEnd();
        //MainCameraControl.FindViewOfPosition(Owner.TargetPoint.position, false, false);
    }

    readonly float finalizeAimDuration = .5f;
    public IEnumerator AdjustTrajectory(PossibleCast cast)
    {
        float timeElapsed = 0;
        while (timeElapsed < finalizeAimDuration)
        {
            
            PhysicalAimAlongTrajectory(cast.Trajectory);
            cast.Trajectory = TrajectoryDefinition.GetTrajectory(emissionPoint.transform.position, cast.Trajectory[^1], out _);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
    }

    readonly float rangeCheckThreshold = 1;
    public bool PointIsInRange(Vector3 target, Vector3 source)
    {
        Vector3 rangeTarget = TrajectoryDefinition.RestrictRange(target, source, range);
        return Vector3.Distance(rangeTarget, target) < rangeCheckThreshold;
    }

    public PossibleCast SimulateCast(Vector3 castTarget, Vector3 ownerPosition = default)
    {
        Vector3 emissionSource;
        Vector3 rangeSource;
        if(ownerPosition == default)
        {
            ownerPosition = Owner.transform.position;
            
            rangeSource = transform.position;
            emissionSource = rangeSource;//emissionPoint.position;
        }
        else
        {
            Vector3 facing = castTarget - ownerPosition;
            emissionSource = JointPositionAt(baseEmissionPosition, ownerPosition, facing, true);
            rangeSource = JointPositionAt(transform.position, ownerPosition, facing);
        }
        PossibleCast eval = new() { Source = ownerPosition };
        Vector3 rangeTarget = TrajectoryDefinition.RestrictRange(castTarget, rangeSource, range);
        eval.Trajectory = TrajectoryDefinition.GetTrajectory(emissionSource, rangeTarget, out Collider collider);
        eval.Hit = collider != null; //|| Physics.CheckSphere(eval.Trajectory[^1], endCheckRadius, TrajectoryDefinition.BlockingLayerMask);
        eval.Targets = GetTargets(eval.Trajectory, collider);
        return eval;
    }

    List<Targetable> GetTargets(List<Vector3> trajectory, Collider hit)
    {
        List<Targetable> targets = new();
        if (range == 0) targets.Add(Owner);
        else if (TargetType != null) targets.AddRange(TargetType.FindTargets(trajectory));
        else if (hit != null && hit.TryGetComponent(out Targetable target)) targets.Add(target);
        return targets;
    }

    Vector3 JointPositionAt(Vector3 jointPosition, Vector3 position, Vector3 facing, bool local = false)
    {
        Vector3 localGun = local ? jointPosition : Owner.transform.InverseTransformPoint(jointPosition);
        Quaternion locationRotation = Owner.Movement.GetRotationFromFacing(position, facing);
        
        Vector3 rotatedGun = locationRotation * localGun;
        return position + rotatedGun;
    }

    public virtual void EndAbility()
    {
        if(DurationModule != null) DurationModule.ClearCallback();
        if(trackingAnimation != null) trackingAnimation.ResetTracking();
        foreach(var effect in endEffects)
            StartCoroutine(effect.PerformEffect(Owner, null, null));
    }

    public void PhysicalAimAlongTrajectory(List<Vector3> trajectory)
    {
        if (trackingAnimation != null) trackingAnimation.Aim(trajectory);
        if (range == 0) return;
        Owner.Movement.PivotToFacePosition(trajectory[^1]);
    }

    public void ResetAim()
    {
        trackingAnimation.ResetTracking();
        trackingToggle.Stop();
    }

    void ScheduleAbilityEnd()
    {
        if (DurationModule == null) EndAbility();
        else DurationModule.SetDuration(Owner, EndAbility);
    }

    public bool IsCastValid(PossibleCast cast)
    {
        if (targetRequirement == TargetRequirement.NONE) return true;
        else if (targetRequirement == TargetRequirement.OPEN && cast.Trajectory != null
            && Pathfinder3D.GetLandingPointBy(cast.Trajectory[^1], Owner.MoveStyle, out _)) return true;
        else if (targetRequirement == TargetRequirement.UNIT && cast.Targets.Count > 0) return true;
        else if (targetRequirement == TargetRequirement.TERRAIN && cast.Hit) return true;
        else if (targetRequirement == TargetRequirement.ALLY && cast.Targets.Count > 0
            && cast.Targets[0].Allegiance == Owner.Allegiance) return true;
        return false;
    }

    public virtual bool IsAvailable()
    {
        if (CurrentCooldown > 0 || Locked) return false;
        return true;
    }

    public void ProhibitAbility(object source, bool prohibit = true)
    {
        if(prohibit) prohibitionSources.Add(source);
        else prohibitionSources.Remove(source);
        ClickableAbility.RefreshUsability.Invoke();
    }

    public Color GetOutlineColor() => targetRequirement switch
    {
        TargetRequirement.ALLY => Color.green,
        TargetRequirement.OPEN => Color.white,
        _ => Color.red,
    };

    enum TargetRequirement
    {
        NONE,
        OPEN,
        UNIT,
        TERRAIN,
        ALLY
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


