using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using static UnityEditor.FilePathAttribute;

public class BotAI
{
    public static readonly float terrainCheckSize = 1.1f;
    readonly TinyBot thisBot;
    readonly int terrainMask;
    readonly float optimalDistance;
    readonly GameObject pointer;
    readonly float lockTime = 1f;
    
    List<ActiveAbility> primaries, dashes, shields;
    #region Initialization
    public BotAI(TinyBot bot)
    {
        thisBot = bot;
        pointer = new("AIPointer");
        terrainMask = LayerMask.GetMask("Terrain");
        optimalDistance = FindOptimalDistance();
        BuildAbilityLists();
    }
    void BuildAbilityLists()
    {
        primaries = new();
        dashes = new();
        shields = new();
        foreach (ActiveAbility ability in thisBot.ActiveAbilities)
        {
            if (ability.Type == AbilityType.DASH) dashes.Add(ability);
            else if (ability.Type == AbilityType.SHIELD) shields.Add(ability);
            else primaries.Add(ability);
        }
    }
    float FindOptimalDistance()
    {
        List<ActiveAbility> rangedAttacks = thisBot.ActiveAbilities.Where(skill => skill.range > 0 && skill.Type == AbilityType.ATTACK).ToList();
        if(rangedAttacks.Count == 0)
        {
            return 10;
        }
        float total = rangedAttacks.Sum(skill => skill.range);
        return total / rangedAttacks.Count;
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
            if(bot.Allegiance == thisBot.Allegiance) allies.Add(bot);
            else enemies.Add(bot);
        }
        enemies = enemies.OrderBy(unit => Vector3.Distance(unit.transform.position, thisBot.transform.position)).ToList();
        closestEnemyPosition = enemies[0].transform.position;

        yield return AttackPhase();
        
        yield return new WaitForSeconds(1);
        EndTurn();

        IEnumerator AttackPhase()
        {
            primaries.Shuffle();
            foreach (var ability in primaries)
            {
                thisBot.Caster.Prepare(ability);
                if (AbilityIsUnavailable(ability)) continue;
                List<TinyBot> targets = new(ability.Type == AbilityType.ATTACK ? enemies : allies);
                foreach(var target in targets)
                {
                    Vector3 targetPoint = target.TargetPoint.position;
                    Vector3Int origin = thisBot.Caster.FindCastingPosition(targetPoint);
                    thisBot.Caster.SetActiveCast(targetPoint, origin, true);
                    if (origin == default) continue;
                    yield return PathToCastingPosition(origin);

                    //lock on and use ability
                    yield return UseAbility(ability);
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
                if (AbilityIsUnavailable(ability)) continue;

                List<Vector3Int> dashLocations = Pathfinder3D.GetDashTargets(thisBot.transform.position, 
                ability.range, thisBot.PrimaryMovement.Style).OrderBy(DistanceFromOptimalRange(closestEnemyPosition)).ToList();
                if(thisBot.PrimaryMovement.Style == MoveStyle.WALK)
                {
                    dashLocations = Pathfinder3D.FilterByWalkAccessible(Vector3Int.RoundToInt(closestEnemyPosition), optimalDistance, dashLocations)
                        .OrderByDescending(p => Vector3.Distance(thisBot.transform.position, p)).ToList();
                }

                foreach (Vector3Int location in dashLocations)
                {
                    if (!DashCanReach(ability, location)) continue;

                    yield return UseAbility(ability); //TODO: FIX
                    yield return AttackPhase();
                    yield break;
                }
            }
            yield return MovePhase();
        }

        IEnumerator MovePhase()
        {
            //this is a problem
            float remainingMove = thisBot.Stats.Current[StatType.MOVEMENT];
            List<Vector3Int> pathableLocations = Pathfinder3D.GetPathableLocations();
            List<Vector3Int> goodLocations = pathableLocations.Where(IsWithinOptimalRange(closestEnemyPosition)).ToList();
            if (goodLocations.Count > 0) pathableLocations = goodLocations;
            pathableLocations = pathableLocations.OrderBy(DistanceFromOptimalRange(closestEnemyPosition)).ToList();

            List<Vector3> path = Pathfinder3D.FindVectorPath(pathableLocations[0], out var moveCosts);
            if (path == null) yield break;

            float maxMove = moveCosts.Where(cost => cost < remainingMove).LastOrDefault();
            int endIndex = moveCosts.IndexOf(maxMove);
            path = path.Take(endIndex).ToList();
            if (path.Count == 0) yield break;

            path = thisBot.PrimaryMovement.SanitizePath(path);
            thisBot.SpendResource(Mathf.RoundToInt(moveCosts[endIndex]), StatType.MOVEMENT);
            
            yield return thisBot.StartCoroutine(thisBot.PrimaryMovement.TraversePath(path));
            yield return ShieldPhase();
        }

        IEnumerator ShieldPhase()
        {
            shields.Shuffle();
            if(shields.Count == 0) yield break;
            ActiveAbility shield = shields[0];
            if(AbilityIsUnavailable(shield)) yield break;
            Vector3 myPosition = thisBot.transform.position;
            List<Vector3> averages = new() {enemies.Select(x => x.TargetPoint.position - myPosition).ToList().Average(), 
                Pathfinder3D.GetCrawlOrientation(thisBot.transform.position) };
            if (allies.Count > 0) averages.Add(-allies.Select(x => x.TargetPoint.position - myPosition).ToList().Average());
            Vector3 finalDirection = averages.Average();
            Vector3 finalPosition = myPosition + finalDirection;
            pointer.transform.position = finalPosition;
            yield return UseAbility(shield); //TODO: FIX
        }
    }
    
