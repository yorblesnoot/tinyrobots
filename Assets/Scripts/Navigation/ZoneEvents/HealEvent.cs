using UnityEngine;
using UnityEngine.Events;

public class HealEvent : ZoneEvent
{
    [Range(0, 1)][SerializeField] float healAmount = .2f;
    [SerializeField] PlayerData playerData;
    public override void Activate(TowerNavZone zone, UnityAction eventComplete)
    {
        foreach(var core in playerData.CoreInventory)
        {
            core.HealthRatio.Value = Mathf.Clamp01(core.HealthRatio.Value + healAmount); 
            core.Mana.Value = core.ModdedCore.FinalStats[StatType.MANA];
        }
        eventComplete();
    }
}
