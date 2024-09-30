using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMission : Mission
{
    public override bool MetEndCondition(TurnManager turnManager)
    {
        return TutorialSequence.Instance.Complete;
    }

    protected override void InitializeMission()
    {
        base.InitializeMission();
        TutorialSequence.Begin();
    }
}
