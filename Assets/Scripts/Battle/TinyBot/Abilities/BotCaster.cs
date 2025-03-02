using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BotCaster : MonoBehaviour
{
    TinyBot Owner;
    bool tracking;
    public ActiveAbility Ability { get; private set; }
    public PossibleCast ActiveCast;
    List<Vector3Int> pathableLocations;
    public static UnityEvent ResetHighlights = new();
    public static UnityEvent ClearCasting = new();

    private void Awake()
    {
        Owner = GetComponent<TinyBot>();
        TinyBot.ClearActiveBot.AddListener(ClearCasting.Invoke);
        ClearCasting.AddListener(Cancel);
    }

    public bool TryPrepare(ActiveAbility ability)
    {
        if (!ability.IsAvailable() || Owner.Stats.Current[StatType.ENERGY] < ability.cost) return false;
        Ability = ability;
        
        pathableLocations = Pathfinder3D.GetPathableLocations(Owner.Stats.Current[StatType.MOVEMENT])
            .OrderBy(position => Vector3.Distance(position, Owner.transform.position))
            .ToList();
        if (Owner.Allegiance == Allegiance.PLAYER) StartCoroutine(PlayerAimAbility(ability, PrimaryCursor.Transform.gameObject));
        Owner.Movement.ToggleAnimations(false);
        return true;
    }

    void Cancel()
    {
        if(Ability == null) return;
        tracking = false;
        HidePlayerTargeting();
        Ability = null;
        Owner.Movement.ToggleAnimations(true);
        CastOutcomeIndicator.Hide();
    }

    public IEnumerator CastActiveAbility()
    {
        Owner.SpendResource(Ability.cost, StatType.ACTION);
        Ability.CurrentCooldown = SceneGlobals.PlayerData.DevMode && Owner.Allegiance == Allegiance.PLAYER ? 0 : Ability.cooldown;
        yield return CastSequence(ActiveCast);
    }

    IEnumerator CastSequence(PossibleCast cast)
    {
        EndTracking();
        PrimaryCursor.TogglePlayerLockout(true);
        yield return Ability.AdjustTrajectory(cast);
        Vector3Int startPosition = Vector3Int.RoundToInt(Owner.transform.position);
        MainCameraControl.PanToPosition(cast.Trajectory[^1]);
        yield return Ability.Execute(cast.Trajectory, cast.Targets);
        if (Vector3Int.RoundToInt(Owner.transform.position) != startPosition) Pathfinder3D.GeneratePathingTree(Owner.MoveStyle, Owner.transform.position);
        if (Ability.EndTurn)
        {
            yield return new WaitForSeconds(1);
            TurnManager.EndTurn(Owner);
        }
        if (Owner.Allegiance == Allegiance.PLAYER) PrimaryCursor.TogglePlayerLockout(false);
        ClearCasting.Invoke();
    }

    IEnumerator PlayerAimAbility(ActiveAbility ability, GameObject trackedTarget)
    {
        tracking = true;
        //Vector3 lastCastTarget = default;
        while (tracking == true)
        {
            Vector3 targetPosition = trackedTarget.transform.position;
            ActiveCast = Ability.SimulateCast(targetPosition);
            ability.PhysicalAimAlongTrajectory(ActiveCast.Trajectory);
            if (ability.range > 0 && Owner.Stats.Current[StatType.MOVEMENT] > 0
                && !ActiveCast.Targets.Contains(PrimaryCursor.TargetedBot)
                && GetTargetQuality(targetPosition, ActiveCast.Trajectory) > targetOffsetTolerance)
            {
                if (!FindValidCast(targetPosition, out ActiveCast, snapTarget: PrimaryCursor.TargetedBot))
                {
                    ActiveCast = FindClosestCast(targetPosition);
                    //owner.EchoMap[ability].PhysicalAimAlongTrajectory(PossibleCast.Trajectory);
                }
                Vector3 facing = targetPosition - ActiveCast.Source;
                PrimaryCursor.Instance.GenerateMovePreview(Vector3Int.RoundToInt(ActiveCast.Source), facing);
            }
            else PrimaryCursor.InvalidatePath();
            DrawPlayerTargeting(ActiveCast);
            CastOutcomeIndicator.Show(ActiveCast.Trajectory[^1], CastIsValid());
            yield return null;
        }
    }

    readonly float targetOffsetTolerance = .5f;
    public bool FindValidCast(Vector3 targetPosition, out PossibleCast cast, bool enforceUsable = false, Targetable snapTarget = null)
    {
        cast = default;
        foreach (Vector3Int pathablePoint in pathableLocations)
        {
            if (!Ability.PointIsInRange(pathablePoint, targetPosition)) continue;
            PossibleCast possibleCast = Ability.SimulateCast(targetPosition, pathablePoint);
            if (enforceUsable && !Ability.IsCastUsable(possibleCast)) continue;
            float quality = GetTargetQuality(targetPosition, possibleCast.Trajectory);
            bool gotSnapTarget = snapTarget != null && possibleCast.Targets.Contains(snapTarget);
            if (quality <= targetOffsetTolerance || gotSnapTarget)
            {
                cast = possibleCast;
                return true;
            }
        }
        return false;
    }

    PossibleCast FindClosestCast(Vector3 targetPoint)
    {
        Vector3 source = pathableLocations.OrderBy(location => Vector3.Distance(location, targetPoint)).First();
        return Ability.SimulateCast(targetPoint, source);
    }

    float GetTargetQuality(Vector3 position, List<Vector3> trajectory)
    {
        float offset = Vector3.Distance(position, trajectory[^1]) - Ability.AddedRange;
        return Mathf.Clamp(offset, 0, float.MaxValue);
    }

    void DrawPlayerTargeting(PossibleCast cast)
    {
        if (cast.Trajectory != null && cast.Trajectory.Count > 0) LineMaker.DrawLine(cast.Trajectory.ToArray());
        if (Ability.TargetType != null) Ability.TargetType.Draw(cast.Trajectory);
        SetHighlightedTargets(cast.Targets);
    }

    public void EndTracking()
    {
        tracking = false;
        HidePlayerTargeting();
    }

    void HidePlayerTargeting()
    {
        LineMaker.HideLine();
        if(Ability.TargetType != null) Ability.TargetType.Hide();
        SetHighlightedTargets(null);
    }

    void SetHighlightedTargets(List<Targetable> newTargets)
    {
        ResetHighlights?.Invoke();
        if (newTargets == null) return;
        for (int i = 0; i < newTargets.Count; i++)
        {
            Targetable bot = newTargets[i];
            if (bot == null) newTargets.Remove(bot);
            bot.SetOutlineColor(Ability.GetOutlineColor());
        }
    }

    public bool CastIsValid()
    {
        return Ability.IsCastUsable(ActiveCast);
    }
}

public class PossibleCast
{
    public Vector3 Source;
    public List<Vector3> Trajectory;
    public List<Targetable> Targets;
    public bool Hit;
}
