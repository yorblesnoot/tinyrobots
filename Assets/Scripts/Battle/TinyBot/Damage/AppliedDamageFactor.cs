using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AppliedDamageFactor 
{
    //this class wraps a damage factor SO and stores data relevant to a specific instance attached to a unit
    public DamageFactor Factor;
    object factorData;
    int potency;
    public AppliedDamageFactor(DamageFactor factor, int potency, object data = null)
    {
        this.Factor = factor;
        this.potency = potency;
        factorData = data;
    }

    public float UseFactor(float incoming, TinyBot source, TinyBot target)
    {
        return Factor.UseFactor(incoming, source, target, potency, factorData);
    }
}
