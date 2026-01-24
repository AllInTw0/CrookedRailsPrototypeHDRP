using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string objectName;
    public string actionName;
    public float timeToInteract;

    public Transform iconPosition;

    [SerializeField] 
    private string interactSound = "Click";
    
    public virtual bool Interact()
    {
        SoundManager.active.PlayAtPos(iconPosition.position,interactSound);
        return true;
    }
    public virtual string GetName()
    {
        return objectName;
    }
    public virtual string GetAction()
    {
        return actionName;
    }
}
