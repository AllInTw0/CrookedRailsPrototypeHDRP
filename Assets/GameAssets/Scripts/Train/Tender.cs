using UnityEngine;

public class Tender : MonoBehaviour
{
    //Variables
    public static Tender active;


    public Health waterLevel;
    public Health fuelLevel;

    public Animator animator;
    public Transform waterHatch;
    public float waterHatchOffset;
    //Run Time
    [HideInInspector]
    public bool hatchOpened;

    private void Start()
    {
        active = this;
    }

    public void HatchInteract()
    {
        hatchOpened = !hatchOpened;
        if (hatchOpened)
            animator.SetBool("open", true);
        else
            animator.SetBool("open", false);
    }
}
