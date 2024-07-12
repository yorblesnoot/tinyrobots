using UnityEngine;

public abstract class IKAnimation : AnimationController
{
    [SerializeField] protected Transform ikTarget;
}
