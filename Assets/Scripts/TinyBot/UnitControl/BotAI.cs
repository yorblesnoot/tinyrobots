using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotAI
{
    readonly TinyBot thisBot;

    public BotAI(TinyBot bot)
    {
        thisBot = bot;
    }

    readonly float lockTime = 1f;
    public IEnumerator TakeTurn()
    {
        thisBot.ToggleActiveLayer(true);
        List<Ability> possibleAbilities = new(thisBot.Abilities);
        List<TinyBot> enemies = TurnManager.TurnTakers.Where(x => x.allegiance != thisBot.allegiance).ToList();
        
        while(possibleAbilities.Count > 0)
        {
            int abilityIndex = Random.Range(0, possibleAbilities.Count);
            yield return thisBot.StartCoroutine(MoveAndUseAbility(possibleAbilities[abilityIndex]));
            possibleAbilities = possibleAbilities.Where(ability => ability.cost <= thisBot.Stats.Current[StatType.ACTION]).ToList();
        }

        thisBot.ToggleActiveLayer(false);
        TurnManager.EndTurn(thisBot);

        IEnumerator MoveAndUseAbility(Ability ability)
        {
            Pathfinder3D.GeneratePathingTree(thisBot.PrimaryMovement.Style, Vector3Int.RoundToInt(thisBot.transform.position));
            List<Vector3Int> pathableLocations = Pathfinder3D.GetPathableLocations(Mathf.FloorToInt(thisBot.Stats.Current[StatType.MOVEMENT]));
            foreach(Vector3Int location in pathableLocations)
            {
                if(AbilityHasTarget(ability, enemies, location, out TinyBot target))
                {
                    //move to location
                    if (Vector3Int.RoundToInt(thisBot.transform.position) != location) 
                        yield return thisBot.StartCoroutine(thisBot.PrimaryMovement.PathToPoint(Pathfinder3D.FindVectorPath(location, out _)));
                    ability.LockOnTo(target.ChassisPoint.gameObject);
                    yield return new WaitForSeconds(lockTime);
                    thisBot.AttemptToSpendResource(ability.cost, StatType.ACTION);
                    yield return thisBot.StartCoroutine(ability.ExecuteAbility());
                    ability.ReleaseLock();
                    yield break;
                }
            }
            possibleAbilities.Remove(ability);
        }
        static bool AbilityHasTarget(Ability ability, List<TinyBot> enemies, Vector3Int location, out TinyBot target)
        {
            target = default;
            foreach (var enemy in enemies)
            {
                if (Vector3.Distance(enemy.ChassisPoint.position, location) <= ability.range)
                {
                    Vector3 direction = enemy.ChassisPoint.position - location;
                    Ray testRay = new(location, direction);
                    if (!Physics.Raycast(testRay, out RaycastHit hitInfo, ability.range)) continue;
                    if (!hitInfo.collider.TryGetComponent(out TinyBot bot)) continue;
                    target = enemy;
                    return true;
                }
            }
            return false;
        }
    }
}
