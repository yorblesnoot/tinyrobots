public abstract class DamageFactor
{
    public abstract int Priority { get; }
    public bool Outgoing = false;
    public int RemainingUses = -1;

    public abstract float UseFactor(float incoming, TinyBot source, TinyBot target);

}
