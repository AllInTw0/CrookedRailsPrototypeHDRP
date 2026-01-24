using UnityEngine;
using UnityEngine.Events;
public class EventInteractable : Interactable
{
    [Header("Event")]
    [SerializeField]
    private UnityEvent interactEvent;
    public override bool Interact()
    {
        interactEvent.Invoke();
        base.Interact();
        return true;
    }
}
