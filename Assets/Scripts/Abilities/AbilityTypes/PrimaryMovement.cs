using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PrimaryMovement : MonoBehaviour
{
    public MoveStyle MoveStyle { get; protected set; }

    public abstract IEnumerator PathToPoint(TinyBot user, List<Vector3> path);
}
