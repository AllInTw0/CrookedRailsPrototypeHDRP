using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CargoSO", menuName = "ScriptableObjects/CargoSO", order = 1)]
public class CargoSO : ScriptableObject
{
    public string cargoName;

    [Header("Visual")]
    public GameObject cargoPrefab;

    [Header("Params")]
    public float weight;

    [Header("HaulingJobParams")]
    public List<CargoSO> fittingCargo = new List<CargoSO>();
    public float dangerLevel;
    public List<RailCarSO> fittingRailCars = new List<RailCarSO>();

    public string GetName()
    {
        if (cargoName == "")
            return this.name;
        else
            return cargoName;
    }
}
