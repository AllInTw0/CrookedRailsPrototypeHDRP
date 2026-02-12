using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
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
    [Header("Temp")]
    [SerializeField]
    private StructureSO waterTowerTemp;

    //Run Time
    private float currentGeneratedDistance = 0f;
    private float lastAngle;
    private Point lastPoint;
    private TrackSection lastSection;
    private bool generateNavMesh;
    //Station stuff
    private bool stationGenerated;
    private float distanceLeftTillNextStation;
    private void Start()
    {
        active = this;
        distanceLeftTillNextStation = stationSpawnDistance;
        GenerateStart();
    }

    private void Update()
    {
        if (generateNavMesh)
        {
            generateNavMesh = false;
            navMeshSurface.BuildNavMesh();
        }

        int sectionsAhead = 0;
        TrackSection trainSection = playerTrain.GetFrontTrackSection();
        for (int i = TrackManager.active.trackSectionList.Count-1; i >= 0; i--)
        {
            if (TrackManager.active.trackSectionList[i] == trainSection)
                break;
            sectionsAhead++;
        }

        int sectionsBehind = 0;
        for (int i = 0; i < TrackManager.active.trackSectionList.Count; i++)
        {
            if (TrackManager.active.trackSectionList[i] == trainSection)
                break;
            sectionsBehind++;
        }

        if (stationGenerated == false && sectionsAhead < generatedSectionsAheadOfPlayer)
        {
            GenerateNextSection();
        }

        if (sectionsBehind > generatedSectionsBehindPlayer)
        {
            Debug.Log("Destroy Section");
            TrackSection section = TrackManager.active.RemoveAtIndexAndReturn(0);
            foreach (var obj in section.associatedObjects)
            {
                Destroy(obj);
            }
            
            //playerTrain.OffsetProgress(-section.length);
            
            GameStateManager.isStartingLocationSpawned = false;
        }
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
            if (TrackManager.active.trackSectionList.Count == 3)
            {
                //SpawnStructureNearTrack(generatedSection.length - 0.1f, generatedSection, waterTowerTemp);
            }
            distanceLeftTillNextStation -= generatedSection.length;
            Debug.Log("Distance Left: " + distanceLeftTillNextStation);

            currentGeneratedDistance += generatedSection.length;

            lastSection.SetNextSection(generatedSection);
            lastSection = generatedSection;
            lastPoint = nextPoint;
        }
        else
        {
            GenerateStation();
        }

        generateNavMesh = true;
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
