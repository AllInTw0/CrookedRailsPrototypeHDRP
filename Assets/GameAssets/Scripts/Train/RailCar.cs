using System;
using System.Collections.Generic;
using UnityEngine;

public class RailCar : MonoBehaviour
{
    //Variables
    public float frontLength = 5f;
    public float backLength = 5f;
    public float wheelLength = 2f;
    public List<RunningGear> railCarRunningGearList;

    //Run Time
    //[NonSerialized]
    //public TrackSection currentFrontSection;
    public void UpdateRailCar(float sectionProgress, TrackSection trackSection, float distanceTravelled)
    {
        foreach (RunningGear runningGear in railCarRunningGearList)
        {
            runningGear.UpdateRunningGearPosition(sectionProgress, trackSection);
            runningGear.UpdateRunningGearRotation(distanceTravelled);
        }
    }
}
