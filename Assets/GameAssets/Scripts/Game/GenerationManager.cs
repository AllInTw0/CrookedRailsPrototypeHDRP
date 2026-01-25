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
    private float minRot = -45f,maxRot = 45f;
    [SerializeField] 
    private NavMeshSurface navMeshSurface;
    //Run Time
    private float currentGeneratedDistance = 0f;
    private Point lastPoint;
    private void Start()
    {
        active = this;
        GenerateStart();
    }

    private void Update()
    {
        int sectionsAhead = 0;
        TrackSection trainSection = playerTrain.GetFrontMostTrackSection();
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
            
            playerTrain.OffsetProgress(-section.length);
            
            GameStateManager.isStartingLocationSpawned = false;
        }
    }

    private void GenerateNextSection()
    {
        Vector3 forwardPos = lastPoint.handleForward;
        Vector3 dir = Quaternion.AngleAxis(Random.Range(minRot, maxRot), Vector3.up) * Vector3.forward * (sectionLength *0.5f);
        
        Point nextPoint = Spline.CreatePoint(forwardPos + dir, forwardPos, false);
        
        Spline.DEBUG_DrawPointGizmos(nextPoint);
        currentGeneratedDistance += TrackManager.active.CreateTrackSection(lastPoint, nextPoint).length;
        lastPoint = nextPoint;
        
        navMeshSurface.BuildNavMesh();
    }
    private void GenerateStart()
    {
        GameStateManager.isStartingLocationSpawned = true;
        
        Point pointA = Spline.CreatePoint(Vector3.zero, Vector3.forward);
        Point pointB = Spline.CreatePoint(Vector3.forward * sectionLength, Vector3.forward * (sectionLength *1.5f));
        currentGeneratedDistance += TrackManager.active.CreateTrackSection(pointA, pointB).length;

        lastPoint = pointB;
        
        Spline.DEBUG_DrawPointGizmos(pointA);
        Spline.DEBUG_DrawPointGizmos(pointB);
    }
}
