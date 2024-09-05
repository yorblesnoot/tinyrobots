using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMission : Mission
{
    public TutorialSequence TutorialSequence;
    public override bool MetEndCondition(TurnManager turnManager)
    {
        return TutorialSequence.Complete;
    }

    protected override void InitializeMission()
    {
        TutorialSequence.Begin();
    }
}
