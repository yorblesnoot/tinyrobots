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
    }

    protected override IEnumerator PostBattle(UnityAction eventComplete)
    {
        SceneGlobals.PlayerData.Difficulty++;
        SceneGlobals.PlayerData.MapData = towerBuilder.BuildTowerFloor(null);
        eventComplete();
        yield break;
    }
}
