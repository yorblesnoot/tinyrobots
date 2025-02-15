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
    protected PossibleCast PossibleCast;
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
        return true;
    }

    public void CastLoadedSkill()
    {
        StartCoroutine(CastSequence(PossibleCast));
    }

    void Cancel()
    {
        if(Ability == null) return;
        tracking = false;
        HidePlayerTargeting();
        Ability = null;
    }

    readonly float finalizeAimDuration = 1.0f;
    public IEnumerator CastSequence()
    {
        yield return CastSequence(PossibleCast);
    }

    IEnumerator CastSequence(PossibleCast cast)
    {
        EndTracking();
        PrimaryCursor.TogglePlayerLockout(true);
        float timeElapsed = 0;
        while(timeElapsed < finalizeAimDuration)
        {
            Ability.PhysicalAimAlongTrajectory(cast.Trajectory);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
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
            PossibleCast = Ability.SimulateCast(targetPosition);
            ability.PhysicalAimAlongTrajectory(PossibleCast.Trajectory);
            ActiveAbility aimer;
            if (GetTargetQuality(targetPosition, PossibleCast.Trajectory) > targetOffsetTolerance)
            {
                if(cleanTarget == lastCastTarget)
                {
                    PossibleCast = Ability.SimulateCast(targetPosition, lastCastSource);
                }
                else if (FindValidCast(targetPosition, out PossibleCast validCast))
                {
                    lastCastSource = validCast.Source;
                    Vector3 facing = targetPosition - validCast.Source;
                    PrimaryCursor.Instance.GenerateMovePreview(Vector3Int.RoundToInt(validCast.Source), facing);
                    PossibleCast = validCast;
                }
                lastCastTarget = cleanTarget;
            }
            else PrimaryCursor.InvalidatePath();
            DrawPlayerTargeting(PossibleCast);
            yield return null;
        }
    }

    readonly float targetOffsetTolerance = .5f;
    bool FindValidCast(Vector3 targetPosition, out PossibleCast cast)
    {
        cast = default;
        foreach (Vector3Int pathablePoint in pathableLocations)
        {
            if (Vector3.Distance(pathablePoint, targetPosition) > Ability.TotalRange) continue;

            PossibleCast possibleCast = Ability.SimulateCast(targetPosition, pathablePoint, false);
            float quality = GetTargetQuality(targetPosition, possibleCast.Trajectory);
            if (quality <= targetOffsetTolerance)
            {
                cast = possibleCast;
                return true;
            }
        }
        return false;
    }

    float GetTargetQuality(Vector3 position, List<Vector3> trajectory)
    {
        float offset = Vector3.Distance(position, trajectory[^1]) - Ability.TargetType.TargetRadius;
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
            bot.SetOutlineColor(Color.red);
        }
    }

    public bool IsCastValid()
    {
        return Ability.IsCastValid(PossibleCast);
    }
}

public struct PossibleCast
{
    public Vector3 Source;
    public List<Vector3> Trajectory;
    public List<Targetable> Targets;
    public bool Hit;
}
