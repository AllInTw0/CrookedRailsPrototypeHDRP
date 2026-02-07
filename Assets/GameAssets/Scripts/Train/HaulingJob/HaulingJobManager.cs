using System.Collections.Generic;
using UnityEngine;

public class HaulingJobManager : MonoBehaviour
{
    public class HaulingJobEntry
    {
        public CargoSO cargo;
        public RailCarSO railCar;

        public float weight;
        public float pay;
    }

    [SerializeField]
    private List<CargoSO> haulingCargoInfoList = new List<CargoSO>();

    [Header("Temp")]
    public float maxCargoDangerLevel; public float targetDangerLevel; public float mixedLevel; public int maxCargoCount; public int currentLevel;
    public PaperRenderer paperRenderer;
    public List<HaulingJobEntry> GenerateHaulingJob(float maxCargoDangerLevel, float targetDangerLevel, float mixedLevel, int maxCargoCount, int currentLevel)
    {
        //Pick first cargo
        CargoSO firstCargo;
        int safety = 0;
        do
        {
            firstCargo = haulingCargoInfoList[Random.Range(0, haulingCargoInfoList.Count)];
            safety++;
        }
        while (firstCargo.dangerLevel > maxCargoDangerLevel && GetEligibleRailCars(firstCargo,currentLevel).Count == 0 && safety < 20);

        if (safety >= 10)
            Debug.LogWarning("Safety : " + safety + "!");

        //Pick rest of cargo
        float dangerSum = firstCargo.dangerLevel;
        List<CargoSO> cargoList = new List<CargoSO>() { firstCargo };
        while (dangerSum <= targetDangerLevel && cargoList.Count < maxCargoCount)
        {
            bool chooseMixed = Random.Range(0f, 1f) <= mixedLevel;
            CargoSO chosenCargo;
            safety = 0;
            do 
            {
                if (chooseMixed)
                {
                    chosenCargo = haulingCargoInfoList[Random.Range(0, haulingCargoInfoList.Count)];
                }
                else
                {
                    chosenCargo = cargoList[^1].fittingCargo[Random.Range(0, cargoList[^1].fittingCargo.Count)];
                }
                safety++;
            } while (GetEligibleRailCars(chosenCargo, currentLevel).Count == 0 && safety < 20);
            if (safety >= 10)
                Debug.LogWarning("Safety : " + safety + "!");

            dangerSum += chosenCargo.dangerLevel;
            cargoList.Add(chosenCargo);

            mixedLevel *= mixedLevel;
        }

        //Setup Haulling Entries
        List<HaulingJobEntry> haulingJobEntryList = new List<HaulingJobEntry>();
        foreach (CargoSO cargo in cargoList)
        {
            HaulingJobEntry entry = new HaulingJobEntry();
            entry.cargo = cargo;

            List<RailCarSO> eligibleRailCarList = GetEligibleRailCars(cargo, currentLevel);
            entry.railCar = eligibleRailCarList[Random.Range(0, eligibleRailCarList.Count)];

            entry.weight = Random.Range(10, 50);
            entry.pay = Random.Range(50, 500);

            haulingJobEntryList.Add(entry);
        }

        string debugString = "Hauling Job: ";
        foreach (HaulingJobEntry haulingJobEntry in haulingJobEntryList)
        {
            debugString += haulingJobEntry.cargo.GetName() + ":" + haulingJobEntry.railCar.GetName() + ", ";
        }
        Debug.Log(debugString);

        return haulingJobEntryList;
        
    }

    public List<RailCarSO> GetEligibleRailCars(CargoSO cargo, int level)
    {
        List<RailCarSO> railCarList = new List<RailCarSO>();
        foreach (RailCarSO railCar in cargo.fittingRailCars)
        {
            if(level >= railCar.minLevel && (level <= railCar.maxLevel || railCar.maxLevel == 0 || railCar.maxLevel == -1))
            {
                railCarList.Add(railCar);
            }
        }
        return railCarList;
    }
}
