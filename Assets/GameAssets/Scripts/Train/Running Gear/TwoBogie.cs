using UnityEngine;


public class TwoBogie : RunningGear
{
    [System.Serializable]
    public class Bogie
    {
        public Transform bogieTransform;
        public float wheelOffset;
    }

    [Header("TwoBogie")]
    public Bogie frontBogie;
    public Bogie backBogie;
    public override TrackSection UpdateRunningGearPosition(float progress)
    {
        TrackSection railCarSectionFront;
        //TwoBogie
        Vector3 UpdateBogie(Bogie bogie, float bogieProgress)
        {
            TrackManager.active.GetTrackPositionFromProgress(bogieProgress + bogie.wheelOffset, out TrackSection sectionFront, out Vector3 posFront);
            TrackManager.active.GetTrackPositionFromProgress(bogieProgress - bogie.wheelOffset, out TrackSection sectionBack, out Vector3 posBack);

            bogie.bogieTransform.LookAt(bogie.bogieTransform.position + posFront - posBack);

            railCarSectionFront = sectionFront;

            return (posFront + posBack) * 0.5f;
        }

        Vector3 posBack = UpdateBogie(backBogie, progress - wheelOffset);
        Vector3 posFront = UpdateBogie(frontBogie, progress + wheelOffset);

        railCarTransform.position = (posFront + posBack) * 0.5f;
        railCarTransform.LookAt(posFront);

        return railCarSectionFront;
    }
}
