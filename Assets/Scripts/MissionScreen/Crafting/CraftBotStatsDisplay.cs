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
    [SerializeField] VisualizedPartInventory blueprintControl;

    Dictionary<StatType, StatEntry> entries;
    List<ModdedPart> activeParts = new();
    int totalWeight;
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
        return totalWeight <= BotCrafter.ActiveCore.EnergyCapacity;
    }

    public void RefreshDisplays()
    {
        foreach (var entry in entries.Values) entry.Value = 0;
        totalHealth = totalWeight = 0;
        List <Ability> activeAbilities = new();

        foreach(var part in activeParts)
        {
            activeAbilities.AddRange(part.Abilities);
            foreach (StatType stat in part.FinalStats.Keys)
            {
                int increment = part.FinalStats[stat];
                if (stat == StatType.HEALTH) totalHealth += increment;
                else if(stat == StatType.ENERGY) totalWeight += increment;
                else entries[stat].Value += increment;
            }
        }

        foreach(var entry in entries.Values) entry.Display.text = entry.Value.ToString();
        healthDisplay.text = $"{Mathf.RoundToInt(BotCrafter.ActiveCore.HealthRatio.Value * totalHealth)} / {totalHealth}";
        weightDisplay.text = $"{totalWeight} / {BotCrafter.ActiveCore.EnergyCapacity}";
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
