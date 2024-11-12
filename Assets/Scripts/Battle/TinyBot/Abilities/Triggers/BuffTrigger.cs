using UnityEngine;

[System.Serializable]
public class BuffTrigger : TriggerController
{
    [SerializeField] BuffType buff;
    [SerializeField] bool apply;
    [SerializeField] int potency;
    public void Initialize(BuffType buff)
    {
        this.buff = buff;
    }
    protected override void ActivateEffect(TinyBot carrier)
    {
        TinyBot target = alwaysTargetSelf ? Owner : carrier;
        if (apply) target.Buffs.AddBuff(Owner, buff, potency);
        else target.Buffs.RemoveBuff(buff);
    }
}
