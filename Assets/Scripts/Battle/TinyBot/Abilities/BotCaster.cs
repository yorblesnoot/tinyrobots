using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BotCaster : MonoBehaviour
{
    TinyBot Owner;
    bool tracking;
    public ActiveAbility PreparedAbility { get; private set; }
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
        if (!ability.IsAvailable() || !ability.IsAffordable()) return false;
        PreparedAbility = ability;
        
        pathableLocations = Pathfinder3D.GetPathableLocations(Owner.Stats.Current[StatType.MOVEMENT])
            .OrderBy(position => Vector3.Distance(position, Owner.transform.position))
            .ToList();
        if (Owner.Allegiance == Allegiance.PLAYER) StartCoroutine(PlayerAimAbility(ability, PrimaryCursor.Transform.gameObject));
        Owner.Movement.ToggleAnimations(false);
        return true;
    }

    void Cancel()
    {
        if(PreparedAbility == null) return;
        tracking = false;
        HidePlayerTargeting();
        PreparedAbility.ResetAim();
        PreparedAbility = null;
        Owner.Movement.ToggleAnimations(true);
        CastOutcomeIndicator.Hide();
    }

    public IEnumerator CastActiveAbility()
    {
        Owner.SpendResource(PreparedAbility.cost, PreparedAbility.CastingResource);
        PreparedAbility.CurrentCooldown = SceneGlobals.PlayerData.DevMode && Owner.Allegiance == Allegiance.PLAYER ? 0 : PreparedAbility.cooldown;
        yield return CastSequence(PreparedAbility, ActiveCast);
    }

    IEnumerator CastSequence(ActiveAbility ability, PossibleCast cast)
    {
        EndTracking();
        PrimaryCursor.TogglePlayerLockout(true);
        yield return ability.AdjustTrajectory(cast);
        Vector3Int startPosition = Vector3Int.RoundToInt(Owner.transform.position);
        MainCameraControl.PanToPosition(cast.Trajectory[^1]);
        yield return ability.Execute(cast.Trajectory, cast.Targets);
        if (Vector3Int.RoundToInt(Owner.transform.position) != startPosition) Pathfinder3D.GeneratePathingTree(Owner.MoveStyle, Owner.transform.position);
        if (ability.EndTurn)
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
            ActiveCast = PreparedAbility.SimulateCast(targetPosition);
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
            PossibleCast possibleCast = SimulatePossibleCast(targetPosition, enforceUsable, pathablePoint);
            if(possibleCast == null) continue;
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

    public PossibleCast SimulatePossibleCast(Vector3 targetPosition, bool enforceUsable, Vector3 origin)
    {
        if (!PreparedAbility.PointIsInRange(targetPosition, origin)) return null;
        PossibleCast possibleCast = PreparedAbility.SimulateCast(targetPosition, origin);
        if (enforceUsable && !PreparedAbility.IsCastable(possibleCast)) return null;
        return possibleCast;
    }

    PossibleCast FindClosestCast(Vector3 targetPoint)
    {
        Vector3 source = pathableLocations.OrderBy(location => Vector3.Distance(location, targetPoint)).First();
        return PreparedAbility.SimulateCast(targetPoint, source);
    }

    float GetTargetQuality(Vector3 position, List<Vector3> trajectory)
    {
        float offset = Vector3.Distance(position, trajectory[^1]) - PreparedAbility.AddedRange;
        return Mathf.Clamp(offset, 0, float.MaxValue);
    }

    void DrawPlayerTargeting(PossibleCast cast)
    {
        DrawTrajectory(cast);
        if (PreparedAbility.TargetType != null) PreparedAbility.TargetType.Draw(cast.Trajectory);
        SetHighlightedTargets(cast.Targets);
    }

    void DrawTrajectory(PossibleCast cast)
    {
        if (cast == null || cast.Trajectory.Count == 0) return;
        if(PreparedAbility.TargetType != null && !PreparedAbility.TargetType.UseLine) return;
        LineMaker.DrawLine(cast.Trajectory.ToArray());
    }

    public void EndTracking()
    {
        tracking = false;
        HidePlayerTargeting();
    }

    void HidePlayerTargeting()
    {
        LineMaker.HideLine();
        if(PreparedAbility.TargetType != null) PreparedAbility.TargetType.Hide();
        ResetHighlights?.Invoke();
    }

    void SetHighlightedTargets(List<Targetable> newTargets)
    {
        ResetHighlights?.Invoke();
        for (int i = 0; i < newTargets.Count; i++)
        {
            Targetable bot = newTargets[i];
            if (bot == null) newTargets.Remove(bot);
            bot.SetOutlineColor(PreparedAbility.GetOutlineColor());
        }
    }

    public bool CastIsValid()
    {
        return PreparedAbility.IsCastable(ActiveCast);
    }
}

public class PossibleCast
{
    public Vector3 Source;
    public List<Vector3> Trajectory;
    public List<Targetable> Targets;
    public bool Hit;
}
