using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using static Util;
using Random = UnityEngine.Random;

public class GenerationManager : MonoBehaviour
{
    public static GenerationManager active;
    
    //Variables
    public Train playerTrain;
    [SerializeField]
    private float sectionLength = 100f;
    [SerializeField]
    private int generatedSectionsAheadOfPlayer = 2;
    [SerializeField]
    private int sectionsStructureSpawnDelay = 2;
    [SerializeField]
    private int sectionsFoliageSpawnDelay = 2;
    [SerializeField]
    private int generatedSectionsBehindPlayer = 2;
    [SerializeField]
    private Vector2 angleClampMinMax;
    [SerializeField]
    private Vector2 randomRotationMinMax;
    [SerializeField] 
    private NavMeshSurface navMeshSurface;
    [Header("Stations")]
    [SerializeField]
    private float stationSpawnDistance = 1500f;
    [SerializeField]
    private float stationSpeedTrackDistance = 200f;
    [SerializeField]
    private StructureSO stationSO;
    [SerializeField]
    private StructureSO preStationSO;

    [Header("Foliage")]
    [SerializeField]
    private List<ProbabilityListElement<ObjectPool>> foliageProbabilityElementList = new List<ProbabilityListElement<ObjectPool>>();
    [SerializeField]
    private int chunkSize;
    [SerializeField]
    private int foliageDensity;
    [SerializeField]
    private AnimationCurve foliageSpawnProbability_DistanceFromTrack;
    [SerializeField]
    private LayerMask foliageRaycastLayerMask;
    [SerializeField]
    private LayerMask foliageSpawnLayerMask;
    private List<Vector2> foliageFilledChunkList = new List<Vector2>();


    private ProbabilityList<ObjectPool> foliageProbabilityList;
    public enum StructureDistanceType
    {
        setDistance,
        fuelDistance
    }
    [System.Serializable]
    public class StructureSpawnParams
    {
        public List<StructureSO> structureInfoList;
        public StructureDistanceType distanceType;
        public float distanceTypeValue; // setDistance = distance; fuelDistance = fuelDistance * value
        public bool repeating;

