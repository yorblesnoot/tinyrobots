using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerPiece : MonoBehaviour
{
    [SerializeField] TowerRoom[] rooms;
    public List<Orientation> orientations;

    private void Awake()
    {
        Initialize(4);
    }
    public void Initialize(int pieceSize)
    {
        Orientation baseOrientation = new() { rotationAngle = 0, floorPositions = new(), doorPositions = new() };
        Debug.Log("rooms: ");
        foreach (var room in rooms)
        {
            //get the room's local position
            Vector3 localPosition = room.transform.localPosition;
            Vector2 flatLocal = new(localPosition.x, localPosition.z);
            Vector2Int gridPosition = Vector2Int.RoundToInt(flatLocal);
            gridPosition /= pieceSize;

            gridPosition.x = gridPosition.x * 3 + 1;
            gridPosition.y = gridPosition.y * 3 + 1;

            List<Vector2Int> doors = room.GetDoorPositions();
            baseOrientation.floorPositions.Add(gridPosition);
            baseOrientation.doorPositions.AddRange(doors.Select(door => door + gridPosition));

            Debug.Log(gridPosition);
            
        }

        Debug.Log("doors: ");
        foreach(var door in baseOrientation.doorPositions)
        {
            Debug.Log(door);
        }
    }

    public class Orientation
    {
        public float rotationAngle;
        public List<Vector2Int> floorPositions, doorPositions;
    }
}
