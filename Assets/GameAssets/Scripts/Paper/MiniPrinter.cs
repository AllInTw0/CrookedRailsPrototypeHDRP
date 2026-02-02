using System.Collections.Generic;
using UnityEngine;


public class MiniPrinter : MonoBehaviour
{
    public static MiniPrinter active;
    public class Notification
    {
        public Texture2D texture;
        public float time;
        public Notification(Texture2D texture, float time)
        {
            this.texture = texture;
            this.time = time;
        }
    }
    //Variables
    [SerializeField]
    private Camera renderCamera;
    [SerializeField]
    private SkinnedMeshRenderer paperMesh;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private string printerLoweredAnimationName;

    //Run time
    private List<Notification> notificationQueList = new List<Notification>();
    private Notification activeNotification;
    void Start()
    {
        animator.SetBool("Enabled", false);
        renderCamera.enabled = false;
        active = this;
    }

    void Update()
    {
        string animationName = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        //Debug.Log("Animation: " + animationName);
        if (activeNotification == null) 
        {
            if (animationName == printerLoweredAnimationName)
            {
                if(notificationQueList.Count > 0)
                {
                    //Begin Print
                    activeNotification = notificationQueList[0];
                    notificationQueList.RemoveAt(0);

                    Debug.Log("Texture: " + activeNotification.texture);
                    paperMesh.sharedMaterial.mainTexture = activeNotification.texture;

                    animator.SetBool("Enabled", true);
                    renderCamera.enabled = true;
                }
                else
                {
                    renderCamera.enabled = false;
                }
            }
        }
        else
        {
            activeNotification.time -= Time.deltaTime;
            if(activeNotification.time < 0f || InputManager.confirmAction.WasPerformedThisFrame())
            {
                animator.SetBool("Enabled", false);
                activeNotification = null;
            }
        }
    }

    public void AddNotification(Texture2D texture, float length = 5f)
    {
        notificationQueList.Add(new Notification(texture, length));
    }
}
