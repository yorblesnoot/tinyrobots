using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TargetPoint : MonoBehaviour
{
    public abstract List<Targetable> FindTargets(Vector3 point);

    public abstract List<Targetable> FindTargetsAI(Vector3 point);
    public abstract void Draw(Vector3 point);

    public abstract void EndTargeting();
}
