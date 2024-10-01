using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSequence : TutorialAction
{
    [SerializeField] bool isMainSequence = false;
    [SerializeField] List<TutorialAction> actions;
    public static TutorialSequence Main;

    public bool Complete { get; private set; } = false;
    private void Awake()
    {
        if(isMainSequence) Main = this;
    }

    IEnumerator RunSequence()
    {
        foreach (var action in actions)
        {
            yield return action.Execute();
        }
        Complete = true;
    }

    public override IEnumerator Execute()
    {
        StartCoroutine(RunSequence());
        yield break;
    }
}
