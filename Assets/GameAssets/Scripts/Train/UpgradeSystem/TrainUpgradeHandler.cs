using System.Collections.Generic;
using UnityEngine;

public enum TrainStatType
{
    None,
    WaterConsumption,
    FuelConsumption,
    Speed,
    Acceleration,
    Deceleration,
    BreakdownChance,
}
public enum UpgradeType
{
    Percent,
    Addition
}
[System.Serializable]
public class Upgrade
{
    public TrainStatType trainStat;
    public UpgradeType upgradeType;
    public float value;
}
public class TrainUpgradeHandler : MonoBehaviour
{
    [System.Serializable]
    public class DefaultStats
    {
        public TrainStatType trainStat;
        public string statName;
        public float defaultValue;
    }

    [SerializeField]
    private List<DefaultStats> defaultStatsList;

    //Run time
    private List<UpgradeSO> boughtUpgradeList = new List<UpgradeSO>();
    private List<UpgradeSO> currentUpgradeList = new List<UpgradeSO>();

    private Dictionary<TrainStatType, DefaultStats> defaultStatsDict = new Dictionary<TrainStatType, DefaultStats>();
    private Dictionary<TrainStatType, Upgrade> currentUpgradeDict = new Dictionary<TrainStatType, Upgrade>();
    void Start()
    {
        foreach (DefaultStats defaultStats in defaultStatsList)
        {
            defaultStatsDict.Add(defaultStats.trainStat, defaultStats);
        }
    }

    void Update()
    {
        
    }
}
