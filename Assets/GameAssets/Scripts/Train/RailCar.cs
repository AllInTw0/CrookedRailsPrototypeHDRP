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
    [NonSerialized]
    public TrackSection currentFrontSection;
    public void UpdateRailCar(float progress, float distanceTravelled)
    {
        foreach (RunningGear runningGear in railCarRunningGearList)
        {
            currentFrontSection = runningGear.UpdateRunningGearPosition(progress);
            runningGear.UpdateRunningGearRotation(distanceTravelled);
        }
    }
}
