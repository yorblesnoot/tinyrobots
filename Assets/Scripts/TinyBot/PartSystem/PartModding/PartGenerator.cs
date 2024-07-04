using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartGenerator : MonoBehaviour
{
    [SerializeField] List<PartMutator> mutators;
    [SerializeField] BotConverter botConverter;
    List<CraftablePart> generableBases;
    private void Awake()
    {
        generableBases = botConverter.PartLibrary.Where(part => part.Type != SlotType.CORE).ToList();
    }
    public ModdedPart Generate(int modNumber)
    {
        CraftablePart partBase = generableBases.GrabRandomly(false);
        ModdedPart modPart = new(partBase);
        List<PartMutator> availableMutators = new();

        HashSet<StatType> partStats = partBase.PartStats.Select(ps => ps.Type).ToHashSet();
        modPart.InstantiateSample();
        Ability[] abilities = modPart.Abilities;
        foreach (var mutator in mutators)
        {
            if(CheckStats(partStats, mutator) && CheckMods(mutator, abilities)) availableMutators.Add(mutator);
        }

        while(modPart.Mutators.Count < modNumber && availableMutators.Count > 0)
        {
            modPart.Mutators.Add(availableMutators.GrabRandomly());
        }

        modPart.InitializePart();
        return modPart;
    }

    bool CheckStats(HashSet<StatType> partStats, PartMutator mutator)
    {
        if (mutator.Stats.Count() == 0) return true;
        foreach (StatValue stat in mutator.Stats)
        {
            if (!partStats.Contains(stat.Type)) return false;
        }
        return true;
    }

    bool CheckMods(PartMutator mutator, Ability[] abilities)
    {
        if(mutator.Mods.Count() == 0) return true;
        foreach(var mod in mutator.Mods) if (!ModHasValidTarget(mod, abilities)) return false;
        return true;
    }

    bool ModHasValidTarget(ModValue mod, Ability[] abilities)
    {
        foreach(var ability in abilities) if (ModCanApply(ability, mod)) return true;
        return false;
    }

    bool ModCanApply(Ability ability, ModValue mod)
    {
        if (mod.Type == ModType.RANGE && ability.ModifiableRange) return true;
        else if (mod.Type == ModType.COOLDOWN && ability.cooldown + mod.Value > 0) return true;
        else if (mod.Type == ModType.DAMAGEPERCENT && ability.damage > 0) return true;
        else if (mod.Type == ModType.COST && ability.cost + mod.Value >= 0) return true;
        return false;
    }
}
