using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetPoint : MonoBehaviour
{
    [SerializeField] protected float TargetRadius = 0;
    public virtual float AddedRange => TargetRadius;
    public abstract bool UseLine { get; }

    public abstract List<Targetable> FindTargets(List<Vector3> trajectory);
    public abstract void Draw(List<Vector3> trajectory);

    public abstract void Hide();

    
}
