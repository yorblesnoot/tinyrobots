using UnityEngine;

public class NoiseVoxelGenerator : MapGenerator
{
    [SerializeField] int xSize;
    [SerializeField] int ySize;
    [SerializeField] int zSize;


    [SerializeField][Range(0, 1f)] float bottomSolidThreshold = .5f;
    [SerializeField][Range(0, 1f)] float topSolidThreshold = .5f;

    int seed1;
    int seed2;
    int seed3;
    float offset = .1f;
    [SerializeField] [Range(0, 1)] float frequency;
    [SerializeField] float scale = 1;
    public static float Scale;
    [SerializeField] int sizeBuffer = 1;

    public static byte[,,] mapGrid;
    private void Awake()
    {
        Scale = scale;
        transform.localScale = Vector3.one * scale;
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
        mapGrid = new byte[xSize + sizeBuffer * 2, ySize + sizeBuffer * 2, zSize + sizeBuffer * 2];
        for (int x = sizeBuffer; x < xSize - sizeBuffer; x++)
        {
            for(int y = sizeBuffer; y < ySize - sizeBuffer; y++)
            {
                for(int z = sizeBuffer; z < zSize - sizeBuffer; z++)
                {
                    float noise = Perlin3D(Modify(x, seed1), Modify(y, seed2), Modify(z, seed3));
                    if (noise > GetSolidThreshold(x,y,z)) mapGrid[x, y, z] = 1;
                }
            }
        }
    }

    float GetSolidThreshold(int x, int y, int z)
    {
        float interpolator = (float)y / ySize;
        return Mathf.Lerp(bottomSolidThreshold, topSolidThreshold, interpolator);
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

    public override void PlaceSecondaries()
    {
        
    }
}
