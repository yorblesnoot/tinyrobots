using MeetAndTalk;
using System.Collections;
using UnityEngine;

public class TutorialDialogue : TutorialAction
{
    [SerializeField] DialogueContainerSO DialogueSO;
    bool dialogueComplete = false;
    public override IEnumerator Execute()
    {
        dialogueComplete = false;
        DialogueManager.Instance.StartDialogue(DialogueSO);
        DialogueManager.Instance.EndDialogueEvent.AddListener(() => dialogueComplete = true);
        yield return new WaitUntil(() => dialogueComplete);
    }
}
