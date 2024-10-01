using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowStatTrigger : TutorialAction
{
    [SerializeField] int threshold = 3;
    [SerializeField] StatType statType;
    public override IEnumerator Execute()
    {
        yield return new WaitUntil(MovementLow);
    }

    bool MovementLow()
    {
        if(UnitControl.PlayerControlledBot == null) return false;
        if (UnitControl.PlayerControlledBot.Stats.Current[statType] > threshold) return false;
        return true;
    }
}
