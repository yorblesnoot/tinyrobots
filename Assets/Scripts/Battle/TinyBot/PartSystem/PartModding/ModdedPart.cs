using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class ModdedPart
{
    public CraftablePart BasePart;
    public Dictionary<StatType, int> FinalStats;
    public Dictionary<StatType, int> StatChanges;
    public Dictionary<ModType, int> Mods;
    //public int[] ExtraAbilities;
    public List<PartMutator> Mutators = new();
    public int EnergyCost { get; private set; }
    public GameObject Sample { get { if (sample == null) InitializePart(); return sample; } }
    GameObject sample;

    public Ability[] Abilities { get { if (abilities == null) InitializePart(); return abilities; } }
    Ability[] abilities;
    public RarityDefinition Rarity;

    public static readonly HashSet<StatType> PercentStats = new() { StatType.ENERGY };
    public ModdedPart() { }
    public ModdedPart(CraftablePart part)
    {
        BasePart = part;
        InitializePart();
    }

    public void InitializePart()
    {
        sample = InstantiateSample(out abilities);
        MutatePart();
    }

    public GameObject InstantiateSample(out Ability[] abilities)
    {
        GameObject output = GameObject.Instantiate(BasePart.AttachableObject);
        PartModifier partMod = output.GetComponent<PartModifier>();
        partMod.SourcePart = this;
        abilities = partMod.Abilities;
        return output;
    }

    void MutatePart()
    {
        List<StatValue> statValues = new();
        List<ModValue> modValues = new();
        foreach (PartMutator mutator in Mutators)
        {
            statValues.AddRange(mutator.Stats);
            modValues.AddRange(mutator.Mods);
        }

        ApplyStats(statValues);
        ApplyMods(modValues);
    }

    void ApplyStats(List<StatValue> statValues)
    {
        FinalStats = new();
        StatChanges = new();
        Dictionary<StatType, int> baseStats = BasePart.PartStats.ToDictionary(val => val.Type, val => val.Value);
        baseStats.Add(StatType.ENERGY, BasePart.EnergyCost);
        //consolidate similar stats
        foreach (var statSet in statValues)
        {
            if (!StatChanges.ContainsKey(statSet.Type)) StatChanges.Add(statSet.Type, 0);
            StatChanges[statSet.Type] += statSet.Value;
        }
        foreach (var stat in StatChanges)
        {
            if (stat.Value == 0) continue;
            int baseStat = baseStats[stat.Key];
            FinalStats.Add(stat.Key, 
                Mathf.RoundToInt(PercentStats.Contains(stat.Key) ? 
                (1 + (float)stat.Value / 100) * baseStat
                : stat.Value + baseStat));
        }
        foreach(var stat in baseStats)
        {
            if (stat.Value == 0 && stat.Key != StatType.ENERGY) continue;
            if (!FinalStats.ContainsKey(stat.Key)) FinalStats.Add(stat.Key, stat.Value);
        }
    }

    void ApplyMods(List<ModValue> modValues)
    {
        //consolidate similar mods
        Mods = new();
        foreach (var modSet in modValues)
        {
            if (Mods.ContainsKey(modSet.Type)) Mods[modSet.Type] += modSet.Value;
            else Mods.Add(modSet.Type, modSet.Value);
        }
        HashSet<ModType> removeMods = new();
        foreach (var mod in Mods)
        {
            if(mod.Value == 0)
            {
                removeMods.Add(mod.Key);
                continue;
            }
            foreach (var ability in Abilities) ApplyMod(ability, mod);
        }
        foreach(var mod in removeMods) Mods.Remove(mod);
    }

    void ApplyMod(Ability ability, KeyValuePair<ModType, int> mod)
    {
        if (mod.Key == ModType.RANGE && ability.ModifiableRange) ability.range += mod.Value;
        else if (mod.Key == ModType.COOLDOWN)
        {
            int newCooldown = mod.Value + ability.cooldown;
            ability.cooldown = Mathf.Clamp(newCooldown, 1, newCooldown);
        }
        else if (mod.Key == ModType.POTENCY)
        {
            float finalDamage = ability.EffectMagnitude;
            finalDamage *= 1 + mod.Value / 100;
            ability.EffectMagnitude = Mathf.RoundToInt(finalDamage);
        }
        else if (mod.Key == ModType.COST)
        {
            int finalCost = ability.cost + mod.Value;
            ability.cost = Mathf.Clamp(finalCost, 0, int.MaxValue);
        }
    }
}
