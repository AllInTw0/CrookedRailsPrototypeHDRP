using System;
using UnityEngine;

public class LocomotiveControls : MonoBehaviour
{
    [SerializeField] 
    private Train train;
    
    [Header("Controls")]
    [SerializeField] 
    private LeverInteractable throttle;
    [SerializeField] 
    private LeverInteractable reverser;
    [SerializeField] 
    private LeverInteractable locoBreaks;
    
    [Header("Values")]
    [SerializeField] 
    private float maxSpeed;
    [SerializeField] 
    private float maxAcceleration;
    [SerializeField] 
    public float locoBreakDeceleration;
    
    //Run Time
    private int throttleNotch;
    private int reverserNotch;
    private int locoBreaksNotch;
    private void Update()
    {
        if (throttleNotch != throttle.currentNotch)
        {
            throttleNotch = throttle.currentNotch;
            
            locoBreaks.currentNotch = 0;
            locoBreaksNotch = 0;
            
            train.SetMaxSpeed((throttleNotch / (float)throttle.notches) * maxSpeed);
            if(throttleNotch == 0)
                train.SetAcceleration(0f);
            else
                train.SetAcceleration(reverserNotch == 0 ? maxAcceleration : -maxAcceleration);
            
            train.SetDeceleration(0f);
        }
        if (locoBreaksNotch != locoBreaks.currentNotch)
        {
            locoBreaksNotch = locoBreaks.currentNotch;

            throttle.currentNotch = 0;
            throttleNotch = 0;
            
            train.SetMaxSpeed(0f);
            train.SetAcceleration(0f);
            train.SetDeceleration(locoBreakDeceleration);
        }
        if (reverserNotch != reverser.currentNotch)
        {
            reverserNotch = reverser.currentNotch;
            
            if(throttleNotch == 0)
                train.SetAcceleration(0f);
            else
                train.SetAcceleration(reverserNotch == 0 ? maxAcceleration : -maxAcceleration);
        }
    }

    public void Break()
    {
        throttle.currentNotch = 0;
        throttleNotch = 0;

        locoBreaks.currentNotch = 1;
        locoBreaksNotch = 1;

        train.SetMaxSpeed(0f);
        train.SetAcceleration(0f);
        train.SetDeceleration(locoBreakDeceleration);
    }
}
