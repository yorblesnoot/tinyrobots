using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BotBuff : ScriptableObject
{
    public int MaxStacks = 1;
    public virtual void Apply(TinyBot bot) 
    {

    }

    public abstract void Remove(TinyBot bot);
}
