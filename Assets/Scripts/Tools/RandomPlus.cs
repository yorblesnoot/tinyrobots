using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RandomPlus 
{
    public static bool Probability(this float decimalChance)
    {
        float random = Random.Range(0f, 1f);
        if(random <= decimalChance) return true;
        else return false;
    }

    public static IWeighted RandomByWeight(this IEnumerable<IWeighted> list)
    {
        int totalWeight = list.Sum(x => x.Weight);
        int random = Random.Range(0, totalWeight);
        int current = 0;
        int count = list.Count();
        foreach(IWeighted weighted in list) 
        {
            current += weighted.Weight;
            if (random <= current) return weighted;
        }
        return default;
    }
}

public interface IWeighted {  public int Weight { get; } }
