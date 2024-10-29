public class ArmorDamage : DamageFactor
{
    public override int Priority => 0;

    public override float UseFactor(float incoming, TinyBot source, TinyBot target)
    {
        float armorMultiplier = 1 - (float)target.Stats.Current[StatType.ARMOR] / 100;
        return armorMultiplier * incoming;
    }
}
