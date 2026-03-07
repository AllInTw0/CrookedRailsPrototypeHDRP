using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropSpawner : StructureGenerator
{
    [System.Serializable]
    public class PropEntry
    {
        public GameObject propPrefab;
        public int maxCount;

        [HideInInspector]
        public int count;
    }

    [SerializeField]
    private List<PropEntry> serializedPropEntryList = new List<PropEntry>();
    [SerializeField]
    private int maxPropCount;
    [SerializeField]
    private int maxSpawnTries;

    public override IEnumerator Generate(StructureMaster structureMaster)
    {
        int spawnedPropCount = 0;
        int spawnTries = 0;

        List<PropEntry> propEntryList = new List<PropEntry>(serializedPropEntryList);
        foreach (PropEntry entry in propEntryList)
        {
            entry.count = 0;
        }

        PropSpawnBoundingBox[] propSpawnArray = transform.GetComponentsInChildren<PropSpawnBoundingBox>();

        while (spawnTries < maxSpawnTries && spawnedPropCount < maxPropCount && propEntryList.Count > 0)
        {
            //Spawn prop
            PropEntry chosenProp = propEntryList[Random.Range(0, propEntryList.Count)];
            BoxCollider propBoundingBox = chosenProp.propPrefab.GetComponent<BoxCollider>();

            //Get list of boundingBoxes the prop fits in
            List<PropSpawnBoundingBox> propSpawnList = new List<PropSpawnBoundingBox>(propSpawnArray);
            for (int i = 0; i < propSpawnList.Count; i++)
            {
                if (DoseBoxFitInBox(propSpawnList[i].boundingBox, propBoundingBox) == false)
                {
                    Debug.Log("Dosent fit");
                    propSpawnList.RemoveAt(i);
                    i--;
                } 
            }

            if(propSpawnList.Count == 0)
            {
                Debug.LogWarning(chosenProp.propPrefab + " dosent fit");
            }
            else
            {
                PropSpawnBoundingBox targetBoundingBoxScript = propSpawnList[Random.Range(0, propSpawnList.Count)];
                BoxCollider targetBoundingBox = targetBoundingBoxScript.boundingBox;

                Vector2 prop2DBounds = new Vector2(propBoundingBox.size.x, propBoundingBox.size.z);
                if (prop2DBounds.y > prop2DBounds.x) prop2DBounds = new Vector2(prop2DBounds.y, prop2DBounds.x);
                float prop2DHypotenuse = prop2DBounds.magnitude;
                Vector2 target2DBounds = new Vector2(targetBoundingBox.size.x, targetBoundingBox.size.z);
                if (target2DBounds.y > target2DBounds.x) target2DBounds = new Vector2(target2DBounds.y, target2DBounds.x);

                //Max rotOffset that fits box
                float rotLimit = Mathf.Rad2Deg * Mathf.Asin(Mathf.Min(target2DBounds.x, target2DBounds.y) / prop2DHypotenuse); // īsākā katete / hipotenūzu
                float propHypotenuseRot = Mathf.Rad2Deg * Mathf.Asin(Mathf.Min(prop2DBounds.x, prop2DBounds.y) / prop2DHypotenuse); // īsākā katete / hipotenūzu
                rotLimit -= propHypotenuseRot;

                if (rotLimit is float.NaN) //If rot limit is NaN then there is no limit
                {
                    Debug.Log("Rot is nan");
                    rotLimit = 180f;
                }


                float randomRot = Random.Range(-rotLimit, rotLimit);

                Vector2 maxOffset1 = new Vector2(target2DBounds.x - Mathf.Cos((propHypotenuseRot + Mathf.Abs(randomRot)) * Mathf.Deg2Rad) * prop2DHypotenuse, target2DBounds.y - Mathf.Sin((propHypotenuseRot + Mathf.Abs(randomRot)) * Mathf.Deg2Rad) * prop2DHypotenuse);
                Vector2 maxOffset2 = new Vector2(target2DBounds.x - Mathf.Cos((propHypotenuseRot - Mathf.Abs(randomRot)) * Mathf.Deg2Rad) * prop2DHypotenuse, target2DBounds.y - Mathf.Sin((propHypotenuseRot - Mathf.Abs(randomRot)) * Mathf.Deg2Rad) * prop2DHypotenuse);
                Vector2 maxOffset = new Vector2(Mathf.Min(maxOffset1.x, maxOffset2.x) * 0.5f, Mathf.Min(maxOffset1.y, maxOffset2.y) * 0.5f);
                Debug.Log("rotLimit: " + rotLimit + ", randomRot: " + randomRot + ", maxOffset: " + maxOffset);

                Vector2 offset = new Vector2(Random.Range(-maxOffset.x, maxOffset.x), Random.Range(-maxOffset.y, maxOffset.y));

                //Spawn prop
                GameObject propCopy = Instantiate(chosenProp.propPrefab, targetBoundingBox.transform);

                //A little spaghety but it works
                void SetPos(bool case1)
                {
                    if (case1)
                    {
                        propCopy.transform.localPosition = new Vector3(offset.x, 0, offset.y);
                        propCopy.transform.localRotation = Quaternion.Euler(0, randomRot, 0);
                    }
                    else
                    {
                        propCopy.transform.localPosition = new Vector3(offset.y, 0, offset.x);
                        propCopy.transform.localRotation = Quaternion.Euler(0, randomRot + 90, 0);
                    }
                }
                if(targetBoundingBox.size.x >= targetBoundingBox.size.z)
                {
                    SetPos(propBoundingBox.size.x >= propBoundingBox.size.z);
                }
                else
                {
                    SetPos(propBoundingBox.size.x <= propBoundingBox.size.z);
                }
                


                chosenProp.count++;
                spawnedPropCount++;

                if(chosenProp.count > chosenProp.maxCount)
                {
                    propEntryList.Remove(chosenProp);
                }
            }

            spawnTries++;
        }

        yield break;
    }

    public bool DoseBoxFitInBox(BoxCollider bounds, BoxCollider box)
    {
        if (box.size.y > bounds.size.y) return false;

        if (Mathf.Min(box.size.x, box.size.z) > Mathf.Min(bounds.size.x, bounds.size.z)) return false;
        if (Mathf.Max(box.size.x, box.size.z) > Mathf.Max(bounds.size.x, bounds.size.z)) return false;

        return true;
    }
}
