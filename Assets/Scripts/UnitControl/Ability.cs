using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Ability : MonoBehaviour
{
    public Sprite icon;
    public abstract void ActivateAbility(TinyBot user, Vector3 target);
} 
