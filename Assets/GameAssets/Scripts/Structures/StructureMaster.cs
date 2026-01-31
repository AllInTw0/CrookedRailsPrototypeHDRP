using System.Collections.Generic;
using UnityEngine;

public enum LengthType
{
    None,
    PlayerTrainLength,
    MaxChosenTrainLength
}
public enum StructureType
{
    None,
    Station
}
public class StructureMaster : MonoBehaviour
{
    public List<StructureGenerator> structureList;
    public StructureType structureType;
    public void Generate()
    {
        foreach (StructureGenerator structure in structureList)
        {
            structure.Generate(this);
        }
        if(structureType == StructureType.Station)
        {
            GameStateManager.isStationSpawned = true;
        }
    }
    public void OnDestroy()
    {
        if (structureType == StructureType.Station)
        {
            GameStateManager.isStationSpawned = false;
        }
    }
    public float GetLength(LengthType lengthType)
    {
        if (lengthType == LengthType.PlayerTrainLength)
            return Train.playerTrain.GetConsistLenght();
        else if(lengthType == LengthType.MaxChosenTrainLength)
            return Train.playerTrain.GetConsistLenght();

        Debug.LogWarning("Couldnt get length");
        return 0f;
    }
}
