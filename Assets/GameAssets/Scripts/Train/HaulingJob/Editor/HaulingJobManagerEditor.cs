using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HaulingJobManager))]
public class HaulingJobManagerEditor : Editor
{
    private HaulingJobManager haulingJobManager;

    void OnEnable()
    {
        haulingJobManager = (HaulingJobManager)target;
    }
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate Hauling Job"))
        {
            var list = haulingJobManager.GenerateHaulingJob(haulingJobManager.maxCargoDangerLevel, haulingJobManager.targetDangerLevel, haulingJobManager.mixedLevel, haulingJobManager.maxCargoCount, haulingJobManager.currentLevel);

            //Render Paper Test
            Override newOverride = new Override("HaulingJob", OverrideType.HaulingJobEntry);
            newOverride.haulingJobEntryListOverride = list;

            Texture2D texture = haulingJobManager.paperRenderer.RenderPaper("HaulingJob", new List<Override>() { newOverride });
            AssetDatabase.CreateAsset(texture, "Assets/GameAssets/PaperTest.asset");

        }

    }
}
