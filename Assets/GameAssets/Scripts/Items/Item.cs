using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Item : Interactable
{
    //Variables
    [Header("Item")]
    public ItemSO itemInfo;

    [SerializeField] 
    private List<MeshRenderer> meshRenderers;
    [SerializeField] 
    private Collider interactableCollider;

    public int count = 1;
    
    //Run time
    private bool falling;
    private float targetY;
    private float distance;
    private float velocity;
    private float rotVelocity;
    void Start()
    {
        DropFromPos(transform.position);
    }

    public override bool Interact()
    {
        bool success = PlayerInventory.active.TryEquipping(this);
        falling = false;
        MovingPlatformManager.active.RemoveEntry(transform);
        Debug.Log("Item Equipped Successfully: " + success);
        
        base.Interact();
        return success;
    }

    public override string GetName()
    {
        if (itemInfo.maxCount > 1)
        {
            return objectName + " [" + count + "]";
        }
        //else
        return objectName;
    }
    private void Update()
    {
        UpdateItem();
    }

    public void UpdateItem()
    {
        if (!falling)
            return;

        velocity += 10f * Time.deltaTime;
        transform.position -= new Vector3(0, velocity * Time.deltaTime, 0f);
        transform.Rotate(0f, rotVelocity * Time.deltaTime, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(0,transform.eulerAngles.y,0), (distance - (transform.position.y - targetY))/distance);
        if (transform.position.y < targetY)
        {
            falling = false;
            transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
            transform.rotation = Quaternion.Euler(0,transform.eulerAngles.y,0);
            SoundManager.active.PlayAtPos(transform.position,"Item - Drop");
        }
    }
    public void DropFromPos(Vector3 startPos)
    {
        transform.position = startPos;
        falling = true;
        velocity = 0;
        rotVelocity = Random.Range(-35f, 35f);
        if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit))
        {
            targetY = hit.point.y;
            distance = transform.position.y - targetY;

            if (hit.transform.CompareTag("Moving"))
            {
                MovingPlatformManager.active.AddEntry(transform,transform,hit.transform);
            }
        }
        else
        {
            falling = false;
            Debug.Log("No Ground Found");
        }
    }
    public void BecomeInvisible()
    {
        foreach (var mesh in meshRenderers)
        {
            mesh.enabled = false;
        }
        interactableCollider.enabled = false;
    }
    public void BecomeVisible()
    {
        foreach (var mesh in meshRenderers)
        {
            mesh.enabled = true;
        }
        interactableCollider.enabled = true;
    }

    public static void SpawnItem(ItemSO itemSO, Vector3 pos, Quaternion rot)
    {
        Transform copy = Instantiate(itemSO.prefab,pos,rot).transform;
        copy.GetComponent<Item>().DropFromPos(pos);
    }
}
