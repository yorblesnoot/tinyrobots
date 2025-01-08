using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartEconomy : MonoBehaviour
{
    static PartEconomy instance;

    [SerializeField] int baseCost = 10;
    [SerializeField] int weightCost = 2;
    [SerializeField] int modCost = 10;
    [SerializeField] int shopCurrencyMultiplier = 85;
    private void Awake()
    {
        instance = this;
    }

    public static int GetCost(ModdedPart part)
    {
        return instance.baseCost + part.EnergyCost * instance.weightCost + part.Mutators.Count * instance.modCost;
    }

    public static int GetShopBudget()
    {
        return SceneGlobals.PlayerData.Difficulty * instance.shopCurrencyMultiplier;
    }
}
