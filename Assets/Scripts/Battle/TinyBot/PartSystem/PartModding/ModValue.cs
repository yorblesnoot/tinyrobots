[System.Serializable]
public struct ModValue 
{
    public ModType Type;
    public int Value;
}

public enum ModType
{
    RANGE,
    COOLDOWN,
    POTENCY,
    COST
}
