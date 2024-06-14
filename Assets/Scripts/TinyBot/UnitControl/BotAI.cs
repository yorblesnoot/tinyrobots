using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class BotAI
{
    readonly TinyBot thisBot;
    readonly int terrainMask;
    readonly float optimalDistance;
    readonly GameObject pointer;

    public BotAI(TinyBot bot)
    {
        thisBot = bot;
        pointer = new("AIPointer");
        terrainMask = LayerMask.GetMask("Terrain");
        optimalDistance = FindOptimalDistance();
    }

    float FindOptimalDistance()
    {
        List<Ability> rangedAttacks = thisBot.Abilities.Where(skill => skill.range > 0 && skill.Type == AbilityType.ATTACK).ToList();
        if(rangedAttacks.Count == 0)
        {
            return 10;
        }
        float total = rangedAttacks.Sum(skill => skill.range);
        return total / rangedAttacks.Count;
    }

    readonly float lockTime = 1f;
    public static readonly float terrainCheckSize = 1.1f;
    public IEnumerator TakeTurn()
    {
        BeginTurn();
        List<Ability> possibleAbilities = new(thisBot.Abilities);
        List<TinyBot> enemies = new();
        List<TinyBot> allies = new();
        foreach (TinyBot bot in TurnManager.TurnTakers)
        {
            if(bot.allegiance == thisBot.allegiance) allies.Add(bot);
            else enemies.Add(bot);
        }
        enemies = enemies.OrderBy(unit => Vector3.Distance(unit.transform.position, thisBot.transform.position)).ToList();
        Vector3 closestEnemyPosition = enemies[0].transform.position;


        while (possibleAbilities.Count > 0)
        {
            int abilityIndex = UnityEngine.Random.Range(0, possibleAbilities.Count);
            yield return thisBot.StartCoroutine(FindUseForAbility(possibleAbilities[abilityIndex]));
            possibleAbilities = possibleAbilities
                .Where(ability => (ability.cost <= thisBot.Stats.Current[StatType.ACTION]) && ability.currentCooldown == 0)
                .ToList();
        }

        yield return thisBot.StartCoroutine(MoveFreely());
        yield return new WaitForSeconds(1);
        EndTurn();

        IEnumerator FindUseForAbility(Ability ability)
        {
            if(ability.Type == AbilityType.DASH)
            {
                List<Vector3Int> dashLocations = Pathfinder3D.GetCompatibleLocations(thisBot.transform.position, ability.range, thisBot.PrimaryMovement.Style)
                    .Where(x =>
                    {
                        float distance = Vector3.Distance(thisBot.transform.position, x);
                        return distance > ability.range / 2 && distance < ability.range;
                    }).OrderBy(DistanceFromOptimalRange()).ToList();
                foreach (Vector3Int location in dashLocations)
                {
                    if(DashCanReach(ability, location))
                    {
                        yield return UseAbility(ability, pointer);
                        yield break;
                    }
                }
            }
            else if(ability.Type == AbilityType.ATTACK || ability.Type == AbilityType.BUFF)
            {
                List<Vector3Int> pathableLocations = Pathfinder3D.GetPathableLocations(Mathf.FloorToInt(thisBot.Stats.Current[StatType.MOVEMENT]));
                foreach (Vector3Int location in pathableLocations)
                {
                    //check if the ability has a valid target from each 
                    if (!AbilityHasTarget(ability, ability.Type == AbilityType.ATTACK ? enemies : allies, location, out TinyBot target)) continue;
                    //move to location
                    yield return PathToCastingPosition(location);

                    //lock on and use ability
                    yield return UseAbility(ability, target.ChassisPoint.gameObject);
                    yield break;
                }
            }
            
            

            possibleAbilities.Remove(ability);

            IEnumerator PathToCastingPosition(Vector3Int location)
            {
                bool moveRequired = Vector3Int.RoundToInt(thisBot.transform.position) != location;
                if (moveRequired)
                {
                    List<Vector3> path = Pathfinder3D.FindVectorPath(location, out var moveCosts);
                    thisBot.AttemptToSpendResource(Mathf.RoundToInt(moveCosts[^1]), StatType.MOVEMENT);
                    yield return thisBot.StartCoroutine(thisBot.PrimaryMovement.TraversePath(path));
                    Pathfinder3D.GeneratePathingTree(thisBot);
                }
            }
        }

        bool AbilityHasTarget(Ability ability, List<TinyBot> targets, Vector3Int baseLocation, out TinyBot target)
        {
            target = default;
            Vector3 location = GunPositionAt(ability, baseLocation);
            if (Physics.CheckSphere(location, terrainCheckSize, terrainMask)) return false;
            Debug.DrawRay(location, Vector3.down, Color.blue, 10f);

            foreach (var playerUnit in targets)
            {
                Transform targetPoint = playerUnit.ChassisPoint;
                if (Vector3.Distance(targetPoint.position, location) > ability.range) continue;

                List<TinyBot> hits = ability.AimAt(targetPoint.gameObject, location, true);
                if (hits == null || hits.Count == 0 || !hits.Contains(playerUnit)) continue;
                target = playerUnit;
                Debug.DrawRay(location, Vector3.up, Color.yellow, 10f);
                return true;
            }
            return false;
        }

        bool DashCanReach(Ability ability, Vector3Int position)
        {
            Debug.DrawRay(position, Vector3.up, Color.yellow, 10f);
            pointer.transform.position = position;
            ability.AimAt(pointer, thisBot.transform.position, true);
            if (ability.IsUsable(pointer.transform.position)) return true;
            return false;
        }

        IEnumerator MoveFreely()
        {
            float remainingMove = thisBot.Stats.Current[StatType.MOVEMENT];
            List<Vector3Int> pathableLocations;
            if (thisBot.Stats.Current[StatType.ACTION] > 0)
            {
                pathableLocations = Pathfinder3D.GetPathableLocations();
                pathableLocations = pathableLocations.OrderBy(location =>
                Vector3.Distance(location, closestEnemyPosition)).ToList();
            }
            else
            {
                pathableLocations = Pathfinder3D.GetPathableLocations(Mathf.FloorToInt(remainingMove));
                pathableLocations = pathableLocations.OrderBy(DistanceFromOptimalRange())
                    .ToList();
            }

            List<Vector3> path = Pathfinder3D.FindVectorPath(pathableLocations[0], out var moveCosts);
            if (path == null) yield break;

            float maxMove = moveCosts.Where(cost => cost < remainingMove).LastOrDefault();
            int endIndex = moveCosts.IndexOf(maxMove);
            path = path.Take(endIndex).ToList();
            if (path.Count == 0) yield break;

            thisBot.AttemptToSpendResource(Mathf.RoundToInt(moveCosts[endIndex]), StatType.MOVEMENT);

            yield return thisBot.StartCoroutine(thisBot.PrimaryMovement.TraversePath(path));

            
        }
        Func<Vector3Int, float> DistanceFromOptimalRange()
        {
            return location => Mathf.Abs(Vector3.Distance(location, closestEnemyPosition) - optimalDistance);
        }

        void EndTurn()
        {
            thisBot.ToggleActiveLayer(false);
            MainCameraControl.RestrictCamera(false);
            MainCameraControl.ReleaseTracking();
            TurnManager.EndTurn(thisBot);
        }

        void BeginTurn()
        {
            MainCameraControl.RestrictCamera(true);
            MainCameraControl.TrackTarget(thisBot.transform);
            thisBot.ToggleActiveLayer(true);
            Pathfinder3D.GeneratePathingTree(thisBot);
        }
    }

    private IEnumerator UseAbility(Ability ability, GameObject target)
    {
        ability.LockOnTo(target, false);
        yield return new WaitForSeconds(lockTime);
        thisBot.AttemptToSpendResource(ability.cost, StatType.ACTION);
        yield return thisBot.StartCoroutine(ability.Execute());
        ability.ReleaseLockOn();
    }

    Vector3 GunPositionAt(Ability ability, Vector3 position)
    {
        Quaternion locationRotation = thisBot.PrimaryMovement.GetRotationAtPosition(position);
        Vector3 gunPosition = ability.emissionPoint.transform.position;
        Vector3 localGun = thisBot.transform.InverseTransformPoint(gunPosition);
        Vector3 rotatedGun = locationRotation * localGun;
        return position + rotatedGun;
    }
}
