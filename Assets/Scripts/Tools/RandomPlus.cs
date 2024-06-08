using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomPlus 
{
    public static bool Probability(float decimalChance)
    {
        float random = Random.Range(0f, 1f);
        if(random <= decimalChance) return true;
        else return false;
    }
}
