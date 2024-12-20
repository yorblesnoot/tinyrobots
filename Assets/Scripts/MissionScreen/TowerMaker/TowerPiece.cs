using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerPiece : MonoBehaviour
{
    public TowerRoom[] rooms;
    public List<Orientation> orientations;
    [HideInInspector] public int pieceIndex;
    public void GeneratePlacementData(int pieceSize)
    {
        Orientation baseOrientation = new() { rotationAngle = 0, floorPositions = new(), doorPositions = new(), anchorPositions = new() };
        foreach (var room in rooms)
        {
            //get the room's local position
            Vector3 localPosition = room.transform.localPosition;
            Vector2 flatLocal = new(localPosition.x, localPosition.z);
            Vector2Int gridPosition = Vector2Int.RoundToInt(flatLocal);
            gridPosition /= pieceSize;

            gridPosition.x = gridPosition.x * 3 + 1;
            gridPosition.y = gridPosition.y * 3 + 1;

            baseOrientation.floorPositions.Add(gridPosition);
            baseOrientation.doorPositions.AddRange(room.GetDoorPositions().Select(door => door + gridPosition));
            baseOrientation.anchorPositions.AddRange(room.GetAnchorPositions().Select(anchor => anchor + gridPosition));
        }

        DeriveOrientations(baseOrientation);
    }

    void DeriveOrientations(Orientation baseOrientation)
    {
        orientations = new() { baseOrientation };
        for(int i = 1; i < 4; i++)
        {
            int rotationAngle = i * 90;
            Orientation rotated = new()
            {
                rotationAngle = rotationAngle,
                doorPositions = baseOrientation.doorPositions.Select(door => Vector2Int.RoundToInt(Rotate(door, rotationAngle))).ToList(),
                anchorPositions = baseOrientation.anchorPositions.Select(anchor => Vector2Int.RoundToInt(Rotate(anchor, rotationAngle))).ToList(),
                floorPositions = baseOrientation.floorPositions.Select(floor => Vector2Int.RoundToInt(Rotate(floor, rotationAngle))).ToList()
            };
            orientations.Add(rotated);
        }
    }

    public static Vector2 Rotate(Vector2 v, float degrees)
    {
        float delta = Mathf.Deg2Rad * degrees;
        return new Vector2(
            v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
            v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
        );
    }

    public class Orientation
    {
        public int rotationAngle;
        public List<Vector2Int> floorPositions, doorPositions, anchorPositions;
    }
}
