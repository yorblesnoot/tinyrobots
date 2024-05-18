using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerBuilder : MonoBehaviour
{
    [SerializeField] PlayerNavigator playerNavigator;
    [SerializeField] List<TowerPiece> bodyPieces;
    [SerializeField] List<TowerPiece> sidePieces;
    [SerializeField] List<TowerPiece> cornerPieces;
    [SerializeField] int sideLength = 6, pieceSize = 4;
    [SerializeField] float circleRadius = 3;
    Dictionary<Vector2Int, MapNode> mapGrid;
    Dictionary<PlacedRoom, TowerNavigableZone> zoneRooms;

    Vector2Int[] corners, sides, body;
    static readonly Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };

    private void Start()
    {
        BuildTowerFloor();
    }

    void BuildTowerFloor()
    {
        zoneRooms = new();
        GenerateGrid();
        DefineCorners();
        DefineSides();
        DefineBody();
        var ends = GetEndPoints();
        List<MapNode> path = GetMazePath(ends.Item1, ends.Item2);
        DebugPath(path);
        GeneratePieceMap(cornerPieces, corners, path);
        GeneratePieceMap(sidePieces, sides, path);
        GeneratePieceMap(bodyPieces, body, path);

        GeneratePieceMap(cornerPieces, corners);
        GeneratePieceMap(bodyPieces, body);
        GeneratePieceMap(sidePieces, sides);
        PopulateNavigableZone();
        PlacePlayer(path);
    }

    void PlacePlayer(List<MapNode> path)
    {
        TowerNavigableZone zone = zoneRooms[path[0].room];
        playerNavigator.transform.position = zone.unitPosition;
        playerNavigator.occupiedZone = zone;
        zone.Reveal(zone.unitPosition);
        zone.RevealNeighbors();
    }

    void PopulateNavigableZone()
    {
        foreach (var room in zoneRooms.Keys)
        {
            if(room.connectedNodes.Count == 0) continue;
            foreach (var connected in room.connectedNodes)
            {
                TowerNavigableZone connectedZone = zoneRooms[connected.room];
                
                zoneRooms[room].neighbors.Add(connectedZone);
            }
        }
    }

    void DefineBody()
    {
        body = mapGrid.Keys.Where(node => !sides.Contains(node) && !corners.Contains(node)).ToArray();
    }

    void GeneratePieceMap(List<TowerPiece> piecePool, Vector2Int[] targetNodes = default, List<MapNode> path = null)
    {
        foreach (var piece in piecePool) piece.GeneratePlacementData(pieceSize);
        List<TowerPiece> legalPieces = new(piecePool);
        while(legalPieces.Count > 0)
        {
            //pick a piece from the list
            int pieceIndex = Random.Range(0, legalPieces.Count);
            TowerPiece piece = legalPieces[pieceIndex];

            bool foundPlace = PlacePieceIfPossible(piece, path, targetNodes);
            if (!foundPlace)
            {
                Debug.Log("banned " + piece.name);
                legalPieces.Remove(piece);
            }
        }
    }

    bool PlacePieceIfPossible(TowerPiece piece, List<MapNode> path, Vector2Int[] targetNodes)
    {
        if (targetNodes == default) targetNodes = mapGrid.Keys.ToArray();
        foreach (var nodeCoords in targetNodes)
        {
            MapNode node = mapGrid[nodeCoords];
            foreach (TowerPiece.Orientation orientation in piece.orientations)
            {
                List<PlacedRoom> placedRooms = VerifyPiecePlacableAt(node, orientation, path, targetNodes);
                if (placedRooms == null) continue;
                PlacePiece(piece, node, orientation, placedRooms);
                return true;
            }
        }
        return false;
    }

    void PlacePiece(TowerPiece piece, MapNode node, TowerPiece.Orientation orientation, List<PlacedRoom> placedRooms)
    {
        Quaternion rotation = Quaternion.Euler(0, -orientation.rotationAngle, 0);
        GameObject spawned = Instantiate(piece, GetWorldVector(node.position), rotation).gameObject;
        TowerNavigableZone zone = spawned.GetComponent<TowerNavigableZone>();
        zone.Initialize();
        
        foreach (PlacedRoom room in placedRooms)
        {
            zoneRooms.Add(room, zone);
            MapNode targetNode = room.node;
            targetNode.room = room;
        }
    }

    List<PlacedRoom> VerifyPiecePlacableAt(MapNode baseNode, TowerPiece.Orientation orientation, List<MapNode> path, Vector2Int[] targetNodes)
    {
        List<MapNode> roomsOccupiedByPiece = new();
        List<PlacedRoom> potentialRooms = new();

        foreach (var floor in orientation.floorPositions)
        {
            Vector2Int worldFloor = floor / 3 + baseNode.position;

            if (!targetNodes.Contains(worldFloor)) return null;
            if (!mapGrid.TryGetValue(worldFloor, out MapNode floorNode)) return null;
            if(floorNode.blocked || floorNode.room != null) return null;

            PlacedRoom room = new() { doors = orientation.doorPositions.Select(door => door - floor).Where(next => next.magnitude == 1).ToList(),
                anchors = orientation.anchorPositions.Select(anchor => anchor - floor).Where(next => next.magnitude == 1).ToList(),
                position = worldFloor, node = floorNode };
            
            potentialRooms.Add(room);
            roomsOccupiedByPiece.Add(floorNode);
        }

        if (path != null)
        {
            int onPath = potentialRooms.Where(room => path.Contains(room.node)).Count();
            if(onPath == 0) return null;
        }
       
       
        foreach(var placed in potentialRooms)
        {
            bool doorsGood = EvaluateDoorFlow(path, roomsOccupiedByPiece, placed);
            if (doorsGood) continue;
            else return null;
        }
        return potentialRooms;
    }

    bool EvaluateDoorFlow(List<MapNode> path, List<MapNode> roomsOccupiedByPiece, PlacedRoom placed)
    {
        placed.connectedNodes = new();
        foreach (MapNode neighbor in placed.node.neighbors)
        {
            if (roomsOccupiedByPiece.Contains(neighbor)) continue;

            Vector2 rawDirection = (neighbor.position - placed.node.position);
            rawDirection.Normalize();
            Vector2Int doorDirection = Vector2Int.RoundToInt(rawDirection);


            if(path != null)
            {
                int thisNodeIndex = path.IndexOf(placed.node);
                int neighborIndex = path.IndexOf(neighbor);
                if(thisNodeIndex > -1 
                && neighborIndex > -1
                && Mathf.Abs(thisNodeIndex - neighborIndex) == 1
                && !placed.doors.Contains(doorDirection)) return false;
            }


            if (placed.anchors.Contains(doorDirection))
            {
                if (neighbor.blocked) return false;
            }

            if (placed.doors.Contains(doorDirection))
            {
                placed.connectedNodes.Add(neighbor);
                if (neighbor.blocked) return false;
                if (neighbor.room == null) continue;
                if (!neighbor.room.doors.Contains(-doorDirection)) return false;
            }
            else
            {
                if (neighbor.blocked) continue;
                if (neighbor.room == null) continue;
                if (neighbor.room.doors.Contains(-doorDirection)) return false;
            }
        }
        return true;
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
                node.neighbors.Add(neighbor);
            }
        }

        bool IsCellOutsideCircle(Vector2Int coord)
        {
            if (Vector2.Distance(coord, center) > circleRadius)
            {
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

    private void DefineCorners()
    {
        int innerCorner = Mathf.RoundToInt(Mathf.Sqrt(circleRadius * circleRadius * .5f));
        int outerCorner = sideLength - (innerCorner + 1);
        corners = new Vector2Int[4];
        corners[0] = new(innerCorner, innerCorner);
        corners[1] = new(innerCorner, outerCorner);
        corners[2] = new(outerCorner, innerCorner);
        corners[3] = new(outerCorner, outerCorner);
    }

    void DefineSides()
    {
        float halfSide = (sideLength - 1) / 2f;
        int longSide = Mathf.RoundToInt(halfSide + circleRadius);
        int shortside = Mathf.RoundToInt(halfSide - circleRadius);
        Debug.Log(halfSide + ", " + circleRadius);
        Debug.Log(shortside + ", " + longSide);
        Vector2Int[] targetNodes = mapGrid.Keys.ToArray();
        targetNodes = targetNodes.Where(node => !mapGrid[node].blocked)
            .Where(node => node.x == shortside || node.x == longSide || node.y == shortside || node.y == longSide).ToArray();
        sides = targetNodes;
        sides.DebugContents();
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
            List<MapNode> possibleNext = node.neighbors.Where(x => !x.blocked && !visited.Contains(x)).ToList();
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
        public List<Vector2Int> doors, anchors;
        public List<MapNode> connectedNodes;
    }
}
