using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TreeParams", menuName = "ScriptableObjects/TreeRenderParams")]
public class TreeRenderParams : ScriptableObject
{
    public int numberOfPanelsPerRing;
    public AnimationCurve thicknessCurve;
}
