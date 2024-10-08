using UnityEngine;

public class TutorialMission : Mission
{
    public override bool MetVictoryCondition()
    {
        return TutorialSequence.Main.Complete;
    }

    protected override void InitializeMission()
    {
        base.InitializeMission();
        StartCoroutine(TutorialSequence.Main.Execute());
    }
}
