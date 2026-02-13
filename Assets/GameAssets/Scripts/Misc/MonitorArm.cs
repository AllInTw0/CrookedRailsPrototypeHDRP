using UnityEngine;
using UnityEngine.Events;

public class MonitorArm : AnimationPlayer
{
    [Header("Light")]
    [SerializeField]
    private Light monitorLight;
    [SerializeField]
    private float turnOnOffTime;
    [Header("Button")]
    [SerializeField]
    private AnimationPlayer buttonAnimationPlayer;
    [SerializeField]
    private EventInteractable buttonInteractable;
    [Header("Printer")]
    [SerializeField]
    public Printer printer;

    public UnityEvent onInteract;

    private float time;
    private bool lightOn;

    private bool monitorEnabled;

    private void Start()
    {
        if (buttonInteractable != null) buttonInteractable.interactEvent.AddListener(() => { onInteract.Invoke(); });
        DisableButton();
    }
    private void Update()
    {
        if(time <= turnOnOffTime)
        {
            time += Time.deltaTime;
            float normlized = (1f-(time / turnOnOffTime)) * 0.5f;
            float pow = normlized * normlized;
            int num = Mathf.RoundToInt(pow * 10f);

            monitorLight.enabled = (num % 2 == 0);
        }
        else
        {
            monitorLight.enabled = lightOn;
        }
    }
    public void TurnOnLight()
    {
        time = 0f;
        lightOn = true;
    }
    public void TurnOffLight()
    {
        time = turnOnOffTime;
        lightOn = false;
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
