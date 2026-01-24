using System;
using UnityEngine;

public class RailCar : MonoBehaviour
{
    //Variables
    public float frontLength = 5f;
    public float backLength = 5f;
    public float wheelLength = 2f;
    
    //Run Time
    [NonSerialized]
    public TrackSection currentFrontSection;
    public void UpdateProgress(float value)
    {
        TrackManager.active.GetTrackPositionFromProgress(value + wheelLength*0.5f, out TrackSection sectionFront, out Vector3 posFront);
        TrackManager.active.GetTrackPositionFromProgress(value - wheelLength*0.5f, out TrackSection sectionBack, out Vector3 posBack);
        
        currentFrontSection = sectionFront;
        
        transform.position = (posFront + posBack) * 0.5f;
        transform.LookAt(posFront);
    }
}
