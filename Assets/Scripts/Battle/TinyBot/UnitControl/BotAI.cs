using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using static UnityEngine.UI.GridLayoutGroup;

public class BotAI
{
    public static readonly float terrainCheckSize = 1.1f;
    readonly TinyBot owner;
    readonly float optimalDistance;
    float RemainingMove => owner.Stats.Current[StatType.MOVEMENT];

    List<ActiveAbility> primaryAbilities, movementAbilities, defensiveAbilities;

    List<TinyBot> enemies = new();
    List<TinyBot> allies = new();
    Vector3 closestEnemyPosition;
    ActiveAbility reservedSkill;
    #region Initialization
    public BotAI(TinyBot bot)
    {
        owner = bot;
        optimalDistance = FindOptimalDistance();
        owner.AbilitiesChanged.AddListener(BuildAbilityLists);
        BuildAbilityLists();
    }
    void BuildAbilityLists()
    {
        primaryAbilities = new();
        movementAbilities = new();
        defensiveAbilities = new();
        foreach (ActiveAbility ability in owner.ActiveAbilities)
        {   
            if (ability.Type == AbilityType.SHIELD) defensiveAbilities.Add(ability);
            else if (ability.Type == AbilityType.DASH) movementAbilities.Add(ability);
            else primaryAbilities.Add(ability);
        }
    }
    float FindOptimalDistance()
    {
        List<Ability> abilities = new(owner.ActiveAbilities.Where(skill => skill.AIPriority >= 0));
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
        yield return CastPhase();
        yield return MovePhase();
        yield return EndPhase();
        yield return new WaitForSeconds(1);
        EndTurn();
    }

    void ShufflePools()
    {
        primaryAbilities = ShufflePool(primaryAbilities);
        movementAbilities = ShufflePool(movementAbilities);
        defensiveAbilities = ShufflePool(defensiveAbilities);

        List<ActiveAbility> ShufflePool(List<ActiveAbility> pool)
        {
            pool.Shuffle();
            return pool.OrderByDescending(ability => ability.AIPriority).ToList();
        }
    }

    void UpdateTargetingData()
    {
        enemies = new();
        allies = new();
        foreach (TinyBot bot in TurnManager.TurnTakers)
        {
            if (bot.Allegiance == owner.Allegiance) allies.Add(bot);
            else enemies.Add(bot);
        }
        enemies = enemies.OrderBy(unit => Vector3.Distance(unit.transform.position, owner.transform.position)).ToList();
        closestEnemyPosition = enemies[0].transform.position;
    }

    IEnumerator CastPhase()
    {
        DebugAction($"entered Cast Phase.");
        primaryAbilities.DebugContents();
        primaryAbilities.Select(a => a.AIPriority).DebugContents();
        UpdateTargetingData();
        foreach (var ability in primaryAbilities)
        {
            if (!owner.Caster.TryPrepare(ability)) continue;
            if (ability.EndTurn)
            {
                if (reservedSkill == null) ReserveSkill(ability);
                continue;
            }
            yield return SelectPhase(ability);
        }
        yield return DashPhase(movementAbilities);
    }

    void ReserveSkill(ActiveAbility ability)
    {
        reservedSkill = ability;
        owner.SpendResource(ability.cost, ability.CastingResource);
    }

    IEnumerator EndPhase()
    {
        if(reservedSkill == null) yield break;
        owner.SpendResource(-reservedSkill.cost, reservedSkill.CastingResource);
        owner.Caster.TryPrepare(reservedSkill);
        SelectPhase(reservedSkill);
    }

    private IEnumerator SelectPhase(ActiveAbility ability)
    {
        if (ability.Type == AbilityType.SUMMON) yield return SummonPhase(ability);
        else if (ability.Type == AbilityType.SHIELD) yield return ShieldPhase();
        else yield return AttackPhase(ability);
    }

    IEnumerator SummonPhase(ActiveAbility ability)
    {
        DebugAction($"entered Summon Phase with {ability}.");
        yield return MovePhase();
        List<Vector3Int> summonLocations = GetDashLocations(ability);
        foreach (Vector3 location in summonLocations)
        {
            PossibleCast cast = owner.Caster.SimulatePossibleCast(location, true, owner.transform.position);
            if (cast == null) continue;
            yield return UseAbility(ability, cast);
            yield return CastPhase();
            yield break;
        }
    }

    IEnumerator AttackPhase(ActiveAbility ability)
    {
        DebugAction($"entered Attack Phase with {ability}.");
        List<TinyBot> targets = new(ability.Type == AbilityType.ATTACK ? enemies : allies);
        foreach (var target in targets)
        {
            Vector3 targetPoint = target.TargetPoint.position;
            if (!owner.Caster.FindValidCast(targetPoint, out var cast, true, target)) continue;
            yield return UseAbility(ability, cast);
            yield return CastPhase();
            yield break;
        }
    }

