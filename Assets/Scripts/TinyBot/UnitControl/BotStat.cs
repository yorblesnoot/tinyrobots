using System;
using System.Collections.Generic;

public enum StatType
{
    HEALTH,
    ACTION,
    MOVEMENT
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
        SetSampleStats();
    }

    void SetSampleStats()
    {
        Max[StatType.ACTION] = 3;
        Max[StatType.MOVEMENT] = 15;
        Max[StatType.HEALTH] = 50;

        Current[StatType.ACTION] = 3;
        Current[StatType.MOVEMENT] = 10;
        Current[StatType.HEALTH] = 50;
    }

    public void SetToMax(StatType type)
    {
        Current[type] = Max[type];
    }
}
