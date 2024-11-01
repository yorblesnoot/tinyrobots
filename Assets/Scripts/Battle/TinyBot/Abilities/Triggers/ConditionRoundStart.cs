using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionRoundStart : TriggerCondition
{
    public ConditionRoundStart(TinyBot target) : base(target)
    {
        TurnManager.RoundEnded.AddListener(ExecuteTrigger);
    }

    

    public override void Remove()
    {
        TurnManager.RoundEnded.RemoveListener(ExecuteTrigger);
    }
}
