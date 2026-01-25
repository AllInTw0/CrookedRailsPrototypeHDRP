using UnityEngine;

public class LeadTruck : RunningGear
{
    [Header("LeadTruck")]
    public Transform leadTruckTransform;

    public override TrackSection UpdateRunningGearPosition(float progress)
    {
        TrackManager.active.GetTrackPositionFromProgress(progress + wheelOffset, out TrackSection section, out Vector3 pos);

        leadTruckTransform.LookAt(pos);

        return section;
    }
}
