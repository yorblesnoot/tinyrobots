using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossEvent : BattleEvent
{
    [SerializeField] TowerBuilder towerBuilder;
    protected override void PreBattle(TowerNavZone zone)
    {
        relay.ActiveSpawnTable = possibleSpawns[0];
        SceneGlobals.PlayerData.Difficulty += 2;
    }

    protected override IEnumerator PostBattle(UnityAction eventComplete)
    {
        SceneGlobals.PlayerData.MapData = new();
        SceneGlobals.PlayerData.Difficulty--;
        ResetMana();
        yield return new WaitForSeconds(2);
        towerBuilder.DeployTowerFloor(SceneGlobals.PlayerData.MapData, true);
        
        eventComplete();
        yield break;
    }

    void ResetMana()
    {
        
        foreach(var character in SceneGlobals.PlayerData.CoreInventory)
        {
            ModdedPart core = character.Bot.Value;
            character.Mana = core.FinalStats[StatType.MANA];
        }
    }
}
