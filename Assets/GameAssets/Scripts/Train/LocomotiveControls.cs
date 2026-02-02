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
    [Header("Values")]
    [SerializeField]
    private float supersonicSpeed;
    [SerializeField]
    private float supersonicAcceleration;
    [SerializeField]
    private float supersonicDeceleration;
    //Run Time
    private int throttleNotch;
    private int reverserNotch;
    private int locoBreaksNotch;

    private bool supersonic;
    private bool locked;
    private void Update()
    {
        if (locked)
        {
            throttle.currentNotch = throttleNotch;
            locoBreaks.currentNotch = locoBreaksNotch;
            reverser.currentNotch = reverserNotch;
            return;
        }
        else if(supersonic)
        {
            if(throttle.currentNotch > 0)
            {
                throttleNotch = throttle.notches;
            }
            if(throttleNotch > 0)
            {
                throttle.SetActionNameOverride("");
                throttle.currentNotch = throttleNotch;

                locoBreaks.currentNotch = 0;
                locoBreaksNotch = 0;

                reverser.currentNotch = 0;
                reverserNotch = 0;

                train.SetMaxSpeed(supersonicSpeed);
                train.SetAcceleration(supersonicAcceleration);
                train.SetDeceleration(0f);
            }
            return;
        }

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
        if(supersonic)
            train.SetDeceleration(supersonicDeceleration);
        else
            train.SetDeceleration(locoBreakDeceleration);

        supersonic = false;
    }

    public void LockControlls()
    {
        locked = true;
        Break();
        reverser.currentNotch = 0;
        reverserNotch = 0;

        throttle.SetLocked(true);
        locoBreaks.SetLocked(true);
        reverser.SetLocked(true);
    }
    public void ActivateSupersonic()
    {
        supersonic = true;
        locked = false;

        throttle.SetActionNameOverride("Set off");

        throttle.SetLocked(false);
        locoBreaks.SetLocked(true);
        reverser.SetLocked(true);
    }
    public void Unlock()
    {
        locked = false;
        throttle.SetLocked(false);
        locoBreaks.SetLocked(false);
        reverser.SetLocked(false);
    }

    public float GetDeceleration()
    {
        if (supersonic)
            return supersonicDeceleration;
        else
            return locoBreakDeceleration;
    }
}
