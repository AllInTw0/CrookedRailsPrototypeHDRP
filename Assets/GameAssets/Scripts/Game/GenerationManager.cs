using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using static Util;
using Random = UnityEngine.Random;

public class GenerationManager : MonoBehaviour
{
    public static GenerationManager active;

    //Variables
    [Header("Scripts")]
    [SerializeField]
    private TerrainGeneration terrainGeneration;
    [SerializeField]
    private TrackGeneration trackGeneration;
    [Header("Player Train")]
    public Train playerTrain;
    [Header("Navigation")]
    [SerializeField] 
    private NavMeshSurface navMeshSurface;
    [Header("Starting Location")]
    [SerializeField]
    private StructureSO startingLocationSO;
    [SerializeField]
    private float startingLocationStructureTrackLength;
    [Header("Stations")]
    [SerializeField]
    private float stationSpawnDistance = 1500f;
    [SerializeField]
    private float stationSpeedTrackDistance = 200f;
    [SerializeField]
    private StructureSO stationSO;
    [SerializeField]
    private StructureSO preStationSO;

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
    //Station stuff
    private bool stationGenerated;
    private float distanceLeftTillNextStation;
    //structures
    private float currentStructureDistanceBuffer;
    private void Start()
    {
        active = this;

        GenerateStart();
    }

    //Generation functions
    public void GenerateTillNextStation()
    {
        LoadingScreen.active.Enable("Generating");

        List<int> pathSeeds = new List<int>();
        for (int i = 0; i < 9; i++)
        {
            pathSeeds.Add(Random.Range(0, 10000));
        }
        ThreadManager.AddThreadJob(() => FindPathThread(pathSeeds), OnFindPaths);
    }
    public void FindPathsRequest(int searchCount, int returnCount)
    {

    }
    public object FindPathThread(List<int> pathSeeds)
    {
        Vector3 targetPos = lastPoint.position + new Vector3(0, 0, stationSpawnDistance);
        TerrainGeneration.Chunk chunk = terrainGeneration.FindFittingChunk(0.5f, 0.4f, 3f, -2f, terrainGeneration.GetChunkCoord(targetPos), 5);
        List<NodePath> nodePathList = trackGeneration.FindPaths(lastPoint.position, chunk.GetWorldPos(0.5f, 0.5f), pathSeeds, 3);
        return nodePathList;
    }
    public void OnFindPaths(object nodePathListObj)
    {
        LoadingScreen.active.SetProgress(0.5f, "Generating Track");
        List<NodePath> nodePathList = (List<NodePath>)nodePathListObj;
        for (int i = 0; i < nodePathList.Count; i++)
        {
            Debug.Log("Track length" + i + ": " + nodePathList[i].length);
        }
        trackGeneration.CreateTrackAlongNodes(nodePathList[0], out List<PathPoint> path, out TrackSection lastTrackSectionOut, lastPoint, lastSection);

        //Structures
        foreach (StructureSpawnParams structureSpawnParams in strutureSpawnParamList)
        {
            structureSpawnParams.ResetDistance(true);
        }
        while (lastSection != lastTrackSectionOut)
        {
            lastSection = lastSection.nextSection;
            UpdateStructureDistances(lastSection);
        }

        //lastSection = lastTrackSectionOut;
        lastPoint = lastSection.pointB;
        LoadingScreen.active.SetProgress(0.55f, "Modifying Ground");
        ThreadManager.AddThreadJob(delegate { trackGeneration.ModifyTerrainToFollowPath(path, true); }, delegate { OnFinishedModifyingGround(); });
    }
    public void OnFinishedModifyingGround()
    {
        GenerateStation();
        StartCoroutine(GenerateNavMeshCoroutine());
    }
    public IEnumerator GenerateNavMeshCoroutine()
    {
        while (StructureMaster.generatingStructures != StructureMaster.finnishedStructures)
        {
            LoadingScreen.active.SetProgress(0.6f + (StructureMaster.finnishedStructures / StructureMaster.generatingStructures) * 0.4f, $"Generating Structures {StructureMaster.finnishedStructures}/{StructureMaster.generatingStructures}");
            yield return new WaitForSecondsRealtime(0.1f);
        }
        //LoadingScreen.active.SetProgress(0.9f, "Generating Navigation Mesh");

        //yield return new WaitForSecondsRealtime(0.2f);

        //if (disableNavMeshGen == false)
        //{
        //    NavMeshBuildSettings settings = new NavMeshBuildSettings();
        //    List<NavMeshBuildSource> sourceList = new List<NavMeshBuildSource>();

        //    NavMeshBuilder.BuildNavMeshData(settings,);
        //    navMeshSurface.BuildNavMesh();
        //    navmeh
        //    Debug.Log("Building Nav Mesh");
        //}

        terrainGeneration.SetUpdateRenderDistance(true);
        LoadingScreen.active.Disable();
        yield break;
    }

