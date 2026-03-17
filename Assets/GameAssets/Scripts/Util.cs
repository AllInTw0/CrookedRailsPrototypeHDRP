using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public static class Util
{
    public static Collider[] PhysicsBoxColliderOverlap(BoxCollider boxCollider, LayerMask layerMask = default)
    {
        return Physics.OverlapBox(boxCollider.transform.TransformPoint(boxCollider.center), boxCollider.size * 0.5f, boxCollider.transform.rotation, layerMask);
    }

    [System.Serializable]
    public class ProbabilityListElement<T>
    {
        public T element;
        public float probability;
        public int maxCount;

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
