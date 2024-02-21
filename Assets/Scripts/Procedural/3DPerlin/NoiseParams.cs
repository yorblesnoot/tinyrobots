using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NoiseParams", menuName = "ScriptableObjects/NoiseParams")]
public class NoiseParams : ScriptableObject
{
    public int xSize;
    public int ySize;
    public int zSize;
    [Range(-2f, 2f)] public float bottomSkew = 1f;
    [Range(-2f, 2f)] public float centerSkew = 1f;

    [Range(0, 1f)] public float bottomSolidThreshold = .5f;
    [Range(0, 1f)] public float topSolidThreshold = .5f;

    [Range(0, 1f)] public float frequency;
    public int sizeBuffer = 1;
}
