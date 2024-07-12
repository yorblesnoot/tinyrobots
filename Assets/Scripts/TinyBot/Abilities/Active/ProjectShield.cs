using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectShield : ActiveAbility
{
    [SerializeField] ToggleAnimation shieldToggle;
    protected override IEnumerator PerformEffects()
    {
        yield break;
    }

    public override void EndAbility()
    {
        shieldToggle.Stop();
    }

}
