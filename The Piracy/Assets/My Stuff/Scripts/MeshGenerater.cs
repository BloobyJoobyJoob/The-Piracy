using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerater
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, TerrainType[] regions, float heightMultiplier, AnimationCurve _heightCurve)
    {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int size = heightMap.GetLength(0);
        int verticesPerSquare = 6;
        MeshData meshData = new MeshData(size, size, verticesPerSquare);
        int vert = 0;

        float halfSize = size / 2f;

        for (var y = 0; y < size - 1; y++)
        {
            for (var x = 0; x < size - 1; x++)
            {
                int a = vert;
                int b = a + 1;
                int c = b + 1;

                int d = c + 1;
                int e = d + 1;
                int f = e + 1;

                meshData.vertices[a] = new Vector3(x - halfSize, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, y - halfSize);
                meshData.vertices[b] = new Vector3(x + 1 - halfSize, heightCurve.Evaluate(heightMap[x + 1, y]) * heightMultiplier, y - halfSize);
                meshData.vertices[c] = new Vector3(x + 1 - halfSize, heightCurve.Evaluate(heightMap[(x + 1), y + 1]) * heightMultiplier, y + 1 - halfSize);

                meshData.vertices[d] = new Vector3(x - halfSize, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, y - halfSize);
                meshData.vertices[e] = new Vector3(x - halfSize, heightCurve.Evaluate(heightMap[x, y + 1]) * heightMultiplier, y + 1 - halfSize);
                meshData.vertices[f] = new Vector3(x + 1 - halfSize, heightCurve.Evaluate(heightMap[x + 1, y + 1]) * heightMultiplier, y + 1 - halfSize);

                AddColoredTri(a, c, b);
                AddColoredTri(d, e, f);

                vert += verticesPerSquare;
            }
        }

        return meshData;

        void AddColoredTri(int a, int b, int c)
        {
            Color color = PickColor((meshData.vertices[a].y + meshData.vertices[b].y + meshData.vertices[c].y) / 3 / heightMultiplier);
            meshData.colors[a] = color;
            meshData.colors[b] = color;
            meshData.colors[c] = color;
            meshData.AddTriangle(a, b, c);
        }

        Color PickColor(float height)
        {
            for (int i = 0; i < regions.Length; i++)
            {
                if (height < regions[i].height)
                {
                    return regions[i].color;
                }
            }
            return Color.cyan;
        }
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public Color[] colors;
    public int[] triangles;

    int triangleIndex;
    public MeshData(int meshWidth, int meshHeight, int verticesPerSquare)
    {
        vertices = new Vector3[meshWidth * meshHeight * verticesPerSquare];
        colors = new Color[meshWidth * meshHeight * verticesPerSquare];
        triangles = new int[meshWidth * meshHeight * verticesPerSquare];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.RecalculateNormals();
        return mesh;
    }
}