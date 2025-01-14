using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetPoint : MonoBehaviour
{
    public float TargetRadius = 0;
    public abstract List<Targetable> FindTargets(List<Vector3> trajectory);

    public abstract List<Targetable> FindTargetsAI(List<Vector3> trajectory);
    public abstract void Draw(List<Vector3> trajectory);

    public abstract void EndTargeting();

    public abstract bool TargetIsAttained(Vector3 position, List<Vector3> trajectory);
}
