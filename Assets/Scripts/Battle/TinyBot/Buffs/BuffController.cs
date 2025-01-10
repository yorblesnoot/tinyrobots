using System.Collections.Generic;
using System.Linq;

public class BuffController
{
    public Dictionary<BuffType, AppliedBuff> ActiveBuffs;
    TinyBot owner;

    public BuffController(TinyBot owner)
    {
        this.owner = owner;
        ActiveBuffs = new();
        owner.EndedTurn.AddListener(ProgressBuffs);
    }

    public void AddBuff(TinyBot source, BuffType buff, int potency)
    {
        if (!ActiveBuffs.TryGetValue(buff, out AppliedBuff applied))
        {
            applied = new(buff, owner, source, potency);
            ActiveBuffs.Add(buff, applied);
        }
        if(applied.Stacks < applied.Buff.MaxStacks)
        {
            applied.ApplyStack();
        }

    }

    void ProgressBuffs()
    {
        List<BuffType> applied = ActiveBuffs.Keys.ToList();
        foreach (var buff in applied)
        {
            if(ActiveBuffs[buff].Tick()) ActiveBuffs.Remove(buff);
        }
    }

    public void RemoveBuff(BuffType buff)
    {
        if(!ActiveBuffs.TryGetValue(buff, out AppliedBuff applied)) return;
        applied.Remove();
    }
}
