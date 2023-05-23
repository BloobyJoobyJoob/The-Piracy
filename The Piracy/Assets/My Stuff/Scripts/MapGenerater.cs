using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainType {
	public float height;
	public Color color;
}

public class MapGenerater : MonoBehaviour
{
	public GameObject meshObject;
	public Material terrainMaterial;
	public int size = 10;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
	public int seed;
	public float scale = 25;
	public int octaves = 5;
	public float persistance = 0.2f, lacunarity = 2;
	public Vector2 offset = Vector2.zero;

	public TerrainType[] regions;

    public static MapGenerater Singleton;

    private void Awake(){
        Singleton = this;
    }

	private void Start()
	{
		MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
		meshFilter.sharedMesh = MeshGenerater.GenerateTerrainMesh(GenerateMap(size, seed, scale, octaves, persistance, lacunarity, offset), regions, meshHeightMultiplier, meshHeightCurve).CreateMesh();
		MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
		meshRenderer.sharedMaterial = terrainMaterial;
	}

	public static float[,] GenerateMap(int size, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
	{
    float[,] noiseMap = new float[size, size];

    System.Random prng = new System.Random(seed);
    Vector2[] octaveOffsets = new Vector2[octaves];
    for (int i = 0; i < octaves; i++)
    {
        float offsetX = prng.Next(-100000, 100000) + offset.x;
        float offsetY = prng.Next(-100000, 100000) + offset.y;
        octaveOffsets[i] = new Vector2(offsetX, offsetY);
    }

    if (scale <= 0)
    {
        scale = 0.0001f;
    }

    float maxNoiseHeight = float.MinValue;
    float minNoiseHeight = float.MaxValue;

    for (int y = 0; y < size; y++)
    {
        for (int x = 0; x < size; x++)
        {
            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;

            for (int i = 0; i < octaves; i++)
            {
                float sampleX = (x - size / 2f) / scale * frequency + octaveOffsets[i].x;
                float sampleY = (y - size / 2f) / scale * frequency + octaveOffsets[i].y;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                noiseHeight += perlinValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
            }

            if (noiseHeight > maxNoiseHeight)
            {
                maxNoiseHeight = noiseHeight;
            }
            else if (noiseHeight < minNoiseHeight)
            {
                minNoiseHeight = noiseHeight;
            }
            noiseMap[x, y] = noiseHeight;
        }
    }

    for (int y = 0; y < size; y++)
    {
        for (int x = 0; x < size; x++)
        {
            noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
        }
    }

    return noiseMap;
	}
}