    IEnumerator DashPhase(List<ActiveAbility> abilities)
    {
        DebugAction($"entered Dash Phase.");
        UpdateTargetingData();
        foreach (var ability in abilities)
        {
            if (!owner.Caster.TryPrepare(ability)) continue;
            DebugAction($"simulated {ability.name}.");
            List<Vector3> dashLocations = GetDashLocations(ability).Select(l => (Vector3)l).ToList();
            if(ability.range == 0) dashLocations.Add(owner.transform.position);
            foreach (Vector3 location in dashLocations)
            {
                PossibleCast cast = owner.Caster.SimulatePossibleCast(location, true, owner.transform.position);
                if (cast == null) continue;
                yield return UseAbility(ability, cast);
                yield return CastPhase();
                yield break;
            }
        }
    }

    

    IEnumerator MovePhase()
    {
        DebugAction($"entered Move Phase.");
        UpdateTargetingData();
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
    }

    IEnumerator ShieldPhase()
    {
        DebugAction($"entered Shield Phase.");
        UpdateTargetingData();
        foreach (var shield in defensiveAbilities)
        {
            if (!owner.Caster.TryPrepare(shield)) continue;
            Vector3 myPosition = owner.transform.position;
            List<Vector3> averages = new() {enemies.Select(x => x.TargetPoint.position - myPosition).ToList().Average(),
                Pathfinder3D.GetCrawlOrientation(owner.transform.position) };
            if (allies.Count > 0) averages.Add(-allies.Select(x => x.TargetPoint.position - myPosition).ToList().Average());
            Vector3 finalDirection = averages.Average();
            Vector3 finalPosition = myPosition + finalDirection;
            PossibleCast cast = owner.Caster.SimulatePossibleCast(finalPosition, false, owner.transform.position);
            if (cast == null) continue;

            yield return UseAbility(shield, cast);
        }
    }

    void DebugAction(string words)
    {
        Debug.Log($"{owner.name} {words}");
    }

    #region Unit Actions
    void BeginTurn()
    {
        PrimaryCursor.TogglePlayerLockout(true);
        Pathfinder3D.GeneratePathingTree(owner.MoveStyle, owner.transform.position);
        ShufflePools();
        reservedSkill = null;
    }
    void EndTurn()
    {
        PrimaryCursor.TogglePlayerLockout(true);
        TurnManager.EndTurn(owner);
    }

    IEnumerator PathToCastingPosition(Vector3Int location)
    {
        if (Vector3Int.RoundToInt(owner.transform.position) == location) yield break;

        List<Vector3> path = Pathfinder3D.FindVectorPath(location, out var moveCosts);
        owner.SpendResource(Mathf.RoundToInt(moveCosts[^1]), StatType.MOVEMENT);
        yield return owner.StartCoroutine(owner.Movement.TraversePath(path));
        Pathfinder3D.GeneratePathingTree(owner.MoveStyle, owner.transform.position);
        MainCameraControl.ReleaseTracking();
    }

    readonly float skillDelay = .5f;
    private IEnumerator UseAbility(ActiveAbility ability, PossibleCast cast)
    {
        DebugAction($"used {ability.name}.");
        owner.Caster.ActiveCast = cast;
        yield return PathToCastingPosition(Vector3Int.RoundToInt(cast.Source));
        yield return owner.Caster.CastActiveAbility();
        yield return new WaitForSeconds(skillDelay);
        if (ability.EndTurn) owner.StopAllCoroutines();
    }
    #endregion

    #region Decision Tools
    private List<Vector3Int> GetDashLocations(ActiveAbility ability)
    {

        float maxDash = ability.range;

        List<Vector3Int> dashLocations = Pathfinder3D.GetDashTargets(owner.transform.position, maxDash, owner.Movement.Style);
        if (owner.Movement.Style == MoveStyle.WALK)
        {
            dashLocations = Pathfinder3D.FilterByAccessToCastPosition(Vector3Int.RoundToInt(closestEnemyPosition), optimalDistance, dashLocations);
        }
        else
        {
            dashLocations = dashLocations.Where(location => Vector3.Distance(location, owner.transform.position) > ability.range / 2).ToList();
        }

        dashLocations = dashLocations.OrderBy(DistanceFromOptimalRange(closestEnemyPosition, optimalDistance)).ToList();
        return dashLocations;
    }

    Func<Vector3Int, float> DistanceFromOptimalRange(Vector3 closestEnemyPosition, float optimalDistance)
    {
        return location => Mathf.Abs(Vector3.Distance(location, closestEnemyPosition) - optimalDistance);
    }
    #endregion
}
