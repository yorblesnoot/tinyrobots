using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapScanner : MonoBehaviour
{
    int xSize, ySize, zSize;
    int mask;

    struct DirectedHit
    {
        public Vector3 position;
        public bool front;
    }

    private void Awake()
    {
        mask = LayerMask.GetMask("Terrain");
    }

    public byte[,,] GetVoxelGrid(GameObject mapObject)
    {
        Vector3Int mapBounds = mapObject.GetComponent<MapBounds>().GetMapSize();
        xSize = mapBounds.x;
        ySize = mapBounds.y;
        zSize = mapBounds.z;
        byte[,,] outputGrid = new byte[xSize, ySize, zSize];
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                DoubleCastScan(x, y, outputGrid);
            }
        }
        return outputGrid;
    }

    private void DoubleCastScan(int x, int y, byte[,,] grid)
    {
        List<DirectedHit> directedHits = GetHitProfile(x, y);
        bool[] finalProfile = new bool[zSize + 1];
        foreach (DirectedHit hit in directedHits)
        {
            int snappedZ = Mathf.RoundToInt(hit.position.z);
            finalProfile[snappedZ] = hit.front;
            Debug.DrawLine(hit.position, hit.position + Vector3.up / 10, hit.front ? Color.red : Color.yellow, 20f);
        }

        directedHits = directedHits.OrderBy(hit => hit.position.z).ToList();

        int hitcount = 0;
        for(int z =  0; z < zSize; z++)
        {
            while(directedHits.Count > 0 && z > directedHits[0].position.z)
            {
                hitcount += directedHits[0].front ? 1 : -1;
                directedHits.RemoveAt(0);
            }
            if (hitcount > 0)
            {
                grid[x, y, z] = 1;
                Vector3 debug = new(x, y, z);
                Debug.DrawLine(debug, debug + Vector3.up/20, Color.blue, 20f);
            }
        }
    }

    private List<DirectedHit> GetHitProfile(int x, int y)
    {
        List<DirectedHit> directedHits = new();
        Vector3 origin = new(x, y, 0);
        Vector3 end = new(x, y, zSize);
        RaycastHit[] fronts = Physics.RaycastAll(origin, Vector3.forward, zSize, mask);
        RaycastHit[] backs = Physics.RaycastAll(end, Vector3.back, zSize, mask);
        directedHits.AddRange(ParseHits(fronts, true));
        directedHits.AddRange(ParseHits(backs, false));
        return directedHits;

        static List<DirectedHit> ParseHits(RaycastHit[] hits, bool front)
        {
            List<DirectedHit> parsed = new();
            foreach (RaycastHit hit in hits)
            {
                DirectedHit parsedHit = new() { front = front, position = hit.point };
                parsed.Add(parsedHit);
            }
            return parsed;
        }
    }

    
}
