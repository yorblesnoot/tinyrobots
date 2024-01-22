using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearTargeting : TargetingParadigm
{
    protected override void ManageReticle()
    {
        reticle.transform.LookAt(PrimaryCursor.Transform.position);
    }

    public override void Toggle(bool on)
    {
        base.Toggle(on);
    }
}
