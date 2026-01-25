using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Enemy : MonoBehaviour
{
    //Variables
    public EnemySO enemyInfo;
    public Transform centerTransform;
    public Health health;

    //Runtime References
    private Rigidbody rb;
    
    //Enemy Movement
    public enum NavigationType
    {
        StraightLine,
        Path
    }
    public enum TargetType
    {
        Position,
        Transform
    }

    [HideInInspector] public NavigationType currentNavigationType;
    public bool IsNavigatingByPath(){return currentNavigationType == NavigationType.Path;}

    [HideInInspector]public TargetType currentTargetType;
    private Vector3 targetPosition;
    private Transform targetTransform;
    private float targetDistance = 5f;

    //Path
    private NavMeshPath navPath;
    private int pathIndex = 0;

    private Vector3 targetDirVector;
    private Vector3 groundNormal;
    private Vector3 previousPosition;
    private float distanceFromTarget;
    
    //Performance
    [HideInInspector] public Vector3 transformPos;
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
    
    public virtual void FixedUpdateCall() // FixedUpdate()
    {
        Movement();
    }

    public virtual void UpdateCall() // Update()
    {
        transformPos = transform.position; //Using this is more performant. (possibly)

        UpdateGroundNormal();
        Rotate();

        // Update nav path index if navigating by nav
        if (IsNavigatingByPath()) 
        {
            if (navPath.corners.Length > 2) // (If the path is equal or less than 2 that means it's just a straight line)
            {
                //Update the target nav point
                if (navPath.corners.Length > pathIndex + 1 && Vector3.Distance(transformPos, navPath.corners[1]) < 0.2f)
                    pathIndex++;
            }
            else
            {
                //Stop navigating by path
                currentNavigationType = NavigationType.StraightLine;
            }
        }    
    }
    public virtual void UpdateBehaviour() // Called in intervals
    {
        CheckDistanceBehavour();

        UpdateNavigationBehavour();
    }
    public bool CheckDistanceBehavour()
    {
        transformPos = transform.position;
        Vector3 targetPos = GetTargetPosition();
        distanceFromTarget = Vector2.Distance(new Vector3(transformPos.x, transformPos.z), new Vector3(targetPos.x, targetPos.z));
        if (distanceFromTarget <= targetDistance)
            return true;
        return false;
    }
    public void UpdateNavigationBehavour()
    {
        //Make sure nav is off if we are near the target
        if (IsNavigatingByPath() && distanceFromTarget < 2f)
        {
            currentNavigationType = NavigationType.StraightLine;
        }
        //Turn on nav if we have stopped
        else if (IsNavigatingByPath() == false && distanceFromTarget > 2f && Vector3.Distance(previousPosition, transformPos) < EnemyManager.active.pathFindingTriggerMovedDistance)
        {
            currentNavigationType = NavigationType.Path;
        }

        //Calculate Nav Path
        if (IsNavigatingByPath())
        {
            NavMesh.CalculatePath(transformPos, GetTargetPosition(), NavMesh.AllAreas, navPath);
            pathIndex = 1;
        }

        previousPosition = transformPos;
    }
    private void Movement()
    {
        //Gravity
        rb.AddForce(-groundNormal * enemyInfo.gravityForce);

        //Check if at target
        CheckDistanceBehavour();
        if (distanceFromTarget <= targetDistance * 0.5f)
            return;

        //Get target Direction depending on navigation type
        if (IsNavigatingByPath() && navPath.corners.Length > pathIndex)
            targetDirVector = navPath.corners[pathIndex] - transformPos;
        else
            targetDirVector = GetTargetPosition() - transformPos;

        //Debug.DrawRay(transform.position,targetDir,navigatingByNav ? Color.red:Color.green);

        //2D and Normalize 
        targetDirVector = new Vector3(targetDirVector.x, 0, targetDirVector.z).normalized;
        
        //Move the enemy
        rb.MovePosition(transformPos + Vector3.ProjectOnPlane(targetDirVector, groundNormal).normalized * (enemyInfo.speed * Time.fixedDeltaTime));
    }

    private void Rotate()
    {
        if(targetDirVector != Vector3.zero)
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(targetDirVector.x,0, targetDirVector.z)), Time.deltaTime * enemyInfo.rotationSpeed);
    }

    private void UpdateGroundNormal()
    {
        //Debug.DrawRay(transform.position + new Vector3(0, 0.1f, 0) + targetDir * 0.25f,Vector3.down);
        if (Physics.Raycast(transformPos + new Vector3(0, 0.1f, 0) + targetDirVector * 0.25f, Vector3.down, out RaycastHit  hit ,0.2f, EnemyManager.active.groundLayer))
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
    public Vector3 GetTargetPosition()
    {
        switch (currentTargetType){
            default:
            case TargetType.Position:
                return targetPosition;
            case TargetType.Transform:
                return targetTransform.position;
        }
    }
    public void SetTarget(Vector3 pos, float targetDistance = 1f)
    {
        currentTargetType = TargetType.Position;
        targetPosition = pos;
        currentNavigationType = NavigationType.StraightLine;
        this.targetDistance = targetDistance;
    }
    public void SetTarget(Transform transform, float targetDistance = 1f)
    {
        currentTargetType = TargetType.Transform;
        targetTransform = transform;
        currentNavigationType = NavigationType.StraightLine;
        this.targetDistance = targetDistance;
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
