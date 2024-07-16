using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BossEvent : BattleEvent
{
    [SerializeField] SpawnTable bossTable;
    protected override void PreBattle(TowerNavZone zone)
    {
        relay.activeSpawnTable = bossTable;
    }

    protected override IEnumerator PostBattle(UnityAction eventComplete)
    {
        //make a new floor
        eventComplete();
        yield break;
    }
}
