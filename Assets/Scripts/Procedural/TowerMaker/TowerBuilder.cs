using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerBuilder : MonoBehaviour
{
    [SerializeField] List<TowerPiece> pieces;
    [SerializeField] GameObject debugger;
    [SerializeField] int sideLength = 6;
    [SerializeField] float radiusDivisor = 2.1f;
    [SerializeField] int pieceSize = 4;
    Dictionary<Vector2Int, MapNode> mapGrid;

    Vector2Int[] corners;
    static readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    private void Awake()
    {
        BuildTowerFloor();
    }

    void BuildTowerFloor()
    {
        GenerateGrid();
        FindCorners();
        var ends = GetEndPoints();
        List<MapNode> path = GetMazePath(ends.Item1, ends.Item2);
        DebugPath(path);
        foreach (var piece in pieces) piece.GeneratePlacementData(pieceSize);
        EvaluatePieces(null);

    }

    void EvaluatePieces(List<MapNode> path)
    {
        while(pieces.Count > 0)
        {
            //pick a piece from the list
            int pieceIndex = Random.Range(0, pieces.Count);
            TowerPiece piece = pieces[pieceIndex];

            //test it on each square, in each of the 4 orientations ->
            bool foundPlace = PlacePieceIfPossible(piece);
            if (!foundPlace) pieces.Remove(piece);
        }
    }

    bool PlacePieceIfPossible(TowerPiece piece)
    {
        foreach (MapNode node in mapGrid.Values)
        {
            foreach (TowerPiece.Orientation orientation in piece.orientations)
            {
                List<PlacedRoom> placedRoom = TryToPlacePiece(node, orientation);
                if(placedRoom == null) continue;
                foreach (PlacedRoom room in placedRoom)
                {
                    MapNode targetNode = room.node;
                    targetNode.room = room;
                    Quaternion rotation = Quaternion.Euler(0, orientation.rotationAngle, 0);
                    Instantiate(piece, GetWorldVector(targetNode.position), rotation);
                    return true;
                }
            }
        }
        return false;
    }

    List<PlacedRoom> TryToPlacePiece(MapNode baseNode, TowerPiece.Orientation orientation, List<MapNode> path = null)
    {
        List<MapNode> containedNodes = new();

        List<PlacedRoom> potentialRooms = new();
        //find the corresponding doors for the room

        foreach (var floor in orientation.floorPositions)
        {
            //figure out where the floors and doors are located on the map grid
            Vector2Int worldFloor = floor / 3 + baseNode.position;
            List<Vector2Int> doors = orientation.doorPositions.Select(door => door - floor).Where(door => door.magnitude == 1).ToList();
            //if a floor is located on an occupied or blocked space, discard
            if (!mapGrid.TryGetValue(worldFloor, out MapNode floorNode)) return null;
            if(floorNode.blocked || floorNode.room != null) return null;

            PlacedRoom room = new() { doors = doors, position = worldFloor, node = floorNode };
            potentialRooms.Add(room);
            containedNodes.Add(floorNode);
        }

        //if a door is placed opposite a wall, discard
        foreach(var placed in potentialRooms)
        {
            foreach(MapNode neighbor in placed.node.neighbors)
            {
                if(containedNodes.Contains(neighbor)) continue;
                if(neighbor.room == null) continue;
                Vector2Int doorDirection = neighbor.room.position - placed.node.position;
                if(placed.doors.Contains(doorDirection) != neighbor.room.doors.Contains(-doorDirection)) return null;
            }
        }

        if (path == null) return potentialRooms;
        //if the edges of the piece that are intersected by the path don't have doors, discard

        //List<PlacedRoom> intersectedRooms = potentialRooms.Where(room => path.Contains(room.node)).ToList();

        return potentialRooms;
    }

    private void DebugPath(List<MapNode> path)
    {
        for (int i = 0; i < path.Count - 1; i++)
        {
            DrawLineBetweenNodes(path[i], path[i + 1], Color.magenta);
        }
    }

    private void DrawLineBetweenNodes(MapNode start, MapNode end, Color color)
    {
        Vector3 drawPos = GetWorldVector(start.position);
        Vector3 endPos = GetWorldVector(end.position);
        Debug.DrawLine(drawPos, endPos, color, 30f);
    }

    void GenerateGrid()
    {
        float halfLength = sideLength - 1;
        halfLength /= 2;
        Vector2 center = new(halfLength, halfLength);
        mapGrid = new();
        for (int x = 0; x < sideLength; x++)
        {
            for (int y = 0; y < sideLength; y++)
            {
                Vector2Int coord = new(x, y);
                MapNode node = new()
                {
                    position = coord,
                    blocked = IsCellOutsideCircle(coord),
                };
                mapGrid.Add(coord, node);
            }
        }

        foreach (MapNode node in mapGrid.Values)
        {
            node.neighbors = new();
            foreach (var direction in directions)
            {
                Vector2Int finalPosition = node.position + direction;
                if (!mapGrid.TryGetValue(finalPosition, out MapNode neighbor)) continue;
                if(neighbor.blocked) continue;
                node.neighbors.Add(neighbor);
            }
        }


        bool IsCellOutsideCircle(Vector2Int coord)
        {
            if (Vector2.Distance(coord, center) > sideLength/ radiusDivisor)
            {
                Instantiate(debugger, GetWorldVector(coord), Quaternion.identity);
                return true;
            }
            return false;
        }
    }

    private Vector3 GetWorldVector(Vector2Int node)
    {
        Vector3 world = new(node.x, 0, node.y);
        return world * pieceSize;
    }

    private void FindCorners()
    {
        corners = new Vector2Int[4];
        corners[0] = new(1, 1);
        corners[1] = new(1, sideLength - 2);
        corners[2] = new(sideLength - 2,1);
        corners[3] = new(sideLength - 2, sideLength - 2);
    }

    (Vector2Int, Vector2Int) GetEndPoints()
    {
        List<Vector2Int> cornerList = corners.ToList();
        while(cornerList.Count > 2)
        {
            int remove = Random.Range(0, cornerList.Count);
            cornerList.RemoveAt(remove);
        }
        return (cornerList[0], cornerList[1]);
    }

    List<MapNode> GetMazePath(Vector2Int start,  Vector2Int end)
    {
        Stack<MapNode> path = new();
        HashSet<MapNode> visited = new();

        MapNode origin = mapGrid[start];

        VisitNode(origin);
        return path.ToList();

        void VisitNode(MapNode node)
        {
            if (node == mapGrid[end])
            {
                path.Push(node);
                return;
            }

            visited.Add(node);
            List<MapNode> possibleNext = node.neighbors.Where(x => !visited.Contains(x)).ToList();
            MapNode targetNode;
            if (possibleNext.Count > 0)
            {
                path.Push(node);
                
                int neighborIndex = Random.Range(0, possibleNext.Count);
                targetNode = possibleNext[neighborIndex];
            }
            else
            {
                targetNode = path.Pop();
            }
            
            VisitNode(targetNode);
        }
    }


    class MapNode
    {
        public Vector2Int position;
        public bool blocked;
        public PlacedRoom room;
        public List<MapNode> neighbors;
    }

    class PlacedRoom
    {
        public Vector2Int position;
        public MapNode node;
        public List<Vector2Int> doors;
    }

}
