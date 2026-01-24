using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyBehaviour : MonoBehaviour
{
    //Variables
    [SerializeField] 
    private float speed = 1f;
    [SerializeField] 
    private float rotationSpeed = 1f;
    [SerializeField] 
    private float gravityForce = 10f;
    [SerializeField] 
    private float navTriggerMovedDistance = 0.2f;
    [SerializeField] 
    private LayerMask groundLayer;

    public Transform center;
    
    //Runtime References
    private Rigidbody rb;
    
    //Enemy state
    private Vector3 targetPosition;
    private NavMeshPath navPath;
    private int pathIndex = 0;
    private bool navigatingByNav;
    private Vector3 targetDir;
    private Vector3 groundNormal;
    private Vector3 previousPosition;
    private float distanceFromTarget;
    
    //Performance
    Vector3 transformPos;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Freeze();
        Invoke(nameof(UnFreeze),1.5f);
    }

    private void Start()
    {
        navPath = new NavMeshPath();
    }
    
    public void FixedUpdateCall()
    {
        Movement();
    }

    public void UpdateCall()
    {

        transformPos = transform.position; //Using this is more performant. (possibly)
        UpdateGroundNormal();
        Rotate();
        
        //Update nav path if navigating by nav 
        if (!navigatingByNav)
            return;
        
        if (navPath.corners.Length > 2) // (If the path is equal or less than 2 that means it's just a straight line)
        {
            if (navPath.corners.Length > pathIndex + 1 && Vector3.Distance(transformPos, navPath.corners[1]) < 0.2f)
                pathIndex++;
        }
        else
        {
            navigatingByNav = false;
        }
        
        
    }

    private void Movement()
    {
        //Gravity
        rb.AddForce(-groundNormal * gravityForce);
        
        //Possibly is more performant
        
        //Get target Direction depending on navigation type
        if (navigatingByNav && navPath.corners.Length > pathIndex)
            targetDir = navPath.corners[pathIndex] - transformPos;
        else
            targetDir = targetPosition - transformPos;

        //Debug.DrawRay(transform.position,targetDir,navigatingByNav ? Color.red:Color.green);
        
        //2D and Normalize 
        targetDir = new Vector3(targetDir.x, 0, targetDir.z).normalized;
        
        //Move the enemy
        rb.MovePosition(transformPos + Vector3.ProjectOnPlane(targetDir,groundNormal) * (speed * Time.fixedDeltaTime));
    }

    private void Rotate()
    {
        if(targetDir != Vector3.zero)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(targetDir.x,0,targetDir.z)), Time.deltaTime * rotationSpeed);
    }
    public void UpdateBehaviour()
    {
        if(PlayerHealth.active.isAlive)
            targetPosition = PlayerMovement.active.transform.position;
        else
            targetPosition = transform.position + new Vector3(Random.Range(-5f,5f),0f,Random.Range(-5f,5f));
        
        distanceFromTarget = Vector3.Distance(transform.position, targetPosition);
        
        //Make sure nav is off if we are near the target
        if (navigatingByNav && distanceFromTarget < 2f)
        {
            navigatingByNav = false;
        }
        
        //Turn on nav if we have stopped
        if (navigatingByNav == false && distanceFromTarget > 2f && Vector3.Distance(previousPosition, transform.position) < navTriggerMovedDistance)
        {
            navigatingByNav = true;
        }
        
        //Calculate Nav Path
        if (navigatingByNav)
        {
            NavMesh.CalculatePath(transform.position, targetPosition, NavMesh.AllAreas, navPath);
            pathIndex = 1;
        }

        previousPosition = transform.position;
    }
    private void UpdateGroundNormal()
    {
        //Debug.DrawRay(transform.position + new Vector3(0, 0.1f, 0) + targetDir * 0.25f,Vector3.down);
        if (Physics.Raycast(transformPos + new Vector3(0, 0.1f, 0) + targetDir * 0.25f, Vector3.down, out RaycastHit  hit ,0.2f, groundLayer))
        {
            groundNormal = hit.normal;
            
            if(Vector3.Angle(Vector3.up, groundNormal) > 60f)
                groundNormal = Vector3.up;
        }
        else
        {
            groundNormal = Vector3.up;
        }
    }

    public void Freeze()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
    public void UnFreeze()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
}
