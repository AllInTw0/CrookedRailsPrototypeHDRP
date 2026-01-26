using System.Collections.Generic;
using UnityEngine;

public class RunningGear : MonoBehaviour
{
    public Transform railCarTransform;
    public float wheelOffset;
    [Header("Wheels")]
    public Animator wheelAnimator;
    public List<Transform> wheelTransformList;
    public float wheelRadius;
    [HideInInspector]
    public float wheelCircumference;

    private void Start()
    {
        wheelCircumference = 2 * Mathf.PI * wheelRadius;

        foreach (Transform wheelTransform in wheelTransformList)
        {
            wheelTransform.localRotation = Quaternion.Euler(Random.Range(0f,360f), 0, 0);
        }
    }
    public virtual void UpdateRunningGearPosition(float sectionProgress, TrackSection section)
    {
        TrackManager.active.GetTrackPositionFromProgress(sectionProgress + wheelOffset, section, out Vector3 posFront);
        TrackManager.active.GetTrackPositionFromProgress(sectionProgress - wheelOffset, section, out Vector3 posBack);

        railCarTransform.position = (posFront + posBack) * 0.5f;
        railCarTransform.LookAt(posFront);
    }

    public virtual void UpdateRunningGearRotation(float distanceTravelled)
    {
        if(wheelAnimator != null)
        {
            wheelAnimator.speed = (distanceTravelled / Time.deltaTime) / wheelCircumference;
        }
        foreach (Transform wheelTransform in wheelTransformList)
        {
            wheelTransform.localRotation *= Quaternion.Euler((distanceTravelled / wheelCircumference) * 360f, 0, 0);
        }
    }
}
