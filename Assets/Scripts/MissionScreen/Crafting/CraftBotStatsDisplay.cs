using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CraftBotStatsDisplay : MonoBehaviour
{
    
    [SerializeField] StatEntry[] stats;
    [SerializeField] TMP_Text weightDisplay;
    [SerializeField] TMP_Text healthDisplay;
    [SerializeField] AbilityDisplay[] abilityDisplays;
    [SerializeField] BlueprintControl blueprintControl;

    Dictionary<StatType, StatEntry> entries;
    List<ModdedPart> activeParts = new();
    int totalWeight;
    int maxWeight;
    float totalHealth;

    public void Initialize()
    {
        if (entries != null) return;
        PartSlot.SlottedPart.AddListener(ModifyPartStats);
        entries = stats.ToDictionary(stat => stat.Type, stat => stat);
    }

    void ModifyPartStats(ModdedPart part, bool add)
    {
        if(add) activeParts.Add(part);
        else activeParts.Remove(part);
        RefreshDisplays();
    }

    public bool IsDeployable()
    {
        return totalWeight <= maxWeight;
    }

    public void RefreshDisplays()
    {
        foreach (var entry in entries.Values) entry.Value = 0;
        totalHealth = totalWeight = maxWeight = 0;
        List<Ability> activeAbilities = new();

        List<ModdedPart> activePartsPlusOrigin = new(activeParts)
        {
            blueprintControl.originPart
        };
        foreach(var part in activePartsPlusOrigin)
        {
            activeAbilities.AddRange(part.Abilities);
            Debug.Log(part.BasePart.name);
            Debug.Log(part.FinalStats);
            foreach (StatType stat in part.FinalStats.Keys)
            {
                int increment = part.FinalStats[stat];
                if (stat == StatType.HEALTH) totalHealth += increment;
                else entries[stat].Value += increment;
            }
            
            if(part.Weight < 0) maxWeight -= part.Weight;
            else totalWeight += part.Weight;
        }

        foreach(var entry in entries.Values) entry.Display.text = entry.Value.ToString();
        healthDisplay.text = $"{UnitSwitcher.ActiveCore.HealthRatio * totalHealth} / {totalHealth}";
        weightDisplay.text = $"{totalWeight} / {(maxWeight == 0 ? "-" : maxWeight)}";
        weightDisplay.color = IsDeployable() ?  Color.white : Color.red;

        activeAbilities.PassDataToUI(abilityDisplays, (ability, display) => display.Become(ability));
    }


    [Serializable]
    class StatEntry
    {
        public StatType Type;
        [HideInInspector] public int Value;
        public TMP_Text Display;
    }
}
