using Codice.CM.Common;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[CustomEditor(typeof(HaulingJobManager))]
public class HaulingJobManagerEditor : Editor
{
    private HaulingJobManager haulingJobManager;
    private Camera renderCam;
    private RenderTexture renderTexture;
    void OnEnable()
    {
        haulingJobManager = (HaulingJobManager)target;
    }
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        base.OnInspectorGUI();

        if (GUILayout.Button("Generate Icons"))
        {
            GenerateIcons();
        }

    }

    private void GenerateIcons()
    {
        renderCam = Util.CreateCamera(haulingJobManager.renderLayer, CameraClearFlags.Color, true);
        renderCam.nearClipPlane = 0.1f;
        renderCam.farClipPlane = 10f;

        //Disable Post Processing
        HDAdditionalCameraData data = renderCam.gameObject.AddComponent<HDAdditionalCameraData>();
        data.customRenderingSettings = true;
        data.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Postprocess] = true;
        data.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Postprocess, false);
        data.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;

        //Render texture
        renderTexture = new RenderTexture(256, 256, 32, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.sRGB);
        renderTexture.Create();

        renderCam.targetTexture = renderTexture;
        renderCam.forceIntoRenderTexture = true;

        foreach (CargoSO cargoInfo in haulingJobManager.haulingCargoInfoList)
        {
            GameObject cargoCopy = null;
            Debug.Log("1");
            if (cargoInfo.cargoPrefab)
            {
                cargoCopy = Instantiate(cargoInfo.cargoPrefab);
                Util.ChangeObjectsLayer(cargoCopy, haulingJobManager.renderLayer);
            }

            cargoInfo.iconList = new List<Texture2D>();

            foreach (RailCarSO railCarInfo in cargoInfo.fittingRailCars)
            {
                Texture2D texture = RenderIcon(railCarInfo, cargoCopy);
                AssetDatabase.CreateAsset(texture, "Assets/GameAssets/SO/Icons/Cargo/" + railCarInfo.GetName() + "_" + cargoInfo.GetName() + "Icon.asset");
                cargoInfo.iconList.Add(texture);
            }
            DestroyImmediate(cargoCopy);
        }
        foreach (RailCarSO railCarInfo in haulingJobManager.railCarInfoList)
        {
            Texture2D texture = RenderIcon(railCarInfo);
            AssetDatabase.CreateAsset(texture, "Assets/GameAssets/SO/Icons/RailCar/" + railCarInfo.GetName() + "Icon.asset");
            railCarInfo.icon = texture;
        }
        DestroyImmediate(renderCam.gameObject);
    }

    private Texture2D RenderIcon(RailCarSO railCarInfo, GameObject cargoCopy = null)
    {
        Debug.Log("2");
        GameObject railCarCopy = Instantiate(railCarInfo.prefab, Vector3.zero, Quaternion.identity);
        Util.ChangeObjectsLayer(railCarCopy, haulingJobManager.renderLayer);
        railCarCopy.transform.position = Vector3.zero;

        if (cargoCopy)
        {
            RailCar railCarScript = railCarCopy.GetComponent<RailCar>();
            railCarScript.ParentCargo(cargoCopy);
        }

        Bounds bounds = new Bounds();
        foreach (Renderer renderer in railCarCopy.GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds.min);
            bounds.Encapsulate(renderer.bounds.max);
        }

        //Position Camera
        renderCam.transform.position = bounds.center + new Vector3(bounds.extents.x + 0.2f, 0, 0);
        renderCam.transform.LookAt(bounds.center);
        renderCam.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.z);

        Texture2D texture = new Texture2D(256, 256);
        RenderTexture.active = renderTexture;
        renderCam.Render();
        texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture.Apply();

        if (cargoCopy)
            cargoCopy.transform.SetParent(null);
        DestroyImmediate(railCarCopy);
        return texture;
    }
}
