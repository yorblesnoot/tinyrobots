public class ConditionDied : TriggerCondition
{ 
    public ConditionDied(TinyBot target) : base(target)
    {
        TinyBot.BotDied.AddListener(CheckForDeath);
    }

    public override void Remove()
    {
        TinyBot.BotDied.RemoveListener(CheckForDeath);
    }

    void CheckForDeath(TinyBot check)
    {
        if(check == Target) OnTriggered.Invoke(Target);
    }
}
