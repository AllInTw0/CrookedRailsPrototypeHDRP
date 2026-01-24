using UnityEngine;

public class ItemGun : Item
{
    //Variables
    [Header("Gun")] 
    [SerializeField] 
    private int clipSize = 2;
    [SerializeField] 
    private ItemSO ammoItem;
    [SerializeField] 
    private int bulletCount = 1;
    [SerializeField] 
    private float bulletDamage = 1;
    [SerializeField] 
    private float spread = 0.05f;
    [SerializeField] 
    private float reloadCoolDown = 0.5f;
    [SerializeField] 
    private float shootCoolDown = 0.5f;
    [SerializeField] 
    private string reloadSound;
    [SerializeField] 
    private string shootSound;
    [SerializeField] 
    private string failedSound;
    //Run Time
    private int clip;
    private float coolDown;
    void Update()
    {
        if (coolDown > 0f)
            coolDown -= Time.deltaTime;
        
       if(PlayerInventory.active.tool == this && coolDown <= 0f)
       {
           if ((InputManager.active.attackAction.IsPressed() && clip > 0) ||(InputManager.active.attackAction.triggered && clip == 0))
           {
               Shoot();
           }
           else if (InputManager.active.reloadAction.triggered)
           {
               Reload();
           }

           UpdateText();
       }
       
       UpdateItem();
    }

    private void Shoot()
    {
        if (clip > 0)
        {
            Transform bulletSpawn = PlayerAvatar.active.GetAnimationInfo().bulletSpawn;
            Vector3 startPos = bulletSpawn.position;
            Vector3 dir = (PlayerCamera.active.GetRaycastPos() - startPos).normalized;

            BulletManager.active.ShootBullets(startPos, dir, bulletCount, bulletDamage, spread);

            SoundManager.active.PlayAtPos(transform.position, shootSound);
            PlayerAvatar.active.animator.SetTrigger("Shoot");

            coolDown = shootCoolDown;
            clip--;
        }
        else
        {
            SoundManager.active.PlayAtPos(transform.position, failedSound);
        }
    }

    private void Reload()
    {
        int ammoFound = 0;
        
        //Loop through items in inventory and get the amount of ammo needed
        for (int i = 0; i < PlayerInventory.active.items.Count; i++)
        {
            Item item = PlayerInventory.active.items[i];
            if (item.itemInfo == ammoItem)
            {
                int ammoToFind = (clipSize - clip) - ammoFound;
                for (int j = 0; j < ammoToFind; j++)
                {
                    item.count--;
                    ammoFound++;
                    if (item.count <= 0)
                    {
                        PlayerInventory.active.DestroyItem(item);
                        i--;
                        break;
                    }

                    if (ammoFound == (clipSize - clip))
                        break;
                }
            } 
            if (ammoFound == (clipSize - clip))
                break;
        }
        InventoryUI.active.UpdateItemIcons();

        if (ammoFound > 0)
        {
            SoundManager.active.PlayAtPos(transform.position, reloadSound);
            PlayerAvatar.active.animator.SetTrigger("Reload");
            coolDown = reloadCoolDown;
            clip += ammoFound;
        }
        else
        {
            SoundManager.active.PlayAtPos(transform.position, failedSound);
        }
    }

    private void UpdateText()
    {
        InventoryUI.active.SetToolText("Clip:"+clip+"/"+clipSize);
    }
}
