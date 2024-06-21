using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapScanner
{
    int[] sideLengths = new int[3];
    int mask;

    readonly int margin = 3;

    struct DirectedHit
    {
        public Vector3 position;
        public bool front;
    }

    public MapScanner()
    {
        mask = LayerMask.GetMask("Terrain");
    }

    readonly int[] dimensions = {0, 1, 2};
    public byte[,,] GetVoxelGrid(GameObject mapObject)
    {
        Vector3Int mapBounds = mapObject.GetComponent<MapBounds>().GetMapSize();
        sideLengths[0] = mapBounds.x;
        sideLengths[1] = mapBounds.y;
        sideLengths[2] = mapBounds.z;
        byte[,,] outputGrid = new byte[mapBounds.x, mapBounds.y, mapBounds.z];
        for (int i = 0; i <= 2; i++)
        {
            ScanDimension(outputGrid, i);
        }
        
        return outputGrid;
    }

    private void ScanDimension(byte[,,] outputGrid, int castDimension)
    {
        List<int> targetDimensions = dimensions.ToList();
        targetDimensions.Remove(castDimension);
        int cSize = sideLengths[castDimension];
        Vector3 rayDirection = Vector3.zero;
        rayDirection[castDimension] = 1;
        for (int a = 0; a < sideLengths[targetDimensions[0]]; a++)
        {
            for (int b = 0; b < sideLengths[targetDimensions[1]]; b++)
            {
                DoubleCastScan(a, b);
            }
        }

        void DoubleCastScan(int a, int b)
        {
            List<DirectedHit> directedHits = GetHitProfile(a, b, castDimension);

            directedHits = directedHits.OrderBy(hit => hit.position[castDimension]).ToList();
            foreach (DirectedHit hit in directedHits)
            {
                Debug.DrawLine(hit.position, hit.position + rayDirection / 5, hit.front ? Color.green : Color.red, 20f);
            }

            int hitcount = 0;
            for (int c = 0; c < cSize; c++)
            {
                while (directedHits.Count > 0 && c > directedHits[0].position[castDimension])
                {
                    hitcount += directedHits[0].front ? 1 : -1;
                    directedHits.RemoveAt(0);
                }
                
                if (hitcount > 0)
                {
                    AssignGridCoord(c);
                }
            }

            void AssignGridCoord(int c)
            {
                if (c < 0 || c >= cSize) return;
                Vector3Int coord = Vector3Int.zero;
                coord[targetDimensions[0]] = a;
                coord[targetDimensions[1]] = b;
                coord[castDimension] = c;
                outputGrid[coord.x, coord.y, coord.z] = 1;
                Debug.DrawLine(coord, coord + rayDirection / 5, Color.blue, 20f);
            }
        }
        List<DirectedHit> GetHitProfile(int a, int b, int dimension)
        {
            List<DirectedHit> directedHits = new();
            Vector3 origin = Vector3.zero;
            origin[targetDimensions[0]] = a;
            origin[targetDimensions[1]] = b;
            origin[dimension] = -margin;

            Vector3 end = origin;
            end[dimension] = cSize + margin;


#pragma warning disable UNT0028 // Use non-allocating physics APIs
            RaycastHit[] fronts = Physics.RaycastAll(origin, rayDirection, cSize + margin * 2, mask);
            RaycastHit[] backs = Physics.RaycastAll(end, -rayDirection, cSize + margin * 2, mask);
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

    

    
}
