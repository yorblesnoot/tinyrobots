using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class UnitStatsDisplay : MonoBehaviour
{
    
    [SerializeField] StatEntry[] stats;
    [SerializeField] TMP_Text weightDisplay;
    [SerializeField] AbilityDisplay[] abilityDisplays;
    [SerializeField] BlueprintControl blueprintControl;

    Dictionary<StatType, StatEntry> entries;
    List<ModdedPart> activeParts = new();
    int totalWeight;
    int maxWeight;

    public void Initialize()
    {
        PartSlot.SlottedPart.AddListener(ModifyPartStats);
        entries = stats.ToDictionary(stat => stat.Type, stat => stat);
    }

    void ModifyPartStats(ModdedPart part, bool add)
    {
        if(add) activeParts.Add(part);
        else activeParts.Remove(part);
        RefreshDisplays();
    }

    public void RefreshDisplays()
    {
        foreach (var entry in entries.Values) entry.Value = 0;
        totalWeight = maxWeight = 0;
        List<Ability> activeAbilities = new();

        List<ModdedPart> activePartsPlusOrigin = new(activeParts)
        {
            blueprintControl.originPart
        };
        foreach(var part in activePartsPlusOrigin)
        {
            activeAbilities.AddRange(part.Abilities);
            foreach(StatType stat in part.FinalStats.Keys) entries[stat].Value += part.FinalStats[stat];
            if(part.Weight < 0) maxWeight -= part.Weight;
            else totalWeight += part.Weight;
        }

        foreach(var entry in entries.Values) entry.Display.text = entry.Value.ToString();
        weightDisplay.text = $"{totalWeight} / {(maxWeight == 0 ? "-" : maxWeight)}";
        weightDisplay.color = totalWeight > maxWeight ? Color.red : Color.white;

        Debug.Log(activeAbilities.Count);
        for(int i = 0; i < abilityDisplays.Length; i++)
        {
            bool abilityExists = i < activeAbilities.Count;
            abilityDisplays[i].gameObject.SetActive(abilityExists);
            if(abilityExists) abilityDisplays[i].Become(activeAbilities[i]);
        }
    }


    [Serializable]
    class StatEntry
    {
        public StatType Type;
        [HideInInspector] public int Value;
        public TMP_Text Display;
    }
}
