using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    //Variables
    [SerializeField] 
    private LayerMask raycastLayer;
    [SerializeField] 
    private LayerMask interactableLayer;
    [SerializeField] 
    private float range;
    [SerializeField] 
    private float raycastRadius;
    
    //Run time
    private Transform currentTarget;
    private Interactable currentInteractable;

    private bool holdingInteractLastFrame;
    private bool finishedInteracting;
    private float timeHoldingInteract;
    void Start()
    {
        
    }


    void Update()
    {
        if (PlayerHealth.active.isAlive == false)
        {
            if(currentTarget != null)
                StopInteracting();
            
            return;
        }
        
        Raycast();

        //Interact Handling
        if (holdingInteractLastFrame == false && InputManager.active.interactAction.IsPressed() && currentInteractable != null)
        {
            //Began To Hold Interact
            Debug.Log("Began Interact");
            InteractIcon.active.animator.SetBool("Interacting",true);
            if (currentInteractable.timeToInteract == 0)
            {
                FinishedInteracting();
            }
        }
        
        //For Interactables With Timers
        if (holdingInteractLastFrame && finishedInteracting == false && currentInteractable != null)
        {
            //Is Interacting
            Debug.Log("Is Interacting");
            timeHoldingInteract += Time.deltaTime;
            InteractIcon.active.SetProgress(Mathf.Clamp01(timeHoldingInteract / currentInteractable.timeToInteract));
            
            if (timeHoldingInteract > currentInteractable.timeToInteract)
            {
                //Finished Interacting
                FinishedInteracting();
            }
        }
        
        if (currentInteractable != null)
            holdingInteractLastFrame = InputManager.active.interactAction.IsPressed();
        else
            holdingInteractLastFrame = false;
        
        if (holdingInteractLastFrame == false)
        {
            finishedInteracting = false;
            timeHoldingInteract = 0;
            InteractIcon.active.SetProgress(0f);
            InteractIcon.active.animator.SetBool("Interacting",false);
        }
    }

    private void Raycast()
    {
        if (Physics.SphereCast(transform.position,raycastRadius, transform.forward, out RaycastHit hit, range, raycastLayer))
        {
            //( mask & (1 << layer)) != 0 returns true if mask has the layer
            if ((interactableLayer & (1 << hit.transform.gameObject.layer)) != 0)
            {
                if (currentTarget != hit.transform)
                {
                    //Different or a new object
                    Interactable interactable = hit.transform.GetComponent<Interactable>();
                    if(interactable != null)
                    {
                        currentTarget = hit.transform;
                        currentInteractable = interactable;
                        
                        InteractIcon.active.Enable(currentInteractable);
                    }
                    else
                    {
                        Debug.LogWarning("Interactable doesn't have a script: " + hit.transform);
                        StopInteracting();
                    }
                }
            }
            else
            {
                StopInteracting();
            }
        }
        else
        {
            StopInteracting();
        }
    }

    private void StopInteracting()
    {
        currentTarget = null;
        currentInteractable = null;
        InteractIcon.active.Disable();
        InteractIcon.active.animator.SetBool("Interacting",false);
        
        holdingInteractLastFrame = false;
    }

    private void FinishedInteracting()
    {
        Debug.Log("Finished Interacting");
        finishedInteracting = true;
        InteractIcon.active.SetProgress(0f);
        InteractIcon.active.animator.SetBool("Interacting",false);
        
        
        bool success = currentInteractable.Interact();
        if(success)
            InteractIcon.active.animator.SetTrigger("Finished");
        else
            InteractIcon.active.animator.SetTrigger("Failed");
    }
}
