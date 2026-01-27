using UnityEngine;

public class HealthDisplay : MonoBehaviour
{
    //Variables
    [SerializeField]
    private Health healthTarget;

    enum DisplayType
    {
        Position,
        Scale
    }

    [SerializeField]
    private Transform displayTransform;
    [SerializeField]
    private DisplayType displayType;
    [SerializeField]
    private bool disableTransformOnZero;
    [Header("Position")]
    [SerializeField]
    private Transform startTransform;
    [SerializeField]
    private Transform endTransform;
    [Header("Scale")]
    [SerializeField]
    private Vector3 startSize;
    [SerializeField]
    private Vector3 endSize;

    private void Update()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if(healthTarget.health <= 0 && disableTransformOnZero)
        {
            displayTransform.gameObject.SetActive(false);
        }
        else if(displayTransform.gameObject.activeSelf == false)
        {
            displayTransform.gameObject.SetActive(true);
        }

        float time = healthTarget.health / healthTarget.maxHealth;

        if(displayType == DisplayType.Position)
            displayTransform.transform.position = Vector3.Lerp(startTransform.position, endTransform.position, time);
        else
            displayTransform.transform.localScale = Vector3.Lerp(startSize, endSize, time);

    }
}
