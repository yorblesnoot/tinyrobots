using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Ability : MonoBehaviour
{
    public Sprite icon;
    protected bool targeting;

    [SerializeField] protected int maxRange;
    [SerializeField] protected GameObject emissionPoint;


    public abstract void ExecuteAbility(TinyBot user, Vector3 target);
    public abstract void ControlTargetLine();

    public virtual void ToggleTargetLine(bool on)
    {
        targeting = on;
        if (on == false) LineMaker.HideLine();
    }

    private void Update()
    {
        if (!targeting) return;
        ControlTargetLine();
    }

    

} 