        public void ResetDistance(bool resetSpawningDisabled = false)
        {
            if (resetSpawningDisabled || repeating) spawningDisabled = false;

            if (spawningDisabled == false)
            {
                if (distanceType == StructureDistanceType.setDistance)
                {
                    distanceTillStructure = distanceTypeValue;
                }
                else if (distanceType == StructureDistanceType.fuelDistance)
                {
                    float minDistance = float.MaxValue;
                    foreach (Train.FuelSource fuelSource in Train.playerTrain.GetFuelSourceList())
                    {
                        minDistance = Mathf.Min(fuelSource.targetHealth.maxHealth / TrainUpgradeHandler.active.GetStatValue(fuelSource.consumptionRate));
                    }
                    distanceTillStructure = minDistance * distanceTypeValue;
                }
            }
        }
        //Run time
        //[HideInInspector]
        public float distanceTillStructure;
        //[HideInInspector]
        public bool spawningDisabled;
    }
    [Header("Structures")]
    [SerializeField]
    private List<StructureSpawnParams> strutureSpawnParamList;
    [SerializeField]
    private float distanceBufferBetweenStructures;
    [Header("Temp")]
    [SerializeField]
    private bool disableNavMeshGen;
    //Run Time
    private float currentGeneratedDistance = 0f;
    private float lastAngle;
    private Point lastPoint;
    private TrackSection lastSection;
    private bool generateNavMesh;
    //Station stuff
    private bool stationGenerated;
    private float distanceLeftTillNextStation;
    //structures
    private float currentStructureDistanceBuffer;
    private void Start()
    {
        active = this;

        foreach (ProbabilityListElement<ObjectPool> entry in foliageProbabilityElementList)
        {
            entry.element.Init(entry.element.maxCapacity, entry.element.defaultCapacity);
        }

        foliageProbabilityList = new ProbabilityList<ObjectPool>(foliageProbabilityElementList);
        ResetGeneration();
        GenerateStart();
    }
    private IEnumerator BuildNavMesh()
    {
        yield return new WaitForEndOfFrame();
        if (disableNavMeshGen == false)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("Building Nav Mesh");
        }
    }
    private void Update()
    {
        if (generateNavMesh)
        {
            generateNavMesh = false;
            StartCoroutine(BuildNavMesh());
        }

        int sectionsAhead = 0;
        TrackSection trackSection = playerTrain.GetFrontTrackSection();
        while(trackSection.nextSection != null)
        {
            sectionsAhead++;
            trackSection = trackSection.nextSection;
        }

        int sectionsBehind = 0;
        trackSection = playerTrain.GetBackTrackSection();
        while (trackSection.previousSection != null)
        {
            sectionsBehind++;
            trackSection = trackSection.previousSection;
        }

        if (stationGenerated == false && sectionsAhead < generatedSectionsAheadOfPlayer)
        {
            GenerateNextSection();
        }

        if (sectionsBehind > generatedSectionsBehindPlayer)
        {
            Debug.Log("Destroy Section");
            TrackManager.active.DestroyTrackSection(trackSection);

            GameStateManager.isStartingLocationSpawned = false;
        }
    }
    public void ResetGeneration()
    {
        stationGenerated = false;
        if (GameStateManager.currentHaulingJob != null)
            distanceLeftTillNextStation = GameStateManager.currentHaulingJob.distance;
        else
            distanceLeftTillNextStation = stationSpawnDistance;
        foreach (StructureSpawnParams structureSpawnParams in strutureSpawnParamList)
        {
            structureSpawnParams.ResetDistance(true);
        }
        currentStructureDistanceBuffer = 0f;
    }
    private void GenerateNextSection()
    {
        if (distanceLeftTillNextStation > 0f)
        {
            //Pick random dir
            lastAngle = Random.Range(Mathf.Max(angleClampMinMax.x, lastAngle - randomRotationMinMax.x), Mathf.Min(angleClampMinMax.y, lastAngle + randomRotationMinMax.x));
            Vector3 dir = Quaternion.AngleAxis(lastAngle, Vector3.up) * Vector3.forward * (sectionLength * 0.5f);

            Vector3 forwardPos = lastPoint.handleForward;
            Point nextPoint = Spline.CreatePoint(forwardPos + dir, forwardPos, false);

            Spline.DEBUG_DrawPointGizmos(nextPoint);

            TrackSection generatedSection = TrackManager.active.CreateTrackSection(lastPoint, nextPoint);

            distanceLeftTillNextStation -= generatedSection.length;
            //Debug.Log("Distance Left: " + distanceLeftTillNextStation);

            currentGeneratedDistance += generatedSection.length;

            lastSection.SetNextSection(generatedSection);
            lastSection = generatedSection;
            lastPoint = nextPoint;

            TrackSection previousSection = generatedSection;
            for (int i = 1; i < sectionsFoliageSpawnDelay; i++)
            {
                if(previousSection.previousSection != null)
                {
                    previousSection = previousSection.previousSection;

                    if (i == sectionsStructureSpawnDelay-1)
                        UpdateStructureDistances(previousSection);
                    if (i == sectionsFoliageSpawnDelay-1)
                        StartCoroutine(GenerateFoliageAroundPoint(previousSection.pointA.position));
                }
            }
        }
        else
        {
            //Spawn rest of the foliage
            TrackSection previousSection = lastSection;
            for (int i = 1; i < sectionsFoliageSpawnDelay; i++)
            {
                if (previousSection.previousSection != null)
                {
                    StartCoroutine(GenerateFoliageAroundPoint(previousSection.pointA.position));
                    previousSection = previousSection.previousSection;
                }
            }
            GenerateStation();
        }

        generateNavMesh = true;
    }
    private void UpdateStructureDistances(TrackSection generatedSection)
    {
        currentStructureDistanceBuffer -= generatedSection.length;

        foreach (StructureSpawnParams structureSpawnParams in strutureSpawnParamList)
        {
            if (structureSpawnParams.spawningDisabled) continue;

            structureSpawnParams.distanceTillStructure -= generatedSection.length;

            if(currentStructureDistanceBuffer <= 0f && structureSpawnParams.distanceTillStructure <= 0f)
            {
                float sectionProgress = Mathf.Clamp01(Mathf.Min(-currentStructureDistanceBuffer, -structureSpawnParams.distanceTillStructure) / generatedSection.length) * generatedSection.length;
                SpawnStructureNearTrack(sectionProgress, generatedSection, structureSpawnParams.structureInfoList[Random.Range(0, structureSpawnParams.structureInfoList.Count)]);
                currentStructureDistanceBuffer = distanceBufferBetweenStructures;

                structureSpawnParams.spawningDisabled = true;
                structureSpawnParams.ResetDistance();
            }
        }
    }
    private void SpawnStructureNearTrack(float sectionProgress, TrackSection section, StructureSO structureInfo)
    {
        TrackManager.active.GetTrackPositionAndDirVectorFromProgress(sectionProgress, section, out Vector3 position, out Vector3 dir);

        GameObject structure = Instantiate(structureInfo.structurePrefab);
        
        structure.transform.position = position;
        structure.transform.LookAt(position + dir);

        if (structureInfo.addAutoStop)
        {
            section.SetAutoStop(sectionProgress, structureInfo.stopType);
        }

        section.AddObject(structure); //Adds it for deletion when track section is deleted

        //Initialize structure generation
        if(structure.TryGetComponent(out StructureMaster structureMaster))
        {
            structureMaster.Generate();
        }
    }
    private void GenerateStart()
    {
        GameStateManager.isStartingLocationSpawned = true;
        
        Point pointA = Spline.CreatePoint(Vector3.zero, Vector3.forward);
        Point pointB = Spline.CreatePoint(Vector3.forward * (playerTrain.GetConsistLenght() + 5f), Vector3.forward * ((playerTrain.GetConsistLenght() + 5f) * 1.5f));

        TrackSection section = TrackManager.active.CreateTrackSection(pointA, pointB);
        currentGeneratedDistance += section.length;     

        playerTrain.Initialize(playerTrain.GetConsistLenght() + 2.5f, section);

        lastPoint = pointB;
        lastSection = section;

        Spline.DEBUG_DrawPointGizmos(pointA);
        Spline.DEBUG_DrawPointGizmos(pointB);
    }
    private void GenerateStation()
    {
        //Generate new hauling jobs
        HaulingJobManager.active.GenerateNewHaulingJobList(3);

        //Pre Station Track
        TrackSection generatedSection = GenerateStraightSection(playerTrain.GetConsistLenght() + 5f);
        SpawnStructureNearTrack(generatedSection.length, generatedSection, preStationSO);
        //generatedSection.SetAutoStop(generatedSection.length - 2.5f,AutoStopType.Front);

        //Speed Track Enter
        generatedSection = GenerateStraightSection(stationSpeedTrackDistance);

        //Station Track
        generatedSection = GenerateStraightSection(playerTrain.GetConsistLenght() + 10f);
        SpawnStructureNearTrack(generatedSection.length - 2, generatedSection, stationSO);
        //generatedSection.SetAutoStop(generatedSection.length - 5f, AutoStopType.Front);

        //Speed Track Exit
        generatedSection = GenerateStraightSection(stationSpeedTrackDistance);
        generatedSection.SetAutoStop(generatedSection.length - 1f, AutoStopType.Front);

        generateNavMesh = true;
        stationGenerated = true;
    }
    private TrackSection GenerateStraightSection(float distance, float handleLength = 10f)
    {
        Vector3 forward = (lastPoint.handleForward - lastPoint.position).normalized;
        Point nextPoint = Spline.CreatePoint(lastPoint.position + forward * (distance), lastPoint.position + forward * (distance + handleLength), true);

        Spline.DEBUG_DrawPointGizmos(nextPoint);

        TrackSection generatedSection = TrackManager.active.CreateTrackSection(lastPoint, nextPoint);
        currentGeneratedDistance += generatedSection.length;

        lastSection.SetNextSection(generatedSection);
        lastSection = generatedSection;
        lastPoint = nextPoint;

        return generatedSection;
    }

    public IEnumerator GenerateFoliageAroundPoint(Vector3 position, int radius = 5)
    {
        if (position.z > 100f)
            yield return new WaitForSeconds(5f);

        Vector2 pos = new Vector2(position.x, position.z);

        Vector2Int chunkPos = new Vector2Int(Mathf.FloorToInt(pos.x / chunkSize), Mathf.FloorToInt(pos.y / chunkSize));
        List<Vector2Int> chunkList = new List<Vector2Int>();

        void AddChunk(Vector2Int chunk)
        {
            if (foliageFilledChunkList.Contains(chunk) == false)
            {
                chunkList.Add(chunk);
                foliageFilledChunkList.Add(chunk);
            }
        }
        for (int x = chunkPos.x - radius; x < chunkPos.x + radius; x++)
        {
            for (int y = chunkPos.y - radius; y < chunkPos.y + radius; y++)
            {
                AddChunk(new Vector2Int(x, y));
            }
        }


        foreach (Vector2Int chunk in chunkList)
        {
            Vector3 cornerWorldPos = new Vector3(chunk.x * chunkSize, 0, chunk.y * chunkSize);
            float increment = chunkSize / foliageDensity;
            for (int x = 0; x < foliageDensity; x++)
            {
                for (int y = 0; y < foliageDensity; y++)
                {
                    if (position.z > 100f)
                        yield return new WaitForSeconds(0.05f);
                    SpawnFoliage(cornerWorldPos + new Vector3(x * increment, 0, y * increment));
                }
            }
        }
    }
    private void SpawnFoliage(Vector3 pos)
    {
        ObjectPool prefabPool = foliageProbabilityList.PickNext();
        if (prefabPool == null) return;

        TrackManager.active.GetClosestTrackSection(pos, out TrackSection trackSection, out float distance);
        float dist = TrackManager.active.GetDistanceFromPath(trackSection.path, pos);
        if (Random.Range(0f, 1f) > foliageSpawnProbability_DistanceFromTrack.Evaluate(dist))
            return;

        Debug.DrawLine(pos, pos + Vector3.up * 20f, Color.coral, 60f);
        Vector2 randomDir = Random.insideUnitCircle;
        pos += new Vector3(randomDir.x * foliageDensity, 0, randomDir.y * foliageDensity);

        if (Physics.SphereCast(new Vector3(pos.x, 50f, pos.z), 2.5f, Vector3.down,out RaycastHit hit, 75f, foliageRaycastLayerMask))
        {
            if ((foliageSpawnLayerMask & (1 << hit.transform.gameObject.layer)) != 0)
            {
                Transform copy = prefabPool.Get().transform;
                copy.position = hit.point - new Vector3(0f,0.1f,0f);
                copy.rotation = Quaternion.Euler(Random.Range(-5f, 5f), Random.Range(0f, 360f), Random.Range(-5f, 5f));

                trackSection.AddObject(prefabPool, copy.gameObject);
            }
            else
            {
                Debug.DrawLine(pos, pos + Vector3.up * 20f, Color.red, 60f);
            }
        }
    }
}
