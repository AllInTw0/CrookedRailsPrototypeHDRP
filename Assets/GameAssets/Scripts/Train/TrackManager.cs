using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PathPoint
{
    public Vector3 position;
    public float distance;

    public PathPoint(Vector3 position,float distance)
    {
        this.position = position;
        this.distance = distance;
    }
}
public enum AutoStopType
{
    Front,
    TenderHatch
}
public class AutoStop
{
    public float distance;
    public bool lockControlls;
    public AutoStopType stopType;
    public bool ignore;
}
public class TrackSection
{
    public Point pointA;
    public Point pointB;
    public List<PathPoint> path;
    public TrackSection nextSection;
    public TrackSection previousSection;

    public AutoStop autoStop;

    public float length
    {
        get
        {
            return path[^1].distance;
        }
    }
    
    public TrackSection(Point pointA,Point pointB,List<PathPoint> path)
    {
        this.pointA = pointA;
        this.pointB = pointB;
        this.path = path;
    }

    public List<GameObject> associatedObjects = new List<GameObject>();

    public void AddObject(GameObject obj)
    {
        associatedObjects.Add(obj);
    }
    public void SetNextSection(TrackSection section)
    {
        nextSection = section;
        section.previousSection = this;
    }
    public void SetPreviousSection(TrackSection section)
    {
        previousSection = section;
        section.nextSection = this;
    }
    public TrackSection GetNextSection()
    {
        return nextSection;
    }
    public TrackSection GetPreviousSection()
    {
        return previousSection;
    }
    public void SetAutoStop(float distance, AutoStopType type = AutoStopType.Front, bool lockControlls = false)
    {
        autoStop = new AutoStop();
        autoStop.distance = distance;
        autoStop.lockControlls = lockControlls;
        autoStop.stopType = type;
    }
}
public class TrackManager : MonoBehaviour
{
    public static TrackManager active;
    //Run Time
    [NonSerialized] 
    public List<TrackSection> trackSectionList = new List<TrackSection>();
    [NonSerialized] 
    public float progressOffset = 0f;
    private void Awake()
    {
        active = this;
    }
    //Track Section Functions
    public TrackSection CreateTrackSection(Point pointA,Point pointB)
    {
        //Dose not automaticlly find next/previous sections

        //Spline.DEBUG_DrawPointGizmos(pointA,60f);
        //Spline.DEBUG_DrawPointGizmos(pointB,60f);
        
        CalculatePath(pointA,pointB,Spline.CalculateSplineLenght(pointA, pointB),out List<PathPoint> path);
        CalculatePath(path,out List<PathPoint> newPath);
        
        //DEBUG_DrawPath(path);
        //DEBUG_DrawPath(newPath);
        
        TrackSection section = new TrackSection(pointA, pointB, newPath);
        
        section.AddObject(Spline.active.GenerateMeshAlongTrackSection(section)); //Track mesh
        
        trackSectionList.Add(section);
        
        return section;
    }
    //Functions with trackSectionList
    public void GetTrackPositionFromProgress(float sectionProgress, TrackSection section, out Vector3 position)
    {
        if (GetTrackSectionFromProgress(sectionProgress, section, out TrackSection newSection, out float newSectionProgress))
        {
            position = GetPathPosition(newSection.path, newSectionProgress);
        }
        else
        {
            Debug.LogWarning("End of track");
            position = GetPathPosition(newSection.path, newSectionProgress);
        }
    }
    public void GetTrackPositionAndDirVectorFromProgress(float sectionProgress, TrackSection section, out Vector3 position, out Vector3 dir)
    {
        if (GetTrackSectionFromProgress(sectionProgress, section, out TrackSection newSection, out float newSectionProgress))
        {
            position = GetPathPosition(newSection.path, newSectionProgress);
            dir = GetPathDirectionVector(newSection, newSectionProgress);
        }
        else
        {
            Debug.LogWarning("End of track");
            position = GetPathPosition(newSection.path, newSectionProgress);
            dir = GetPathDirectionVector(newSection, newSectionProgress);
        }
    }
    public bool GetTrackSectionFromProgress(float sectionProgress, TrackSection section, out TrackSection newSection, out float newSectionProgress)
    {
        newSection = section;
        newSectionProgress = sectionProgress;

        while (newSectionProgress > newSection.length)
        {    
            TrackSection nextSection = newSection.GetNextSection();
            if(nextSection != null)
            {
                newSectionProgress -= newSection.length;
                newSection = nextSection;
            }
            else
            {
                newSectionProgress = Mathf.Clamp(newSectionProgress, 0f, newSection.length);
                return false;
            }
        }

        while (newSectionProgress < 0)
        {
            TrackSection previousSection = newSection.GetPreviousSection();
            if (previousSection != null)
            {
                newSectionProgress = previousSection.length + newSectionProgress;
                newSection = previousSection;
            }
            else
            {
                newSectionProgress = Mathf.Clamp(newSectionProgress, 0f, newSection.length);
                return false;
            }
        }

        return true;
    }
    public bool GetNearestAutoStop(float sectionProgress, TrackSection trackSection, int maxTrackSectionCheck, out AutoStop nearestAutoStop, out float distanceToAutoStop)
    {
        nearestAutoStop = null;
        distanceToAutoStop = 0f;

        for (int i = 0; i < maxTrackSectionCheck; i++)
        {
            if (trackSection == null)
                return false;

            if(i == 0)
            {
                if(trackSection.autoStop != null &&  trackSection.autoStop.ignore == false && trackSection.autoStop.distance >= sectionProgress)
                {
                    nearestAutoStop = trackSection.autoStop;
                    distanceToAutoStop = trackSection.autoStop.distance - sectionProgress;
                    return true;
                }
                else
                {
                    distanceToAutoStop = trackSection.length - sectionProgress;
                    trackSection = trackSection.GetNextSection(); 
                }
            }
            else
            {
                if (trackSection.autoStop != null && trackSection.autoStop.ignore == false)
                {
                    nearestAutoStop = trackSection.autoStop;
                    distanceToAutoStop += trackSection.autoStop.distance;
                    return true;
                }
                else
                {
                    distanceToAutoStop += trackSection.length;
                    trackSection = trackSection.GetNextSection();     
                }
            }
        }
        return false;
    }
    public TrackSection RemoveAtIndexAndReturn(int index = 0)
    {
        TrackSection section = trackSectionList[index];
        trackSectionList.RemoveAt(index);
        return section;
    }
    //Get Position On Path
    public static Vector3 GetPathPosition(List<PathPoint> path, float progress)
    {
        for (int i = 1; i < path.Count; i++)
        {
            if (path[i].distance >= progress || i == path.Count - 1)
            {
                float time = (progress - path[i - 1].distance) / (path[i].distance - path[i - 1].distance);
                return Vector3.Lerp(path[i - 1].position, path[i].position, time);
            }
        }

        Debug.LogWarning("Couldn't Get Path Pos. " + path[^1].distance + " , " + progress);
        return Vector3.zero;
    }
    public static Vector3 GetPathDirectionVector(TrackSection section, float progress)
    {
        if (progress == 0f)
            return (section.pointA.handleForward - section.pointA.position).normalized;
        
        if (progress >= section.length - 0.001f)
            return (section.pointB.handleForward - section.pointB.position).normalized;

        for (int i = 1; i < section.path.Count; i++)
        {
            if (section.path[i].distance >= progress)
            {
                return (section.path[i].position -section.path[i - 1].position).normalized;
            }
        }
        Debug.LogWarning("Couldn't Get Path Direction Vector." + section + ", " + progress);
        return Vector3.zero;
    }
    //Generating Path Functions
    public void CalculatePath(Point pointA, Point pointB, float splineLenght, out List<PathPoint> path, float resolution = 2.5f)
    {
        path = new List<PathPoint>();

        float increment = 1f / (float)Mathf.RoundToInt(splineLenght * resolution);

        Vector3 lastPos = pointA.position;
        float pathLenght = 0;

        float time = 0f;
        while (time <= 1f)
        {
            Vector3 pos = Spline.CalculateSplinePosition(pointA, pointB, time);
            pathLenght += Vector3.Distance(lastPos, pos);
            
            PathPoint point = new PathPoint(pos,pathLenght);
            path.Add(point);

            lastPos = pos;
            
            //Making sure the last point Gets Generated
            if (time < 1f && time + increment > 1f)
                time = 1f;
            else
                time += increment;
        }

        Debug.Log("PointDist: " + pathLenght + " SplineDist: " + splineLenght);
    }
    public void CalculatePath(List<PathPoint> path, out List<PathPoint> newPath, float resolution = 1f)
    {
        newPath = new List<PathPoint>();
        
        //New thing I learned: ^1 = path.Count-1
        float pathLenght = path[^1].distance;
        
        float increment = pathLenght / (float)Mathf.RoundToInt(pathLenght * resolution);

        Vector3 lastPos = path[0].position;
        float newPathLenght = 0;

        float progress = 0f;
        while (progress <= pathLenght)
        {
            Vector3 pos = GetPathPosition(path, progress);
            newPathLenght += Vector3.Distance(lastPos, pos);
            
            PathPoint point = new PathPoint(pos,newPathLenght);
            newPath.Add(point);

            lastPos = pos;
            
            //Making sure the last point Gets Generated
            if (progress < pathLenght && progress + increment > pathLenght)
                progress = pathLenght;
            else
                progress += increment;
        }

        Debug.Log("OldPathLenght: " + pathLenght + " NewPathLenght: " + newPathLenght);
    }
    //Debug Functions
    private void DEBUG_DrawPath(List<PathPoint> path, float duration = 60f)
    {
        Color color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        for (int i = 0; i < path.Count; i++)
        {
            Debug.DrawRay(path[i].position,Vector3.up *1.5f,color,duration);
        }
    }
}
