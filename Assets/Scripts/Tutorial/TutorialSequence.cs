using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSequence : MonoBehaviour
{
    [SerializeField] List<TutorialAction> Actions;
    public static TutorialSequence Instance;

    public bool Complete { get; private set; } = false;
    private void Awake()
    {
        Instance = this;
    }
    public static void Begin()
    {
        Instance.StartCoroutine(Instance.RunSequence());
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
