public class TutorialMission : Mission
{
    public override bool MetEndCondition(TurnManager turnManager)
    {
        return TutorialSequence.Main.Complete;
    }

    protected override void InitializeMission()
    {
        base.InitializeMission();
        StartCoroutine(TutorialSequence.Main.Execute());
    }
}
