using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BotCaster : MonoBehaviour
{
    TinyBot owner;
    bool tracking;
    public ActiveAbility Ability { get; private set; }
    public PossibleCast ActiveCast;
    List<Vector3Int> pathableLocations;
    public static UnityEvent ResetHighlights = new();
    public static UnityEvent ClearCasting = new();

    private void Awake()
    {
        owner = GetComponent<TinyBot>();
        TinyBot.ClearActiveBot.AddListener(ClearCasting.Invoke);
        ClearCasting.AddListener(Cancel);
    }

    public bool TryPrepare(ActiveAbility ability)
    {
        if (!ability.IsAvailable() || owner.Stats.Current[StatType.ENERGY] < ability.cost) return false;
        Ability = ability;
        
        pathableLocations = Pathfinder3D.GetPathableLocations(owner.Stats.Current[StatType.MOVEMENT])
            .OrderBy(position => Vector3.Distance(position, owner.transform.position))
            .ToList();
        if (owner.Allegiance == Allegiance.PLAYER) StartCoroutine(PlayerAimAbility(ability, PrimaryCursor.Transform.gameObject));
        owner.Movement.ToggleAnimations(false);
        return true;
    }

    public void CastLoadedSkill()
    {
        StartCoroutine(CastSequence(ActiveCast));
    }

    void Cancel()
    {
        if(Ability == null) return;
        tracking = false;
        HidePlayerTargeting();
        Ability = null;
        owner.Movement.ToggleAnimations(true);
        CastOutcomeIndicator.Hide();
    }

    public IEnumerator CastSequence()
    {
        yield return CastSequence(ActiveCast);
    }

    IEnumerator CastSequence(PossibleCast cast)
    {
        EndTracking();
        PrimaryCursor.TogglePlayerLockout(true);
        yield return Ability.AdjustTrajectory(cast);
        Vector3Int startPosition = Vector3Int.RoundToInt(owner.transform.position);
        MainCameraControl.PanToPosition(cast.Trajectory[^1]);
        yield return Ability.Execute(cast.Trajectory, cast.Targets);
        if (Vector3Int.RoundToInt(owner.transform.position) != startPosition) Pathfinder3D.GeneratePathingTree(owner.MoveStyle, owner.transform.position);
        if (Ability.EndTurn) yield return new WaitForSeconds(1);
        PrimaryCursor.TogglePlayerLockout(false);
        if (Ability.EndTurn) TurnManager.EndTurn(owner);
        ClearCasting.Invoke();
    }

    IEnumerator PlayerAimAbility(ActiveAbility ability, GameObject trackedTarget)
    {
        tracking = true;
        Vector3Int lastCastTarget = default;
        Vector3 lastCastSource = owner.transform.position;
        while (tracking == true)
        {
            Vector3 targetPosition = trackedTarget.transform.position;
            Vector3Int cleanTarget = Vector3Int.RoundToInt(targetPosition);
            ActiveCast = Ability.SimulateCast(targetPosition);
            ability.PhysicalAimAlongTrajectory(ActiveCast.Trajectory);
            if (ability.range > 0 && GetTargetQuality(targetPosition, ActiveCast.Trajectory) > targetOffsetTolerance)
            {
                if (cleanTarget != lastCastTarget && FindValidCast(targetPosition, out PossibleCast validCast, PrimaryCursor.TargetedBot))
                {
                    lastCastSource = validCast.Source;
                    Vector3 facing = targetPosition - validCast.Source;
                    PrimaryCursor.Instance.GenerateMovePreview(Vector3Int.RoundToInt(validCast.Source), facing);
                    //owner.EchoMap[ability].PhysicalAimAlongTrajectory(PossibleCast.Trajectory);
                    ActiveCast = validCast;
                }
                else ActiveCast = Ability.SimulateCast(targetPosition, lastCastSource);
                lastCastTarget = cleanTarget;
                
            }
            else PrimaryCursor.InvalidatePath();
            DrawPlayerTargeting(ActiveCast);
            CastOutcomeIndicator.Show(ActiveCast.Trajectory[^1], CastIsValid());
            yield return null;
        }
    }

    readonly float targetOffsetTolerance = .5f;
    public bool FindValidCast(Vector3 targetPosition, out PossibleCast cast, Targetable snapTarget = null)
    {
        cast = default;
        foreach (Vector3Int pathablePoint in pathableLocations)
        {
            if (Vector3.Distance(pathablePoint, targetPosition) > Ability.TotalRange) continue;

            PossibleCast possibleCast = Ability.SimulateCast(targetPosition, pathablePoint, false);
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

    float GetTargetQuality(Vector3 position, List<Vector3> trajectory)
    {
        float offset = Vector3.Distance(position, trajectory[^1]) - Ability.TargetType.AddedRange;
        return Mathf.Clamp(offset, 0, float.MaxValue);
    }

    void DrawPlayerTargeting(PossibleCast cast)
    {
        if (cast.Trajectory != null && cast.Trajectory.Count > 0) LineMaker.DrawLine(cast.Trajectory.ToArray());
        Ability.TargetType.Draw(cast.Trajectory);
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
        Ability.TargetType.Hide();
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
        return Ability.IsCastValid(ActiveCast);
    }
}

public class PossibleCast
{
    public Vector3 Source;
    public List<Vector3> Trajectory;
    public List<Targetable> Targets;
    public bool Hit;
}
