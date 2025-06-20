using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShopEvent : ZoneEvent
{
    [SerializeField] ShopWindow shopUI;
    [SerializeField] PartGenerator partGenerator;
    public override void Activate(TowerNavZone zone, UnityAction eventComplete)
    {
        shopUI.Open(zone.ZoneIndex, eventComplete);
    }

    public override void Visualize(TowerNavZone zone)
    {
        base.Visualize(zone);
        SceneGlobals.PlayerData.ShopData.GenerateShop(zone.ZoneIndex, partGenerator);
    }
}
