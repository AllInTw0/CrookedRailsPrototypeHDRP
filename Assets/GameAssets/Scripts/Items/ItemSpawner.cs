using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : StructureGenerator
{
    [System.Serializable]
    public class ItemSpawnEntry
    {
        public ItemSO item;
        public float probability;
        public Vector2Int minMaxStackCount;
    }

    [SerializeField]
    private List<ItemSpawnEntry> itemSpawnEntryList;
    [SerializeField]
    private Vector2Int minMaxCount;

    public override IEnumerator Generate(StructureMaster structureMaster)
    {
        if (itemSpawnEntryList.Count == 0)
        {
            Debug.LogWarning("No Items Defined!");
            yield break;
        }

        ItemSpawnBounds[] structureItemSpawnBoundsArray =transform.GetComponentsInChildren<ItemSpawnBounds>();

        //Sort
        bool sorted = false;
        while (sorted == false)
        {
            sorted = true;
            for (int i = 0; i < itemSpawnEntryList.Count - 1; i++)
            {
                if (itemSpawnEntryList[i].probability < itemSpawnEntryList[i + 1].probability)
                {
                    var temp = itemSpawnEntryList[i];
                    itemSpawnEntryList[i] = itemSpawnEntryList[i + 1];
                    itemSpawnEntryList[i + 1] = temp;
                    sorted = false;
                }
            }
        }

        float probabilitySum = 0f;
        foreach (ItemSpawnEntry item in itemSpawnEntryList)
        {
            probabilitySum += item.probability;
        }

        int count = Random.Range(minMaxCount.x, minMaxCount.y);
        for (int a = 0; a < count; a++)
        {
            float randomProbability = Random.Range(0f, probabilitySum);

            for (int i = 0; i < itemSpawnEntryList.Count; i++)
            {
                randomProbability -= itemSpawnEntryList[i].probability;
                if (randomProbability <= 0f)
                {
                    ItemSO itemSO = itemSpawnEntryList[i].item;

                    //Get valid spawns points
                    List<ItemSpawnBounds> itemSpawnBoundsList = new List<ItemSpawnBounds>(structureItemSpawnBoundsArray);
                    for (int b = 0; b < itemSpawnBoundsList.Count; b++)
                    {
                        if (itemSpawnBoundsList[b].CanSpawnItem(itemSO) == false)
                        {
                            itemSpawnBoundsList.RemoveAt(b);
                            b--;
                        }
                    }

                    if (itemSpawnBoundsList.Count == 0)
                    {
                        Debug.Log("No valid bounds. Spawning next item in probability list? Is this a good idea?");
                    }
                    else
                    {
                        ItemSpawnBounds chosenBounds = itemSpawnBoundsList[Random.Range(0, itemSpawnBoundsList.Count)];
                        Vector3 pos = chosenBounds.transform.position + chosenBounds.transform.right * Random.Range(-chosenBounds.maxRandomOffset.x, chosenBounds.maxRandomOffset.x) + chosenBounds.transform.up * Random.Range(-chosenBounds.maxRandomOffset.y, chosenBounds.maxRandomOffset.y) + chosenBounds.transform.forward * Random.Range(-chosenBounds.maxRandomOffset.z, chosenBounds.maxRandomOffset.z);
                        Item spawnedItem = Item.SpawnItem(itemSO, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), Random.Range(itemSpawnEntryList[i].minMaxStackCount.x, itemSpawnEntryList[i].minMaxStackCount.y));
                        spawnedItem.transform.SetParent(transform);
                        chosenBounds.AddItem(spawnedItem);
                        yield return new WaitForSeconds(0.05f);
                        break;
                    }
                }
            }
        }

        yield break;
    }
}
