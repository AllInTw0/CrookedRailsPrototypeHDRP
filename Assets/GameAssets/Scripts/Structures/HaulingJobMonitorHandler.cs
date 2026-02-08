using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class HaulingJobMonitorHandler : MonoBehaviour
{
    enum State
    {
        Welcome,
        Stats,
        ChooseHaullingJob,
        HaullingJobChosen
    }

    [SerializeField]
    private MonitorArm mainMonitorArm;
    [SerializeField]
    private List<MonitorArm> monitorArmList;

    private State currentState = State.Welcome;

    private List<List<HaulingJobManager.HaulingJobEntry>> haulingJobList;
    private List<Texture2D> haullingJobRenders;
    private void Start()
    {
        haulingJobList = new List<List<HaulingJobManager.HaulingJobEntry>>();  
        for (int i = 0; i < monitorArmList.Count; i++)
        {
            haulingJobList.Add(HaulingJobManager.active.GenerateHaulingJob(i * 0.5f, i * 1f, 0.4f, 8 - i * 2, 1));
        }
    }

    public void PlayerEntered()
    {
        //Player has steped inside of the trigger
        if (currentState != State.HaullingJobChosen)
            SetupMonitors();
    }
    public void PlayerExited()
    {
        //Player has steped outside of the trigger
        if (currentState != State.HaullingJobChosen)
        {
            ResetMonitors();
            foreach (MonitorArm monitor in monitorArmList)
            {
                monitor.DisableMonitor();
            }
        }
    }
    public void SetupMonitors()
    {
        switch (currentState)
        {
            case State.Welcome:
                mainMonitorArm.EnableMonitor();
                mainMonitorArm.printer.AddNotification(PaperRenderer.active.RenderPaper("StationWelcome", new List<Override>()), float.MaxValue);
                mainMonitorArm.onInteract.AddListener(() => {
                    currentState = State.Stats;
                    ResetMonitors();
                    Debug.Log("Still active!");
                    SetupMonitors();
                });
                break;
            case State.Stats:
                mainMonitorArm.EnableMonitor();
                mainMonitorArm.printer.AddNotification(PaperRenderer.active.RenderPaper("HaulReciept", new List<Override>()), float.MaxValue);
                mainMonitorArm.onInteract.AddListener(() => {
                    currentState = State.ChooseHaullingJob;
                    ResetMonitors();
                    Debug.Log("Reciept");
                    SetupMonitors();
                });
                break;
            case State.ChooseHaullingJob:
                if(haullingJobRenders == null)
                {
                    haullingJobRenders = new List<Texture2D>();
                    for (int i = 0; i < monitorArmList.Count; i++)
                    {
                        Override newOverride = new Override("HaulingJob", OverrideType.HaulingJobEntry);
                        newOverride.haulingJobEntryListOverride = haulingJobList[i];

                        haullingJobRenders.Add(PaperRenderer.active.RenderPaper("HaulingJob", new List<Override>() { newOverride }));
                    }
                }
                for (int i = 0; i < monitorArmList.Count; i++)
                {
                    monitorArmList[i].EnableMonitor();
                    monitorArmList[i].printer.AddNotification(haullingJobRenders[i], float.MaxValue);

                    int index = i;
                    monitorArmList[i].onInteract.AddListener(() =>
                    {
                        currentState = State.HaullingJobChosen;
                        Debug.Log("HaulingJobChosen");

                        for (int j = 0; j < monitorArmList.Count; j++)
                        {
                            if (index != j)
                            {
                                monitorArmList[j].DisableMonitor();
                            }
                        }
                        ResetMonitors();
                    });
                }
                break;
            default:
                break;
        }
    }
    public void ResetMonitors()
    {
        foreach (MonitorArm monitor in monitorArmList)
        {
            if (monitor.printer.activeNotification != null && currentState != State.HaullingJobChosen)
                monitor.printer.activeNotification.time = 0f;
            monitor.onInteract.RemoveAllListeners();

            if (currentState == State.HaullingJobChosen)
                monitor.DisableButton();
        }
    }
}
