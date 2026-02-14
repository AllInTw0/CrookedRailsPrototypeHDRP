using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ShopStand : AnimationPlayer
{
    [Header("Glow")]
    [SerializeField]
    private Flickerer glowFlickerer;
    [SerializeField]
    private Transform glowCenter;
    [Header("Button")]
    [SerializeField]
    private AnimationPlayer buttonAnimationPlayer;
    [SerializeField]
    private EventInteractable buttonInteractable;
    [Header("Text")]
    [SerializeField]
    private TMP_Text nameText;
    [SerializeField]
    private TMP_Text priceText;

    public UnityEvent onInteract;

    //Run time
    private bool shopStandEnabled;
    private bool outOfStock;
    private ShopItem shopItem;
    void Start()
    {
        nameText.text = "";
        priceText.text = "";

        if (buttonInteractable != null) buttonInteractable.interactEvent.AddListener(() => { onInteract.Invoke(); });
    }

    void Update()
    {
        if(shopItem != null)
        {
            if(shopItem.stock <= 0 && outOfStock == false)
            {
                DisableButton();
                glowFlickerer.TurnOff();
                outOfStock = true;
            }
        }
    }

    public void EnableStand()
    {
        if (shopStandEnabled == true) return;

        glowFlickerer.TurnOn();
        PlayAniamtion(animName, 1f);
        EnableButton();
        shopStandEnabled = true;
    }
    public void DisableStand()
    {
        if (shopStandEnabled == false) return;

        glowFlickerer.TurnOff();
        PlayAniamtion(animName, -1f);
        DisableButton();
        shopStandEnabled = false;
    }
    public void EnableButton()
    {
        if (buttonAnimationPlayer != null) buttonAnimationPlayer.PlayAniamtion(buttonAnimationPlayer.animName, 1f);
        if (buttonInteractable != null) buttonInteractable.gameObject.SetActive(true);
    }
    public void DisableButton()
    {
        if (buttonAnimationPlayer != null) buttonAnimationPlayer.PlayAniamtion(buttonAnimationPlayer.animName, -1f);
        if (buttonInteractable != null) buttonInteractable.gameObject.SetActive(false);
    }
    public void Intialize(ShopItem shopItem)
    {
        this.shopItem = shopItem;

        Transform copy = Instantiate(shopItem.itemInfo.prefab,glowCenter).transform;

        if (copy.TryGetComponent(out Item item)) item.enabled = false;
        copy.gameObject.layer = 0;

        copy.localPosition = Vector3.zero;
        copy.localRotation = Quaternion.identity;

        nameText.text = shopItem.itemInfo.itemName;
        priceText.text = shopItem.price + "$";

        EnableStand();

        onInteract.AddListener(() => { shopItem.linkedShop.SelectShopItem(shopItem); });
    }

}
