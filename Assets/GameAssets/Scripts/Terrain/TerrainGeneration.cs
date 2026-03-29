using System.Collections.Generic;
using UnityEngine;

public class TerrainGeneration : MonoBehaviour
{
    public static TerrainGeneration active;

    [Header("Size params")]
    [SerializeField]
    private int _chunkSize; public static int chunkSize;
    [SerializeField]
    private int _chunkVertBorderCount; public static int chunkVertBorderCount; public static float vertSpacing;

    [Header("Material")]
    [SerializeField]
    private Material _terrainMaterial; public static Material terrainMaterial;

    [Header("Terrain Params")]
    [SerializeField]
    private List<NoiseSettings> noiseSettingsList;
    [Header("Seed")]
    [SerializeField]
    private int seed;

    [Header("Editor Previews")]
    [SerializeField]
    public bool autoUpdate;
    [SerializeField]
    public int renderDistance;
    //Runtime
    private Dictionary<Vector2Int, Chunk> chunkDictionary = new Dictionary<Vector2Int, Chunk>();

    private void Start()
    {
        SetValues();
    }
    private void SetValues()
    {
        active = this;
        chunkSize = _chunkSize;
        chunkVertBorderCount = _chunkVertBorderCount;
        vertSpacing = (float)chunkSize / (float)chunkVertBorderCount;
        terrainMaterial = _terrainMaterial;

        chunkDictionary = new Dictionary<Vector2Int, Chunk>();
        Noise.Initialize(seed, noiseSettingsList);
    }
    public void GeneratePreviewEditor()
    {
        foreach (Chunk chunk in chunkDictionary.Values)
        {
            chunk.meshData.Destroy();
        }
        foreach (Transform transform in GetComponentsInChildren<Transform>())
        {
            if (transform != this.transform)
                DestroyImmediate(transform.gameObject);
        }

        SetValues();

        for (int x = -renderDistance-1; x <= renderDistance; x++)
        {
            for (int y = -renderDistance-1; y <= renderDistance; y++)
            {
                CreateOrUpdateChunk(new Vector2Int(x, y), true);
            }
        }
    }
    public Chunk CreateOrGetChunk(Vector2Int coord)
    {
        if (chunkDictionary.TryGetValue(coord, out Chunk chunk))
        {
            return chunk;
        }
        else
        {
            Chunk newChunk = new Chunk(coord);
            chunkDictionary.Add(coord, newChunk);
            return newChunk;
        }
    }
    public void CreateOrUpdateChunk(Vector2Int coord, bool regenerateHeightMesh = false)
    {
        if(chunkDictionary.TryGetValue(coord,out Chunk chunk))
        {
            if(regenerateHeightMesh)
                chunk.GenerateHeightMap();
            chunk.GenerateMesh();
        }
        else
        {
            Chunk newChunk = new Chunk(coord);
            newChunk.GenerateMesh();
            chunkDictionary.Add(coord, newChunk);
        }
    }
    public Vector2Int GetChunkCoord(Vector3 worldPos)
    {
        return GetChunkCoord(worldPos.x, worldPos.z);
    }
    public Vector2Int GetChunkCoord(float worldPosX, float worldPosZ)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPosX / (float)chunkSize), Mathf.FloorToInt(worldPosZ / (float)chunkSize));
    }
    public float GetHeight(Vector3 worldPos)
    {
        return GetHeight(worldPos.x, worldPos.z);
    }
    public float GetHeight(float worldPosX, float worldPosZ)
    {
        Vector2Int coord = GetChunkCoord(worldPosX, worldPosZ);
        if (chunkDictionary.TryGetValue(coord, out Chunk chunk))
        {
            int vertX = Mathf.FloorToInt((worldPosX - coord.x * chunkSize) / vertSpacing);
            int vertY = Mathf.FloorToInt((worldPosZ - coord.y * chunkSize) / vertSpacing);

            return chunk.heightMap[vertX, vertY];
        }
        else
        {
            return Noise.SampleNoise(worldPosX, worldPosZ);
        }
    }
    public class Chunk
    {
        public float[,] heightMap;
        public Vector2Int worldPos;
        public Vector2Int coord;
        //Mesh
        public MeshData meshData;

        public Chunk(Vector2Int coord)
        {
            this.coord = coord;
            worldPos = coord * chunkSize;
            GenerateHeightMap();
        }
        public void GenerateHeightMap()
        {
            heightMap = new float[chunkVertBorderCount+1, chunkVertBorderCount+1];
            for (int x = 0; x < chunkVertBorderCount+1; x++)
            {
                for (int y = 0; y < chunkVertBorderCount + 1; y++)
                {
                    heightMap[x, y] = Noise.SampleNoise(worldPos.x + x * vertSpacing, worldPos.y + y * vertSpacing);
                }
            }
        }
        public void GenerateMesh()
        {
            if (meshData != null)
                meshData.Destroy();

            float[,] extendedHeightMap = new float[heightMap.GetLength(0) + 2, heightMap.GetLength(1) + 2];
            for (int x = 0; x < extendedHeightMap.GetLength(0); x++)
            {
                for (int y = 0; y < extendedHeightMap.GetLength(1); y++)
                {
                    if (x > 0 && x < extendedHeightMap.GetLength(0) - 1 && y > 0 && y < extendedHeightMap.GetLength(1) - 1)
                    {
                        //Debug.Log(x + ", " + y);
                        extendedHeightMap[x, y] = heightMap[x - 1, y - 1];
                    }
                    else
                    {
                        extendedHeightMap[x, y] = active.GetHeight(worldPos.x + (x-1) * vertSpacing, worldPos.y + (y - 1) * vertSpacing);
                    }
                }
            }
            meshData = MeshGenerator.GenerateTerrainMesh(extendedHeightMap, vertSpacing);
            meshData.CreateMesh();
            meshData.CreateMeshObject(worldPos);
        }
        public Vector3 GetVertexWorldPos(int x, int y)
        {
            return new Vector3(x * vertSpacing + worldPos.x, heightMap[x, y], y * vertSpacing + worldPos.y);
        }
    }
}
