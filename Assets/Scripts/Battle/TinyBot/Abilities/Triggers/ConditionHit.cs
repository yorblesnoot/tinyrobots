using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionHit : TriggerCondition
{
    public ConditionHit(TinyBot target) : base(target)
    {
        target.ReceivedHit.AddListener(ExecuteTrigger);
    }

    public override void Remove()
    {
        Target.ReceivedHit.RemoveListener(ExecuteTrigger);
    }
}
