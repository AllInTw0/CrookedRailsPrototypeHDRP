using System;
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
    private float stationSpawnDistance = 1500f;
    [SerializeField]
    private Vector2 angleClampMinMax;
    [SerializeField]
    private Vector2 randomRotationMinMax;
    [SerializeField] 
    private NavMeshSurface navMeshSurface;

    //Run Time
    private float currentGeneratedDistance = 0f;
    private float lastAngle;
    private Point lastPoint;
    private TrackSection lastSection;
    private void Start()
    {
        active = this;
        GenerateStart();
    }

    private void Update()
    {
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

        if (sectionsAhead < generatedSectionsAheadOfPlayer)
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
        //Pick random dir
        lastAngle = Random.Range(Mathf.Max(angleClampMinMax.x, lastAngle - randomRotationMinMax.x), Mathf.Min(angleClampMinMax.y, lastAngle + randomRotationMinMax.x));
        Vector3 dir = Quaternion.AngleAxis(lastAngle, Vector3.up) * Vector3.forward * (sectionLength *0.5f);

        Vector3 forwardPos = lastPoint.handleForward;
        Point nextPoint = Spline.CreatePoint(forwardPos + dir, forwardPos, false);
        
        Spline.DEBUG_DrawPointGizmos(nextPoint);

        TrackSection generatedSection = TrackManager.active.CreateTrackSection(lastPoint, nextPoint);
        currentGeneratedDistance += generatedSection.length;

        lastSection.SetNextSection(generatedSection);
        lastSection = generatedSection;
        lastPoint = nextPoint;
        
        navMeshSurface.BuildNavMesh();
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
}
