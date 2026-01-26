using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Train : MonoBehaviour
{
    [SerializeField]
    private List<RailCar> consist = new List<RailCar>();
    [SerializeField] 
    private float couplerLength;
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
    private void Start()
    {
        RecalculateConsistLength();
    }

    private void Update()
    {
        if(frontTrackSection == null)
        {
            Debug.LogWarning("No track section assigned!");
            return;
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
        if (TrackManager.active.GetTrackSectionFromProgress(sectionProgress, frontTrackSection, out TrackSection newSection, out float newSectionProgress))
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
}
