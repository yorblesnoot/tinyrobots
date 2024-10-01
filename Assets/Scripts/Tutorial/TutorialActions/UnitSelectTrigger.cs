using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectTrigger : TutorialAction
{
    [SerializeField] GameObject indicator;
    private void Awake()
    {
        indicator.SetActive(false);
    }
    public override IEnumerator Execute()
    {
        indicator.SetActive(true);
        yield return new WaitUntil(() => UnitControl.PlayerControlledBot != null);
        indicator.SetActive(false);
    }
}
