using System;
using System.Collections.Generic;

public enum StatType
{
    HEALTH,
    ACTION,
    MOVEMENT,
    ARMOR,
    WEIGHT
}
public class BotStats
{
    public Dictionary<StatType, int> Max;
    public Dictionary<StatType, int> Current;

    public BotStats()
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
