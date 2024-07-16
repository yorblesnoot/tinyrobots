using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleEvent : ZoneEvent
{
    [SerializeField] SceneLoader loader;
    [SerializeField] protected SceneRelay relay;
    [SerializeField] List<SpawnTable> possibleSpawns;
    
    public override void Activate(TowerNavZone zone, UnityAction eventComplete)
    {
        if (zone.battleMap != null && !relay.BattleComplete)
        {
            PreBattle(zone);
            relay.PrepareBattle(zone.battleMap);
            loader.Load(SceneType.BATTLE);
        }
        else
        {
            relay.BattleComplete = false;
            StartCoroutine(PostBattle(eventComplete));
        }
    }

    protected virtual void PreBattle(TowerNavZone zone)
    {
        relay.activeSpawnTable = possibleSpawns.GrabRandomly(false);
        
    }

    protected virtual IEnumerator PostBattle(UnityAction eventComplete)
    {
        eventComplete();
        yield break;
    }
}
