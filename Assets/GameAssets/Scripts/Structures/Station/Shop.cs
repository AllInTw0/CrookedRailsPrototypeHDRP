using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ShopItem
{
    public ItemSO itemInfo;
    public int price;
    public int stock;

    public Shop linkedShop;


    public ShopItem(ItemSO itemInfo, int price, int stock, Shop linkedShop)
    {
        this.itemInfo = itemInfo;
        this.price = price;
        this.linkedShop = linkedShop;
        this.stock = stock;
    }
}
[System.Serializable]
public class ShopItemEntry
{
    public ItemSO itemInfo;
    [Header("Params")]
    public AnimationCurve probabilityCurve;
    public AnimationCurve priceCurve;
    public AnimationCurve stockCurve;
    public AnimationCurve randomnessCurve;

    [HideInInspector] public float probability;
    [HideInInspector] public int price;
    [HideInInspector] public int stock;
    [HideInInspector] public float randomness;
    public void EvaluateCurves(float value)
    {
        randomness = randomnessCurve.Evaluate(value);

        probability = probabilityCurve.Evaluate(value) * (Random.value * randomness + 1f);
        price = Mathf.RoundToInt(Mathf.Clamp(priceCurve.Evaluate(value) + (Random.value * randomness * 20f), 1f, float.MaxValue));
        stock = Mathf.RoundToInt(Mathf.Clamp(stockCurve.Evaluate(value) * (Random.value * randomness * 1f + 1f), 1f, float.MaxValue));
    }
}
public class Shop : MonoBehaviour
{
    [Header("Refrences")]
    [SerializeField]
    private List<ShopStand> shopStandList;
    [SerializeField]
    private MonitorArm shopMonitor;
    [SerializeField]
    private Transform itemSpawnPos;
    [SerializeField]
    private Flickerer shopLight;
    [Header("Items")]
    [SerializeField]
    private List<ShopItemEntry> shopItemEntryList;

    private List<ShopItem> shopItemList;

    private ShopItem selectedShopItem;

    private void Start()
    {
        shopMonitor.onInteract.AddListener(() => {
            //Buy item
            if (selectedShopItem != null)
            {
                if (selectedShopItem.stock > 0 && Money.CanAfford(selectedShopItem.price))
                {
                    Vector3 pos = itemSpawnPos.position + new Vector3(Random.Range(-0.4f, 0.4f), 0, Random.Range(-0.4f, 0.4f));
                    Quaternion rot = Quaternion.Euler(Random.Range(-90f, 90), Random.Range(-90f, 90), Random.Range(-90f, 90));
                    Item.SpawnItem(selectedShopItem.itemInfo, pos, rot);

                    selectedShopItem.stock--;
                    Money.AddMoney(-selectedShopItem.price);
                    RenderMonitorPaper();

                    if(selectedShopItem.stock <= 0)
                        shopMonitor.DisableButton();
                }
                else
                {
                    Debug.LogWarning("Cant buy item");
                }
            }
            else
            {
                Debug.LogWarning("No selected item");
            }
        });
    }
    public void Initialize()
    {
        float probabilitySum = 0f;
        for (int i = 0; i < shopItemEntryList.Count; i++)
        {
            shopItemEntryList[i].EvaluateCurves(GameStateManager.currentLevel);
            if(shopItemEntryList[i].probability > 0)
            probabilitySum += shopItemEntryList[i].probability;
        }

        //Sort list based on probability
        while (true)
        {
            bool swaped = false;

            for (int i = 0; i < shopItemEntryList.Count-1; i++)
            {
                if (shopItemEntryList[i].probability < shopItemEntryList[i+1].probability)
                {
                    var temp = shopItemEntryList[i];
                    shopItemEntryList[i] = shopItemEntryList[i + 1];
                    shopItemEntryList[i + 1] = temp;
                    swaped = true;
                }
            }

            if (swaped == false) break;
        }

        string str = "";
        for (int i = 0; i < shopItemEntryList.Count; i++)
        {
            str += shopItemEntryList[i].itemInfo + ":" + shopItemEntryList[i].probability + ", ";
        }
        Debug.Log("Shop probbability [" + GameStateManager.currentLevel +"]: " + str);

        //Generate Shop Item List
        shopItemList = new List<ShopItem>();
        List<ShopItemEntry> shopItemEntryListCopy = new List<ShopItemEntry>(shopItemEntryList);
        for (int i = 0; i < shopStandList.Count; i++)
        {
            float probability = Random.Range(0f, probabilitySum);
            ShopItemEntry randomEntry = null;
            for (int j = 0; j < shopItemEntryListCopy.Count; j++)
            {
                if (shopItemEntryListCopy[j].probability <= 0) continue;

                if(probability - shopItemEntryListCopy[j].probability <= 0)
                {
                    randomEntry = shopItemEntryListCopy[j];
                    probabilitySum -= randomEntry.probability;
                    shopItemEntryListCopy.RemoveAt(j);
                    j--;
                    break;
                }

                probability -= shopItemEntryListCopy[j].probability;
            }

            if (randomEntry != null)
                shopItemList.Add(new ShopItem(randomEntry.itemInfo, randomEntry.price, randomEntry.stock, this));
            else
                Debug.LogWarning("No available items above 0 probability");
        }

        for (int i = 0; i < shopStandList.Count; i++)
        {
            if(i < shopItemList.Count)
                shopStandList[i].Intialize(shopItemList[i]);
        }
    }

    public void SelectShopItem(ShopItem shopItem)
    {
        selectedShopItem = shopItem;

        shopMonitor.EnableButton();
        RenderMonitorPaper();     
    }

    private void RenderMonitorPaper()
    {
        shopMonitor.EnableMonitor();
        shopMonitor.printer.ClearNotifications();

        Override override1 = new Override("Title", OverrideType.Text);
        override1.stringOverride = selectedShopItem.itemInfo.itemName;
        Override override2 = new Override("ItemIcon", OverrideType.RawImageTexture);
        override2.textureOverride = selectedShopItem.itemInfo.icon;
        Override override3 = new Override("ItemDescription", OverrideType.Text);
        override3.stringOverride = selectedShopItem.itemInfo.itemName;
        Override override4 = new Override("ItemShopStats", OverrideType.Text);
        override4.stringOverride = "Stock:" + selectedShopItem.stock + " Price:" + selectedShopItem.price + "$";

        shopMonitor.printer.AddNotification(PaperRenderer.active.RenderPaper("ItemInfo", new List<Override>() { override1 , override2, override3, override4}), float.MaxValue);
    }
    public void PlayerEntered()
    {
        if (shopLight != null) shopLight.TurnOn();
        if(selectedShopItem != null) RenderMonitorPaper();
    }

    public void PlayerExited()
    {
        if (shopLight != null) shopLight.TurnOff();

        shopMonitor.DisableMonitor();
        shopMonitor.printer.ClearNotifications();
    }
}
