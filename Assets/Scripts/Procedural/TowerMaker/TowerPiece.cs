using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPiece : MonoBehaviour
{
    [SerializeField] Transform doors;
    public List<Orientation> orientations;

    public void Initialize()
    {

    }

    public class Orientation
    {
        public float rotationAngle;
        public List<Vector2Int> floorPositions, doorPositions;
    }
}