    //Generation functions end
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
        structure.transform.LookAt(position + (structureInfo.onlyRotateY ? new Vector3(dir.x, 0, dir.z) : dir));

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

        TerrainGeneration.Chunk chunk = terrainGeneration.FindFittingChunk(0.5f, 0.6f, 3f, -2f, Vector2Int.zero, 5);

        Vector3 pos = chunk.GetWorldPos(0.5f, 1f);
        if (pos.y < 0f)
            pos.y = 0f;

        Point pointA = Spline.CreatePoint(pos - Vector3.forward * startingLocationStructureTrackLength, pos - Vector3.forward * startingLocationStructureTrackLength * 0.5f);
        Point pointB = Spline.CreatePoint(pos, pos + Vector3.forward * 15f);

        TrackSection section = TrackManager.active.CreateTrackSection(pointA, pointB);
        currentGeneratedDistance += section.length;

        SpawnStructureNearTrack(section.length, section, startingLocationSO);

        playerTrain.Initialize(playerTrain.GetConsistLenght() + 1.2f, section);

        lastPoint = pointB;
        lastSection = section;

        //Track to starting location
        Point point = Spline.CreatePoint(pos - Vector3.forward * startingLocationStructureTrackLength, pos - Vector3.forward * startingLocationStructureTrackLength * 1.5f);
        trackGeneration.GenerateTrack(point.position, point.position + Vector3.back * 150f, point);

        Spline.DEBUG_DrawPointGizmos(pointA);
        Spline.DEBUG_DrawPointGizmos(pointB);
    }
    private void GenerateStation()
    {
        List<PathPoint> ListOfPathPoints = new List<PathPoint>();
        //Generate new hauling jobs
        HaulingJobManager.active.GenerateNewHaulingJobList(3);

        //Pre Station Track
        TrackSection generatedSection = GenerateStraightSection(playerTrain.GetConsistLenght() + 5f);
        SpawnStructureNearTrack(generatedSection.length, generatedSection, preStationSO);
        //generatedSection.SetAutoStop(generatedSection.length - 2.5f,AutoStopType.Front);

        //Speed Track Enter
        generatedSection = GenerateStraightSection(stationSpeedTrackDistance);
        ListOfPathPoints.AddRange(generatedSection.path);

        //Station Track
        float startionTrackLength = playerTrain.GetConsistLenght() + 10f;
        generatedSection = GenerateStraightSection(startionTrackLength);
        ListOfPathPoints.AddRange(generatedSection.path);
        SpawnStructureNearTrack(generatedSection.length - 2, generatedSection, stationSO);
        Vector3 middlePos = TrackManager.GetPathPosition(generatedSection.path, generatedSection.length * 0.5f);
        
        //generatedSection.SetAutoStop(generatedSection.length - 5f, AutoStopType.Front);

        //Speed Track Exit
        generatedSection = GenerateStraightSection(stationSpeedTrackDistance);
        ListOfPathPoints.AddRange(generatedSection.path);
        generatedSection.SetAutoStop(generatedSection.length - 1f, AutoStopType.Front);

        //Modify Ground
        float dist = startionTrackLength * 0.5f + stationSpeedTrackDistance;
        AnimationCurve newAnimationCurve = new AnimationCurve();
        newAnimationCurve.keys = new Keyframe[] { new Keyframe(0f, 1f), new Keyframe(dist - 30f, 1f), new Keyframe(dist, 0f) };
        TerrainModifier.ModifyGround(new Vector3(middlePos.x, middlePos.y - 100f, middlePos.z), dist, newAnimationCurve);

        dist = startionTrackLength * 0.5f + stationSpeedTrackDistance * 0.5f;
        AnimationCurve newAnimationCurve1 = new AnimationCurve();
        newAnimationCurve1.keys = new Keyframe[] { new Keyframe(0f, 1f), new Keyframe(dist - 30f, 1f), new Keyframe(dist, 0f) };
        TerrainModifier.ModifyGround(new Vector3(middlePos.x, middlePos.y, middlePos.z), dist, newAnimationCurve1);

        ThreadManager.AddThreadJob(delegate { trackGeneration.ModifyTerrainToFollowPath(ListOfPathPoints, true); }, delegate {  });
        
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
    
}
