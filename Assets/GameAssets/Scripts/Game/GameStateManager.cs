using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class UnlockEntry
{
    public ShopItemSO targetShopItem;
    public List<ShopItemSO> neededShopItemList;
    public bool isUnlocked;
}
public class GameStateManager : MonoBehaviour
{
    //Serialized

    //Run Time
    public static HaulingJob currentHaulingJob;

    public static bool isStartingLocationSpawned;
    public static bool isStationSpawned;

    public static int currentLevel = 1;

    public static float distanceTravelled = 0f;

    public static bool canEnemiesSpawn
    {
        get
        {
            return !(isStartingLocationSpawned || isStationSpawned);
        }
    }
    public static List<DistanceWaypoint> waypointList = new List<DistanceWaypoint>();

    public static List<ShopItemSO> boughtItemList = new List<ShopItemSO>();

    public static List<UnlockEntry> unlockList;
    [SerializeField]
    private List<UnlockEntry> _unlockList;
    private void Start()
    {
        unlockList = _unlockList;
        Money.SetStartingMoney();
    }

    public static bool IsItemUnlocked(ShopItemSO shopItem)
    {
        foreach (ShopItemSO neededBoughtItem in shopItem.neededBoughtItems)
        {
            if(boughtItemList.Contains(neededBoughtItem) == false)
            {
                return false;
            }
        }

        foreach (UnlockEntry unlockEntry in unlockList)
        {
            if(unlockEntry.targetShopItem == shopItem && unlockEntry.isUnlocked == false)
            {
                return false;
            }
        }

        if(shopItem.maxCountBought >= 0)
        {
            int countFound = 0;
            for (int i = 0; i < boughtItemList.Count; i++)
            {
                if (boughtItemList[i] == shopItem) countFound++;
            }
            if (countFound >= shopItem.maxCountBought)
                return false;
        }
        return true;
    }
}
