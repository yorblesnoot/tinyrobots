using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetingParadigm : MonoBehaviour
{
    [SerializeField] protected GameObject reticle;
    bool active;
    void Update()
    {
        if (!active) return;
        ManageReticle();
    }

    public virtual void Toggle(bool on)
    {
        active = on;
        reticle.SetActive(on);
    }

    protected abstract void ManageReticle();
}
