using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TerrainGeneration;
using static Util;

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
    [SerializeField]
    private int _terrainLayer; public static int terrainLayer;
    [Header("Foliage")]
    [SerializeField]
    private List<ProbabilityListElement<ObjectPool>> foliageProbabilityElementList = new List<ProbabilityListElement<ObjectPool>>();
    [SerializeField]
    private int _foliageDensity; public static int foliageDensity;
    [SerializeField]
    private AnimationCurve foliageSpawnProbability_DistanceFromTrack;
    [SerializeField]
    private LayerMask foliageRaycastLayerMask;
    [SerializeField]
    private LayerMask foliageSpawnLayerMask;
    [SerializeField]
    private float waterHeight;
    [SerializeField]
    private float minNormalY;

    private ProbabilityList<ObjectPool> foliageProbabilityList;
    [Header("Render Distance")]
    [SerializeField]
    private Transform player;
    [SerializeField]
    private float renderDistanceMeters;
    private bool updateRenderDistance = false;
    private Vector2Int lastUpdateCoord;
    [Header("Seed")]
    [SerializeField]
    private int seed;

    [Header("Editor Previews")]
    [SerializeField]
    public bool autoUpdate;
    //Runtime
    private Dictionary<Vector2Int, Chunk> chunkDictionary = new Dictionary<Vector2Int, Chunk>();
    
    private void Start()
    {
        SetValues();
    }
    private void Update()
    {
        UpdateRenderDistance(false, true, false, true);
    }
    private void SetValues()
    {
        active = this;
        chunkSize = _chunkSize;
        chunkVertBorderCount = _chunkVertBorderCount;
        vertSpacing = (float)chunkSize / (float)chunkVertBorderCount;
        terrainMaterial = _terrainMaterial;
        foliageDensity = _foliageDensity;
        terrainLayer = _terrainLayer;
        lastUpdateCoord = new Vector2Int(-1000000, -1000000);

        foreach (ProbabilityListElement<ObjectPool> entry in foliageProbabilityElementList)
        {
            entry.element.Init(entry.element.maxCapacity, entry.element.defaultCapacity);
        }
        foliageProbabilityList = new ProbabilityList<ObjectPool>(foliageProbabilityElementList);

        chunkDictionary = new Dictionary<Vector2Int, Chunk>();
        Noise.Initialize(seed, noiseSettingsList);
    }
    public void SetUpdateRenderDistance(bool value)
    {
        updateRenderDistance = value;
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
        UpdateRenderDistance(true,true,true,false);
    }
    private void UpdateRenderDistance(bool regenerateHeightMesh = false, bool createMesh = false, bool alwaysUpdate = false, bool threadHeightMapGen = false)
    {
        Vector2Int chunk = Vector2Int.zero;
        if(player != null)
            chunk = GetChunkCoord(player.position);

        if (alwaysUpdate || (updateRenderDistance && chunk != lastUpdateCoord))
        {
            int renderDistanceChunks = Mathf.RoundToInt(renderDistanceMeters / chunkSize);

            for (int x = -renderDistanceChunks - 1; x <= renderDistanceChunks; x++)
            {
                for (int y = -renderDistanceChunks - 1; y <= renderDistanceChunks; y++)
                {
                    CreateOrUpdateChunk(new Vector2Int(chunk.x + x, chunk.y + y), regenerateHeightMesh, createMesh, threadHeightMapGen);
                }
            }
            foreach (KeyValuePair<Vector2Int,Chunk> keyValuePair in chunkDictionary)
            {
                if(keyValuePair.Key.x < chunk.x - renderDistanceChunks - 2 || keyValuePair.Key.x > chunk.x + renderDistanceChunks + 2 ||
                   keyValuePair.Key.y < chunk.y - renderDistanceChunks - 2 || keyValuePair.Key.y > chunk.y + renderDistanceChunks + 2)
                {
                    keyValuePair.Value.Destroy();
                }
            }
            lastUpdateCoord = chunk;
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
    public void CreateOrUpdateChunk(Vector2Int coord, bool regenerateHeightMesh = false, bool createMesh = false, bool threadHeightMapGen = false)
    {
        if (chunkDictionary.TryGetValue(coord, out Chunk chunk))
        {
            if (regenerateHeightMesh)
            {
                chunk.GenerateHeightMap();
                if(createMesh)
                    chunk.GenerateMesh();
            }
            else if(createMesh && chunk.meshData == null)
                chunk.GenerateMesh();
        }
        else
        {
            Chunk newChunk = new Chunk(coord, threadHeightMapGen);
            if (createMesh)
                newChunk.GenerateMesh();
            chunkDictionary.Add(coord, newChunk);
        }
    }
    public Chunk FindFittingChunk(float targetAverage, float permissibleThreshold, float maxHeight, float minHeight, Vector2Int coord, int maxSearchDistance)
    {
        float minAverage = targetAverage - permissibleThreshold;
        float maxAverage = targetAverage + permissibleThreshold;

        Chunk mostFitting = null;
        float distFromTarget = float.MaxValue;
        bool TestChunk(Vector2Int coord, out Chunk chunk)
        {
            float heightMax = float.MinValue;
            float heightMin = float.MaxValue;
            float heightAverage = 0f;
            for (int chunkX = -1; chunkX <= 1; chunkX++)
            {
                for (int chunkY = -1; chunkY <=1; chunkY++)
                {
                    Chunk chunk1 = CreateOrGetChunk(new Vector2Int(coord.x + chunkX, coord.y + chunkY));
                    maxHeight = Mathf.Max(maxHeight,chunk1.heightMax);
                    minHeight = Mathf.Max(minHeight, chunk1.heightMin);
                    heightAverage += chunk1.heightAverage;
                }
            }
            heightAverage = heightAverage / 9f;

            chunk = CreateOrGetChunk(coord);
            if (heightAverage >= minAverage && heightAverage <= maxAverage)
            {
                return true;
            }

            float distance = Mathf.Abs(targetAverage - heightAverage);
            if (distance < distFromTarget)
            {
                if (heightMax <= maxHeight && heightMin >= minHeight)
                {
                    mostFitting = chunk;
                    distFromTarget = distance;
                }
            }
            return false;
        }

        for (int chunkX = -maxSearchDistance; chunkX < maxSearchDistance; chunkX++)
        {
            for (int chunkY = -maxSearchDistance; chunkY < maxSearchDistance; chunkY++)
            {
                if(TestChunk(new Vector2Int(coord.x + chunkX, coord.y + chunkY), out Chunk chunk))
                {
                    return chunk;
                }
            }
        }

        return mostFitting;
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
        if (chunkDictionary.TryGetValue(coord, out Chunk chunk) && chunk.heightMap != null)
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

        //Height statistic
        public float heightAverage;
        public float heightMax;
        public float heightMin;

        //Mesh
        public MeshData meshData;
        public Dictionary<ObjectPool, List<GameObject>> poolDictionary;

        public bool threading = false;
        public Chunk(Vector2Int coord, bool threadHeightMapGen = false)
        {
            this.coord = coord;
            worldPos = coord * chunkSize;
            poolDictionary = new Dictionary<ObjectPool, List<GameObject>>();
            if (threadHeightMapGen)
                GenerateHeightMapThreaded();
            else
                GenerateHeightMap();
        }
        public void GenerateHeightMapThreaded()
        {
            if(threading)
            {
                Debug.Log("Is currently therading heightMap. Ignoreing");
                return;
            }
            threading = true;
            ThreadManager.AddThreadJob(() => GenerateHeightMapThread(), OnHeightMapRecieved);
        }
        public object GenerateHeightMapThread()
        {
            float[,] heightMap = new float[chunkVertBorderCount + 1, chunkVertBorderCount + 1];
            heightAverage = 0f;
            heightMax = float.MinValue;
            heightMin = float.MaxValue;
            for (int x = 0; x < chunkVertBorderCount + 1; x++)
            {
                for (int y = 0; y < chunkVertBorderCount + 1; y++)
                {
                    float height = Noise.SampleNoise(worldPos.x + x * vertSpacing, worldPos.y + y * vertSpacing);
                    heightMap[x, y] = height;
                    if (height < heightMin)
                        heightMin = height;
                    if (height > heightMax)
                        heightMax = height;
                    heightAverage += height;
                }
            }
            heightAverage = heightAverage / ((chunkVertBorderCount + 1) * (chunkVertBorderCount + 1));
            return heightMap;
        }
        public void GenerateHeightMap()
        {
            heightMap = (float[,])GenerateHeightMapThread();
        }
        public void OnHeightMapRecieved(object heightMapObj)
        {
            heightMap = (float[,])heightMapObj;
            threading = false;
        }
        public void GenerateMesh()
        {
            if (threading)
            {
                Debug.Log("Is currently therading. Ignoreing");
                return;
            }

            if (meshData != null)
                meshData.Destroy();

            threading = true;
            ThreadManager.AddThreadJob(() => GenerateMeshThread(),OnMeshRecieved);
        }
        private object GenerateMeshThread()
        {
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
                        extendedHeightMap[x, y] = active.GetHeight(worldPos.x + (x - 1) * vertSpacing, worldPos.y + (y - 1) * vertSpacing);
                    }
                }
            }
            return MeshGenerator.GenerateTerrainMesh(extendedHeightMap, vertSpacing);
        }
        private void OnMeshRecieved(object meshDataObj)
        {
            meshData = (MeshData)meshDataObj;
            meshData.CreateMesh();
            meshData.CreateMeshObject(worldPos);
            active.StartCoroutine(SpawnFoliage());       
            threading = false;
        }

        public IEnumerator SpawnFoliage()
        {
            yield return new WaitForSeconds(1f);

            float increment = chunkSize / foliageDensity;
            for (int x = 0; x < foliageDensity; x++)
            {
                for (int y = 0; y < foliageDensity; y++)
                {
                    if (coord.y > 3)
                        yield return new WaitForSeconds(0.05f);
                    SpawnFoliageAtPos(new Vector3(worldPos.x, 0, worldPos.y) + new Vector3(x * increment, 0, y * increment));
                }
            }
        }

        private void SpawnFoliageAtPos(Vector3 pos)
        {
            ObjectPool prefabPool = active.foliageProbabilityList.PickNext();
            if (prefabPool == null) return;

            TrackManager.active.GetClosestTrackSection(pos, out TrackSection trackSection, out float distance);
            float dist = TrackManager.active.GetDistanceFromPath(trackSection.path, pos);
            if (Random.Range(0f, 1f) > active.foliageSpawnProbability_DistanceFromTrack.Evaluate(dist))
                return;

            //Debug.DrawLine(pos, pos + Vector3.up * 20f, Color.coral, 60f);
            Vector2 randomDir = Random.insideUnitCircle;
            pos += new Vector3(randomDir.x * foliageDensity, 0, randomDir.y * foliageDensity);

            if (Physics.SphereCast(new Vector3(pos.x, 50f, pos.z), 2.5f, Vector3.down, out RaycastHit hit, 75f, active.foliageRaycastLayerMask))
            {
                if (hit.point.y >= active.waterHeight && hit.normal.y >= active.minNormalY && (active.foliageSpawnLayerMask & (1 << hit.transform.gameObject.layer)) != 0)
                {
                    Transform copy = prefabPool.Get().transform;
                    copy.position = hit.point - new Vector3(0f, 0.1f, 0f);
                    copy.rotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-5f, 5f));

                    if (poolDictionary.TryGetValue(prefabPool, out List<GameObject> objectList))
                    {
                        objectList.Add(copy.gameObject);
                    }
                    else
                    {
                        poolDictionary.Add(prefabPool, new List<GameObject>() { copy.gameObject });
                    }

                    //trackSection.AddObject(prefabPool, copy.gameObject);
                }
                else
                {
                    //Debug.DrawLine(pos, pos + Vector3.up * 20f, Color.red, 60f);
                }
            }
        }

        public Vector3 GetVertexWorldPos(int x, int y)
        {
            return new Vector3(x * vertSpacing + worldPos.x, heightMap[Mathf.Clamp(x,0, heightMap.GetLength(0) - 1), Mathf.Clamp(y, 0, heightMap.GetLength(1) - 1)], y * vertSpacing + worldPos.y);
        }
        public Vector3 GetWorldPos(float timeX, float timeZ)
        {
            return new Vector3(worldPos.x + timeX * chunkSize, GetVertexWorldPos(Mathf.FloorToInt(timeX * (heightMap.GetLength(0)-1f)), Mathf.FloorToInt(timeZ * (heightMap.GetLength(1) - 1f))).y, worldPos.y + timeZ * chunkSize); 
        }
        public void Destroy()
        {
            if (meshData == null)
                return;

            foreach (KeyValuePair<ObjectPool, List<GameObject>> poolObjectPair in poolDictionary)
            {
                foreach (GameObject obj in poolObjectPair.Value)
                {
                    poolObjectPair.Key.Add(obj);
                }
            }

            meshData.Destroy();
            meshData = null;

            poolDictionary = new Dictionary<ObjectPool, List<GameObject>>();
        }
    }
}
