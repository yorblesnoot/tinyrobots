using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ModdedPart
{
    public CraftablePart BasePart;
    public Dictionary<StatType, int> Stats;
    public Dictionary<AbilityModifier, int> Mods;
    //public int[] ExtraAbilities;
    public List<PartMutator> Mutators = new();
    public int Weight { get; private set; }
    public GameObject Sample;

    public ModdedPart() { }
    public ModdedPart(CraftablePart part)
    {
        BasePart = part;
    }

    public void InitializePart()
    {
        Sample = GameObject.Instantiate(BasePart.AttachableObject);
        if(Sample.TryGetComponent(out Collider collider)) collider.enabled = false;
        Weight = BasePart.Weight;
        MutatePart();
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
            if (Stats.ContainsKey(statSet.type)) Stats[statSet.type] += statSet.value;
            else Stats.Add(statSet.type, statSet.value);
        }

        Mods = new();
        foreach (var modSet in modValues)
        {
            if (Mods.ContainsKey(modSet.Type)) Mods[modSet.Type] += modSet.Value;
            else Mods.Add(modSet.Type, modSet.Value);
        }

        if (Stats.TryGetValue(StatType.WEIGHT, out int extraWeight))
        {
            Stats.Remove(StatType.WEIGHT);
            Weight += extraWeight;
        }
    }
}
