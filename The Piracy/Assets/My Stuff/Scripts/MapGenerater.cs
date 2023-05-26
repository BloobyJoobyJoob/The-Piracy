using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

[System.Serializable]
public class TerrainType {
	public float height;
	public Color color;
}

public class MapGenerater : MonoBehaviour
{
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

    Queue<MapData> meshQueue = new Queue<MapData>();

    public static MapGenerater Singleton;

    private void Awake(){ 
        Singleton = this;
    }
    public void GenorateMeshOnThread(Vector2 center, Action<MeshData> callback){
        ThreadStart threadStart = delegate {
            GeneratingMesh(center, callback);
        };

        new Thread(threadStart).Start();
    }

    private void GeneratingMesh(Vector2 center, Action<MeshData> callback) {
        MeshData meshData = MeshGenerater.GenerateTerrainMesh(GenerateMap(size + 1, seed, scale, octaves, persistance, lacunarity, center + offset), regions, meshHeightMultiplier, meshHeightCurve);
            
        lock (meshQueue)
        {
            meshQueue.Enqueue(new MapData(callback, meshData));
        }
    }


    void Update() {
        if (meshQueue.Count > 0)
        {
            for (var i = 0; i < meshQueue.Count; i++)
            {
                MapData mapData = meshQueue.Dequeue();
                Mesh mesh = mapData.meshData.CreateMesh();
                mapData.callback(mapData.meshData);
            }
        }
    }


    struct MapData {
        public Action<MeshData> callback;
        public MeshData meshData;
        public MapData(Action<MeshData> callback, MeshData meshData)
        {
            this.callback = callback;
            this.meshData = meshData;
        }
    }

	public static float[,] GenerateMap(int size, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
	{
    float[,] noiseMap = new float[size, size];

    System.Random prng = new System.Random(seed);
    Vector2[] octaveOffsets = new Vector2[octaves];

    float maxHeight = 0;
    float amplitude = 1;

    for (int i = 0; i < octaves; i++)
    {
        float offsetX = prng.Next(-100000, 100000) + offset.x;
        float offsetY = prng.Next(-100000, 100000) + offset.y;
        octaveOffsets[i] = new Vector2(offsetX, offsetY);

        maxHeight += amplitude;
        amplitude *= persistance;
    }

    if (scale <= 0)
    {
        scale = 0.0001f;
    }

    float maxLocalNoiseHeight = float.MinValue;
    float minLocalNoiseHeight = float.MaxValue;

    for (int y = 0; y < size; y++)
    {
        for (int x = 0; x < size; x++)
        {
            amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;

            for (int i = 0; i < octaves; i++)
            {
                float sampleX = (x - (size / 2f) + octaveOffsets[i].x) / scale * frequency;
                float sampleY = (y - (size / 2f) + octaveOffsets[i].y) / scale * frequency;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                noiseHeight += perlinValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
            }

            if (noiseHeight > maxLocalNoiseHeight)
            {
                maxLocalNoiseHeight = noiseHeight;
            }
            else if (noiseHeight < minLocalNoiseHeight)
            {
                minLocalNoiseHeight = noiseHeight;
            }
            noiseMap[x, y] = noiseHeight;
        }
    }

    for (int y = 0; y < size; y++)
    {
        for (int x = 0; x < size; x++)
        {
            float normalizedHeight = (noiseMap[x, y] + 1) / (2 * maxHeight / 2.25f);
            noiseMap [x, y] = normalizedHeight;
        }
    }

    return noiseMap;
	}
}
