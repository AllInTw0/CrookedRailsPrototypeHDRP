using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static Collider[] PhysicsBoxColliderOverlap(BoxCollider boxCollider, LayerMask layerMask = default)
    {
        return Physics.OverlapBox(boxCollider.transform.TransformPoint(boxCollider.center), boxCollider.size * 0.5f, boxCollider.transform.rotation, layerMask);
    }


    //OLD CODE FROM CASUAL INDUSTRIALIZATION GAME
    public static Camera CreateCamera(int cullingMask = -1, CameraClearFlags clearFlags = CameraClearFlags.Skybox, bool orthographic = false)
    {
        GameObject cam_object = new GameObject();

        Camera cam = cam_object.AddComponent<Camera>();

        if (cullingMask != -1)
            cam.cullingMask = cullingMask;

        cam.clearFlags = clearFlags;

        if (clearFlags == CameraClearFlags.Color)
            cam.backgroundColor = new Color(0, 0, 0, 0);

        cam.orthographic = orthographic;

        return cam;
    }
    public static void ChangeObjectsLayer(GameObject _object, LayerMask layer)
    {
        var object_list = _object.GetComponentsInChildren<Transform>();
        foreach (Transform _t in object_list)
        {
            _t.gameObject.layer = Mathf.FloorToInt(Mathf.Log(layer, 2));
        }
    }
    public static void ToggleObjectsMeshRenderers(GameObject _object, bool enable = false)
    {
        var renderer_list = _object.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer _mesh in renderer_list)
        {
            _mesh.enabled = enable;
        }
    }

    //Code from unity discussions
    //(https://discussions.unity.com/t/test-to-see-if-a-vector3-point-is-within-a-boxcollider/17385)
    public static bool PointInBox(Vector3 point, BoxCollider box)
    {
        point = box.transform.InverseTransformPoint(point) - box.center;

        float halfX = (box.size.x * 0.5f);
        float halfY = (box.size.y * 0.5f);
        float halfZ = (box.size.z * 0.5f);

        if (point.x <= halfX && point.x >= -halfX &&
           point.y <= halfY && point.y >= -halfY &&
           point.z <= halfZ && point.z >= -halfZ)
            return true;
        else
            return false;
    }

    [System.Serializable]
    public class ProbabilityListElement<T>
    {
        public T element;
        public float probability;
        public int maxCount;

        public ProbabilityListElement(T element, float probability, int maxCount)
        {
            this.element = element;
            this.probability = probability;
            this.maxCount = maxCount;
        }

        [HideInInspector]
        public int pickCount;
    }

    public class ProbabilityList<T>
    {
        private List<ProbabilityListElement<T>> probabilityList = new List<ProbabilityListElement<T>>();
        private float probabilitySum = 0f;
        private int lastPicked = -1;
        public ProbabilityList(List<ProbabilityListElement<T>> probabilityList)
        {
            //Copy
            probabilityList = new List<ProbabilityListElement<T>>(probabilityList);

            //Get probability sum and reset count just in case
            for (int i = 0; i < probabilityList.Count; i++)
            {
                if(probabilityList[i].maxCount == 0)
                {
                    probabilityList.RemoveAt(i);
                    i--;
                    continue;
                }
                probabilityList[i].pickCount = 0;
                probabilitySum += probabilityList[i].probability;
            }

            //Sort
            bool sorted = false;
            while (sorted == false)
            {
                sorted = true;
                for (int i = 0; i < probabilityList.Count - 1; i++)
                {
                    if (probabilityList[i].probability < probabilityList[i + 1].probability)
                    {
                        var temp = probabilityList[i];
                        probabilityList[i] = probabilityList[i + 1];
                        probabilityList[i + 1] = temp;
                        sorted = false;
                    }
                }
            }

            this.probabilityList = probabilityList;
        }
        public T PickNext(bool increasePickCount = true)
        {
            if (probabilityList.Count == 0) return default(T);

            float randomProbability = Random.Range(0f, probabilitySum);
            //Debug.Log("randomProbability: " + randomProbability+", max: " + probabilitySum);
            for (int i = 0; i < probabilityList.Count; i++)
            {

                randomProbability -= probabilityList[i].probability;
                if (randomProbability <= 0f)
                {
                    lastPicked = i;
                    T pickedElement = probabilityList[i].element;
                    if (increasePickCount) IncreasePickCount();
                    return pickedElement;
                }
            }

            Debug.LogWarning("Couldnt pick from probabilityList. probabilitySum incorrect?");
            return default(T);
        }
        public void IncreasePickCount()
        {
            probabilityList[lastPicked].pickCount++;
            if (probabilityList[lastPicked].pickCount >= probabilityList[lastPicked].maxCount && probabilityList[lastPicked].maxCount > 0)
            {
                T element = probabilityList[lastPicked].element;
                probabilitySum -= probabilityList[lastPicked].probability;
                probabilityList.RemoveAt(lastPicked);
                lastPicked = -1;
            }
        }
        public bool HasItemsLeft()
        {
            return probabilityList.Count > 0;
        }
        public void RemoveLastPicked()
        {
            if (lastPicked != -1)
            {
                probabilitySum -= probabilityList[lastPicked].probability;
                probabilityList.RemoveAt(lastPicked);
                lastPicked = -1;
            }
        }
    }
}
