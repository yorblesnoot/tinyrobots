using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class BotAI
{
    public static readonly float terrainCheckSize = 1.1f;
    readonly TinyBot owner;
    readonly float optimalDistance;
    float RemainingMove => owner.Stats.Current[StatType.MOVEMENT];
    
    List<ActiveAbility> primaries, dashes, shields;
    #region Initialization
    public BotAI(TinyBot bot)
    {
        owner = bot;
        optimalDistance = FindOptimalDistance();
        BuildAbilityLists();
    }
    void BuildAbilityLists()
    {
        primaries = new();
        dashes = new();
        shields = new();
        foreach (ActiveAbility ability in owner.ActiveAbilities)
        {
            if (ability.Type == AbilityType.DASH) dashes.Add(ability);
            else if (ability.Type == AbilityType.SHIELD) shields.Add(ability);
            else primaries.Add(ability);
        }
    }
    float FindOptimalDistance()
    {
        List<Ability> abilities = new(owner.ActiveAbilities);
        abilities.AddRange(owner.PassiveAbilities);
        abilities = abilities.Where(skill => skill.range > 0).ToList();
        if(abilities.Count == 0)
        {
            return 10;
        }
        float total = abilities.Sum(skill => skill.range);
        return total / abilities.Count;
    }

    #endregion

    public IEnumerator TakeTurn()
    {

        BeginTurn();
        List<TinyBot> enemies = new();
        List<TinyBot> allies = new();
        Vector3 closestEnemyPosition;
        foreach (TinyBot bot in TurnManager.TurnTakers)
        {
            if(bot.Allegiance == owner.Allegiance) allies.Add(bot);
            else enemies.Add(bot);
        }
        enemies = enemies.OrderBy(unit => Vector3.Distance(unit.transform.position, owner.transform.position)).ToList();
        closestEnemyPosition = enemies[0].transform.position;

        yield return AttackPhase();
        
        yield return new WaitForSeconds(1);
        EndTurn();

        IEnumerator AttackPhase()
        {
            primaries.Shuffle();
            foreach (var ability in primaries)
            {
                if (!owner.Caster.TryPrepare(ability)) continue;
                List<TinyBot> targets = new(ability.Type == AbilityType.ATTACK ? enemies : allies);
                foreach(var target in targets)
                {
                    Vector3 targetPoint = target.TargetPoint.position;
                    if (!owner.Caster.FindValidCast(targetPoint, out var cast, target)) continue;
                    yield return UseAbility(ability, cast);
                    yield return AttackPhase();
                    yield break;
                }
            }
            yield return DashPhase();
        }

        IEnumerator DashPhase()
        {
            foreach (var ability in dashes)
            {
                if (!owner.Caster.TryPrepare(ability)) continue;
                float maxDash = ability.range + RemainingMove;

                List<Vector3Int> dashLocations = Pathfinder3D.GetDashTargets(owner.transform.position, maxDash, owner.Movement.Style);
                if(owner.Movement.Style == MoveStyle.WALK)
                {
                    dashLocations = Pathfinder3D.FilterByAccessToCastPosition(Vector3Int.RoundToInt(closestEnemyPosition), optimalDistance, dashLocations);
                }
                else
                {
                    dashLocations = dashLocations.Where(location => Vector3.Distance(location, owner.transform.position) > ability.range/2).ToList();
                }

                dashLocations = dashLocations.OrderBy(DistanceFromOptimalRange(closestEnemyPosition, optimalDistance)).ToList();

                foreach (Vector3Int location in dashLocations)
                {
                    if (!owner.Caster.FindValidCast(location, out var cast)) continue;
                    yield return UseAbility(ability, cast);
                    yield return AttackPhase();
                    yield break;
                }
            }
            yield return MovePhase();
        }

        IEnumerator MovePhase()
        {
            //this is a problem
            List<Vector3Int> pathableLocations = Pathfinder3D.GetPathableLocations();
            pathableLocations = pathableLocations.OrderBy(DistanceFromOptimalRange(closestEnemyPosition, optimalDistance)).ToList();

            List<Vector3> path = Pathfinder3D.FindVectorPath(pathableLocations[0], out var moveCosts);
            if (path == null) yield break;

            float maxMove = moveCosts.Where(cost => cost < RemainingMove).LastOrDefault();
            int endIndex = moveCosts.IndexOf(maxMove);
            path = path.Take(endIndex).ToList();
            if (path.Count == 0) yield break;

            path = owner.Movement.SanitizePath(path);
            owner.SpendResource(Mathf.RoundToInt(moveCosts[endIndex]), StatType.MOVEMENT);
            
            yield return owner.StartCoroutine(owner.Movement.TraversePath(path));
            yield return ShieldPhase();
        }

        IEnumerator ShieldPhase()
        {
            shields.Shuffle();
            foreach (var shield in shields)
            {
                if (!owner.Caster.TryPrepare(shield)) continue;
                Vector3 myPosition = owner.transform.position;
                List<Vector3> averages = new() {enemies.Select(x => x.TargetPoint.position - myPosition).ToList().Average(),
                Pathfinder3D.GetCrawlOrientation(owner.transform.position) };
                if (allies.Count > 0) averages.Add(-allies.Select(x => x.TargetPoint.position - myPosition).ToList().Average());
                Vector3 finalDirection = averages.Average();
                Vector3 finalPosition = myPosition + finalDirection;
                owner.Caster.FindValidCast(finalPosition, out var cast);
                yield return UseAbility(shield, cast); 
            }
            
        }
    }
    
    #region Unit Actions
    void BeginTurn()
    {
        PrimaryCursor.TogglePlayerLockout(true);
        Pathfinder3D.GeneratePathingTree(owner.MoveStyle, owner.transform.position);
    }
    void EndTurn()
    {
        owner.ToggleActiveLayer(false);
        PrimaryCursor.TogglePlayerLockout(true);
        TurnManager.EndTurn(owner);
    }

    IEnumerator PathToCastingPosition(Vector3Int location)
    {
        if (Vector3Int.RoundToInt(owner.transform.position) == location) yield break;

        MainCameraControl.TrackTarget(owner.TargetPoint);
        List<Vector3> path = Pathfinder3D.FindVectorPath(location, out var moveCosts);
        owner.SpendResource(Mathf.RoundToInt(moveCosts[^1]), StatType.MOVEMENT);
        yield return owner.StartCoroutine(owner.Movement.TraversePath(path));
        Pathfinder3D.GeneratePathingTree(owner.MoveStyle, owner.transform.position);
        MainCameraControl.ReleaseTracking();
    }
    private IEnumerator UseAbility(ActiveAbility ability, PossibleCast cast)
    {
        owner.Caster.ActiveCast = cast;
        yield return PathToCastingPosition(Vector3Int.RoundToInt(cast.Source));
        yield return owner.Caster.CastSequence();
        if (ability.EndTurn) owner.StopAllCoroutines();
    }
    #endregion

    #region Decision Tools
    
    Func<Vector3Int, float> DistanceFromOptimalRange(Vector3 closestEnemyPosition, float optimalDistance)
    {
        return location => Mathf.Abs(Vector3.Distance(location, closestEnemyPosition) - optimalDistance);
    }
    #endregion
}
