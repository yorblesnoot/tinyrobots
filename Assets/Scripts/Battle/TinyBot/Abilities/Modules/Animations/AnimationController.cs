using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationController : MonoBehaviour
{
    public abstract IEnumerator Play(TinyBot owner, List<Vector3> trajectory, List<Targetable> targets);
}
