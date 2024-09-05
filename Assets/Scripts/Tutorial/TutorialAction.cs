using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TutorialAction : MonoBehaviour
{
    public abstract IEnumerator Execute();
}
