using UnityEngine;
using UnityEngine.Events;
public class Trigger : MonoBehaviour
{
    public UnityEvent onEnter;
    public UnityEvent onEnterOnce;
    public UnityEvent onExit;

    private bool triggered = false;
    private void OnTriggerEnter(Collider other)
    {
        if (triggered == false) onEnterOnce.Invoke();
        onEnter.Invoke();
        triggered = true;
    }
    private void OnTriggerExit(Collider other)
    {
        onExit.Invoke();
    }
}
