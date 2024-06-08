using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleEvent : ZoneEvent
{
    [SerializeField] SceneLoader loader;
    [SerializeField] SceneRelay relay;
    
    public override void Activate(TowerNavZone zone, UnityAction eventComplete)
    {
        if (zone.battleMap != null)
        {
            relay.battleMap = zone.battleMap;
            loader.Load(SceneType.BATTLE);
        }
        else eventComplete();
    }
}
