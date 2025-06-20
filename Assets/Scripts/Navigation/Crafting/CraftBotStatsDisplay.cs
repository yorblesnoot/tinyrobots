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
    
    int totalWeight;
    float totalHealth;

    public void Initialize()
    {
        if (entries != null) return;
        PartSlot.ModifiedParts.AddListener(RefreshDisplays);
        BotCrafter.Instance.PartInventory.PartActivated.AddListener((_) => RefreshDisplays());
        entries = stats.ToDictionary(stat => stat.Type, stat => stat);
    }

    public void RefreshDisplays()
    {
        //dont try to refresh if the crafting screen is not active; otherwise, this will be activated by the shop
        if (BotCrafter.Instance.gameObject.activeInHierarchy == false) return;
        foreach (var entry in entries.Values) entry.Value = 0;
        totalHealth = totalWeight = 0;
        List <Ability> activeAbilities = new();

        foreach(var part in PartSlot.SlottedParts)
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
        string weightText = totalWeight.ToString();
        if(BotCrafter.Instance.PartInventory.ActivePart != null)
        {
            int activeWeight = BotCrafter.Instance.PartInventory.ActivePart.FinalStats[StatType.ENERGY];
            weightText += " " + BotCrafter.Instance.PartInventory.ActivePart != null ? " + " + activeWeight.ToString() : "";
        }
        weightText += " / " + BotCrafter.ActiveCore.EnergyCapacity.ToString();
        weightDisplay.text = weightText;
        weightDisplay.color = PartSlot.ActivePartIsUnderWeightLimit() ?  Color.green : Color.red;

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
