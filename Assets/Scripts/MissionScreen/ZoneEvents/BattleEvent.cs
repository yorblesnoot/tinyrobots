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
        if (zone.battleMap == null) return;
        relay.battleMap = zone.battleMap;
        loader.Load(SceneType.BATTLE);
        eventComplete();
    }

    public override void Visualize(TowerNavZone zone)
    {
        Debug.Log("visualizing battle");
    }
}