    #region Unit Actions
    void BeginTurn()
    {
        PrimaryCursor.TogglePlayerLockout(true);
        Pathfinder3D.GeneratePathingTree(thisBot.MoveStyle, thisBot.transform.position);
    }
    void EndTurn()
    {
        thisBot.ToggleActiveLayer(false);
        PrimaryCursor.TogglePlayerLockout(true);
        TurnManager.EndTurn(thisBot);
    }

    IEnumerator PathToCastingPosition(Vector3Int location)
    {
        if (Vector3Int.RoundToInt(thisBot.transform.position) != location)
        {
            MainCameraControl.TrackTarget(thisBot.TargetPoint);
            List<Vector3> path = Pathfinder3D.FindVectorPath(location, out var moveCosts);
            if(path == null || path.Count == 0) yield break;
            thisBot.SpendResource(Mathf.RoundToInt(moveCosts[^1]), StatType.MOVEMENT);
            yield return thisBot.StartCoroutine(thisBot.PrimaryMovement.TraversePath(path));
            Pathfinder3D.GeneratePathingTree(thisBot.MoveStyle, thisBot.transform.position);
            MainCameraControl.ReleaseTracking();
        }
    }
    private IEnumerator UseAbility(ActiveAbility ability)
    {
        yield return thisBot.Caster.CastSequence();
        if (ability.EndTurn) thisBot.StopAllCoroutines();
    }
    #endregion

    #region Decision Tools
    
    bool DashCanReach(ActiveAbility ability, Vector3Int position)
    {
        Debug.DrawRay(position, Vector3.up, Color.yellow, 10f);
        pointer.transform.position = position;
        ability.SimulateCast(position, thisBot.transform.position, true);
        //if (ability.IsUsable()) return true; //TODO: FIX
        return false;
    }
    private bool AbilityIsUnavailable(ActiveAbility ability)
    {
        return ability.cost > thisBot.Stats.Current[StatType.ACTION] || !ability.IsAvailable();
    }
    
    Func<Vector3Int, float> DistanceFromOptimalRange(Vector3 closestEnemyPosition)
    {
        return location => Mathf.Abs(Vector3.Distance(location, closestEnemyPosition) - optimalDistance);
    }

    Func<Vector3Int, bool> IsWithinOptimalRange(Vector3 closestEnemyPosition)
    {
        return location => Vector3.Distance(location, closestEnemyPosition) <= optimalDistance;
    }
    #endregion
}
