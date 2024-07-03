using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PartOverviewPanel : MonoBehaviour
{
    [SerializeField] TMP_Text nameDisplay;
    [SerializeField] TMP_Text weightDisplay;
    [SerializeField] AbilityDisplay[] abilityDisplays;
    [SerializeField] TMP_Text[] modLines;
    [SerializeField] PartStatIcon[] statDisplays;
    public void Become(ModdedPart part)
    {
        gameObject.SetActive(true);
        nameDisplay.text = part.BasePart.name;
        SetAbilities(part);
        SetStats(part);
        SetMods(part);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    void SetMods(ModdedPart part)
    {
        List<string> words = new();
        foreach(var stat in part.Stats) words.Add(GetStatPhrase(stat.Key, stat.Value));
        foreach(var mod in part.Mods) words.Add(GetModPhrase(mod.Key, mod.Value));
        for(int i = 0; i < modLines.Length; i++)
        {
            if (i < words.Count) modLines[i].text = words[i];
            else modLines[i].text = "";
        }
    }

    string GetStatPhrase(StatType type, int value)
    {
        string statName = type.ToString().ToLower().FirstToUpper();
        string plusOrMinus = value > 0 ? "+" : "-";
        if (type == StatType.WEIGHT || type == StatType.ARMOR)
        {
            return $"{plusOrMinus}{value}% {statName}";
        }
        else if(type == StatType.ACTION || type == StatType.MOVEMENT)
        {
            return $"{plusOrMinus}{value} {statName} points";
        }
        else if (type == StatType.HEALTH || type == StatType.INITIATIVE || type == StatType.SHIELD)
        {
            return $"{plusOrMinus}{value} {statName}";
        }
        return "";
    }

    string GetModPhrase(ModType type, int value)
    {
        string modName = type.ToString().ToLower().FirstToUpper();
        string plusOrMinus = value > 0 ? "+" : "-";
        if (type == ModType.DAMAGEPERCENT)
        {
            return $"{plusOrMinus}{value}% Ability Damage";
        }
        else return $"{plusOrMinus}{value}% {modName}";
    }

    void SetStats(ModdedPart part)
    {
        List<StatType> statTypes = part.Stats.Keys.ToList();
        for (int i = 0; i < statDisplays.Count(); i++)
        {
            if (i < statTypes.Count)
            {
                statDisplays[i].AssignStat(statTypes[i], part.Stats[statTypes[i]]);
            }
            else statDisplays[i].Hide();
        }
        weightDisplay.text = part.Weight.ToString();
    }

    void SetAbilities(ModdedPart part)
    {
        Ability[] activeAbilities = part.Abilities;
        for (int i = 0; i < abilityDisplays.Length; i++)
        {
            bool abilityExists = i < activeAbilities.Length;
            abilityDisplays[i].gameObject.SetActive(abilityExists);
            if (abilityExists) abilityDisplays[i].Become(activeAbilities[i]);
        }
    }

    
}
