using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BuffType : ScriptableObject
{
    public int MaxStacks = 1;
    public Sprite Thumbnail;
    public int Duration = 0;
    [TextArea(3, 10)] public string Description;
    public abstract object ApplyEffect(TinyBot target, TinyBot source, int potency);

    public abstract void RemoveEffect(TinyBot target, int potency, object data);

    public virtual void TickEffect() { }
}
