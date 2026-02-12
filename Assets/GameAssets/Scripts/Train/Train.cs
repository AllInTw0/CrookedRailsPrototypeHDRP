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
    public float couplerLength;
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
    private bool activeSupersonic;
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
        //Debug.Log(deceleration);
        if(frontTrackSection == null)
        {
            Debug.LogWarning("No track section assigned!");
            return;
        }

        //Handle AutoStops
        TrackManager.active.GetTrackSectionFromProgress(sectionProgress - maxAutoStopOffset, frontTrackSection, out TrackSection newSection, out float newSectionProgress);
        if (deceleration == 0f && TrackManager.active.GetNearestAutoStop(newSectionProgress, newSection, 3, out AutoStop nearestAutoStop, out float distanceToAutoStop))
        {
            TrackManager.active.GetTrackPositionFromProgress(newSectionProgress, newSection, out Vector3 pos1);
            TrackManager.active.GetTrackPositionFromProgress(newSectionProgress + distanceToAutoStop, newSection, out Vector3 pos2);
            Debug.DrawLine(pos1 + Vector3.up, pos2 + Vector3.up, Color.violetRed);

            float distanceToStop = (speed * speed) / (2 * controlls.GetDeceleration()); //v^2 - v0^2 = 2as

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
                    activeSupersonic = true;
                }
                else
                {
                    controlls.Break();
                }
                nearestAutoStop.ignore = true;
            }
        }
        if(activeSupersonic && Mathf.Abs(speed) < 0.01f)
        {
            activeSupersonic = false;
            controlls.ActivateSupersonic();
        }
        //update Speed    
        if (Mathf.Abs(speed) < Mathf.Abs(maxSpeed) || (speed > 0 && acceleration < 0) || (speed < 0 && acceleration > 0))
        {
            speed += acceleration * Time.deltaTime;
            speed = Mathf.Clamp(speed , -maxSpeed, maxSpeed);
        }
        
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
        consistLength = CalculateConsistLength(false);
    }
    public float CalculateConsistLength(bool onlyPlayerRailCars = false)
    {
        float length = 0f;
        foreach (RailCar railCar in consist)
        {
            if(onlyPlayerRailCars)
            {
                if(railCar.isPlayerRailCar)
                    length += railCar.frontLength + railCar.backLength + couplerLength;
            }
            else
                length += railCar.frontLength + railCar.backLength + couplerLength;
        }
        return length > 0 ? length - couplerLength : 0;
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

    public List<CargoInfo> GetConsistCargoInfo()
    {
        List<CargoInfo> cargoInfoList = new List<CargoInfo>();

        foreach (RailCar railCar in consist)
        {
            cargoInfoList.Add(railCar.GetCargoInfo());
        }

        return cargoInfoList;
    }
    public RailCar AddRailCar(RailCarSO railCarInfo, int index)
    {
        RailCar railCarCopy = Instantiate(railCarInfo.prefab,transform).GetComponent<RailCar>();
        List<RailCar> listFront = new List<RailCar>();
        List<RailCar> listBack = new List<RailCar>();

        for (int i = 0; i < consist.Count; i++)
        {
            if (i < index)
                listFront.Add(consist[i]);
            else
                listBack.Add(consist[i]);
        }

        consist = listFront;
        consist.Add(railCarCopy);
        consist.AddRange(listBack);

        RecalculateConsistLength();

        return railCarCopy;
    }

    public void RemoveNonPlayerRailCars()
    {
        for (int i = 0; i < consist.Count; i++)
        {
            if (consist[i].isPlayerRailCar == false)
            {
                Destroy(consist[i].gameObject);
                consist.RemoveAt(i);
                i--;
            }
        }

        Debug.Log("Removed NonPlayer RailCars");
    }
}
