using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PartOverviewPanel : MonoBehaviour
{
    [SerializeField] TMP_Text nameDisplay;
    [SerializeField] TMP_Text weightDisplay;
    [SerializeField] MoveStyleDisplay moveStyleDisplay;
    [SerializeField] AbilityDisplay[] abilityDisplays;
    [SerializeField] TMP_Text[] modLines;
    [SerializeField] PartStatIcon[] statDisplays;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Become(ModdedPart part)
    {
        if(part == null)
        {
            gameObject.SetActive(false);
            return;
        }
        moveStyleDisplay.Become(part);
        gameObject.SetActive(true);
        nameDisplay.text = part.BasePart.name;
        SetAbilities(part);
        SetBaseStats(part);
        SetMods(part);
    }

    void SetMods(ModdedPart part)
    {
        List<string> words = new();
        foreach(var stat in part.StatChanges) words.Add(GetStatPhrase(stat.Key, stat.Value));
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
        if (type == StatType.ACTION || type == StatType.MOVEMENT) statName += " Points";
        else if (type == StatType.ENERGY) statName += " Cost";
        string plus = value > 0 ? "+" : "";

        return $"{plus}{value}{(ModdedPart.PercentStats.Contains(type) ? "%" : "")} {statName}";
    }

    string GetModPhrase(ModType type, int value)
    {
        string modName = type.ToString().ToLower().FirstToUpper();
        string plus = value > 0 ? "+" : "";
        if (type == ModType.POTENCY)
        {
            return $"{plus}{value}% Ability Potency";
        }
        else return $"{plus}{value} {modName}";
    }

    void SetBaseStats(ModdedPart part)
    {
        List<StatType> statTypes = part.FinalStats.Keys.ToList();
        statTypes.Remove(StatType.ENERGY);
        weightDisplay.text = part.FinalStats[StatType.ENERGY].ToString();
        for (int i = 0; i < statDisplays.Count(); i++)
        {
            if (i < statTypes.Count)
            {
                statDisplays[i].AssignStat(statTypes[i], part.FinalStats[statTypes[i]]);
            }
            else statDisplays[i].Hide();
        }
        
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
