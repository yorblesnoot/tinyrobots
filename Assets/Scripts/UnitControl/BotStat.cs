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
    public Dictionary<StatType, float> Max;
    public Dictionary<StatType, float> Current;

    public BotStats()
    {
        Max = new();
        Current = new();
        foreach(StatType type in Enum.GetValues(typeof(StatType)))
        {
            Max.Add(type, 0f);
            Current.Add(type, 0f);
        }
        SetSampleStats();
    }

    void SetSampleStats()
    {
        Max[StatType.ACTION] = 3f;
        Max[StatType.MOVEMENT] = 30f;
        Max[StatType.HEALTH] = 100f;

        Current[StatType.ACTION] = 3f;
        Current[StatType.MOVEMENT] = 10f;
        Current[StatType.HEALTH] = 100f;
    }

    public void SetToMax(StatType type)
    {
        Current[type] = Max[type];
    }
}
