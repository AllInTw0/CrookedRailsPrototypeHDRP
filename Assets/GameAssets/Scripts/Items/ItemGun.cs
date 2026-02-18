using UnityEngine;

public class ItemGun : Item
{
    //Variables
    [Header("Gun")] 
    [SerializeField] 
    public int clipSize = 2;
    [SerializeField] 
    public ItemSO ammoItem;
    [SerializeField] 
    public int bulletCount = 1;
    [SerializeField] 
    public float bulletDamage = 1;
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
           if ((InputManager.attackAction.IsPressed() && clip > 0) ||(InputManager.attackAction.triggered && clip == 0))
           {
               Shoot();
           }
           else if (InputManager.reloadAction.triggered)
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
        if (PlayerInventory.active.RemoveItem(ammoItem, out int itemCountRemoved, clipSize - clip))
        {
            SoundManager.active.PlayAtPos(transform.position, reloadSound);
            PlayerAvatar.active.animator.SetTrigger("Reload");
            coolDown = reloadCoolDown;
            clip += itemCountRemoved;
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
