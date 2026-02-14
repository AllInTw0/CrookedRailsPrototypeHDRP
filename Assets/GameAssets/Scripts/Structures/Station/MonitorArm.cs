using UnityEngine;
using UnityEngine.Events;

public class MonitorArm : AnimationPlayer
{
    [Header("Light")]
    [SerializeField]
    private Flickerer lightFlickerer;
    [Header("Button")]
    [SerializeField]
    private AnimationPlayer buttonAnimationPlayer;
    [SerializeField]
    private EventInteractable buttonInteractable;
    [Header("Printer")]
    [SerializeField]
    public Printer printer;

    public UnityEvent onInteract;

    private bool monitorEnabled;

    private void Start()
    {
        if (buttonInteractable != null) buttonInteractable.interactEvent.AddListener(() => { onInteract.Invoke(); });
        DisableButton();
    }
    public void TurnOnLight()
    {
        lightFlickerer.TurnOn();
    }
    public void TurnOffLight()
    {
        lightFlickerer.TurnOff();
    }
    public void EnableButton()
    {
        if (buttonAnimationPlayer != null) buttonAnimationPlayer.PlayAniamtion(buttonAnimationPlayer.animName, 1f);
        if (buttonInteractable != null) buttonInteractable.gameObject.SetActive(true);
    }
    public void DisableButton()
    {
        if (buttonAnimationPlayer != null) buttonAnimationPlayer.PlayAniamtion(buttonAnimationPlayer.animName, -1f);
        if (buttonInteractable != null) buttonInteractable.gameObject.SetActive(false);
    }

    public void EnableMonitor()
    {
        if (monitorEnabled == true) return;

        TurnOnLight();
        PlayAniamtion(animName, 1f);
        EnableButton();
        monitorEnabled = true;
    }
    public void DisableMonitor()
    {
        if (monitorEnabled == false) return;

        TurnOffLight();
        PlayAniamtion(animName, -1f);
        DisableButton();
        monitorEnabled = false;
    }
}
