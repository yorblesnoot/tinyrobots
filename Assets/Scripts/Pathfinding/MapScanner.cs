using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class MapScanner
{
    int[] sideLengths = new int[3];
    int terrainMask;
    int interiorMask;
    string terrain = "Terrain";
    string interior = "TerrainInterior";
    readonly int margin = 3;
    readonly float sphereRadius = .0001f;
    readonly bool debugMode;

    struct DirectedHit
    {
        public Vector3 position;
        public bool front;
    }

    public MapScanner(bool debugMode)
    {
        terrainMask = LayerMask.GetMask(terrain);
        interiorMask = LayerMask.GetMask(interior);
        this.debugMode = debugMode;
    }

    readonly int[] dimensions = {0, 1, 2};
    public byte[,,] GetVoxelGrid(GameObject mapObject)
    {
        Vector3Int mapBounds = mapObject.GetComponent<MapBounds>().GetMapSize();
        sideLengths[0] = mapBounds.x;
        sideLengths[1] = mapBounds.y;
        sideLengths[2] = mapBounds.z;
        byte[,,] outputGrid = new byte[mapBounds.x, mapBounds.y, mapBounds.z];
        for (int i = 0; i <= 2; i++) ScanDimension(outputGrid, i);
        MeshCollider[] interiors = mapObject.GetComponentsInChildren<MeshCollider>().Where(i => i.gameObject.layer == LayerMask.NameToLayer(interior)).ToArray();
        foreach (MeshCollider interior in interiors) interior.gameObject.layer = LayerMask.NameToLayer(terrain);
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
            if(directedHits.Count == 0) return;

            directedHits = directedHits.OrderBy(hit => hit.position[castDimension]).ToList();
            if(debugMode) foreach (DirectedHit hit in directedHits) 
                Debug.DrawLine(hit.position, hit.position + rayDirection / 5, hit.front ? Color.green : Color.red, 20f);
            

            int index = 0;
            bool hitBackFace = false;
            DirectedHit lastFront = directedHits[0].front ? directedHits[0] : default;
            while(index < directedHits.Count)
            {
                DirectedHit hit = directedHits[index];
                if (hit.front && hitBackFace)
                {
                    MarkInteriorRegion(Mathf.RoundToInt(directedHits[index - 1].position[castDimension]));
                    lastFront = directedHits[index];
                }
                hitBackFace = !hit.front;
                index++;
            }
            if (hitBackFace) MarkInteriorRegion(Mathf.RoundToInt(directedHits[index - 1].position[castDimension]));
            else MarkInteriorRegion(cSize - 1);

            void MarkInteriorRegion(int end)
            {
                for (int c = Mathf.RoundToInt(lastFront.position[castDimension]); c < end; c++)
                {
                    if (c < 0 || c >= cSize) return;
                    Vector3Int coord = Vector3Int.zero;
                    coord[targetDimensions[0]] = a;
                    coord[targetDimensions[1]] = b;
                    coord[castDimension] = c;
                    outputGrid[coord.x, coord.y, coord.z] = 1;
                    if (debugMode) Debug.DrawLine(coord, coord + rayDirection / 5, Color.blue, 20f);
                }
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
            float rayLength = cSize + margin * 2;
            GetHits(terrainMask, Physics.SphereCastAll);
            GetHits(interiorMask, ChainSphereCast);
            return directedHits;

            void GetHits(int mask, Func<Vector3, float, Vector3, float, int, RaycastHit[]> cast)
            {
                RaycastHit[] fronts = cast(origin, sphereRadius, rayDirection, rayLength, mask);
                RaycastHit[] backs = cast(end, sphereRadius, -rayDirection, rayLength, mask);
                directedHits.AddRange(ParseHits(fronts, true));
                directedHits.AddRange(ParseHits(backs, false));
            }

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

    List<DirectedHit> CleanRedundantHits(List<DirectedHit> hits)
    {
        List<DirectedHit> output = new();
        bool frontMode = false;
        int index = 0;
        while (index < hits.Count)
        {
            DirectedHit hit = hits[index];
            if (hit.front)
            {
                if (!frontMode)
                {
                    int last = index - 1;
                    if (last >= 0) output.Add(hits[last]);
                    output.Add(hit);
                    frontMode = true;
                }
            }
            else
            {
                frontMode = false;
            }
            index++;
        }
        if (!frontMode && index > 0) output.Add(hits[index - 1]);
        return output;
    }

    RaycastHit[] ChainSphereCast(Vector3 origin, float radius, Vector3 direction, float maxDistance, int mask)
    {
        List<RaycastHit> hits = new();
        while (Physics.Raycast(origin, direction, out RaycastHit hitPoint, maxDistance, mask))
        {
            origin = hitPoint.point + direction * radius;
            maxDistance -= hitPoint.distance;
            hits.Add(hitPoint);
        }
        return hits.ToArray();
    }

}
