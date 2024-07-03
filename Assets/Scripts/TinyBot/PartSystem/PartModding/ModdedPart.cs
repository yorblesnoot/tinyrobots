using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModdedPart
{
    public CraftablePart BasePart;
    public Dictionary<StatType, int> Stats;
    public Dictionary<ModType, int> Mods;
    //public int[] ExtraAbilities;
    public List<PartMutator> Mutators = new();
    public int Weight { get; private set; }
    [HideInInspector] public GameObject Sample;
    [HideInInspector] public Ability[] Abilities;


    public ModdedPart() { }
    public ModdedPart(CraftablePart part)
    {
        BasePart = part;
    }

    public void InitializePart()
    {
        InstantiateSample();
        Weight = BasePart.Weight;
        MutatePart();
    }

    public void InstantiateSample()
    {
        if (Sample != null) return;
        Sample = GameObject.Instantiate(BasePart.AttachableObject);
        if (Sample.TryGetComponent(out Collider collider)) collider.enabled = false;
        Abilities = Sample.GetComponents<Ability>();
    }

    void MutatePart()
    {
        List<StatValue> statValues = new(BasePart.PartStats);
        List<ModValue> modValues = new();
        if (Mutators != null)
        {
            foreach (PartMutator mutator in Mutators)
            {
                statValues.AddRange(mutator.Stats);
                modValues.AddRange(mutator.Mods);
            }
        }
        
        Stats = new();
        foreach (var statSet in statValues)
        {
            if (Stats.ContainsKey(statSet.Type)) Stats[statSet.Type] += statSet.Value;
            else Stats.Add(statSet.Type, statSet.Value);
        }

        Mods = new();
        foreach (var modSet in modValues)
        {
            if (Mods.ContainsKey(modSet.Type)) Mods[modSet.Type] += modSet.Value;
            else Mods.Add(modSet.Type, modSet.Value);
        }

        foreach(var ability in Abilities)
        {
            foreach (var mod in Mods)
            {
                if(mod.Key == ModType.RANGE && ability.ModifiableRange) ability.range += mod.Value;
                else if(mod.Key == ModType.COOLDOWN)
                {
                    int newCooldown = mod.Value + ability.cooldown;
                    ability.cooldown = Mathf.Clamp(newCooldown, 1, newCooldown);
                }
                else if (mod.Key == ModType.DAMAGEPERCENT)
                {
                    float finalDamage = ability.damage;
                    finalDamage *= 1 + mod.Value/100;
                    ability.damage = Mathf.RoundToInt(finalDamage);
                }
                else if (mod.Key == ModType.COST)
                {
                    int finalCost = ability.cost + mod.Value;
                    ability.cost = Mathf.Clamp(finalCost, 0, int.MaxValue); 
                }
            }
        }
        

        if (Stats.TryGetValue(StatType.WEIGHT, out int extraWeight))
        {
            Stats.Remove(StatType.WEIGHT);
            Weight += extraWeight;
        }
    }
}
