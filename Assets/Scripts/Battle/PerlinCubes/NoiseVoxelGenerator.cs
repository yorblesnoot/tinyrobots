using UnityEngine;

public class NoiseVoxelGenerator : MapGenerator
{

    int seed1;
    int seed2;
    int seed3;
    float offset = .1f;

    public NoiseParams np;
    public static byte[,,] mapGrid;

    Vector2 flatCenter;
    private void Awake()
    {
        seed1 = Random.Range(0, 999999);
        seed2 = Random.Range(0, 999999);
        seed3 = Random.Range(0, 999999);
    }

    public override byte[,,] GetByteMap()
    {
        return mapGrid;
    }
    public override void GenerateCoreMap()
    {
        flatCenter = new(np.xSize / 2, np.zSize / 2);
        mapGrid = new byte[np.xSize + np.sizeBuffer * 2, np.ySize + np.sizeBuffer * 2, np.zSize + np.sizeBuffer * 2];
        for (int x = np.sizeBuffer; x < np.xSize - np.sizeBuffer; x++)
        {
            for(int y = np.sizeBuffer; y < np.ySize - np.sizeBuffer; y++)
            {
                for(int z = np.sizeBuffer; z < np.zSize - np.sizeBuffer; z++)
                {
                    float noise = Perlin3D(Modify(x, seed1), Modify(y, seed2), Modify(z, seed3));
                    if (noise > GetSolidThreshold(x,y,z)) mapGrid[x, y, z] = 1;
                }
            }
        }
    }

    float GetSolidThreshold(int x, int y, int z)
    {
        float heightFactor = (float)y / np.ySize;
        Vector2 flatPoint = new(x, z);
        float centerFactor = 1 - Vector2.Distance(flatPoint, flatCenter) / (np.xSize / 2);
        return Mathf.Lerp(np.bottomSolidThreshold, np.topSolidThreshold, heightFactor * np.bottomSkew - centerFactor * np.centerSkew);
    }


    public float Modify(int val, int seed)
    {
        return (offset + val + seed) * np.frequency;
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

    public override void PlaceSecondaries()
    {
        
    }
}
