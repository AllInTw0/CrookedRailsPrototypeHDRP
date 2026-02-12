using System;
using System.Collections.Generic;
using UnityEngine;

public class CargoInfo
{
    public CargoSO cargoInfo;
    public GameObject cargoObject;

    public float cargoValue;
    public Health cargoHealth;

    public float railCarValue;
    public Health railCarHealth;

    public RailCar railCarRefrence;

    public float GetValueSum()
    {
        return cargoValue + railCarValue;
    }
    public float GetExpensesSum()
    {
        return GetValueSum() * (2f - (cargoHealth.health / cargoHealth.maxHealth) - (railCarHealth.health / railCarHealth.maxHealth));
    }
    public float GetPaySum()
    {
        return GetValueSum() - GetExpensesSum();
    }
}
public class RailCar : MonoBehaviour
{
    //Variables
    [Header("Parameters")]
    public RailCarSO railCarSO;
    public float frontLength = 5f;
    public float backLength = 5f;
    public bool isPlayerRailCar;
    //public float wheelLength = 2f;
    public List<RunningGear> railCarRunningGearList;
    [Header("Health")]
    [SerializeField]
    private Health health;
    [Header("Cargo")]
    [SerializeField]
    private Transform cargoOrigin;

    //Run Time
    //[NonSerialized]
    //public TrackSection currentFrontSection;

    private CargoInfo currentCargo;

    private void Awake()
    {
        currentCargo = new CargoInfo();
        currentCargo.railCarRefrence = this;
        currentCargo.railCarHealth = health;
    }
    public void UpdateRailCar(float sectionProgress, TrackSection trackSection, float distanceTravelled)
    {
        foreach (RunningGear runningGear in railCarRunningGearList)
        {
            runningGear.UpdateRunningGearPosition(sectionProgress, trackSection);
            runningGear.UpdateRunningGearRotation(distanceTravelled);
        }
    }
    public CargoInfo GetCargoInfo()
    {
        return currentCargo;
    }
    public void SetCargo(CargoSO cargoSO, float cargoValue, float railCarValue)
    {
        CargoInfo cargoInfo = new CargoInfo();
        cargoInfo.cargoInfo = cargoSO;

        cargoInfo.cargoObject = Instantiate(cargoSO.cargoPrefab, cargoOrigin);
        cargoInfo.cargoObject.transform.localPosition = Vector3.zero;
        cargoInfo.cargoObject.transform.localRotation = Quaternion.identity;

        cargoInfo.cargoValue = cargoValue;
        cargoInfo.cargoHealth = cargoInfo.cargoObject.GetComponent<Health>();

        cargoInfo.railCarValue = railCarValue;
        cargoInfo.railCarHealth = health;

        cargoInfo.railCarRefrence = this;

        currentCargo = cargoInfo;
    }
}
