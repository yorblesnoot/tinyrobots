using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProceduralAnimation : MonoBehaviour
{
    [SerializeField] protected Transform ikTarget;

    public abstract IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets);
    
}
