using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossEvent : BattleEvent
{
    [SerializeField] TowerBuilder towerBuilder;
    [SerializeField] PlayerData playerData;
    protected override void PreBattle(TowerNavZone zone)
    {
        relay.activeSpawnTable = possibleSpawns[0];
    }

    protected override IEnumerator PostBattle(UnityAction eventComplete)
    {
        playerData.MapData = towerBuilder.BuildTowerFloor(null);
        eventComplete();
        yield break;
    }
}
