
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralMapGenerator : MonoBehaviour
{
    int seed1;
    int seed2;
    int seed3;
    float offset = .1f;
    private void Awake()
    {
        seed1 = Random.Range(0, 999999);
        seed2 = Random.Range(0, 999999);
        seed3 = Random.Range(0, 999999);
    }
    public byte[,,] Generate(int mapSize, float solidThreshold)
    {
        byte[,,] mapGrid;
        mapGrid = new byte[mapSize, mapSize, mapSize];
        for(int x = 0; x < mapSize; x++)
        {
            for(int y = 0; y < mapSize; y++)
            {
                for(int z = 0; z < mapSize; z++)
                {
                    float noise = Perlin3D(offset + x + seed1, offset + y + seed2, offset + z + seed3);
                    Debug.Log(noise);
                    if (noise > solidThreshold) mapGrid[x, y, z] = 1;
                }
            }
        }
        return mapGrid;
    }

    //dunno how this works. copied it from somewhere.
    public static float Perlin3D(float x, float y, float z)
    {
        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);

        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }

}
