using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
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
    private Vector3 maxRandomOffset;
    public void Start()
    {
        SpawnItems();
    }

    public void SpawnItems()
    {
        if(itemSpawnEntryList.Count == 0)
        {
            Debug.LogWarning("No Items Defined!");
            return;
        }

        //Sort
        bool sorted = false;
        while(sorted == false)
        {
            sorted = true;
            for (int i = 0; i < itemSpawnEntryList.Count-1; i++)
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

        float randomProbability = Random.Range(0f, probabilitySum);

        for (int i = 0; i < itemSpawnEntryList.Count; i++)
        {
            randomProbability -= itemSpawnEntryList[i].probability;
            if (randomProbability <= 0f)
            {
                Vector3 pos = transform.position += new Vector3(Random.Range(-maxRandomOffset.x, maxRandomOffset.x), Random.Range(-maxRandomOffset.y, maxRandomOffset.y), Random.Range(-maxRandomOffset.z, maxRandomOffset.z));
                Item.SpawnItem(itemSpawnEntryList[i].item, pos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), Random.Range(itemSpawnEntryList[i].minMaxStackCount.x, itemSpawnEntryList[i].minMaxStackCount.y));
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.purple;

        Gizmos.DrawWireCube(transform.position, maxRandomOffset * 2f);
    }
}
