using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RailCarSO", menuName = "ScriptableObjects/RailCarSO", order = 2)]
public class RailCarSO : ScriptableObject
{
    public string railCarName;

    [Header("Visual")]
    public GameObject prefab;
    public Sprite icon;

    [Header("Params")]
    public float weight;

    [Header("HaulingJobParams")]
    public int minLevel;
    public int maxLevel;
    public string GetName()
    {
        if (railCarName == "")
            return this.name;
        else
            return railCarName;
    }
}
