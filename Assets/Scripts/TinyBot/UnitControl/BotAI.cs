using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotAI
{
    readonly TinyBot thisBot;
    int terrainMask;

    public BotAI(TinyBot bot)
    {
        thisBot = bot;
        terrainMask = LayerMask.GetMask("Terrain");
    }

    readonly float lockTime = 1f;
    public static readonly float terrainCheckSize = 1.1f;
    public IEnumerator TakeTurn()
    {
        MainCameraControl.RestrictCamera(true);
        MainCameraControl.TrackTarget(thisBot.transform);
        thisBot.ToggleActiveLayer(true);
        List<Ability> possibleAbilities = new(thisBot.Abilities);
        List<TinyBot> enemies = TurnManager.TurnTakers.Where(x => x.allegiance != thisBot.allegiance).ToList();
        Pathfinder3D.GeneratePathingTree(thisBot);

        while (possibleAbilities.Count > 0)
        {
            int abilityIndex = Random.Range(0, possibleAbilities.Count);
            yield return thisBot.StartCoroutine(MoveAndUseAbility(possibleAbilities[abilityIndex]));
            possibleAbilities = possibleAbilities
                .Where(ability => (ability.cost <= thisBot.Stats.Current[StatType.ACTION]) && ability.currentCooldown == 0)
                .ToList();
        }

        yield return thisBot.StartCoroutine(MoveFreely());
        yield return new WaitForSeconds(1);

        thisBot.ToggleActiveLayer(false);
        MainCameraControl.RestrictCamera(false);
        MainCameraControl.ReleaseTracking();
        TurnManager.EndTurn(thisBot);
        

        IEnumerator MoveAndUseAbility(Ability ability)
        {
            
            List<Vector3Int> pathableLocations = Pathfinder3D.GetPathableLocations(Mathf.FloorToInt(thisBot.Stats.Current[StatType.MOVEMENT]));
            foreach(Vector3Int location in pathableLocations)
            {
                if(AbilityHasTarget(ability, enemies, location, out TinyBot target))
                {
                    //move to location
                    if (Vector3Int.RoundToInt(thisBot.transform.position) != location)
                    {
                        List<Vector3> path = Pathfinder3D.FindVectorPath(location, out var moveCosts);
                        thisBot.AttemptToSpendResource(Mathf.RoundToInt(moveCosts[^1]), StatType.MOVEMENT);
                        yield return thisBot.StartCoroutine(thisBot.PrimaryMovement.TraversePath(path));
                    }
                        
                    ability.LockOnTo(target.ChassisPoint.gameObject, false);
                    yield return new WaitForSeconds(lockTime);
                    thisBot.AttemptToSpendResource(ability.cost, StatType.ACTION);
                    yield return thisBot.StartCoroutine(ability.Execute());
                    ability.ReleaseLockOn();
                    Pathfinder3D.GeneratePathingTree(thisBot);
                    yield break;
                }
            }
            possibleAbilities.Remove(ability);
        }

        
        bool AbilityHasTarget(Ability ability, List<TinyBot> targets, Vector3Int baseLocation, out TinyBot target)
        {
            target = default;
            Vector3 location = GunPositionAt(ability, baseLocation);
            if(Physics.CheckSphere(location, terrainCheckSize, terrainMask)) return false;
            Debug.DrawRay(location, Vector3.down, Color.blue, 10f);
            
            foreach (var playerUnit in targets)
            {
                Transform targetPoint = playerUnit.ChassisPoint;
                if (Vector3.Distance(targetPoint.position, location) <= ability.range)
                {
                    List<TinyBot> hits = ability.AimAt(targetPoint.gameObject, location, true);
                    Debug.Log(hits.Count);
                    if (hits == null || hits.Count == 0 || !hits.Contains(playerUnit)) continue;
                    target = playerUnit;
                    Debug.DrawRay(location, Vector3.up, Color.yellow, 10f);
                    return true;
                }
            }
            return false;
        }


        IEnumerator MoveFreely()
        {
            enemies.OrderBy(unit => Vector3.Distance(unit.transform.position, thisBot.transform.position)).ToList();
            Vector3 closestEnemyPosition = enemies[0].transform.position;
            List<Vector3Int> pathableLocations = Pathfinder3D.GetPathableLocations();
            pathableLocations = pathableLocations.OrderBy(location => Vector3.Distance(location, closestEnemyPosition)).ToList();
            List<Vector3> path = Pathfinder3D.FindVectorPath(pathableLocations[0], out var moveCosts);
            int endIndex = 0;
            if (moveCosts == null || moveCosts.Count == 0) yield break;

            while (moveCosts[endIndex] < thisBot.Stats.Current[StatType.MOVEMENT]) endIndex++;
            path = path.Take(endIndex).ToList();
            thisBot.AttemptToSpendResource(Mathf.RoundToInt(moveCosts[endIndex]), StatType.MOVEMENT);

            yield return thisBot.StartCoroutine(thisBot.PrimaryMovement.TraversePath(path));
        }
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
