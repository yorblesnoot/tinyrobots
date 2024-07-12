using System;
using System.Collections.Generic;

public enum StatType
{
    HEALTH, //flat
    ACTION, //flat
    MOVEMENT, //flat
    ARMOR, //flat
    SHIELD, //flat
    INITIATIVE, //flat
    WEIGHT //percent
}
public class UnitStats
{
    public Dictionary<StatType, int> Max;
    public Dictionary<StatType, int> Current;

    public UnitStats()
    {
        Max = new();
        Current = new();
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
}
