using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string objectName;
    public string actionName;
    public float timeToInteract;

    public Transform iconPosition;

    [SerializeField] 
    private string interactSound = "Click";

    [HideInInspector]
    public string objectNameOverride;
    [HideInInspector]
    public string actionNameOverride;

    public virtual bool Interact()
    {
        SoundManager.active.PlayAtPos(iconPosition.position,interactSound);
        return true;
    }
    public virtual string GetName()
    {
        if (objectNameOverride != "")
            return objectNameOverride;
        else
            return objectName;
    }
    public virtual string GetAction()
    {
        if (actionNameOverride != "")
            return actionNameOverride;
        else
            return actionName;
    }

    public void SetObjectNameOverride(string name = "")
    {
        objectNameOverride = name;
        InteractIcon.active.Refresh();
    }
    public void SetActionNameOverride(string name = "")
    {
        actionNameOverride = name;
        InteractIcon.active.Refresh();
    }

    public void ClearOverrides()
    {
        objectNameOverride = "";
        actionNameOverride = "";
        InteractIcon.active.Refresh();
    }
}
