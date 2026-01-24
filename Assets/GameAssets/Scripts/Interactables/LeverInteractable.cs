using System;
using UnityEngine;

public class LeverInteractable : Interactable
{
    [Header("Lever")] 
    [SerializeField] 
    private Vector3 affectedAxis;
    [SerializeField] 
    private float minRot;
    [SerializeField] 
    private float maxRot;
    [SerializeField] 
    public int notches;
    [SerializeField] 
    private float rotSpeed;
    [SerializeField] 
    private bool displayNotches = true;
    //RunTime
    [NonSerialized] 
    public int currentNotch;

    private void Update()
    {
        float targetRot = minRot + (maxRot - minRot) * (currentNotch / (float)notches);
        transform.localRotation = Quaternion.RotateTowards(transform.localRotation,Quaternion.Euler(affectedAxis * targetRot),rotSpeed * Time.deltaTime);
    }

    public override bool Interact()
    {
        currentNotch++;
        if (currentNotch > notches)
            currentNotch = 0;
        
        InteractIcon.active.Refresh();
        return base.Interact();
    }
    
    public override string GetName()
    {
        if (displayNotches)
            return objectName + " [" + currentNotch + "/" + notches + "]";
        
        //else
        return base.GetName();
    }
}
