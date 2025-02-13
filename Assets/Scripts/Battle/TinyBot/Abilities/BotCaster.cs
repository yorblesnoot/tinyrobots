using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Playables;
using UnityEngine;
using UnityEngine.Events;

public class BotCaster : MonoBehaviour
{
    TinyBot owner;
    bool tracking;
    protected ActiveAbility Ability;
    protected PossibleCast PossibleCast;
    [HideInInspector] public static UnityEvent ResetHighlights = new();

    private void Awake()
    {
        owner = GetComponent<TinyBot>();
    }

    public void Prepare(ActiveAbility ability)
    {
        Ability = ability;
        if (owner.Allegiance == Allegiance.PLAYER)
            StartCoroutine(PlayerAimAbility(ability, PrimaryCursor.Transform.gameObject));
    }

    public void Confirm()
    {
        StartCoroutine(CastSequence(PossibleCast));
    }

    public void Cancel()
    {
        tracking = false;
        HidePlayerTargeting();
    }

    readonly float finalizeAimDuration = 1.0f;
    IEnumerator CastSequence(PossibleCast cast)
    {
        tracking = false;
        HidePlayerTargeting();
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
        ClickableAbility.PlayerUsedAbility?.Invoke();
        if (Vector3Int.RoundToInt(owner.transform.position) != startPosition) Pathfinder3D.GeneratePathingTree(owner.MoveStyle, owner.transform.position);
        if (Ability.EndTurn) yield return new WaitForSeconds(1);
        PrimaryCursor.TogglePlayerLockout(false);
        Cancel();
        if (Ability.EndTurn) TurnManager.EndTurn(owner);
    }

    IEnumerator PlayerAimAbility(ActiveAbility ability, GameObject trackedTarget)
    {
        tracking = true;
        while (tracking == true)
        {
            Vector3 targetPosition = trackedTarget.transform.position;
            Vector3Int cleanTarget = Vector3Int.RoundToInt(targetPosition);
            PossibleCast = ability.SimulateCast(targetPosition, ability.emissionPoint.position, false);
            ActiveAbility aimer = ability;
            if (GetTargetQuality(targetPosition, PossibleCast.Trajectory) > targetOffsetTolerance)
            {
                if (cleanTarget != lastValidCastSource)
                {
                    Vector3Int alternateCastSource = EvaluateAlternateCastPositions(targetPosition);
                    if (alternateCastSource == default) alternateCastSource = lastValidCastSource;
                    else lastValidCastSource = alternateCastSource;
                    Vector3 facing = targetPosition - alternateCastSource;
                    PrimaryCursor.Instance.GenerateMovePreview(alternateCastSource, facing);
                }
                aimer = owner.EchoMap[ability];
                PossibleCast = aimer.SimulateCast(targetPosition, aimer.emissionPoint.position, false);
            }
            else PrimaryCursor.InvalidatePath();
            aimer.PhysicalAimAlongTrajectory(PossibleCast.Trajectory);
            DrawPlayerTargeting(PossibleCast);
            yield return null;
        }
    }

    readonly float targetOffsetTolerance = .1f;
    Vector3Int lastValidCastSource;
    Vector3Int EvaluateAlternateCastPositions(Vector3 targetPosition)
    {
        List<Vector3Int> pathableLocations = Pathfinder3D.GetPathableLocations(owner.Stats.Current[StatType.MOVEMENT])
            .Where(position => Vector3.Distance(position, targetPosition) <= Ability.TotalRange)
            .OrderBy(position => Vector3.Distance(position, owner.transform.position))
            .ToList();
        PriorityQueue<Vector3Int, float> priority = new();
        Vector3Int position = default;
        foreach (Vector3Int location in pathableLocations)
        {
            PossibleCast cast = Ability.SimulateCast(targetPosition, location, true);
            float quality = GetTargetQuality(targetPosition, cast.Trajectory);
            if (quality == 0)
            {
                position = location;
                break;
            }
            priority.Enqueue(location, quality);
        }
        if (position == default && pathableLocations.Count > 0) position = priority.Dequeue();
        return position;
    }

    

    public float GetTargetQuality(Vector3 position, List<Vector3> trajectory)
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
    public List<Vector3> Trajectory;
    public List<Targetable> Targets;
    public bool Hit;
}
