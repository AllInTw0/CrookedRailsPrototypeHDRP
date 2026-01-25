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
    private float progress;
    private float consistLength;
    private void Start()
    {
        foreach (var railCar in consist)
        {
            progress += railCar.frontLength + railCar.backLength + couplerLength;
        }
        consistLength = progress - couplerLength;
        progress = consistLength + 5f;
    }

    private void Update()
    {
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
        progress += distanceTravelled;

        if (progress < consistLength && speed < 0)
        {
            speed = -speed * 0.5f;
            progress = consistLength;
        }
        
        float railCarProgress = progress;
        foreach (var railCar in consist)
        {
            railCarProgress -= railCar.frontLength;
            railCar.UpdateRailCar(railCarProgress, distanceTravelled);
            railCarProgress -= railCar.backLength;
            railCarProgress -= couplerLength;
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

    public TrackSection GetFrontMostTrackSection()
    {
        return consist[0].currentFrontSection;
    }
    public RailCar GetRailCarAtIndex(int index)
    {
        return consist[index];
    }
    public void OffsetProgress(float value)
    {
        progress += value;
    }
}
