using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModulePrototype : MonoBehaviour
{
    enum Symmetry
    {
        NONE,
        BILATERAL,
        QUADRILATERAL
    }
    [SerializeField] Symmetry symmetry;
    public int PieceIndex;
    [HideInInspector] public int OrientationIndex;
    public int BaseWeight = 1;

    public void Initialize()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        OrientationIndex = Mathf.RoundToInt(euler.y / 90);
    }

    public List<int> GetImpliedOrientations()
    {
        List<int> output = new()
        {
            OrientationIndex
        };
        if (symmetry == Symmetry.BILATERAL) output.Add((OrientationIndex + 2) % 4);
        else if (symmetry == Symmetry.QUADRILATERAL) output = new() { 0, 1, 2, 3 };
        return output;
    }
}
