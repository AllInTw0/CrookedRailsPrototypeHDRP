using UnityEngine;

public class MonitorArm : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private string defaultAnimName = "startPose1";
    [Header("Light")]
    [SerializeField]
    private Light monitorLight;
    [SerializeField]
    private float turnOnOffTime;
    [Header("Debug")]
    [SerializeField]
    public string animName = "move1";
    [SerializeField]
    public float speed = 1f;

    private float time;
    private bool lightOn;
    private void Start()
    {
        PlayAniamtion(defaultAnimName, 1f);
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
    public void PlayAniamtion(string name, float speed)
    {
        if(speed >= 0)
            animator.Play(animName, 0, 0f);
        else
            animator.Play(animName, 0, 1f);

        animator.SetFloat("speed", speed);
    }
}
