using MeetAndTalk;
using System.Collections;
using UnityEngine;

public class TutorialDialogue : TutorialAction
{
    [SerializeField] string startId;
    [SerializeField] DialogueContainerSO DialogueSO;
    bool dialogueComplete = false;
    public override IEnumerator Execute()
    {
        PrimaryCursor.TogglePlayerLockout(true);
        dialogueComplete = false;
        DialogueManager.Instance.StartDialogue(DialogueSO, startId);
        DialogueManager.Instance.EndDialogueEvent.AddListener(FinishDialogue);
        yield return new WaitUntil(() => dialogueComplete);
    }

    void FinishDialogue()
    {
        PrimaryCursor.TogglePlayerLockout(false);
        dialogueComplete = true;
    }
}
