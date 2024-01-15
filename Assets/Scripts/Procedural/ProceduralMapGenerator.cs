
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class ProceduralMapGenerator : MonoBehaviour
{
    [SerializeField] int mapSize;
    [SerializeField] float scale = 1;
    [SerializeField][Range(.4f, .6f)] float solidThreshold = .5f;

    int seed1;
    int seed2;
    int seed3;
    float offset = .1f;
    [SerializeField] [Range(0, 1)] float frequency;
    [SerializeField] bool spherize;
    private void Awake()
    {
        seed1 = Random.Range(0, 999999);
        seed2 = Random.Range(0, 999999);
        seed3 = Random.Range(0, 999999);
    }
    public byte[,,] Generate()
    {
        int bufferedSize = mapSize + 2;
        byte[,,] mapGrid;
        mapGrid = new byte[bufferedSize, bufferedSize, bufferedSize];
        Vector3 cubeCenter = new(bufferedSize /2, bufferedSize /2, bufferedSize /2);
        for (int x = 1; x < mapSize - 1; x++)
        {
            for(int y = 1; y < mapSize - 1; y++)
            {
                for(int z = 1; z < mapSize - 1; z++)
                {
                    if (spherize && (cubeCenter - new Vector3(x, y, z)).magnitude > mapSize/2) continue;
                    float noise = Perlin3D(Modify(x, seed1), Modify(y, seed2), Modify(z, seed3));
                    if (noise > solidThreshold) mapGrid[x, y, z] = 1;
                }
            }
        }
        return mapGrid;
    }


    public float Modify(int val, int seed)
    {
        return (offset + val + seed) * frequency;
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
