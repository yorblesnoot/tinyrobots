using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossEvent : BattleEvent
{
    [SerializeField] SpawnTable bossTable;
    [SerializeField] TowerBuilder towerBuilder;
    [SerializeField] PlayerData playerData;
    protected override void PreBattle(TowerNavZone zone)
    {
        relay.activeSpawnTable = bossTable;
    }

    protected override IEnumerator PostBattle(UnityAction eventComplete)
    {
        playerData.MapData = towerBuilder.BuildTowerFloor(null);
        eventComplete();
        yield break;
    }
}
