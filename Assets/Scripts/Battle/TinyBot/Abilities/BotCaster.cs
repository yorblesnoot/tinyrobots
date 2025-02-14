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
        
        while (tracking == true)
        {
            Vector3 targetPosition = trackedTarget.transform.position;
            Vector3Int cleanTarget = Vector3Int.RoundToInt(targetPosition);
            SetActiveCast(targetPosition, ability.emissionPoint.position, false);
            ActiveAbility aimer = ability;
            if (GetTargetQuality(targetPosition, PossibleCast.Trajectory) > targetOffsetTolerance)
            {
                if (cleanTarget != lastValidCastSource)
                {
                    Vector3Int alternateCastSource = FindClosestCastingPosition(targetPosition);
                    if (alternateCastSource == default) alternateCastSource = lastValidCastSource;
                    else lastValidCastSource = alternateCastSource;
                    Vector3 facing = targetPosition - alternateCastSource;
                    PrimaryCursor.Instance.GenerateMovePreview(alternateCastSource, facing);
                }
                aimer = owner.EchoMap[ability];
                SetActiveCast(targetPosition, aimer.emissionPoint.position, false);
            }
            else PrimaryCursor.InvalidatePath();
            ability.PhysicalAimAlongTrajectory(PossibleCast.Trajectory);
            DrawPlayerTargeting(PossibleCast);
            yield return null;
        }
    }

    public void SetActiveCast(Vector3 targetPosition, Vector3 sourcePosition, bool imaginary = false)
    {
        PossibleCast = Ability.SimulateCast(targetPosition, sourcePosition, imaginary);
    }

    readonly float targetOffsetTolerance = .5f;
    Vector3Int lastValidCastSource;
    Vector3Int FindClosestCastingPosition(Vector3 targetPosition)
    {
        PriorityQueue<Vector3Int, float> priority = new();
        Vector3Int position = FindCastingPosition(targetPosition, priority);
        if (position == default && priority.Count > 0) position = priority.Dequeue();
        return position;
    }

    public Vector3Int FindCastingPosition(Vector3 targetPosition, PriorityQueue<Vector3Int, float> priority = null)
    {
        Vector3Int position = default;
        foreach (Vector3Int pathablePoint in pathableLocations)
        {
            if (Vector3.Distance(pathablePoint, targetPosition) > Ability.TotalRange) continue;
            Vector3 emissionLocation = GunPositionAt(Ability, pathablePoint, targetPosition - pathablePoint);
            PossibleCast cast = Ability.SimulateCast(targetPosition, emissionLocation, false);
            float quality = GetTargetQuality(targetPosition, cast.Trajectory);
            if (quality <= targetOffsetTolerance)
            {
                position = pathablePoint;
                break;
            }
            priority?.Enqueue(pathablePoint, quality);
        }
        return position;
    }

    Vector3 GunPositionAt(ActiveAbility ability, Vector3 position, Vector3 facing)
    {
        Quaternion locationRotation = owner.PrimaryMovement.GetRotationFromFacing(position, facing);
        Vector3 gunPosition = ability.emissionPoint.position;
        Vector3 localGun = owner.transform.InverseTransformPoint(gunPosition);
        Vector3 rotatedGun = locationRotation * localGun;
        return position + rotatedGun;
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
    public List<Vector3> Trajectory;
    public List<Targetable> Targets;
    public bool Hit;
}
