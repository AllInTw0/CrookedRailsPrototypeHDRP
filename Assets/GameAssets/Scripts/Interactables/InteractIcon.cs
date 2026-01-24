using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class InteractIcon : MonoBehaviour
{
    public static InteractIcon active;
    //Variables
    [SerializeField]
    private Vector2 renderTextureResolution;
    [SerializeField]
    private RectTransform mainTransform;
    [SerializeField]
    private float scalingFactor = 1f;
    [SerializeField]
    private Image progressImage;
    [SerializeField]
    private float progressSpeed = 1f;
    public Animator animator;
    
    [Header("Text")] 
    [SerializeField] 
    private TMP_Text objectText;
    [SerializeField] 
    private TMP_Text actionText;
    //Run Time
    private Interactable activeTarget;
    private float currentProgress;
    private float targetProgress;
    private void Awake()
    {
        active = this;
    }
    
    private void LateUpdate()
    {
        if (activeTarget)
        {
            //Position
            PlayerCamera.active.WorldPosToUI(activeTarget.iconPosition.position, out Vector3 screenPos, out bool onScreen);
            if (onScreen == false)
            {
                if(mainTransform.gameObject.activeSelf == true)
                    mainTransform.gameObject.SetActive(false);
            }
            else
            {
                if(mainTransform.gameObject.activeSelf == false)
                    mainTransform.gameObject.SetActive(true);
                mainTransform.position = screenPos;
            }
            
            //Scale
            float scale = scalingFactor / Vector3.Distance(PlayerCamera.active.transform.position,activeTarget.iconPosition.position);
            mainTransform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            if(mainTransform.gameObject.activeSelf == true)
                mainTransform.gameObject.SetActive(false);
        }
        //Interaction Progress
        currentProgress = math.lerp(currentProgress, targetProgress, progressSpeed * Time.deltaTime);
        progressImage.fillAmount = currentProgress;
    }

    public void Enable(Interactable interactable)
    {
        activeTarget = interactable;
        objectText.text = interactable.GetName();
        actionText.text = interactable.GetAction();
    }

    public void Refresh()
    {
        if(activeTarget == null)
            return;
        
        objectText.text = activeTarget.GetName();
        actionText.text = activeTarget.GetAction();
    }
    public void Disable()
    {
        activeTarget = null;
    }

    public void SetProgress(float progress)
    {
        targetProgress = progress;
    }
}
