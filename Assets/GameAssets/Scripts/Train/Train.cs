using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Train : MonoBehaviour
{
    public static Train playerTrain;

    //Variables
    [SerializeField]
    private bool isPlayerTrain;
    [SerializeField]
    private List<RailCar> consist = new List<RailCar>();
    [SerializeField] 
    private float couplerLength;
    [SerializeField]
    private LocomotiveControls controlls;

    //Run Time
    [NonSerialized]
    public float acceleration;
    [NonSerialized]
    public float deceleration;
    [NonSerialized]
    public float maxSpeed;
    
    private float speed;
    private TrackSection frontTrackSection;
    private float sectionProgress;
    private float consistLength;

    private float maxAutoStopOffset;
    private void Start()
    {
        if (isPlayerTrain)
            playerTrain = this;

        RecalculateConsistLength();

        foreach (AutoStopType type in Enum.GetValues(typeof(AutoStopType)))
        {
            float offset = GetAutoStopTypeOffset(type);
            if (offset > maxAutoStopOffset)
                maxAutoStopOffset = offset;
        }
    }

    private void Update()
    {
        if(frontTrackSection == null)
        {
            Debug.LogWarning("No track section assigned!");
            return;
        }

        //Handle AutoStops
        TrackManager.active.GetTrackSectionFromProgress(sectionProgress - maxAutoStopOffset, frontTrackSection, out TrackSection newSection, out float newSectionProgress);
        if (TrackManager.active.GetNearestAutoStop(newSectionProgress, newSection, 3, out AutoStop nearestAutoStop, out float distanceToAutoStop))
        {
            float distanceToStop = (speed * speed) / (2 * controlls.locoBreakDeceleration); //v^2 - v0^2 = 2as

            //Handle diffrent types of autostop type distances
            float autoStopOffset = GetAutoStopTypeOffset(nearestAutoStop.stopType);
            distanceToAutoStop -= maxAutoStopOffset - autoStopOffset;

            //Debug.Log("Distance to stop: " + distanceToStop + ", distance: " + distanceToAutoStop);
            if(distanceToAutoStop <= distanceToStop)
            {
                Debug.Log("Engaging breaks! Detected autostop");
                if (nearestAutoStop.stopType == AutoStopType.Supersonic)
                {
                    controlls.LockControlls();
                }
                else
                {
                    controlls.Break();
                }
                nearestAutoStop.ignore = true;
            }
        }

        //update Speed    
        if(Mathf.Abs(speed) < Mathf.Abs(maxSpeed) || (speed > 0 && acceleration < 0) || (speed < 0 && acceleration > 0))
            speed += acceleration * Time.deltaTime;
        
        if (deceleration > 0f)
        {
            if (speed > 0)
            {
                speed -= deceleration * Time.deltaTime;
                if (speed < 0)
                    speed = 0;
            }
            if (speed < 0)
            {
                speed += deceleration * Time.deltaTime;
                if (speed > 0)
                    speed = 0;
            }
        }

        float distanceTravelled = speed * Time.deltaTime;
        sectionProgress += distanceTravelled;

        //Check front
        if (TrackManager.active.GetTrackSectionFromProgress(sectionProgress, frontTrackSection, out newSection, out newSectionProgress))
        {
            //Update section
            sectionProgress = newSectionProgress;
            frontTrackSection = newSection;
        }
        else if(speed > 0)
        {
            //End of track
            Debug.Log("End of track front");
            speed *= -0.5f;
        }


        //Check back
        if (TrackManager.active.GetTrackSectionFromProgress(sectionProgress - consistLength, frontTrackSection, out newSection, out newSectionProgress) == false && speed < 0)
        {
            Debug.Log("End of track back");
            speed *= -0.5f;
        }


        float railCarProgress = sectionProgress;
        TrackSection railCarSection = frontTrackSection;
        foreach (var railCar in consist)
        {
            TrackManager.active.GetTrackSectionFromProgress(railCarProgress - railCar.frontLength, railCarSection, out newSection, out newSectionProgress);
            railCarProgress = newSectionProgress;
            railCarSection = newSection;

            railCar.UpdateRailCar(railCarProgress, railCarSection, distanceTravelled);

            railCarProgress -= railCar.backLength + couplerLength;
        }
    }

    public void SetAcceleration(float value)
    {
        acceleration = value;
    }
    public void SetDeceleration(float value)
    {
        deceleration = value;
    }
    public void SetMaxSpeed(float value)
    {
        maxSpeed = value;
    }
    public RailCar GetRailCarAtIndex(int index)
    {
        return consist[index];
    }
    public void OffsetProgress(float value)
    {
        sectionProgress += value;
    }
    private void RecalculateConsistLength()
    {
        float length = 0f;
        foreach (var railCar in consist)
        {
            length += railCar.frontLength + railCar.backLength + couplerLength;
        }
        consistLength = length - couplerLength;
    }
    public float GetConsistLenght()
    {
        if (consistLength == 0)
            RecalculateConsistLength();

        return consistLength;
    }
    public void Initialize(float sectionProgress, TrackSection trackSection)
    {
        this.frontTrackSection = trackSection;
        this.sectionProgress = sectionProgress;
    }
    public TrackSection GetFrontTrackSection()
    {
        return frontTrackSection;
    }
    public float GetMaxDeceleration()
    {
        return controlls.locoBreakDeceleration;
    }
    public float GetSpeed()
    {
        return speed;
    }
    public float GetAutoStopTypeOffset(AutoStopType type)
    {
        if (type == AutoStopType.Front || type == AutoStopType.Supersonic)
            return 0f;
        else if(type == AutoStopType.TenderHatch)
        {
            float offset = 0f;
            for (int i = 0; i < consist.Count; i++)
            {
                offset += consist[i].frontLength;
                if (consist[i].TryGetComponent(out Tender tender))
                {
                    return offset += tender.waterHatchOffset;
                }
                offset += consist[i].backLength;
            }
        }
        return 0f;
    }
}
