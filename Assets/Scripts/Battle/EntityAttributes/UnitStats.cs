using System;
using System.Collections.Generic;
using UnityEngine.Events;

public enum StatType
{
    HEALTH, //flat
    ACTION, //flat
    MOVEMENT, //flat
    ARMOR, //flat
    SHIELD, //flat
    INITIATIVE, //flat
    ENERGY, //percent
    MANA
}
public class UnitStats
{
    public StatBlock Max;
    public StatBlock Current;
    public UnityEvent StatModified = new();

    public UnitStats()
    {
        Max = new(StatModified);
        Current = new(StatModified);
        foreach(StatType type in Enum.GetValues(typeof(StatType)))
        {
            Max.Add(type, 0);
            Current.Add(type, 0);
        }
    }

    public void MaxAll()
    {
        foreach (StatType type in Enum.GetValues(typeof(StatType)))
        {
            SetToMax(type);
        }
    }

    public void SetToMax(StatType type)
    {
        Current[type] = Max[type];
    }

    public void TestMode()
    {
        Max[StatType.HEALTH] = 1000;
        Max[StatType.MOVEMENT] = 100000;
        Max[StatType.ACTION] = 1000;
        Current[StatType.MANA] = 1000;
    }

    public void AddPartStats(ModdedPart part)
    {
        foreach (var stat in part.FinalStats)
        {
            Max[stat.Key] += stat.Value;
        }
    }
}

public class StatBlock
{
    Dictionary<StatType, int> map = new();
    readonly UnityEvent modified;
    public StatBlock(UnityEvent modified)
    {
        this.modified = modified;
    }

    public int this[StatType stat]
    {
        get
        {
            return map[stat];
        }
        set
        {
            map[stat] = value;
            modified.Invoke();
        }
    }

    public void Add(StatType stat, int value)
    {
        map.Add(stat, value);
    }
}
