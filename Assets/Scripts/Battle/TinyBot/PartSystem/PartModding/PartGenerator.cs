using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PartGenerator : MonoBehaviour
{
    [SerializeField] List<PartMutator> mutators;
    [SerializeField] BotConverter botConverter;
    [SerializeField] PartRarityDefinitions rarityPalette;
    static List<CraftablePart> partDropPool;

    [SerializeField] int minDrops = 2;
    [SerializeField] int maxDrops = 4;
    private void Awake()
    {
        partDropPool = botConverter.PartLibrary.Where(part => part.Type != SlotType.CORE && part.Collectible).ToList();
    }

    public static void SubmitBotParts(TreeNode<ModdedPart> partTree)
    {
        partTree.Traverse(part => partDropPool.Add(part.BasePart));
    }

    public List<ModdedPart> GenerateDropList()
    {
        int tier = GetTier();
        int dropCount = Random.Range(minDrops, maxDrops);
        List<RarityDefinition> droppedRarities = new();
        while (droppedRarities.Count < dropCount) droppedRarities.Add(rarityPalette.GetWeightedRarity(tier));
        droppedRarities = droppedRarities.OrderByDescending(x => x.ModCounts[0]).ToList();

        List<ModdedPart> output = droppedRarities.Select(r => Generate(r)).ToList();


        SceneGlobals.PlayerData.PartInventory.AddRange(output);
        return output;
    }
    public ModdedPart Generate(RarityDefinition rarity, CraftablePart partBase = null)
    {
        int modNumber = rarity.ModCounts.GrabRandomly(false);
        partBase = partBase != null ? partBase : partDropPool.GrabRandomly(false);
        ModdedPart modPart = new(partBase)
        {
            Rarity = rarity
        };
        List<PartMutator> availableMutators = new();
        HashSet<StatType> partStats = partBase.PartStats.Select(ps => ps.Type).ToHashSet();
        partStats.Add(StatType.ENERGY);
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

    int GetTier()
    {
        return SceneGlobals.PlayerData.Difficulty;
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
        else if (mod.Type == ModType.POTENCY && ability.EffectMagnitude > 0) return true;
        else if (ability.IsActive && mod.Type == ModType.COOLDOWN && ability.cooldown + mod.Value > 0) return true;
        else if (ability.IsActive && mod.Type == ModType.COST && ability.cost + mod.Value >= 0) return true;
        return false;
    }
}
