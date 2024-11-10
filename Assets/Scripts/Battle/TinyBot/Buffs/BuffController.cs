using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    public Dictionary<BuffType, AppliedBuff> ActiveBuffs;
    TinyBot owner;
    private void Awake()
    {
        ActiveBuffs = new();
        owner = GetComponent<TinyBot>();
    }

    public void AddBuff(TinyBot source, BuffType buff, int potency)
    {
        AppliedBuff targetBuff;
        if (!ActiveBuffs.TryGetValue(buff, out targetBuff))
        {
            targetBuff = new(buff, potency, source, );
        }
    }

    public void RemoveBuff(BuffType buff, int potency)
    {
        if (!ActiveBuffs.Remove((buff, potency))) return;
        buff.RemoveEffect(owner, potency);
        
    }
}
