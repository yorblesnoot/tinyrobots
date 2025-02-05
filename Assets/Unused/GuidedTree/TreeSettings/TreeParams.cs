using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "TreeParams", menuName = "ScriptableObjects/TreeParams")]
public class TreeParams : ScriptableObject
{
    public int mapSize = 30;

    public int sphereLevels = 3;
    public int sphereBoost;
    public int inflationFactor = 2;

    public float jitterSize = .5f;

    public FirstIteration initialGeneration;
    public Iteration[] iterations;
    public Iteration[] leafIterations;

    [System.Serializable]
    public class Iteration
    {
        public int branches;
        [Range(-100, 100)] public int rotationFactor = 5;
        [Range(1, 50)] public int rotationChangeThreshold = 5;
        [Range(-100, 100)] public int secondaryRotationFactor = 5;
        public int surfaceRadius;
        [Range(0f, .95f)] public float inclusionZone;
    }

    [System.Serializable]
    public class FirstIteration
    {
        [Range(-50, 50)] public int rotationFactor;
        public int canopyCenterHeight, canopyRadius, branches;
    }
}
