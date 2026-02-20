using System.Collections.Generic;
using UnityEngine;

public class RailCarHealth : Health
{
    [Header("Railcar Health")]
    [SerializeField]
    private RailCar linkedRailCar;
    [SerializeField]
    private List<EventInteractable> rerailInteractableList;
    [SerializeField]
    private List<GameObject> brokenDownParticles;

    private bool brokenDown;
    private void Start()
    {
        foreach (EventInteractable interactable in rerailInteractableList)
        {
            interactable.interactEvent.AddListener(() =>
            {
                Rerail();
            });

            interactable.gameObject.SetActive(false);

            interactable.objectName = linkedRailCar.railCarSO.GetName();
            interactable.actionName = "Rerail";

            interactable.timeToInteract = 10f;
        }
        foreach (GameObject obj in brokenDownParticles)
        {
            obj.SetActive(false);
        }
    }

    public override void HealthReachedZero()
    {
        if (brokenDown) return;
        
        brokenDown = true;
        Derail();

        base.HealthReachedZero();
    }

    private void Rerail()
    {
        foreach (EventInteractable interactable in rerailInteractableList)
        {
            interactable.gameObject.SetActive(false);
        }
        linkedRailCar.derailed = false;
    }
    private void Derail()
    {
        foreach (EventInteractable interactable in rerailInteractableList)
        {
            interactable.gameObject.SetActive(true);
        }

        linkedRailCar.derailed = true;

        foreach (GameObject obj in brokenDownParticles)
        {
            obj.SetActive(true);
        }
    }

    public void ResetHealth()
    {
        Rerail();
        brokenDown = false;
        health = maxHealth;
    }
    public bool IsBroken()
    {
        return brokenDown;
    }
}
