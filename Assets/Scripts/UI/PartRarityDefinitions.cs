using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RarityDefinitions", menuName = "ScriptableObjects/RarityDefinitions")]
public class PartRarityDefinitions : ScriptableObject
{
    [SerializeField] List<RarityDefinition> rarityDefinitions;

    public RarityDefinition GetWeightedRarity(int tier)
    {
        RarityDefinition.ActiveTier = tier;
        return RandomPlus.RandomByWeight(rarityDefinitions);
    }
}

[System.Serializable]
public class RarityDefinition: IWeighted
{
    public static int ActiveTier = 0;
    public Color TextColor;
    public List<int> ModCounts;
    public int BaseWeight;
    public int TierWeight;
    public int Weight { get { return BaseWeight + TierWeight * ActiveTier; } }
    
}
