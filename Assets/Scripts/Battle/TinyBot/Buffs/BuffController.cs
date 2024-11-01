using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    public List<(BotBuff, int)> ActiveBuffs;
    TinyBot owner;
    private void Awake()
    {
        ActiveBuffs = new();
        owner = GetComponent<TinyBot>();
    }

    public void AddBuff(BotBuff buff, int potency)
    {
        int buffCount = ActiveBuffs.Where(b => b.Item1 ==  buff).Count();
        if (buffCount >= buff.MaxStacks) return;
        buff.ApplyEffect(owner, potency);
        ActiveBuffs.Add((buff, potency));
    }

    public void RemoveBuff(BotBuff buff, int potency)
    {
        if (!ActiveBuffs.Remove((buff, potency))) return;
        buff.RemoveEffect(owner, potency);
        
    }
}
