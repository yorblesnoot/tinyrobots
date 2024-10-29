using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplierDamage : DamageFactor
{
    public override int Priority => 0;
    public float Multiplier;

    public override float UseFactor(float incoming, TinyBot source, TinyBot target)
    {
        return Multiplier * incoming;
    }
}
