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
    public Iteration leafIteration;

    [System.Serializable]
    public class Iteration
    {
        public int branches;
        [Range(-10, 10)] public int rotationFactor = 5;
        public int branchLength;
        [Range(0f, .95f)] public float inclusionZone;
    }

    [System.Serializable]
    public class FirstIteration
    {
        [Range(-10, 10)] public int rotationFactor;
        public int canopyCenterHeight, canopyRadius, branches;
    }
}
