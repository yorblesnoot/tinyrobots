using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSequence : MonoBehaviour
{
    [SerializeField] List<TutorialAction> Actions;

    public bool Complete { get; private set; } = false;

    public void Begin()
    {
        StartCoroutine(RunSequence());
    }

    IEnumerator RunSequence()
    {
        foreach (var action in Actions)
        {
            yield return action.Execute();
        }
        Complete = true;
    }
}
