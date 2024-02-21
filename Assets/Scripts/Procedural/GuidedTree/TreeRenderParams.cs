using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TreeParams", menuName = "ScriptableObjects/TreeRenderParams")]
public class TreeRenderParams : ScriptableObject
{
    [Range(1f, 5f)] public float treeSizeMultiplier;
    public int numberOfPanelsPerRing;
    public AnimationCurve thicknessCurve;
    public float leafSizeVariance = .2f;
    public float leafPositionVariance = .2f;
    [Range(1f, 5f)] public float trunkThicknessMultiplier;
    [Range(.01f, 1f)] public float leafThicknessMultiplier;
    

}
