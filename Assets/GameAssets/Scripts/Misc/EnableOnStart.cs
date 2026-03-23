using UnityEngine;

public class EnableOnStart : MonoBehaviour
{
    public GameObject targetObject;

    private void Awake()
    {
        targetObject.SetActive(true);
    }
}
