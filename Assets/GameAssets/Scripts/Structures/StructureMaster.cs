using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
[System.Serializable]
public class Connection 
{
    //public string name;
    public Transform connectionTransform;
    public List<string> validConnectionNameList;
    public List<GameObject> endPrefabList;

    //Nonserializeable
    [HideInInspector]
    public Connection connectedConnection;
}
[System.Serializable]
public class GenerationEntry
{
    public List<GameObject> sectionPrefabList;
    
    public enum CountType
    {
        minMaxRandom,
        fillLenght,
        probabilityCurve
    }
    [Header("Count")]
    public CountType countType;
    public Vector2Int minMaxCount = Vector2Int.one;
    public LengthType lengthType;
    public float lengthAddition;
    public AnimationCurve countCurve;
}
public class StructureMaster : MonoBehaviour
{
    public List<StructureGenerator> structureList;
    public StructureType structureType;
    public Connection startConnection;
    public LayerMask overlapCheckLayerMask;

    private List<Connection> structureConnectionList;
    private List<Section> structureSectionList;
    private int count;
    public void Generate()
    {
        structureConnectionList = new List<Connection>();
        startConnection.connectedConnection = null;
        structureConnectionList.Add(startConnection);
        structureSectionList = new List<Section>();
        count = 0;

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
    public void DestroyStructure()
    {
        foreach (Section section in structureSectionList)
        {
            DestroyImmediate(section.gameObject);
        }
    }
    public float GetLength(LengthType lengthType)
    {
        if (lengthType == LengthType.PlayerTrainLength)
            return Train.playerTrain != null ? Train.playerTrain.GetConsistLenght() : 50f;
        else if(lengthType == LengthType.MaxChosenTrainLength)
        {
            float maxLength = 0f;
            for (int i = 0; i < HaulingJobManager.generatedHaulingJobList.Count; i++)
            {
                float l = HaulingJobManager.generatedHaulingJobList[i].GetConsistLength() + Train.playerTrain.CalculateConsistLength(true);
                if (l > maxLength) maxLength = l;
            }
            return maxLength;
        }

        Debug.LogWarning("Couldnt get length");
        return 0f;
    }
    public void SpawnEndPrefabs()
    {
        foreach (Connection connection in structureConnectionList)
        {
            if((connection.connectedConnection == null || connection.connectedConnection == startConnection) && connection.endPrefabList.Count > 0)
            {
                Transform copy = Instantiate(connection.endPrefabList[Random.Range(0, connection.endPrefabList.Count)]).transform;
                copy.SetParent(connection.connectionTransform);
                copy.localPosition = Vector3.zero;
                copy.localRotation = Quaternion.identity;
            }
        }
    }
    public Section SpawnSection(GameObject prefab)
    {
        GameObject sectionObject = Instantiate(prefab, transform);
        Transform sectionTransform = sectionObject.transform;
        Section sectionScript = sectionObject.GetComponent<Section>();

        sectionScript.Initialize();

        //Get all valid connections
        List<Connection> sectionConnectionList = new List<Connection>();
        List<List<Connection>> validConnectionList = new List<List<Connection>>();
        foreach (Connection connection in sectionScript.GetConnectingConnectionList())
        {
            List<Connection> connectionList = new List<Connection>();
            foreach (Connection otherConnection in structureConnectionList)
            {
                if(otherConnection.connectedConnection == null && DoesStringListMatch(connection.validConnectionNameList, otherConnection.connectionTransform.name))
                {
                    connectionList.Add(otherConnection);
                }
            }

            if (connectionList.Count > 0) 
            {
                sectionConnectionList.Add(connection);
                validConnectionList.Add(connectionList);
            }
        }

        //Chose connection
        if(validConnectionList.Count == 0)
        {
            Debug.LogWarning("Cant find valid connection: validConnectionList.Count == 0, " + sectionObject.name);
            return sectionScript;
        }
        while (sectionConnectionList.Count > 0 && validConnectionList.Count > 0)
        {
            int connectionListIndex = Random.Range(0, validConnectionList.Count);
            int connectionIndex = Random.Range(0, validConnectionList[connectionListIndex].Count);
            Connection sectionConnection = sectionConnectionList[connectionListIndex];
            Connection otherSectionConnection = validConnectionList[connectionListIndex][connectionIndex];


            //Set position
            sectionTransform.SetParent(otherSectionConnection.connectionTransform);
            sectionTransform.localRotation = Quaternion.identity;
            sectionTransform.localPosition = -sectionConnection.connectionTransform.localPosition;
            sectionTransform.RotateAround(sectionConnection.connectionTransform.position, Vector3.up, otherSectionConnection.connectionTransform.eulerAngles.y - sectionConnection.connectionTransform.eulerAngles.y + 180f);

            //Check overlap
            if (DoesSectionOverlap(sectionScript))
            {
                //try another connection
                validConnectionList[connectionListIndex].RemoveAt(connectionIndex);
                if(validConnectionList[connectionListIndex].Count == 0)
                {
                    validConnectionList.RemoveAt(connectionListIndex);
                    sectionConnectionList.RemoveAt(connectionListIndex);
                }

                if(sectionConnectionList.Count == 0 || validConnectionList.Count == 0)
                {
                    DestroyImmediate(sectionObject);
                    Debug.Log("Couldnt spawn " + sectionObject.name);
                    return null;
                }
                continue;
            }

            //Connect section
            sectionConnection.connectedConnection = otherSectionConnection;
            otherSectionConnection.connectedConnection = sectionConnection;
            break;
        }
        //Debug.DrawLine(sectionConnection.connectionTransform.position, otherSectionConnection.connectionTransform.position, Color.purple, 60f);
        //Debug.DrawRay(sectionConnection.connectionTransform.position, Vector3.up, Color.purple, 60f);

        sectionTransform.SetParent(transform);

        //Rename strings + connect connections close to each other
        count++;
        foreach (Connection connection in sectionScript.GetAllConnectionList())
        {
            //rename
            connection.connectionTransform.name = RenameString(connection.connectionTransform.name, count);

            //connect
            foreach (Connection connectionB in structureConnectionList)
            {
                if(Vector3.Distance(connection.connectionTransform.position,connectionB.connectionTransform.position) < 0.01f)
                {
                    connection.connectedConnection = connectionB;
                    connectionB.connectedConnection = connection;
                }
            }
        }

        structureConnectionList.AddRange(sectionScript.GetAllConnectionList());
        structureSectionList.Add(sectionScript);

        return sectionScript;
    }
    public bool DoesSectionOverlap(Section section)
    {
        BoxCollider[] boxColliderArray = section.GetBoxColliderArray();
        foreach (BoxCollider boxCollider in boxColliderArray)
        {
            Collider[] overlapingColiderArray = Physics.OverlapBox(section.transform.TransformPoint(boxCollider.center), boxCollider.size * 0.5f, section.transform.rotation, overlapCheckLayerMask);
            foreach (Collider collider in overlapingColiderArray)
            {
                foreach (BoxCollider sectionBoxCollider in boxColliderArray)
                {
                    if (collider != sectionBoxCollider) 
                    { 
                        Debug.Log(section.gameObject.name + " overlaps " + collider.gameObject.name);
                        Debug.DrawLine(section.transform.TransformPoint(boxCollider.center), collider.transform.position,Color.red,60f);
                        return true; 
                    }
                }
            }
        }
        return false;
    }
    public bool DoesStringListMatch(List<string> stringList, string targetString)
    {
        foreach (string str in stringList)
        {
            bool stringsMatch = DoStringsMatch(str, targetString);
            Debug.Log(str + ", " + targetString + " : " + stringsMatch);

            if (stringsMatch) return true;
        }
        return false;
    }

    public bool DoStringsMatch(string str, string targetStr)
    {
        str = str.Trim();
        targetStr = targetStr.Trim();

        if (str == targetStr) return true;

        //Special Characters
        int starIndex = str.IndexOf('*');
        if(starIndex != -1)
        {
            if(targetStr.Length >= starIndex)
                return str[0..starIndex] == targetStr[0..starIndex];
        }

        return false;
    }
    public string RenameString(string str, int countIndex)
    {
        int index = str.IndexOf("_");
        if (index != -1)
        {
            str = str[0..index] + countIndex;
        }
        return str;
    }
}
