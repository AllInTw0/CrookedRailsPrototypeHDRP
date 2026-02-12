using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
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

                Override newOverride = new Override("HaulReceipt", OverrideType.HaulReceipt);
                newOverride.cargoListOverride = Train.playerTrain.GetConsistCargoInfo();

                mainMonitorArm.printer.AddNotification(PaperRenderer.active.RenderPaper("HaulReceipt", new List<Override>() { newOverride }), float.MaxValue);
                mainMonitorArm.onInteract.AddListener(() => {
                    currentState = State.ChooseHaullingJob;
                    ResetMonitors();
                    Debug.Log("Reciept");
                    float sum = 0f;
                    List<CargoInfo> cargoList = Train.playerTrain.GetConsistCargoInfo();
                    for (int i = 0; i < cargoList.Count; i++)
                    {
                        if(cargoList[i].GetValueSum() != 0)
                            sum += cargoList[i].GetPaySum();
                    }

                    Override newOverride = new Override("Sum", OverrideType.Text);
                    newOverride.stringOverride = sum + "$";

                    MiniPrinter.active.AddNotification(PaperRenderer.active.RenderPaper("Receipt", new List<Override>() { newOverride }));

                    Train.playerTrain.RemoveNonPlayerRailCars();

                    SetupMonitors();
                });
                break;
            case State.ChooseHaullingJob:
                for (int i = 0; i < monitorArmList.Count; i++)
                {
                    monitorArmList[i].EnableMonitor();

                    Override newOverride1 = new Override("HaulingJob", OverrideType.HaulingJobEntry);
                    newOverride1.haulingJobOverride = HaulingJobManager.generatedHaulingJobList[i];

                    monitorArmList[i].printer.AddNotification(PaperRenderer.active.RenderPaper("HaulingJob", new List<Override>() { newOverride1 }), float.MaxValue);

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

                        LoadHaulingJob(HaulingJobManager.generatedHaulingJobList[index]);
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
    public void LoadHaulingJob(HaulingJob haulingJob)
    {
        foreach (HaulingJobEntry haulingJobEntry in haulingJob.haulingJobEntryList)
        {
            RailCar railCar = Train.playerTrain.AddRailCar(haulingJobEntry.railCar, 2); // 2 because 0-locomotive, 1-tender

            railCar.SetCargo(haulingJobEntry.cargo, haulingJobEntry.pay * 0.6f, haulingJobEntry.pay * 0.4f);
        }
    }
}
