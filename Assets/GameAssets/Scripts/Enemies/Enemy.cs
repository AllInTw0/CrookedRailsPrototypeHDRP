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
        Transform,
        Pack
    }

    [HideInInspector] public NavigationType currentNavigationType;
    public bool IsNavigatingByPath(){return currentNavigationType == NavigationType.Path;}

    [HideInInspector] public TargetType currentTargetType;
    private Vector3 targetPosition;
    private Transform targetTransform;
    [HideInInspector] public EnemyPack targetPack;
    private float targetDistance = 5f;
    //Animation
    [Header("Animation")]
    public Animator animator;
    public float spawnFreezeTime;
    public float walkCycleSpeedMult;

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
        Invoke(nameof(UnFreeze), spawnFreezeTime);
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
                if (navPath.corners.Length > pathIndex + 1 && Vector3.Distance(transformPos, navPath.corners[pathIndex]) < 0.2f)
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
        //if (IsNavigatingByPath() && distanceFromTarget < 1f)
        //{
        //    currentNavigationType = NavigationType.StraightLine;
        //}
        //Turn on nav if we have stopped
        if (IsNavigatingByPath() == false && Vector3.Distance(previousPosition, transformPos) < EnemyManager.active.pathFindingTriggerMovedDistance)
        {
            currentNavigationType = NavigationType.Path;
        }

        //Calculate Nav Path
        if (IsNavigatingByPath() && NavMesh.SamplePosition(transformPos,out NavMeshHit hitStart, 10f, NavMesh.AllAreas) && NavMesh.SamplePosition(GetTargetPosition(), out NavMeshHit hitEnd, 10f, NavMesh.AllAreas))
        {
            NavMesh.CalculatePath(hitStart.position, hitEnd.position, NavMesh.AllAreas, navPath);
            pathIndex = 1;
        }

        previousPosition = transformPos;
    }
    private void Movement()
    {
        //Gravity
        rb.AddForce(-groundNormal * enemyInfo.gravityForce);

        float targetSpeed = enemyInfo.speed;

        //Check if at target
        CheckDistanceBehavour();
        if (distanceFromTarget <= targetDistance * 0.5f)
            targetSpeed = 0f; 


        //Get target Direction depending on navigation type
        if (IsNavigatingByPath() && navPath.corners.Length > pathIndex)
            targetDirVector = navPath.corners[pathIndex] - transformPos;
        else
            targetDirVector = GetTargetPosition() - transformPos;

        //Debug.DrawRay(transform.position,targetDir,navigatingByNav ? Color.red:Color.green);

        //2D and Normalize and Project
        targetDirVector = new Vector3(targetDirVector.x, 0, targetDirVector.z).normalized;
        Vector3 projectedDirVector = Vector3.ProjectOnPlane(targetDirVector, groundNormal).normalized;

        //Move the enemy
        if(targetSpeed != 0f)
            rb.AddForce(projectedDirVector * enemyInfo.acceleration);

        //Drag
        Vector3 velocityPlane = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float velocity = velocityPlane.magnitude;
        if (velocity > 0.1f && targetSpeed == 0f)
        {
            rb.AddForce(-velocityPlane.normalized * enemyInfo.deceleration);
        }

        //Limit speed
        float angleBetweenDirections = Vector2.Angle(new Vector2(rb.linearVelocity.x, rb.linearVelocity.z), new Vector2(targetDirVector.x, targetDirVector.z));
        if (velocity > enemyInfo.speed && targetSpeed != 0f && angleBetweenDirections <= 90f)
        {
            float fallSpeed = rb.linearVelocity.y;
            Vector3 n = rb.linearVelocity.normalized * targetSpeed;
            rb.linearVelocity = new Vector3(n.x, fallSpeed, n.z);
        }

        //Counter movement
        if (targetSpeed != 0f && angleBetweenDirections > 90f)
        {
            rb.AddForce(projectedDirVector * enemyInfo.acceleration);
        }

        //Animation
        if (animator != null)
        {
            //Debug.Log("vel: " + velocity);
            animator.SetFloat("Velocity", velocity);
            animator.SetFloat("SpeedMult", velocity * walkCycleSpeedMult);
        }
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
            case TargetType.Pack:
                return targetPack.centerPos + targetPosition; // + offset
        }
    }
    public bool IsInPack()
    {
        return currentTargetType == TargetType.Pack;
    }
    public void SetTarget(Vector3 pos, float targetDistance = 1f)
    {
        if (IsInPack()) LeavePack();

        currentTargetType = TargetType.Position;
        targetPosition = pos;
        //currentNavigationType = NavigationType.StraightLine;
        this.targetDistance = targetDistance;
    }
    public void SetTarget(Transform transform, float targetDistance = 1f)
    {
        if (IsInPack()) LeavePack();

        currentTargetType = TargetType.Transform;
        targetTransform = transform;
        //currentNavigationType = NavigationType.StraightLine;
        this.targetDistance = targetDistance;
    }
    public void SetTarget(EnemyPack pack, Vector3 offset, float targetDistance = 1f)
    {
        if (IsInPack()) LeavePack();
        pack.AddEnemy(this);

        currentTargetType = TargetType.Pack;
        targetPack = pack;
        targetPosition = offset;
        //currentNavigationType = NavigationType.StraightLine;
        this.targetDistance = targetDistance;
    }
    public void LeavePack()
    {
        targetPack.RemoveEnemy(this);
        targetPack = null;
    }
    public void Freeze()
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }
    public void UnFreeze()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }
    public bool DoIWantToAttack()
    {
        return Vector3.Distance(transform.position, GetTargetPosition()) <= 5f;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = IsNavigatingByPath() ? Color.green : Color.red;
        Gizmos.DrawLine(transformPos, GetTargetPosition());
        if (IsNavigatingByPath() && navPath.corners.Length > pathIndex)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transformPos, navPath.corners[pathIndex]);
        }
    }
}
