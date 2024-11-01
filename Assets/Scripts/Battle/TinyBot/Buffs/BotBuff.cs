using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BotBuff : ScriptableObject
{
    public int MaxStacks = 1;
    public Sprite Thumbnail;
    [TextArea(3, 10)] public string Description;
    public abstract void ApplyEffect(TinyBot bot, int potency);

    public abstract void RemoveEffect(TinyBot bot, int potency);
}
