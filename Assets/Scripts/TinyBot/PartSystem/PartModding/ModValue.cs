[System.Serializable]
public struct ModValue 
{
    public AbilityModifier Type;
    public int Value;
}

public enum AbilityModifier
{
    RANGE,
    COOLDOWN,
    DAMAGEPERCENT,
    COST
}
