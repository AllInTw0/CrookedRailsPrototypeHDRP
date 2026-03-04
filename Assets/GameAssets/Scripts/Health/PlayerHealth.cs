using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : Health
{
    public static PlayerHealth active;
    [Header("Player")] 
    [SerializeField] 
    private GameObject ragdollPrefab;
    [SerializeField] 
    private PlayerAvatar playerAvatar;
    [SerializeField] 
    private PlayerMovement playerMovement;
    [SerializeField] 
    private PlayerCamera playerCamera;
    [SerializeField] 
    private PlayerInventory playerInventory;
    //RunTime
    private GameObject ragdoll;
    [NonSerialized]
    public bool isAlive = true;
    private void Start()
    {
        active = this;
        isAlive = true;
        AddHealthToGlobalList();
    }

    private void Update()
    {
        StatUI.active.UpdateHealth(health,maxHealth);
        UpdateCall();
    }

    public override void HealthReachedZero(Vector3 force = default)
    {
        SpawnRagdoll(playerAvatar.animator.gameObject, ragdollPrefab, force);
        playerInventory.UnEquipAll();
        playerMovement.EnableNoclip();
        playerAvatar.Hide();
        
        InventoryUI.active.Hide();
        SpectateUI.active.Enable();
        StatUI.active.Hide();
        
        isAlive = false;
    }
}
